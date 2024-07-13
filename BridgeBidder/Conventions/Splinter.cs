using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
    public class Splinter : Bidder
	{

		public static IEnumerable<CallFeature> InitiateConvention(PositionState ps, Suit openSuit)
		{
            Debug.Assert(!ps.IsPassedHand && ps.RHO.Passed);
            Debug.Assert(openSuit.IsMajor());
            var bids = new List<CallFeature>();
            foreach (Suit splinterSuit in Card.Suits)
            {
                if (splinterSuit != openSuit)
                {
                    Bid bid = new Bid(splinterSuit == Suit.Spades ? 3 : 4, splinterSuit);
                    bids.Add(Properties(bid, partnerBids: OpenerRebid, convention: UserText.Splinter, alert: UserText.SplinterDescription));
                    bids.Add(Shows(bid, DummyPoints(openSuit, (11, 15)), Shape(openSuit, 4, 8), Shape(splinterSuit, 0, 1)));
                }
            }
            return bids;
		}

        public static PositionCalls OpenerRebid(PositionState ps)
        {
            // TODO: Need to add blakkwood and other conventions here...
            return new PositionCalls(ps).AddRules(
                Shows(Bid._4H, Fit()),
                Shows(Bid._4S, Fit())
            );
        }
    }
}