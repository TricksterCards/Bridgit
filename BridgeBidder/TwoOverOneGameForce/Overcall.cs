using System.Collections.Generic;


namespace BridgeBidding
{
    public class Overcall: TwoOverOneGameForce
    {

        public static new PositionCalls GetPositionCalls(PositionState ps)
        {

            var choices = new PositionCalls(ps);
            choices.AddRules(SuitOvercall);
            choices.AddRules(NoTrump.StrongOvercall);
            choices.AddRules(TakeoutDouble.InitiateConvention);
            choices.AddRules(NoTrump.BalancingOvercall);
            choices.AddPassRule(Points(LessThanOvercall));

            return choices;
        }

		public static IEnumerable<CallFeature> SuitOvercall(PositionState _)
        {
            return new CallFeature[] {
                // TODO: What is the level of interference we can take
                PartnerBids(Advance.FirstBid), 

                // Weak overcall takes precedence if good suit and low points
				Nonforcing(Bid._2D, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),
				Nonforcing(Bid._2H, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),
				Nonforcing(Bid._2S, Jump(1), CueBid(false), Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),

				Nonforcing(Bid._3C, Jump(1), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Nonforcing(Bid._3D, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Nonforcing(Bid._3H, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Nonforcing(Bid._3S, Jump(1, 2), CueBid(false), Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),

				Nonforcing(Bid._1D, Points(Overcall1Level), Shape(6, 10)),
				Nonforcing(Bid._1H, Points(Overcall1Level), Shape(6, 10)),
				Nonforcing(Bid._1S, Points(Overcall1Level), Shape(6, 10)),

                // TODO: May want to consider more rules for 1-level overcall.  If you have 10 points an a crummy suit for example...
                Nonforcing(Bid._1D, Points(Overcall1Level), Shape(5), DecentPlusSuit),
                Nonforcing(Bid._1H, Points(Overcall1Level), Shape(5), DecentPlusSuit),
                Nonforcing(Bid._1S, Points(Overcall1Level), Shape(5), DecentPlusSuit),


                Nonforcing(Bid._2C, CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid._2D, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid._2H, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
                Nonforcing(Bid._2S, Jump(0), CueBid(false), Points(OvercallStrong2Level), Shape(5, 11)),
            };
          
        }




        private static (int, int) SupportAdvancer = (12, 17);

        public static IEnumerable<CallFeature> SecondBid(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(Advance.SecondBid),

                Nonforcing(Bid._2H,   NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),
                Nonforcing(Bid._2S, NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),
                Nonforcing(Bid._3C, NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),
                Nonforcing(Bid._3D, NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),
                Nonforcing(Bid._3H, NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),
                Nonforcing(Bid._3S, NotRebid, Fit(), Jump(0), Points(SupportAdvancer), ShowsTrump),

               
                // TODO: Pass if appropriate
                // TODO: Rebid 6+ card suit if appropriate
                // TODO: Bid some level of NT if appropriate...

                Signoff(Bid._3NT, OppsStopped(), PairPoints((25, 30)) )

            };
        }


    }

}
