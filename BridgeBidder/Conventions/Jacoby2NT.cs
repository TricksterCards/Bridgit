using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class Jacoby2NT : Bidder
	{

        private static (int, int) RespondPoints = (13, 40);
        private static (int, int) OpenerPointsMin = (12, 13);
        private static (int, int) OpenerPointsMedium = (13, 14);
        private static (int, int) OpenerPointsMax = (15, 40);


		public static IEnumerable<BidRule> InitiateConvention(PositionState ps)
		{
			return new BidRule[]
            {
                // DefaultPartnerBids...
                PartnerBids(Bid.TwoNoTrump, OpenerRebid),
                Forcing(Bid.TwoNoTrump, Fit(Suit.Hearts), Shape(Suit.Hearts, 4, 10), DummyPoints(Suit.Hearts, RespondPoints)),
                Forcing(Bid.TwoNoTrump, Fit(Suit.Spades), Shape(Suit.Spades, 4, 10), DummyPoints(Suit.Spades, RespondPoints))
            };

		}

        public static IEnumerable<BidRule> OpenerRebid(PositionState ps)
        {
            return new BidRule[]
            {
                Forcing(Bid.ThreeClubs, Shape(0, 1)),
                Forcing(Bid.ThreeDiamonds, Shape(0, 1)),
                Forcing(Bid.ThreeHearts, OpeningBid(Bid.OneSpade), Shape(0, 1)),
                Forcing(Bid.ThreeSpades, OpeningBid(Bid.OneHeart), Shape(0, 1)),

                Forcing(Bid.ThreeHearts, OpeningBid(Bid.OneHeart), Points(OpenerPointsMax)),
                Forcing(Bid.ThreeSpades, OpeningBid(Bid.OneSpade), Points(OpenerPointsMax)),

                Forcing(Bid.ThreeNoTrump, Points(OpenerPointsMedium)),

                Nonforcing(Bid.FourHearts, OpeningBid(Bid.OneHeart), Points(OpenerPointsMin)),
                Nonforcing(Bid.FourSpades, OpeningBid(Bid.OneSpade), Points(OpenerPointsMin)),
            };
        }
    }
}