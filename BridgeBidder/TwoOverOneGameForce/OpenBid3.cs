using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class OpenBid3: Open
	{

		public static IEnumerable<BidRule> ThirdBid(PositionState ps)
		{
			var bids = new List<BidRule>()
			{
				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid.OneNoTrump, Balanced(), Points(Rebid1NT)),
				Nonforcing(Bid.TwoNoTrump, Balanced(), Points(Rebid2NT)),
				// TODO: What about 3NT...
            };

			bids.AddRange(Compete.CompBids(ps));
			return bids;
		}
    }
}
