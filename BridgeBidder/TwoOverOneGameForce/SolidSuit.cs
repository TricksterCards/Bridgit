using System.Collections;
using System.Collections.Generic;



namespace BridgeBidding
{
    public class SolidSuit: TwoOverOneGameForce
    {
        public static IEnumerable<BidRule> Bids(PositionState ps)
        {
            var bids = new List<BidRule>();
            foreach (var suit in Card.Suits)
            {
                bids.Add(Signoff(new Bid(7, suit), Shape(13)));
                bids.Add(Signoff(new Bid(7, suit), Shape(12), Aces(2)));
                bids.Add(Signoff(new Bid(6, suit), Shape(12), Aces(1)));
            }
            return bids;
        }
    }
}