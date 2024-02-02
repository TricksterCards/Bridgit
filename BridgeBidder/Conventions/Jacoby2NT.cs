using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace BridgeBidding
{
    public class Jacoby2NT : Bidder
	{

        private static (int, int) RespondPoints = (13, 40);
        private static (int, int) OpenerPointsMin = (12, 13);
        private static (int, int) OpenerPointsMedium = (13, 14);
        private static (int, int) OpenerPointsMax = (15, 40);


		public static IEnumerable<CallFeature> InitiateConvention(PositionState ps)
		{
            Debug.Assert(!ps.IsPassedHand && ps.RHO.Passed);

			return new CallFeature[]
            {
                Convention(Bid._2NT, UserText.Jacoby2NT),
                Alert(Bid._2NT, UserText.Jacoby2NTDescription),

                PartnerBids(Bid._2NT, OpenerRebid),

                Forcing(Bid._2NT, Fit(Suit.Hearts), Shape(Suit.Hearts, 4, 10), DummyPoints(Suit.Hearts, RespondPoints)),
                Forcing(Bid._2NT, Fit(Suit.Spades), Shape(Suit.Spades, 4, 10), DummyPoints(Suit.Spades, RespondPoints))
            };

		}

        public static IEnumerable<CallFeature> OpenerRebid(PositionState ps)
        {
            return new CallFeature[]
            {
                Alert(Bid._3C, UserText.ShowsVoidOrSingleton),
                Alert(Bid._3D, UserText.ShowsVoidOrSingleton),
                Alert(Bid._3H, UserText.ShowsVoidOrSingleton, OpeningBid(Bid._1S)),
                Alert(Bid._3S, UserText.ShowsVoidOrSingleton, OpeningBid(Bid._1H)),

                // TODO: Any other alerts?  3NT?
                
                PartnerBids(PlaceContract),

                Forcing(Bid._3C, Shape(0, 1)),
                Forcing(Bid._3D, Shape(0, 1)),
                Forcing(Bid._3H, OpeningBid(Bid._1S), Shape(0, 1)),
                Forcing(Bid._3S, OpeningBid(Bid._1H), Shape(0, 1)),

                Forcing(Bid._3H, OpeningBid(Bid._1H), Points(OpenerPointsMax)),
                Forcing(Bid._3S, OpeningBid(Bid._1S), Points(OpenerPointsMax)),

                Forcing(Bid._3NT, Points(OpenerPointsMedium)),

                Nonforcing(Bid._4H, OpeningBid(Bid._1H), Points(OpenerPointsMin)),
                Nonforcing(Bid._4S, OpeningBid(Bid._1S), Points(OpenerPointsMin)),
            };
        }

        public static IEnumerable<CallFeature> PlaceContract(PositionState ps)
        {
            return new CallFeature[]
            {
                // TODO: IMPLEMENT RESPONSES!!!
                // TOOD: What is the thing to do here??  Perhaps bid controls.  Bid Blackwood. 
                Signoff(Bid._4H, OpeningBid(Bid._1H)),
                Signoff(Bid._4S, OpeningBid(Bid._1S))
            };            
        }
    }
}