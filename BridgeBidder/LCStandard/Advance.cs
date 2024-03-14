using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
    public class Advance : LCStandard
    {
        public static (int, int) AdvanceNewSuit1Level = (6, 40); // TODO: Highest level for this?
        public static (int, int) NewSuit2Level = (11, 40); // Same here...
        public static (int, int) AdvanceTo1NT = (6, 10);
        public static (int, int) PairAdvanceTo2NT = (23, 24);
        public static (int, int) PairAdvanceTo3NT = (25, 31);   
        public static (int, int) WeakJumpRaise = (0, 8);   // TODO: What is the high end of jump raise weak
        public static (int, int) Raise = (6, 10);
        public static (int, int) AdvanceCuebid = (11, 40);


        public static PositionCalls FirstBid(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            if (ps.Partner.LastCall is Bid partnerBid &&
                partnerBid.Suit is Suit partnerSuit)
            {
                choices.AddRules(
                    // TODO: What is the level of interference we can take
                    PartnerBids(Overcall.SecondBid),

                                        // Weak jumps to game are highter priority than simple raises.
                    // Fill this out better but for now just go on law of total trump, jumping if weak.  
                    Shows(Bid._4C, IsJump(1, 2), Fit(10), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._4D, IsJump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._4H, IsJump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._4S, IsJump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise)),


                    // If we have support for partner
                    Shows(Bid._2D, RaisePartner(), DummyPoints(Raise)),
                    Shows(Bid._2H, RaisePartner(), DummyPoints(Raise)),
                    Shows(Bid._2S, RaisePartner(), DummyPoints(Raise)),


                    Shows(Bid._1H, Points(AdvanceNewSuit1Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._1H, Points(AdvanceNewSuit1Level), Shape(6, 11)),

                    Shows(Bid._1S, Points(AdvanceNewSuit1Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._1S, Points(AdvanceNewSuit1Level), Shape(6, 11)),

               
                    // TODO: Should these be prioirty - 5 - support should be higher priorty.  Seems reasonable
                    Shows(Bid._2C, IsNewSuit, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._2C, IsNewSuit, Points(NewSuit2Level), Shape(6, 11)),
                    Shows(Bid._2D, IsNewSuit, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._2D, IsNewSuit, Points(NewSuit2Level), Shape(6, 11)),
                    Shows(Bid._2H, IsNewSuit, IsNonJump, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._2H, IsNewSuit, IsNonJump, Points(NewSuit2Level), Shape(6, 11)),
                    Shows(Bid._2S, IsNewSuit, IsNonJump, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Shows(Bid._2S, IsNewSuit, IsNonJump, Points(NewSuit2Level), Shape(6, 11)),



                    // TODO: Make a special CallFeature here to handle rebid after cuebid...
                    Forcing(Bid._2C, IsCueBid, Fit(partnerSuit), DummyPoints(AdvanceCuebid)),
                    Forcing(Bid._2D, IsCueBid, Fit(partnerSuit), DummyPoints(AdvanceCuebid)),
                    Forcing(Bid._2H, IsCueBid, Fit(partnerSuit), DummyPoints(AdvanceCuebid)),
                    Forcing(Bid._2S, IsCueBid, Fit(partnerSuit), DummyPoints(AdvanceCuebid)),

 

                    Shows(Bid._3C, IsSingleJump, Fit(9), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._3D, IsSingleJump, Fit(9), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._3H, IsSingleJump, Fit(9), DummyPoints(WeakJumpRaise)),
                    Shows(Bid._3S, IsSingleJump, Fit(9), DummyPoints(WeakJumpRaise)),


                    // Need to differentiate between weak and strong overcalls and advance properly.
                    // Perhaps depend more on PairPoints(). 

                    // Lowest priority is to bid some level of NT - all fit() bids should be higher priority.
                    Shows(Bid._1NT, OppsStopped(), Points(AdvanceTo1NT)),
                    Shows(Bid._2NT, OppsStopped(), PairPoints(PairAdvanceTo2NT)),
					Shows(Bid._3NT, OppsStopped(), PairPoints(PairAdvanceTo3NT))


                    // TODO: Any specification of PASS?>>
                );
                // TODO: Should this be higher priority?
                // TODO: Are there situations where 4NT is not blackwood.  Overcall 4D advanace 4NT?
                choices.AddRules(Blackwood.InitiateConvention(ps));
                choices.AddPassRule();
                return choices;
            }
            // TODO: Throw?  What?  Perhaps a new exception that just reverts
            // to competition if bidders fail but stop in debug mode...
            Debug.Fail("Partner.LastCall is not a bid.  How in the world did we get here?");
            return choices;
        }


        public static IEnumerable<CallFeature> SecondBid(PositionState _)
        {
            return new CallFeature[] { 
                // TODO: ONly bid these if they are necessary.  Minors don't need to go the 4-level unless forced there...
                Shows(Bid._4C, Fit(), PairPoints((26, 28))),
                Shows(Bid._4D, Fit(), PairPoints((26, 28))),
                Shows(Bid._4H, Fit(), PairPoints((26, 31))),
                Shows(Bid._4S, Fit(), PairPoints((26, 31))),

                Shows(Call.Pass),
            };
        }

    }
}
