using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace BridgeBidding
{
    public class ForcedBid: Bidder
	{
		// TODO: Where should we check position state to see if the forced bids are necessary?  Here?  
		public static IEnumerable<BidRule> Bids(PositionState ps)
		{
			var bids = new List<BidRule>();
			if (ps.ForcedToBid)
			{
				bids.AddRange(new BidRule[] {
					Nonforcing(Bid.TwoClubs, Fit(7)),
					Nonforcing(Bid.TwoDiamonds, Fit(7)),
					Nonforcing(Bid.TwoHearts, Fit(7)),
					Nonforcing(Bid.TwoSpades, Fit(7)),

					Nonforcing(Bid.ThreeClubs, Jump(0), Fit(7)),
					Nonforcing(Bid.ThreeDiamonds, Jump(0), Fit(7)),
					Nonforcing(Bid.ThreeHearts, Jump(0), Fit(7)),
					Nonforcing(Bid.ThreeSpades, Jump(0), Fit(7)),

					Nonforcing(Bid.FourClubs, Jump(0), Fit(7)),
					Nonforcing(Bid.FourDiamonds, Jump(0), Fit(7)),
					Nonforcing(Bid.FourHearts, Jump(0), Fit(7)),
					Nonforcing(Bid.FourSpades, Jump(0), Fit(7)), 

					// Now the worst possible cases.  NT if no 7-card fit
					Nonforcing(Bid.OneNoTrump),
					Nonforcing(Bid.TwoNoTrump, Jump(0)),
					Nonforcing(Bid.ThreeNoTrump, Jump(0))

				});
			};
			return bids;
		}
	}
}
