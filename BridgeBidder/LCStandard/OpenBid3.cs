using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class OpenBid3: Open
	{

		public static IEnumerable<CallFeature> ThirdBid(PositionState ps)
		{
			var bids = new List<CallFeature>()
			{
				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid._1NT, Balanced, Points(Rebid1NT)),
				Nonforcing(Bid._2NT, Balanced, Points(Rebid2NT)),
				// TODO: What about 3NT...
            };

			bids.AddRange(Compete.CompBids(ps));
			return bids;
		}
    }
}
