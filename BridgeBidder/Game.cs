using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace BridgeBidding
{
	public class Game
	{
		public string Event = null;
		public int Board = 0;	
		public Scoring Scoring = Scoring.MP;
		
		// TODO: I don't think we should do this here.  Probably just be strings, not objects
		// It dosen't make sense for this to 
		public string BidSystemEW = null;
		public string BidSystemNS = null;

        public Vulnerable Vulnerable = Vulnerable.None;
        public Direction Dealer = Direction.N;
        public Deal Deal { get; }
		public Direction? Declarer = null;

		public Contract Contract = null;

		public Auction Auction { get; }

		public Game()
		{
			Deal = new Deal(this);
			Auction = new Auction(this);
		}

		public void SetStandardBoard(int board)
		{
			if (board <= 0)
			{
				throw new ArgumentException($"Board number must be >=0.  Value is {board}");
			}
			this.Board = board;
			this.Dealer = (Direction)((board - 1) % 4);	// Deal goes N, E, S, W as does enum.
			int vulOffset = ((board - 1) / 4);
			this.Vulnerable = (Vulnerable)((board - 1 + vulOffset) % 4);
		}



		public Game Clone()
		{
			return Game.Parse(this.ToString());
		}

		// TODO: Is this really a thing?  Should this go away?
		public static Game Parse(string deal, string vulnerable)
		{	
			var game = new Game();
			game.ParseDeal(deal, overrideDealer: true);
			if (vulnerable == null)
			{
				throw new ArgumentNullException("vulnerable");
			}
			else
			{
				if (!Enum.TryParse<Vulnerable>(vulnerable, out game.Vulnerable))
				{
					throw new FormatException($"Vulnerable value {vulnerable} is invalid.");
				}
			}
			return game;
		}


		// Takes a single game in Portable Bridge Notation and returns Game class
		public static Game Parse(string pbnGame)
		{
			var game = new Game();
            var tags = PBN.FromString.TokenizeTags(pbnGame);

            foreach (var tag in tags)
            {
				if (tag.Name != "Note")
				{
					if (tag.Name == "Event")
					{
						game.Event = tag.Value;
					}
					if (tag.Name == "BidSystemEW")
					{
						game.BidSystemEW = tag.Value;
					}
					if (tag.Name == "BidSystemNS")
					{
						game.BidSystemNS = tag.Value;
					}
                    if (tag.Name == "Dealer")
                    {
                        Enum.TryParse<Direction>(tag.Value, out game.Dealer);
                    }
					if (tag.Name == "Board") 
					{
						int.TryParse(tag.Value, out game.Board);
					}
					if (tag.Name == "Deal")
					{
						game.ParseDeal(tag.Value);
					}
					if (tag.Name == "Auction")
					{
						game.ParseAuction(string.Join(" ", tag.Data));
					}
					if (tag.Name == "Scoring")
					{
						Enum.TryParse<Scoring>(tag.Value, out game.Scoring);
					}
					if (tag.Name == "Vulnerable")
					{
						Enum.TryParse<Vulnerable>(tag.Value, out game.Vulnerable);
					}
					if (tag.Name == "Declarer")
					{
						if (string.IsNullOrEmpty(tag.Value))
						{
							game.Declarer = null;
						}
						else
						{
							Direction d;
							Enum.TryParse<Direction>(tag.Value, out d);
							game.Declarer = d;
						}
					}
					if (tag.Name == "Contract")
					{
						if (string.IsNullOrEmpty(tag.Value))
						{
							game.Contract = null;
						}
						else
						{
							game.Contract = Contract.Parse(tag.Value);
						}
					}
					// TODO: Should tags we parse be added to the dictionary or not?  
					// For the time being i will say yes.
					game.Tags[tag.Name] = tag.Value;
					if (tag.Data.Count > 0)
					{
						game.TagData[tag.Name] = tag.Data;
					}
				}
            }
			return game;
		}



		public void ParseAuction(string auction)
		{
			// This sets the current auction to the value of auction.  Note that the auction
			// string is allowed to be the full [Auction ...] followed by the auction text and
			// optionally [Note] tags.  OR the text can simply be a series of calls.  
			this.Auction.Parse(auction);
		}

		public void ParseDeal(string deal, bool overrideDealer = false)
		{
			if (deal == null)
			{
				throw new ArgumentNullException("deal");
			}
			if (deal.Length < 9)
			{
				throw new FormatException("deal paramerter is too short to be valid PBN deal format");
			}
			Direction direction;
			if (deal.Substring(1, 1) != ":" || !Enum.TryParse<Direction>(deal.Substring(0,1), out direction))
			{
				throw new FormatException($"Deal prefix {deal.Substring(0, 2)} is invalid");
			};
			if (overrideDealer)
			{
				Dealer = direction;
			}
			else if (direction != Dealer)
			{
				throw new ArgumentException($"Deal direction prefix {direction} does not match game Dealer {Dealer}");
			}
            var handStrings = deal.Substring(2).Split(' ');
			if (handStrings.Length != 4) 
			{
				throw new ArgumentException("deal must contain 4 hands");
			}
			Debug.Assert(direction == Dealer);
            foreach (var handString in handStrings)
            {
				Deal[direction] = Hand.Parse(handString);
				direction = direction.LeftHandOpponent();
            }

			int totalExpected = 0;
			var allCards = new HashSet<Card>();
			foreach (var hand in Deal)
			{
				if (hand.Value != null) 
				{
					allCards.UnionWith(hand.Value);
					totalExpected += 13;
					if (allCards.Count < totalExpected)
					{
						foreach (var otherHand in Deal)
						{
							if (otherHand.Key != hand.Key && otherHand.Value != null)
							{
								var dup = otherHand.Value.Intersect(hand.Value);
								if (dup.Count() > 0)
								{
									throw new FormatException($"{dup.First()} duplicated in {hand.Key} and {otherHand.Key}");
								}
							}
						}
						throw new FormatException($"One or more duplicated cards in {deal}");
					}
				}
			}
        }


		public void DealCards(List<Card> deck)
		{
			if (deck.Count != 52)
			{
				throw new ArgumentException($"Deck must contain 52 cards.  deck parameter count = {deck.Count}");
			}
			var direction = Direction.N;
			for (int h = 0; h < 4; h++)
			{
				Deal[direction] =  new Hand(deck.GetRange(h * 13, 13));
				direction = direction.LeftHandOpponent();
			}
		}

		public void DealRandomHands()
		{
			DealCards(Card.NewDeck(true));
		}
    



		public Dictionary<string, string> Tags = new Dictionary<string, string>();
		public Dictionary<string, List<String>> TagData = new Dictionary<string, List<string>>();
		public Dictionary<string, string> TagCommentary = new Dictionary<string, string>();

		//public Notes AuctionNotes = new Notes();
		//public Notes PlayNotes = new Notes();

		public static string[] MTS = new string[]
		{
			"Event",
			"Site",
			"Date", 
			"Board",
			"West",       
			"North",      
			"East",       
			"South",      
			"Dealer",     
			"Vulnerable", 
			"Deal",       
			"Scoring",    
			"Declarer",   
			"Contract",   
			"Result"    
		};



		private string GetTagData(string tagName)
		{
			var s = "";
			if (TagData.ContainsKey(tagName))
			{
				foreach (var line in TagData[tagName])
				{
					s += $"{line}\n";
				}
			}
			return s;
		}

		private string GetCommentary(string tagName)
		{
			var s = "";
			if (TagCommentary.ContainsKey(tagName))
			{
				s = $"{{\n{TagCommentary[tagName]}\n}}\n";
			}
			return s;
		}

		public static string TagString(string key, string value)
		{
			if (value == null) return $"[{key} \"\"]\n";
			return $"[{key} \"{value}\"]\n";
		}

		private static string OptionalTag(string key, string value)
		{
			if (value == null) return "";
			return TagString(key, value);
		}


		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var tagName in MTS)
			{
				switch (tagName)
				{
					case "Event":
						sb.Append(TagString(tagName, Event));
						break;
					case "Dealer":
						sb.Append(TagString(tagName, Dealer.ToString()));
						break;
					case "Deal":
						sb.Append(TagString(tagName, Deal.ToString()));
						break;
					case "Vulnerable":
						sb.Append(TagString(tagName, Vulnerable.ToString()));
						break;
					case "Scoring":
						sb.Append(TagString(tagName, Scoring.ToString()));
						break;
					case "Board":
						sb.Append(TagString(tagName, Board == 0 ? "" : Board.ToString()));
						break;
					case "Declarer":
						sb.Append(TagString(tagName, Declarer == null ? "" : Declarer.ToString()));
						break;
					case "Contract":
						sb.Append(TagString(tagName, Contract == null ? "" : Contract.ToString()));
						break;
					// TODO: Need to do BidSystemxxx also.  But for now ignore it...
					default:
						if (Tags.ContainsKey(tagName))
						{
							sb.Append(TagString(tagName, Tags[tagName]));
							sb.Append(GetTagData(tagName));
							sb.Append(GetCommentary(tagName));
						}
						else 
						{
							sb.Append(TagString(tagName, ""));
						}
						break;
				}
			}
			sb.Append(Auction.ToString());	// Auction string contains all PBN tags.
			//sb.Append(GetSectionWithNotes("Auction", AuctionNotes));
			// TODO: Any tags not in MTS need to be alphabatized and rendered...
			sb.Append(OptionalTag("BidSystemEW", BidSystemEW));
			sb.Append(OptionalTag("BidSystemNS", BidSystemNS));
			return sb.ToString();
		}

		public void UpdateContractFromAuction()
		{
			var contract = ContractState.FromCalls(Dealer, Auction.Calls);
			if (contract.AuctionComplete)
			{
				this.Contract = contract;
				this.Declarer = contract.PassedOut ? null : contract.Declarer;
			}
			else
			{
				this.Contract = null;
				this.Declarer = null;
			}
		}

    }
}