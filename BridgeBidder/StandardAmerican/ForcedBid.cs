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
					Nonforcing(2, Strain.Clubs, Fit(7)),
					Nonforcing(2, Strain.Diamonds, Fit(7)),
					Nonforcing(2, Strain.Hearts, Fit(7)),
					Nonforcing(2, Strain.Spades, Fit(7)),

					Nonforcing(3, Strain.Clubs, Jump(0), Fit(7)),
					Nonforcing(3, Strain.Diamonds, Jump(0), Fit(7)),
					Nonforcing(3, Strain.Hearts, Jump(0), Fit(7)),
					Nonforcing(3, Strain.Spades, Jump(0), Fit(7)),

					Nonforcing(4, Strain.Clubs, Jump(0), Fit(7)),
					Nonforcing(4, Strain.Diamonds, Jump(0), Fit(7)),
					Nonforcing(4, Strain.Hearts, Jump(0), Fit(7)),
					Nonforcing(4, Strain.Spades, Jump(0), Fit(7)), 

					// Now the worst possible cases.  NT if no 7-card fit
					Nonforcing(1, Strain.NoTrump),
					Nonforcing(2, Strain.NoTrump, Jump(0)),
					Nonforcing(3, Strain.NoTrump, Jump(0))

				});
			};
			return bids;
		}
	}
}
