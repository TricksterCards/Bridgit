using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
    public class Advance : TwoOverOneGameForce
    {
        public static (int, int) AdvanceNewSuit1Level = (6, 40); // TODO: Highest level for this?
        public static (int, int) NewSuit2Level = (11, 40); // Same here...
        public static (int, int) AdvanceTo1NT = (6, 10);
        public static (int, int) PairAdvanceTo2NT = (23, 24);
        public static (int, int) PairAdvanceTo3NT = (25, 31);   
        public static (int, int) WeakJumpRaise = (0, 8);   // TODO: What is the high end of jump raise weak
        public static (int, int) Raise = (6, 10);
        public static (int, int) AdvanceCuebid = (11, 40);


        public static IEnumerable<CallFeature> FirstBid(PositionState ps)
        {
            if (ps.Partner.LastCall is Bid partnerBid &&
                partnerBid.Suit is Suit partnerSuit)
            {
                var bids = new List<CallFeature>
                {
                    // TODO: What is the level of interference we can take
                    PartnerBids(Overcall.SecondBid),

                                        // Weak jumps to game are highter priority than simple raises.
                    // Fill this out better but for now just go on law of total trump, jumping if weak.  
                    Nonforcing(Bid._4C, Jump(1, 2), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._4D, Break(true, "4D Weak"), Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._4H, Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._4S, Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump),


                    // If we have support for partner
                    Nonforcing(Bid._2D,  Fit(), DummyPoints(Raise), ShowsTrump),
                    Nonforcing(Bid._2H,    Fit(), DummyPoints(Raise), ShowsTrump),
                    Nonforcing(Bid._2S,    Fit(), DummyPoints(Raise), ShowsTrump),


                    Nonforcing(Bid._1H, Points(AdvanceNewSuit1Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._1H, Points(AdvanceNewSuit1Level), Shape(6, 11)),

                    Nonforcing(Bid._1S, Points(AdvanceNewSuit1Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._1S, Points(AdvanceNewSuit1Level), Shape(6, 11)),

               
                    // TODO: Should these be prioirty - 5 - support should be higher priorty.  Seems reasonable
                    Nonforcing(Bid._2C, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._2C, Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid._2D, Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._2D, Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid._2H, Jump(0), Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._2H, Jump(0), Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid._2S, Jump(0), Points(NewSuit2Level), Shape(5), GoodPlusSuit),
                    Nonforcing(Bid._2S, Jump(0), Points(NewSuit2Level), Shape(6, 11)),



                    // TODO: Make a special CallFeature here to handle rebid after cuebid...
                    Forcing(Bid._2C, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrumpSuit(partnerSuit)),
                    Forcing(Bid._2D, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrumpSuit(partnerSuit)),
                    Forcing(Bid._2H, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrumpSuit(partnerSuit)),
                    Forcing(Bid._2S, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrumpSuit(partnerSuit)),

 

                    Nonforcing(Bid._3C, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._3D, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._3H, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump),
                    Nonforcing(Bid._3S, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump),


                    // Need to differentiate between weak and strong overcalls and advance properly.
                    // Perhaps depend more on PairPoints(). 

                    // Lowest priority is to bid some level of NT - all fit() bids should be higher priority.
                    Nonforcing(Bid._1NT, OppsStopped(), Points(AdvanceTo1NT)),
                    Nonforcing(Bid._2NT, OppsStopped(), PairPoints(PairAdvanceTo2NT)),
					Nonforcing(Bid._3NT, OppsStopped(), PairPoints(PairAdvanceTo3NT))


                    // TODO: Any specification of PASS?>>
                };
                // TODO: Should this be higher priority?
                // TODO: Are there situations where 4NT is not blackwood.  Overcall 4D advanace 4NT?
                bids.AddRange(Blackwood.InitiateConvention(ps));
                bids.Add(Nonforcing(Call.Pass));
                return bids;
            }
            // TODO: Throw?  What?  Perhaps a new exception that just reverts
            // to competition if bidders fail but stop in debug mode...
            Debug.Fail("Partner.LastCall is not a bid.  How in the world did we get here?");
            return new CallFeature[0];
        }


        public static IEnumerable<CallFeature> SecondBid(PositionState _)
        {
            return new CallFeature[] { 
                // TODO: ONly bid these if they are necessary.  Minors don't need to go the 4-level unless forced there...
                Signoff(Bid._4C, Fit(), PairPoints((26, 28)), ShowsTrump),
                Signoff(Bid._4D, Fit(), PairPoints((26, 28)), ShowsTrump),
                Signoff(Bid._4H, Fit(), PairPoints((26, 31)), ShowsTrump),
                Signoff(Bid._4S, Fit(), PairPoints((26, 31)), ShowsTrump),

                Signoff(Call.Pass),
            };
        }

    }
}
