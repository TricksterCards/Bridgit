using System;
using System.Collections.Generic;

namespace BridgeBidding
{


    public enum Direction { North = 0, East = 1, South = 2, West = 3 }

	public static class BridgeBidder
	{
		public static string SuggestBid(string hand, string vul, string[] historyStrings)
		{
			var handList = new List<Hand> { null, null, null, null };
			var i = historyStrings.Length % 4;
			
			handList[i] = Hand.FromTricksterFormat(hand);

			var biddingState = new BiddingState(handList.ToArray(), Direction.North, vul);

			var bid = biddingState.SuggestBid(historyStrings);

			return bid.ToString();
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

	}
}
