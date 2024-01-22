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
                    PartnerBids(Overcall.Rebid),

                                        // Weak jumps to game are highter priority than simple raises.
                    // Fill this out better but for now just go on law of total trump, jumping if weak.  
                    Nonforcing(Bid.FourClubs, Jump(1, 2), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.FourDiamonds, Break(true, "4D Weak"), Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.FourHearts, Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.FourSpades, Jump(1, 2, 3), Fit(10), DummyPoints(WeakJumpRaise), ShowsTrump()),


                    // If we have support for partner
                    Nonforcing(Bid.TwoDiamonds,  Fit(), DummyPoints(Raise), ShowsTrump()),
                    Nonforcing(Bid.TwoHearts,    Fit(), DummyPoints(Raise), ShowsTrump()),
                    Nonforcing(Bid.TwoSpades,    Fit(), DummyPoints(Raise), ShowsTrump()),


                    Nonforcing(Bid.OneHeart, Points(AdvanceNewSuit1Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.OneHeart, Points(AdvanceNewSuit1Level), Shape(6, 11)),

                    Nonforcing(Bid.OneSpade, Points(AdvanceNewSuit1Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.OneSpade, Points(AdvanceNewSuit1Level), Shape(6, 11)),

               
                    // TODO: Should these be prioirty - 5 - support should be higher priorty.  Seems reasonable
                    Nonforcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid.TwoHearts, Jump(0), Points(NewSuit2Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.TwoHearts, Jump(0), Points(NewSuit2Level), Shape(6, 11)),
                    Nonforcing(Bid.TwoSpades, Jump(0), Points(NewSuit2Level), Shape(5), GoodSuit()),
                    Nonforcing(Bid.TwoSpades, Jump(0), Points(NewSuit2Level), Shape(6, 11)),



                    // TODO: Make a special CallFeature here to handle rebid after cuebid...
                    Forcing(Bid.TwoClubs, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrump(partnerSuit)),
                    Forcing(Bid.TwoDiamonds, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrump(partnerSuit)),
                    Forcing(Bid.TwoHearts, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrump(partnerSuit)),
                    Forcing(Bid.TwoSpades, CueBid(), Fit(partnerSuit), DummyPoints(AdvanceCuebid), ShowsTrump(partnerSuit)),

 

                    Nonforcing(Bid.ThreeClubs, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.ThreeDiamonds, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.ThreeHearts, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump()),
                    Nonforcing(Bid.ThreeSpades, Jump(1), Fit(9), DummyPoints(WeakJumpRaise), ShowsTrump()),


                    // Need to differentiate between weak and strong overcalls and advance properly.
                    // Perhaps depend more on PairPoints(). 

                    // Lowest priority is to bid some level of NT - all fit() bids should be higher priority.
                    Nonforcing(Bid.OneNoTrump, OppsStopped(), Points(AdvanceTo1NT)),
                    Nonforcing(Bid.TwoNoTrump, OppsStopped(), PairPoints(PairAdvanceTo2NT)),
					Nonforcing(Bid.ThreeNoTrump, OppsStopped(), PairPoints(PairAdvanceTo3NT))


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


        public static IEnumerable<CallFeature> Rebid(PositionState _)
        {
            return new CallFeature[] { 
                // TODO: ONly bid these if they are necessary.  Minors don't need to go the 4-level unless forced there...
                Signoff(Bid.FourClubs, Fit(), PairPoints((26, 28)), ShowsTrump()),
                Signoff(Bid.FourDiamonds, Fit(), PairPoints((26, 28)), ShowsTrump()),
                Signoff(Bid.FourHearts, Fit(), PairPoints((26, 31)), ShowsTrump()),
                Signoff(Bid.FourSpades, Fit(), PairPoints((26, 31)), ShowsTrump())
            };
        }

    }
}
