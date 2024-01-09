using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class OpenBid2: Open
	{
        public static IEnumerable<BidRule> ResponderChangedSuits(PositionState ps)
		{
			var bids = new List<BidRule>()
			{
				DefaultPartnerBids(Bid.Double, Respond.Rebid),


				// Responder bid a major suits and we have a fit.  Support at appropriate level.
				// RaisePartner() requires a known 8+ card fit.  If the selected, the rule shows trump
				Nonforcing(Bid.TwoHearts,   RaisePartner(1), DummyPoints(Minimum)),
				Nonforcing(Bid.TwoSpades,   RaisePartner(1), DummyPoints(Minimum)),
				Nonforcing(Bid.ThreeHearts, RaisePartner(2), DummyPoints(Medium)),
				Nonforcing(Bid.ThreeSpades, RaisePartner(2), DummyPoints(Medium)),
                Nonforcing(Bid.FourHearts,  RaisePartner(3), DummyPoints(Maximum)),
				Nonforcing(Bid.FourSpades,  RaisePartner(3), DummyPoints(Maximum)),

				// TODO: There is the possibility that slam will push us beyond
				// "maximum" - need to handle in Blackwood...

				// We can't raise partner's suit.  
				// TODO: Here is where welsh bidding would happen...  1NT or 2NT
				Nonforcing(Bid.OneHeart, Shape(4, 6)),
				Nonforcing(Bid.OneSpade, Shape(4, 6)),

				// TODO: These need to be lower priority...
				Nonforcing(Bid.TwoDiamonds, RaisePartner(), Points(Minimum)),
				Nonforcing(Bid.ThreeDiamonds, RaisePartner(2), Points(Medium)),


				// With a big hand we need to make a forcing bid.  Reverse if possible.
				Forcing(Bid.TwoDiamonds, Reverse(), Points(MediumOrBetter)),
				Forcing(Bid.TwoHearts, Reverse(), Points(MediumOrBetter)),
				Forcing(Bid.TwoSpades, Reverse(), Points(MediumOrBetter)),

		//		Forcing(3, Strain.Diamonds, Jump(0), Reverse(), Points(Maximum)),
		//		Forcing(3, Strain.Hearts, Jump(0), Reverse(), Points(Maximum)),
		//		Forcing(3, Strain.Spades, Jump(0), Reverse(), Points(Maximum)),

				// TODO: What about minors.  This is bad. Think we want to fall through to 3NT...
                //Nonforcing(4, Strain.Clubs, DefaultPriority + 10, Fit(), ShowsTrump(), Points(MediumOpener)),
                //Nonforcing(4, Strain.Diamonds, DefaultPriority + 10, Fit(), ShowsTrump(), Points(MediumOpener)),



				// Show a new suit at an appropriate level...
	//			Nonforcing(Bid.TwoClubs, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
    //            Nonforcing(Bid.TwoClubs, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
                Nonforcing(Bid.TwoHearts, Not(Rebid()), Not(IsReverse()), Balanced(false), Points(Minimum), Shape(4, 6)),
                Nonforcing(Bid.TwoClubs, Not(Rebid()), Balanced(false), Points(Minimum), Shape(4, 6)),
                Nonforcing(Bid.TwoDiamonds, Not(Rebid()), Not(IsReverse()), Balanced(false), Points(Minimum), Shape(4, 6)),
        


				// Rebid a 6 card suit
				Nonforcing(Bid.TwoClubs,      Rebid(), Shape(6, 11), Points(Minimum)),
				Nonforcing(Bid.TwoDiamonds,   Rebid(), Shape(6, 11), Points(Minimum)),
				Nonforcing(Bid.TwoHearts,     Rebid(), Shape(6, 11), Points(Minimum)),
				Nonforcing(Bid.TwoSpades,     Rebid(), Shape(6, 11), Points(Minimum)),

				Nonforcing(Bid.ThreeClubs,    Rebid(), Shape(6, 11), Points(Medium)),
				Nonforcing(Bid.ThreeDiamonds, Rebid(), Shape(6, 11), Points(Medium)),
				Nonforcing(Bid.ThreeHearts,   Rebid(), Shape(6, 11), Points(Medium)),
				Nonforcing(Bid.ThreeSpades,   Rebid(), Shape(6, 11), Points(Medium)),

				// TODO: Need jump shifts here....

				Nonforcing(Bid.TwoHearts, LastBid(Bid.OneSpade), Shape(4, 6), Points(LessThanJumpShift)),
				Nonforcing(Bid.ThreeHearts, LastBid(Bid.OneSpade), Shape(4, 5), Points(JumpShift)),

				// TODO: Need to implement 3NT bid if long running minor.  Suits stopped????

				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid.OneNoTrump, Balanced(), Points(Rebid1NT)),
				Nonforcing(Bid.TwoNoTrump, Balanced(), Points(Rebid2NT)),

            };
			bids.AddRange(Compete.CompBids(ps));
			return bids;
		}

		public static IEnumerable<BidRule> ResponderBidNT(PositionState ps, int level)
		{
			return ResponderChangedSuits(ps);
			// TODO: Do something more here
		}


		public static IEnumerable<BidRule> ResponderRaisedMinor(PositionState ps)
		{
			// TODO: More to do here...
			return Compete.CompBids(ps);
		}

		public static IEnumerable<BidRule> ResponderRaisedMajor(PositionState ps)
		{
			// TODO: Help suit raises?
			var bids = new List<BidRule>()
			{
				PartnerBids(Bid.ThreeHearts, Bid.FourDiamonds, Respond.OpenerInvitedGame),
				PartnerBids(Bid.ThreeSpades, Bid.FourHearts, Respond.OpenerInvitedGame),

				// TODO: Game invitation shoudl always be help suit...  At least if that convention
				// is in use.  
				Nonforcing(Bid.ThreeHearts, Fit(), ShowsTrump(), PairPoints(PairGameInvite)),
				Nonforcing(Bid.ThreeSpades, Fit(), ShowsTrump(), PairPoints(PairGameInvite)),

                Nonforcing(Bid.FourHearts, Fit(), ShowsTrump(), PairPoints(PairGame)),
				Nonforcing(Bid.FourSpades, Fit(), ShowsTrump(), PairPoints(PairGame)),

            };
			// Competative bids include Blackwood...
			bids.AddRange(Compete.CompBids(ps));
			return bids;
		}
	

	}
}
