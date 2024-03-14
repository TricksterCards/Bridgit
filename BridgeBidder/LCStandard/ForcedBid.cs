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
					Shows(Bid._2C, Fit()),
					Shows(Bid._2D, Fit()),
					Shows(Bid._2H, Fit()),
					Shows(Bid._2S, Fit()),

					Shows(Bid._3C, IsNonJump, Fit()),
					Shows(Bid._3D, IsNonJump, Fit()),
					Shows(Bid._3H, IsNonJump, Fit()),
					Shows(Bid._3S, IsNonJump, Fit()),

					Shows(Bid._4C, IsNonJump, Fit()),
					Shows(Bid._4D, IsNonJump, Fit()),
					Shows(Bid._4H, IsNonJump, Fit()),
					Shows(Bid._4S, IsNonJump, Fit()), 

					// Now the worst possible cases.  NT if no 7-card fit
					Shows(Bid._1NT),
					Shows(Bid._2NT, IsNonJump),
					Shows(Bid._3NT, IsNonJump)
				});
			};
			return bids;
		}
	}
}
