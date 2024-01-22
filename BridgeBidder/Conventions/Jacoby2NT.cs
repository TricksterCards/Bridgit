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
                Convention(Bid.TwoNoTrump, UserText.Jacoby2NT),
                Alert(Bid.TwoNoTrump, UserText.Jacoby2NTDescription),

                PartnerBids(Bid.TwoNoTrump, OpenerRebid),

                Forcing(Bid.TwoNoTrump, Fit(Suit.Hearts), Shape(Suit.Hearts, 4, 10), DummyPoints(Suit.Hearts, RespondPoints)),
                Forcing(Bid.TwoNoTrump, Fit(Suit.Spades), Shape(Suit.Spades, 4, 10), DummyPoints(Suit.Spades, RespondPoints))
            };

		}

        public static IEnumerable<CallFeature> OpenerRebid(PositionState ps)
        {
            return new CallFeature[]
            {
                Alert(Bid.ThreeClubs, UserText.ShowsVoidOrSingleton),
                Alert(Bid.ThreeDiamonds, UserText.ShowsVoidOrSingleton),
                Alert(Bid.ThreeHearts, UserText.ShowsVoidOrSingleton, OpeningBid(Bid.OneSpade)),
                Alert(Bid.ThreeSpades, UserText.ShowsVoidOrSingleton, OpeningBid(Bid.OneHeart)),

                // TODO: Any other alerts?  3NT?
                
                PartnerBids(PlaceContract),

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

        public static IEnumerable<CallFeature> PlaceContract(PositionState ps)
        {
            return new CallFeature[]
            {
                // TODO: IMPLEMENT RESPONSES!!!
                // TOOD: What is the thing to do here??  Perhaps bid controls.  Bid Blackwood. 
                Signoff(Bid.FourHearts, OpeningBid(Bid.OneHeart)),
                Signoff(Bid.FourSpades, OpeningBid(Bid.OneSpade))
            };            
        }
    }
}