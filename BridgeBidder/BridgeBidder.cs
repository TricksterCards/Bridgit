using System;
using System.Collections.Generic;

namespace BridgeBidding
{


    public enum Direction { North = 0, East = 1, South = 2, West = 3 }

	public static class BridgeBidder
	{
		public static string SuggestBid(string deal, string vul, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			Direction dealer;
			var hands = ParseDeal(deal, out dealer);
			var vulPairs = ParseVul(vul);
			var bidHistory = ParseAuction(auction);

			// For now we will only allow SAYC bidding system.
            if (nsSystem != "SAYC" || ewSystem != "SAYC")
            {
                throw new ArgumentException("Bidding system is limited to SAYC");
            }
            IBiddingSystem sayc = new StandardAmerican();

			var biddingState = new BiddingState(hands, dealer, vulPairs, sayc, sayc);
			biddingState.ReplayAuction(bidHistory);
			var bid = biddingState.SuggestBid();

			return bid.ToString();
		}

		public static string[] ExplainHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}

		public static HandSummary[] SummarizeHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}


		public static HashSet<Pair> ParseVul(string vul)
		{
			var vulPairs = new HashSet<Pair>();
            switch (vul)
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
                    throw new ArgumentException($"Invalid vulnerablity parameter value {vul}");

            }
			return vulPairs;
        }

		private static Call[] ParseAuction(string auction)
		{
			var bidHistory = new List<Call>();
			if (auction == null) {
				throw new ArgumentNullException("auction");
			}
			var calls = auction.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var call in calls)
			{
				bidHistory.Add(Call.FromString(call));
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

/*
		public static Direction ParseDirection(string direction)
		{
			Direction d;
			if (direction == null)
			{
				throw new ArgumentNullException("directionString");
			}
			if (StringToDirection.TryGetValue(direction.ToUpper(), out d))
			{
				return d;
			};
			throw new ArgumentException($"{direction} is not a valid direction string value");
		}
*/
		private static Dictionary<string, Direction> StringToDirection = new Dictionary<string, Direction>
		{
			{ "N",     Direction.North },
			{ "NORTH", Direction.North },
			{ "E",     Direction.East  },
			{ "EAST",  Direction.East  },
			{ "S",     Direction.South },
			{ "SOUTH", Direction.South },
			{ "W",     Direction.West  },
			{ "WEST",  Direction.West  }
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
				throw new ArgumentException("dealer specifier invalid");
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
			foreach (Hand hand in hands.Values)
			{
				if (hand != null) 
				{
					allCards.UnionWith(hand);
					totalExpected += 13;
					if (allCards.Count < totalExpected)
					{
						throw new ArgumentException($"One or more duplicated cards in {deal}");
					}
				}
			}

            return hands;
   
		}

	}
}
