using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class OpenBid2: Open
	{
        public static PositionCalls ResponderChangedSuits(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(new CallFeature[]
			{
				PartnerBids(RespondBid2.SecondBid),


				// Responder bid a major suits and we have a fit.  Support at appropriate level.
				// RaisePartner() requires a known 8+ card fit.  If the selected, the rule shows trump
				Nonforcing(Bid.TwoHearts,   RaisePartner(1), DummyMinimum),
				Nonforcing(Bid.TwoSpades,   RaisePartner(1), DummyMinimum),
				Nonforcing(Bid.ThreeHearts, RaisePartner(2), DummyMedium),
				Nonforcing(Bid.ThreeSpades, RaisePartner(2), DummyMedium),
                Nonforcing(Bid.FourHearts,  RaisePartner(3), DummyMaximum),
				Nonforcing(Bid.FourSpades,  RaisePartner(3), DummyMaximum),

				// TODO: There is the possibility that slam will push us beyond
				// "maximum" - need to handle in Blackwood...

				// We can't raise partner's suit.  
				// TODO: Here is where welsh bidding would happen...  1NT or 2NT
				Nonforcing(Bid.OneHeart, Shape(4, 6)),
				Nonforcing(Bid.OneSpade, Shape(4, 6)),

				// TODO: These need to be lower priority...
				Nonforcing(Bid.TwoDiamonds,RaisePartner(), Minimum),
				Nonforcing(Bid.ThreeDiamonds, RaisePartner(2), Medium),


				// With a big hand we need to make a forcing bid.  Reverse if possible.
				Forcing(Bid.TwoDiamonds, Reverse(), MediumOrBetter),
				Forcing(Bid.TwoHearts, Reverse(), MediumOrBetter),
				Forcing(Bid.TwoSpades, Reverse(), MediumOrBetter),

		//		Forcing(3, Strain.Diamonds, Jump(0), Reverse(), Maximum),
		//		Forcing(3, Strain.Hearts, Jump(0), Reverse(), Maximum),
		//		Forcing(3, Strain.Spades, Jump(0), Reverse(), Maximum),

				// TODO: What about minors.  This is bad. Think we want to fall through to 3NT...
                //Nonforcing(4, Strain.Clubs, DefaultPriority + 10, Fit(), ShowsTrump(), Points(MediumOpener)),
                //Nonforcing(4, Strain.Diamonds, DefaultPriority + 10, Fit(), ShowsTrump(), Points(MediumOpener)),



				// Show a new suit at an appropriate level...
	//			Nonforcing(Bid.TwoClubs, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
    //            Nonforcing(Bid.TwoClubs, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
                Nonforcing(Bid.TwoHearts, Not(Rebid), Not(IsReverse), Balanced(false), Minimum, Shape(4, 6)),
                Nonforcing(Bid.TwoClubs, Not(Rebid), Balanced(false), Minimum, Shape(4, 6)),
                Nonforcing(Bid.TwoDiamonds, Not(Rebid), Not(IsReverse), Balanced(false), Minimum, Shape(4, 6)),
        


				// Rebid a 6 card suit
				Nonforcing(Bid.TwoClubs,      Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoDiamonds,   Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoHearts,     Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoSpades,     Rebid, Shape(6, 11), Minimum),

				Nonforcing(Bid.ThreeClubs,    Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid.ThreeDiamonds, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid.ThreeHearts,   Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid.ThreeSpades,   Rebid, Shape(6, 11), Medium),

				// TODO: Need jump shifts here....

				Nonforcing(Bid.TwoHearts, LastBid(Bid.OneSpade), Shape(4, 6), Points(LessThanJumpShift)),
				Nonforcing(Bid.ThreeHearts, LastBid(Bid.OneSpade), Shape(4, 5), Points(JumpShift)),

				// TODO: Need to implement 3NT bid if long running minor.  Suits stopped????

				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid.OneNoTrump, Balanced(), Points(Rebid1NT)),
				Nonforcing(Bid.TwoNoTrump, Balanced(), Points(Rebid2NT)),

            });
			choices.AddRules(Compete.CompBids(ps));
			return choices;
		}

		public static IEnumerable<CallFeature> ResponderPassedInCompetition(PositionState ps)
		{
			return new CallFeature[]
			{
				// TODO: This is way not finished.  Also I think that perhaps min-medium
				// would just rebid at the cheapest level??? Competition...
				// Rebid a 6 card suit
				Nonforcing(Bid.TwoClubs,      Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoDiamonds,   Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoHearts,     Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid.TwoSpades,     Rebid, Shape(6, 11), Minimum),

				Nonforcing(Bid.ThreeClubs,    Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid.ThreeDiamonds, Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid.ThreeHearts,   Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid.ThreeSpades,   Rebid, Shape(6, 11), MediumOrBetter),

				Nonforcing(Call.Pass)
			};
		}

		public static PositionCalls OneNTOverMajorOpen(PositionState ps)
		{
			return ResponderChangedSuits(ps);
			// TODO: Do something more here
		}


		public static PositionCalls OneNTOverMinorOpen(PositionState ps)
		{
			return ResponderChangedSuits(ps);
		}

		public static PositionCalls TwoNTOverMinorOpen(PositionState ps)
		{
			return ResponderChangedSuits(ps);
		}

		public static PositionCalls ThreeNTOverClubOpen(PositionState ps)
		{
			return ResponderChangedSuits(ps);
		}

		public static IEnumerable<CallFeature> ResponderRaisedMinor(PositionState ps)
		{
			// TODO: More to do here...
			return Compete.CompBids(ps);
		}

		public static IEnumerable<CallFeature> ResponderRaisedMajor(PositionState ps)
		{
			// TODO: Help suit raises?
			var bids = new List<CallFeature>()
			{
				// TODO: These are not reall game invitations...
				PartnerBids(Bid.ThreeHearts, RespondBid2.OpenerInvitedGame),
				PartnerBids(Bid.ThreeSpades, RespondBid2.OpenerInvitedGame),

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
