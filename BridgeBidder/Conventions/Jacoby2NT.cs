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
            if (ps.Partner.Bid.Suit is Suit suit && suit.IsMajor())
            {
                return new CallFeature[]
                {
                    Properties(Bid._2NT, OpenerRebid, forcingToGame: true, alert: UserText.Jacoby2NTDescription, convention: UserText.Jacoby2NT),

                    Shows(Bid._2NT, Fit(suit), Shape(suit, 4, 10), DummyPoints(suit, RespondPoints)),
                };
            }
            return new CallFeature[0];
		}

        public static PositionCalls OpenerRebid(PositionState ps)
        {
            return new PositionCalls(ps).AddRules(
                PartnerBids(PlaceContract),

                Properties(Bid._3C, alert: UserText.ShowsVoidOrSingleton),
                Properties(Bid._3D, alert: UserText.ShowsVoidOrSingleton),
                Properties(Bid._3H, alert: UserText.ShowsVoidOrSingleton, onlyIf: IsOpeningBid(Bid._1S)),
                Properties(Bid._3S, alert: UserText.ShowsVoidOrSingleton, onlyIf: IsOpeningBid(Bid._1H)),

                // TODO: Any other alerts?  3NT?
                Shows(Bid._3C, Shape(0, 1)),
                Shows(Bid._3D, Shape(0, 1)),
                Shows(Bid._3H, IsOpeningBid(Bid._1S), Shape(0, 1)),
                Shows(Bid._3S, IsOpeningBid(Bid._1H), Shape(0, 1)),

                Shows(Bid._3H, IsOpeningBid(Bid._1H), Points(OpenerPointsMax)),
                Shows(Bid._3S, IsOpeningBid(Bid._1S), Points(OpenerPointsMax)),

                // TODO: Should this be points in the bid suit?  Pair Points()?  Same with 3M bids...
                Shows(Bid._3NT, Points(OpenerPointsMedium)),

                Shows(Bid._4H, IsOpeningBid(Bid._1H), Points(OpenerPointsMin)),
                Shows(Bid._4S, IsOpeningBid(Bid._1S), Points(OpenerPointsMin))
            );
        }

        public static PositionCalls PlaceContract(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(Blackwood.InitiateConvention);
            choices.AddRules(new CallFeature[]
            {
                // TODO: IMPLEMENT RESPONSES!!!
                // TOOD: What is the thing to do here??  Perhaps bid controls.  
                Shows(Bid._4H, IsOpeningBid(Bid._1H)),
                Shows(Bid._4S, IsOpeningBid(Bid._1S)),

                Shows(Bid.Pass)
            });     
            return choices;       
        }
    }
}