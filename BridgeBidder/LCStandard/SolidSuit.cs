using System.Collections;
using System.Collections.Generic;



namespace BridgeBidding
{
    public class SolidSuit: LCStandard
    {
        public static IEnumerable<CallFeature> Bids(PositionState ps)
        {
            var bids = new List<CallFeature>();
            foreach (var suit in Card.Suits)
            {
                bids.Add(Shows(new Bid(7, suit), Shape(13)));
                bids.Add(Shows(new Bid(7, suit), Shape(12), Aces(2)));
                bids.Add(Shows(new Bid(6, suit), Shape(12), Aces(1)));
            }
            return bids;
        }
    }
}