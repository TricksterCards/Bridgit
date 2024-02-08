using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace BridgeBidding
{
    public class ForcedBid: Bidder
	{
		// TODO: Where should we check position state to see if the forced bids are necessary?  Here?  
		public static IEnumerable<CallFeature> Bids(PositionState ps)
		{
			var bids = new List<CallFeature>();
			if (ps.ForcedToBid)
			{
				bids.AddRange(new CallFeature[] {
					Nonforcing(Bid._2C, Fit()),
					Nonforcing(Bid._2D, Fit()),
					Nonforcing(Bid._2H, Fit()),
					Nonforcing(Bid._2S, Fit()),

					Nonforcing(Bid._3C, Jump(0), Fit()),
					Nonforcing(Bid._3D, Jump(0), Fit()),
					Nonforcing(Bid._3H, Jump(0), Fit()),
					Nonforcing(Bid._3S, Jump(0), Fit()),

					Nonforcing(Bid._4C, Jump(0), Fit()),
					Nonforcing(Bid._4D, Jump(0), Fit()),
					Nonforcing(Bid._4H, Jump(0), Fit()),
					Nonforcing(Bid._4S, Jump(0), Fit()), 

					// Now the worst possible cases.  NT if no 7-card fit
					Nonforcing(Bid._1NT),
					Nonforcing(Bid._2NT, Jump(0)),
					Nonforcing(Bid._3NT, Jump(0))
				});
			};
			return bids;
		}
	}
}
