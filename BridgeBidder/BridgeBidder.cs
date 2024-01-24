using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BridgeBidding
{
    public enum Direction { North = 0, East = 1, South = 2, West = 3 }

	public static class BridgeBidder
	{
		/// <summary>
		/// Suggests the next bid in the auction given a specified hand in a deal.  Where applicable, strings
		/// adhere to the Portable Bridge Notation (PBN) format.
		/// </summary>
		/// <param name="deal">Deal string in PBN format of D:h1 h2 h3 h4 where D: is the direction of the
		/// dealer, and h1-4 are full hands in PBN format or "-" to indicate unknown hands.  The hand for the next to act
		/// must be known.</param>
		/// <param name="vulnerable">One of "None", "All", "NS" or "EW"</param>
		/// <param name="auction">Can be null to or empty string to indicate no auction.  Otherwise contains
		/// a space separated string compatible with the PBN auction format.</param>
		/// <param name="bidSystemNS">For future use.</param>
		/// <param name="bidSysttemEW">For future use.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="AuctionException"></exception> 
		public static string SuggestBid(string deal, string vulnerable, string auction, string bidSystemNS = "TwoOverOneGameForce", string bidSystemEW = "TwoOverOneGameForce")
		{
			Direction dealer;
			var hands = ParseDeal(deal, out dealer);
			var vulPairs = ParseVulnerable(vulnerable);
			var bidHistory = ParseAuction(auction);

			// For now we will only allow SAYC bidding system.
            if (bidSystemNS != "TwoOverOneGameForce" || bidSystemEW != "TwoOverOneGameForce")
            {
                throw new ArgumentException("Bidding system is limited to 2/1");
            }
            IBiddingSystem twoOverOne = new TwoOverOneGameForce();

			var biddingState = new BiddingState(hands, dealer, vulPairs, twoOverOne, twoOverOne);
			biddingState.ReplayAuction(bidHistory);
			var call = biddingState.SuggestCall();

			//Debug.WriteLine(biddingState.NextToAct.RightHandOpponent.PublicHandSummary.ToString());

			return call.ToString();
		}

		// Kind of a hack for now - use for console app...
		public static string FullAuction(string deal, string vulnerable)
		{
			Direction dealer;
			var hands = ParseDeal(deal, out dealer);
			var vulPairs = ParseVulnerable(vulnerable);
		    IBiddingSystem twoOverOne = new TwoOverOneGameForce();

			var output = $"[Deal \"{deal}\"]\n[Vulnerable \"{vulnerable}\"]\n[Auction \"{deal.Substring(0,1)}\"]\n";
			var biddingState = new BiddingState(hands, dealer, vulPairs, twoOverOne, twoOverOne);
			var n = 0;
			while (!biddingState.Contract.AuctionComplete)
			{
				var call = biddingState.SuggestCall();
				output += call.ToString() + " ";
				n += 1;
				if (n % 4 == 0) output += "\n";
			}
			if (n % 4 != 0) output += "\n";
			output += $"[Contract \"{biddingState.Contract}\"]\n";
			return output;
		}
		public static string[] ExplainHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}

		public static HandSummary[] SummarizeHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}


		public static HashSet<Pair> ParseVulnerable(string vulnerable)
		{
			if (vulnerable == null)
				throw new ArgumentNullException("vulnerable");

			var vulPairs = new HashSet<Pair>();
            switch (vulnerable)
            {
                case "None":
                    break;
                case "All":
                    vulPairs.Add(Pair.NorthSouth);
					vulPairs.Add(Pair.EastWest);
					break;
                case "NS":
					vulPairs.Add(Pair.NorthSouth);
					break;
                case "EW":
					vulPairs.Add(Pair.EastWest);
					break;
                default:
                    throw new ArgumentException($"Invalid vulnerablity parameter value {vulnerable}");

            }
			return vulPairs;
        }

		// null is allowed for the auction string - returns an empty array of Calls.
		private static Call[] ParseAuction(string auction)
		{
			var bidHistory = new List<Call>();
			if (auction != null) 
			{
				var calls = auction.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var call in calls)
				{
					bidHistory.Add(Call.FromString(call));
				}
			}
			return bidHistory.ToArray();
		}


		public static Direction Partner(Direction direction)
		{
			return (Direction)(((int)direction + 2) % 4);
		}

		public static Direction RightHandOpponent(Direction direction)
		{
			return (Direction)(((int)direction + 3) % 4);
		}

		public static Direction LeftHandOpponent(Direction direction)
		{
			return (Direction)(((int)direction + 1) % 4);
		}


		private static Dictionary<string, Direction> StringToDirection = new Dictionary<string, Direction>
		{
			{ "N",     Direction.North },
			{ "E",     Direction.East  },
			{ "S",     Direction.South },
			{ "W",     Direction.West  },
		};

		// PBN defines deal import as D:(hand1) (hand2) (hand3) (hand4)
		// where "D" is be equal to the dealer.  
		
		private static Dictionary<Direction, Hand> ParseDeal(string deal, out Direction dealer)
		{
			if (deal == null)
			{
				throw new ArgumentNullException("deal");
			}
			if (deal.Length < 9)
			{
				throw new ArgumentException("deal paramerter is too short to be valid PBN deal format");
			}
			if (deal.Substring(1, 1) != ":" || !StringToDirection.TryGetValue(deal.Substring(0,1).ToUpper(), out dealer))
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
				hands[direction] = Hand.ParsePbnFormat(handString, requireFullHand: true);
				direction = LeftHandOpponent(direction);
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

            return hands;
   
		}

	}
}
