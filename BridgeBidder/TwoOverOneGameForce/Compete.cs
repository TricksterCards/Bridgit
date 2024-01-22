using System.Collections.Generic;

namespace BridgeBidding
{
    internal class Compete : Bidder
    {
        private static (int, int) CompeteTo2 = (20, 22);
        private static (int, int) CompeteTo3 = (23, 25);
        private static (int, int) CompeteTo2NT = (20, 24);
        private static (int, int) CompeteTo3NT = (25, 31); // TODO: Add more...
        private static (int, int) CompeteTo4 = (26, 28);
        private static (int, int) CompeteTo5 = (29, 32);



        // TODO: This is super ugly.  Need to think through how bids work / fall-through or get them like this
        // throug a static function.  These are all duplicated.  Can be appended to the end of another list.  
        // right now used by ResponderRebid.  

        public static IEnumerable<CallFeature> CompBids(PositionState ps)
        {
            var bids = new List<CallFeature>();
            bids.AddRange(Blackwood.InitiateConvention(ps));
            bids.AddRange(Gerber.InitiateConvention(ps));
            bids.AddRange(new CallFeature[]
            {

             //   Nonforcing(Call.Pass, 0),    // TOD   aO: What points?  This is the last gasp attempt here...

                Nonforcing(Bid.FourHearts, Fit(), PairPoints(CompeteTo4), ShowsTrump()),
                Nonforcing(Bid.FourSpades, Fit(), PairPoints(CompeteTo4), ShowsTrump()),



                Nonforcing(Bid.TwoClubs, Fit(), PairPoints(CompeteTo2), ShowsTrump()),
                Nonforcing(Bid.TwoDiamonds, Fit(), PairPoints(CompeteTo2), ShowsTrump()),
                Nonforcing(Bid.TwoHearts, Fit(), PairPoints(CompeteTo2), ShowsTrump()),
                Nonforcing(Bid.TwoSpades, Fit(), PairPoints(CompeteTo2), ShowsTrump()),

                Nonforcing(Bid.ThreeClubs,  Fit(), PairPoints(CompeteTo3), ShowsTrump()),
                Nonforcing(Bid.ThreeDiamonds,  Fit(), PairPoints(CompeteTo3), ShowsTrump()),
                Nonforcing(Bid.ThreeHearts, Fit(), PairPoints(CompeteTo3), ShowsTrump()),
                Nonforcing(Bid.ThreeSpades, Fit(), PairPoints(CompeteTo3), ShowsTrump()),

                Signoff(Bid.ThreeNoTrump, OppsStopped(), PairPoints(CompeteTo3NT)),

                Signoff(Bid.TwoNoTrump, OppsContract(), OppsStopped(), PairPoints(CompeteTo2NT)),


                Nonforcing(Bid.FourClubs, Not(Gerber.Applies), Fit(), PairPoints(CompeteTo4), ShowsTrump()),
                Nonforcing(Bid.FourDiamonds, Fit(), PairPoints(CompeteTo4), ShowsTrump()),

                Nonforcing(Bid.FiveClubs, Fit(), PairPoints(CompeteTo5), ShowsTrump()),
                Nonforcing(Bid.FiveDiamonds, Fit(), PairPoints(CompeteTo5), ShowsTrump()),

                // TODO: Penalty doubles for game contracts.
                //Signoff(Call.Double, OppsContract(), PairPoints((12, 40)), RuleOf9()),


            });
            bids.AddRange(ForcedBid.Bids(ps));
            bids.Add(Nonforcing(Call.Pass));
            return bids;
        }


    }
}
