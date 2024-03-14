using System.Collections.Generic;


namespace BridgeBidding
{
    public class Overcall: LCStandard
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
				Shows(Bid._2D, IsSingleJump, IsNotCueBid, Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),
				Shows(Bid._2H, IsSingleJump, IsNotCueBid, Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),
				Shows(Bid._2S, IsSingleJump, IsNotCueBid, Points(OvercallWeak2Level), Shape(6), GoodPlusSuit),

				Shows(Bid._3C, IsSingleJump, IsNotCueBid, Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Shows(Bid._3D, IsJump(1, 2), IsNotCueBid, Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Shows(Bid._3H, IsJump(1, 2), IsNotCueBid, Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),
				Shows(Bid._3S, IsJump(1, 2), IsNotCueBid, Points(OvercallWeak3Level), Shape(7), DecentPlusSuit),

                // We want to bid the highest suit first if we have two, so go from spades down to diamonds

				Shows(Bid._1S, Points(Overcall1Level), Shape(6, 10)),
				Shows(Bid._1H, Points(Overcall1Level), Shape(6, 10)),
				Shows(Bid._1D, Points(Overcall1Level), Shape(6, 10)),

                // TODO: May want to consider more rules for 1-level overcall.  If you have 10 points an a crummy suit for example...
                Shows(Bid._1S, Points(Overcall1Level), Shape(5), DecentPlusSuit),
                Shows(Bid._1H, Points(Overcall1Level), Shape(5), DecentPlusSuit),
                Shows(Bid._1D, Points(Overcall1Level), Shape(5), DecentPlusSuit),

                Shows(Bid._1S, Points(10, 16), Shape(5)),
                Shows(Bid._1H, Points(10, 16), Shape(5)),
                Shows(Bid._1D, Points(10, 16), Shape(5)),


                Shows(Bid._2S, IsNonJump, IsNotCueBid, Points(OvercallStrong2Level), Shape(5, 11)),
                Shows(Bid._2H, IsNonJump, IsNotCueBid, Points(OvercallStrong2Level), Shape(5, 11)),
                Shows(Bid._2D, IsNonJump, IsNotCueBid, Points(OvercallStrong2Level), Shape(5, 11)),
                Shows(Bid._2C, IsNotCueBid, Points(OvercallStrong2Level), Shape(5, 11))
            };
          
        }




        private static (int, int) SupportAdvancer = (12, 17);

        public static IEnumerable<CallFeature> SecondBid(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(Advance.SecondBid),

                Shows(Bid._2H, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),
                Shows(Bid._2S, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),
                Shows(Bid._3C, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),
                Shows(Bid._3D, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),
                Shows(Bid._3H, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),
                Shows(Bid._3S, IsNotRebid, Fit(), IsNonJump, Points(SupportAdvancer)),

                Shows(Bid._2H, IsRebid, Shape(6, 10)),
                Shows(Bid._2S, IsRebid, Shape(6, 10)),
                Shows(Bid._3C, IsRebid, Shape(6, 10), IsNonJump),
                Shows(Bid._3D, IsRebid, Shape(6, 10), IsNonJump),
                Shows(Bid._3H, IsRebid, Shape(6, 10), IsNonJump),
                Shows(Bid._3S, IsRebid, Shape(6, 10), IsNonJump),
               
                // TODO: Pass if appropriate
                // TODO: Rebid 6+ card suit if appropriate
                // TODO: Bid some level of NT if appropriate...

                Shows(Bid._3NT, OppsStopped(), PairPoints((25, 30))),
                Shows(Call.Pass)
            };
        }


    }

}
