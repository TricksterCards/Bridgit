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

                Nonforcing(Bid._4H, Fit(), PairPoints(CompeteTo4), ShowsTrump),
                Nonforcing(Bid._4S, Fit(), PairPoints(CompeteTo4), ShowsTrump),

                Nonforcing(Bid._2C, Fit(), PairPoints(CompeteTo2), ShowsTrump),
                Nonforcing(Bid._2D, Fit(), PairPoints(CompeteTo2), ShowsTrump),
                Nonforcing(Bid._2H, Fit(), PairPoints(CompeteTo2), ShowsTrump),
                Nonforcing(Bid._2S, Fit(), PairPoints(CompeteTo2), ShowsTrump),

                Nonforcing(Bid._3C, Fit(), PairPoints(CompeteTo3), ShowsTrump),
                Nonforcing(Bid._3D, Fit(), PairPoints(CompeteTo3), ShowsTrump),
                Nonforcing(Bid._3H, Fit(), PairPoints(CompeteTo3), ShowsTrump),
                Nonforcing(Bid._3S, Fit(), PairPoints(CompeteTo3), ShowsTrump),

                Signoff(Bid._3NT, OppsStopped(), PairPoints(CompeteTo3NT)),

                Signoff(Bid._2NT, OppsContract(), OppsStopped(), PairPoints(CompeteTo2NT)),


                Nonforcing(Bid._4C, Not(Gerber.Applies), Fit(), PairPoints(CompeteTo4), ShowsTrump),
                Nonforcing(Bid._4D, Fit(), PairPoints(CompeteTo4), ShowsTrump),

                Nonforcing(Bid._5C, Fit(), PairPoints(CompeteTo5), ShowsTrump),
                Nonforcing(Bid._5D, Fit(), PairPoints(CompeteTo5), ShowsTrump),

                // TODO: Penalty doubles for game contracts.
                //Signoff(Call.Double, OppsContract(), PairPoints((12, 40)), RuleOf9()),


            });
            bids.AddRange(ForcedBid.Bids(ps));
            bids.Add(Nonforcing(Call.Pass));
            return bids;
        }


    }
}
