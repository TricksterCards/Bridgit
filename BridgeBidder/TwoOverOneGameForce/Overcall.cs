﻿using System.Collections.Generic;


namespace BridgeBidding
{
    public class Overcall: TwoOverOneGameForce
    {

        public static new BidChoices GetBidChoices(PositionState ps)
        {

            var choices = new BidChoices(ps);
            choices.AddRules(SuitOvercall);
            choices.AddRules(NoTrump.StrongOvercall);
            choices.AddRules(TakeoutDouble.InitiateConvention);
            choices.AddRules(NoTrump.BalancingOvercall);

            // TODO: Perhaps do this for open also -- Pass as separate and final rule group...
            choices.AddRules(new BidRule[] { Nonforcing(Call.Pass, Points(LessThanOvercall)) });

            return choices;
        }

		public static IEnumerable<BidRule> SuitOvercall(PositionState _)
        {
            return new BidRule[] {
                // TODO: What is the level of interference we can take
                DefaultPartnerBids(Bid.FourNoTrump, Advance.FirstBid), 


                // Weak overcall takes precedence if good suit and low points
				Nonforcing(Bid.TwoDiamonds, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoHearts, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoSpades, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodSuit()),

				Nonforcing(Bid.ThreeClubs, Jump(1), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentSuit()),
				Nonforcing(Bid.ThreeDiamonds, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentSuit()),
				Nonforcing(Bid.ThreeHearts, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentSuit()),
				Nonforcing(Bid.ThreeSpades, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentSuit()),

				Nonforcing(Bid.OneDiamond, Points(Overcall1Level), Shape(6, 11)),
				Nonforcing(Bid.OneHeart, Points(Overcall1Level), Shape(6, 11)),
				Nonforcing(Bid.OneSpade, Points(Overcall1Level), Shape(6, 11)),

                // TODO: May want to consider more rules for 1-level overcall.  If you have 10 points an a crummy suit for example...
                Nonforcing(Bid.OneDiamond, Points(Overcall1Level), Shape(5), GoodSuit()),
                Nonforcing(Bid.OneHeart, Points(Overcall1Level), Shape(5), GoodSuit()),
                Nonforcing(Bid.OneSpade, Points(Overcall1Level), Shape(5), GoodSuit()),


                // TODO: NT Overcall needs to have suit stopped...

                Nonforcing(Bid.TwoClubs, CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid.TwoDiamonds, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid.TwoHearts, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid.TwoSpades, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),


            };
          
        }




        private static (int, int) SupportAdvancer = (12, 17);

        public static IEnumerable<BidRule> Rebid(PositionState _)
        {
            return new BidRule[] {
                DefaultPartnerBids(new Bid(4, Suit.Hearts), Advance.Rebid),

                Nonforcing(Bid.TwoHearts, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),
                Nonforcing(Bid.TwoSpades, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),
                Nonforcing(Bid.ThreeClubs, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),
                Nonforcing(Bid.ThreeDiamonds, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),
                Nonforcing(Bid.ThreeHearts, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),
                Nonforcing(Bid.ThreeSpades, Rebid(false), Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump()),

               
                // TODO: Pass if appropriate
                // TODO: Rebid 6+ card suit if appropriate
                // TODO: Bid some level of NT if appropriate...

                Signoff(Bid.ThreeNoTrump, OppsStopped(), PairPoints((25, 30)) )

            };
        }


    }

}