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
				Nonforcing(Bid._2H, RaisePartner(1), DummyMinimum),
				Nonforcing(Bid._2S, RaisePartner(1), DummyMinimum),
				Nonforcing(Bid._3H, RaisePartner(2), DummyMedium),
				Nonforcing(Bid._3S, RaisePartner(2), DummyMedium),
                Nonforcing(Bid._4H, RaisePartner(3), DummyMaximum),
				Nonforcing(Bid._4S, RaisePartner(3), DummyMaximum),

				// TODO: There is the possibility that slam will push us beyond
				// "maximum" - need to handle in Blackwood...

				// We can't raise partner's suit.  
				// TODO: Here is where welsh bidding would happen...  1NT or 2NT
				Nonforcing(Bid._1H, Shape(4, 6)),
				Nonforcing(Bid._1S, Shape(4, 6)),

				// TODO: These need to be lower priority...
				Nonforcing(Bid._2D, RaisePartner(), Minimum),
				Nonforcing(Bid._3D, RaisePartner(2), Medium),


				// With a big hand we need to make a forcing bid.  Reverse if possible.
				Forcing(Bid._2D, Reverse(), MediumOrBetter),
				Forcing(Bid._2H, Reverse(), MediumOrBetter),
				Forcing(Bid._2S, Reverse(), MediumOrBetter),

		//		Forcing(3, Strain.Diamonds, Jump(0), Reverse(), Maximum),
		//		Forcing(3, Strain.Hearts, Jump(0), Reverse(), Maximum),
		//		Forcing(3, Strain.Spades, Jump(0), Reverse(), Maximum),

				// TODO: What about minors.  This is bad. Think we want to fall through to 3NT...
                //Nonforcing(4, Strain.Clubs, DefaultPriority + 10, Fit(), ShowsTrump, Points(MediumOpener)),
                //Nonforcing(4, Strain.Diamonds, DefaultPriority + 10, Fit(), ShowsTrump, Points(MediumOpener)),



				// Show a new suit at an appropriate level...
	//			Nonforcing(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
    //            Nonforcing(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
                Nonforcing(Bid._2H, Not(Rebid), Not(IsReverse), Balanced(false), Minimum, Shape(4, 6)),
                Nonforcing(Bid._2C, Not(Rebid), Balanced(false), Minimum, Shape(4, 6)),
                Nonforcing(Bid._2D, Not(Rebid), Not(IsReverse), Balanced(false), Minimum, Shape(4, 6)),
        


				// Rebid a 6 card suit
				Nonforcing(Bid._2C, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2D, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2H, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2S, Rebid, Shape(6, 11), Minimum),

				Nonforcing(Bid._3C, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3D, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3H, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3S, Rebid, Shape(6, 11), Medium),

				// TODO: Need jump shifts here....

				Nonforcing(Bid._2H, LastBid(Bid._1S), Shape(4, 6), Points(LessThanJumpShift)),
				Nonforcing(Bid._3H, LastBid(Bid._1S), Shape(4, 5), Points(JumpShift)),

				// TODO: Need to implement 3NT bid if long running minor.  Suits stopped????

				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid._1NT, Balanced(), Points(Rebid1NT)),
				Nonforcing(Bid._2NT, Balanced(), Points(Rebid2NT)),

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
				Nonforcing(Bid._2C, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2D, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2H, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2S, Rebid, Shape(6, 11), Minimum),

				Nonforcing(Bid._3C, Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid._3D, Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid._3H, Rebid, Shape(6, 11), MediumOrBetter),
				Nonforcing(Bid._3S, Rebid, Shape(6, 11), MediumOrBetter),

				Nonforcing(Call.Pass)
			};
		}

		//public static PositionCalls ResponderBidInCompetition(PositionState ps)
		//{

		//}

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
				PartnerBids(Bid._3H, RespondBid2.OpenerInvitedGame),
				PartnerBids(Bid._3S, RespondBid2.OpenerInvitedGame),

				// TODO: Game invitation shoudl always be help suit...  At least if that convention
				// is in use.  
				Nonforcing(Bid._3H, Fit(), ShowsTrump, PairPoints(PairGameInvite)),
				Nonforcing(Bid._3S, Fit(), ShowsTrump, PairPoints(PairGameInvite)),

                Nonforcing(Bid._4H, Fit(), ShowsTrump, PairPoints(PairGame)),
				Nonforcing(Bid._4S, Fit(), ShowsTrump, PairPoints(PairGame)),

            };
			// Competative bids include Blackwood...
			bids.AddRange(Compete.CompBids(ps));
			return bids;
		}
	

	}
}
