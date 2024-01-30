using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace BridgeBidding.PBN
{
    public static class FromString
    {
		
		public static Board Board(string deal, string vulnerable, int? number = null)
		{
			var parsedDeal = FromString.Deal(deal);
			return new Board
			{
				Number = number,
				Dealer = parsedDeal.Dealer,
				Hands = parsedDeal.Hands,
				Vulnerable = FromString.Vulnerable(vulnerable)
			};
		}

		public static (Direction Dealer, Dictionary<Direction, Hand> Hands) Deal(string deal)
		{
            Direction dealer;
			if (deal == null)
			{
				throw new ArgumentNullException("deal");
			}
			if (deal.Length < 9)
			{
				throw new ArgumentException("deal paramerter is too short to be valid PBN deal format");
			}
			if (deal.Substring(1, 1) != ":" || !Enum.TryParse<Direction>(deal.Substring(0,1), out dealer))
			{
				throw new ArgumentException($"Dealer prefix {deal.Substring(0, 2)} is invalid");
			};
          	var hands = new Dictionary<Direction, Hand>();
            var handStrings = deal.Substring(2).Split(' ');
			if (handStrings.Length != 4) 
			{
				throw new ArgumentException("deal must contain 4 hands");
			}
			var direction = dealer;
            foreach (var handString in handStrings)
            {
				hands[direction] = FromString.Hand(handString);
				direction = BridgeBidder.LeftHandOpponent(direction);
            }

			int totalExpected = 0;
			var allCards = new HashSet<Card>();
			foreach (var hand in hands)
			{
				if (hand.Value != null) 
				{
					allCards.UnionWith(hand.Value);
					totalExpected += 13;
					if (allCards.Count < totalExpected)
					{
						foreach (var otherHand in hands)
						{
							if (otherHand.Key != hand.Key && otherHand.Value != null)
							{
								var dup = otherHand.Value.Intersect(hand.Value);
								if (dup.Count() > 0)
								{
									throw new ArgumentException($"{dup.First()} duplicated in {hand.Key} and {otherHand.Key}");
								}
							}
						}
						// Throw a useful excpetion to help debug problems with 
						throw new ArgumentException($"One or more duplicated cards in {deal}");
					}
				}
			}

            return (dealer, hands);
		}

		public static Suit[] HandSuitOrder = new Suit[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };

		public static Hand Hand(string handString)
		{
			if (handString == null)
			{
				throw new ArgumentNullException("handString");
			}
			if (handString == "-") 
			{
				return null;
			}

			var suits = handString.Split('.');
			if (suits.Length != HandSuitOrder.Length)
			{
				throw new ArgumentException("handString does not contain four suits");
			}
			Hand hand = new Hand();			
			for (var i = 0; i < suits.Length; i++)
				foreach (var rankChar in suits[i])
				{
					var card = new Card(Card.ParseRank(rankChar), HandSuitOrder[i]);
					if (hand.Contains(card))
					{
						throw new ArgumentException($"Duplicate card {card} in {handString}");
					}
					hand.Add(card);
				}
		
			if (hand.Count != 13)
			{
				throw new ArgumentException($"hand {handString} contains {hand.Count} cards.  Should have 13.");
			}

			return hand;
		}


		// TODO: Is this correct?  Should we require a value?
		// If the null string is passed into this method, Vulnerable.None is returned.
		public static Vulnerable Vulnerable(string vulnerable)
		{
			if (vulnerable == null)
			{
				throw new ArgumentNullException("vulnerable");
			}
			Vulnerable vulEnum;
			if (Enum.TryParse<Vulnerable>(vulnerable, out vulEnum))
			{
				return vulEnum;
			}
            throw new ArgumentException($"Invalid vulnerablity parameter value {vulnerable}");
        }

        // TODO: Annothations along with the calls???  Seems overkill and silly
		// null is allowed for the auction string - returns an empty array of Calls.
		public static Call[] Auction(List<string> auctionData)
		{
			return Auction(string.Join(" ", auctionData));
		}

		// TOOD: Perhaps make public but probably not.   Really want to parse note section too, so probs
		// need more parameterst to above function
		public static Call[] Auction(string auction)
		{	
			var bidHistory = new List<Call>();
			if (auction != null) 
			{
				var tokens = auction.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var token in tokens)
				{
					if (!token.StartsWith("$") && !token.StartsWith("="))
					{
						bidHistory.Add(Call.FromString(token));
					}
				}
			}
			return bidHistory.ToArray();
		}
	
       	public static List<Game> Games(string text)
        {
            var games = new List<Game>();
			Game game = null; 
            var tags = TokenizeTags(text);

			// This code is not completely correct in that it triggers off of the [Event] tag to indicate the
			// start of a game section of a afile.


            foreach (var tag in tags)
            {
				if (tag.Name == "Event")
				{		
					if (game != null)
					{
						games.Add(game);
					}
					game = new Game();
				}
				Debug.Assert(game != null);
				// For now we will not import Note tags.  Ignored.  They are the only tags allowed to be duplicates.
				// TODO: Deal with note in auction and play sections.
				if (tag.Name != "Note")
				{
					game.Tags[tag.Name] = tag.Value;
					if (tag.Data.Count > 0)
					{
						game.TagData[tag.Name] = tag.Data;
					}
				}
            }

			if (game != null)
			{
				games.Add(game);
			}

            return games;
        }




        private static List<string> ImportAuction(List<string> bidLines)
        {
            return string.Join(" ", bidLines)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }


        private static List<PBNTag> TokenizeTags(string text)
        {
            var tag = new PBNTag { Data = new List<string>() };
            var tags = new List<PBNTag>();
            var lines = text.Split(new[] { "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("%"));
            foreach (var line in lines)
                if (line.StartsWith("["))
                {
                    tag = new PBNTag { Data = new List<string>() };
                    tags.Add(tag);
                    tag.Name = line.Substring(1, line.IndexOf(' ') - 1);
                    var start = line.IndexOf('"') + 1;
                    var end = line.LastIndexOf('"') - start;
                    tag.Value = line.Substring(start, end);
                }
                else
                {
                    tag.Data.Add(line);
                }

            return tags;
        }

        private class PBNTag
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public List<string> Data { get; set; }
        }
	}
}