using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace BridgeBidding
{
    public class OpenBid2: Open
	{

        public static PositionCalls ResponderChangedSuits(PositionState ps)
		{
			if (ps.Partner.Bid.Level == 2 && !ps.Partner.IsPassedHand)
			{
				return TwoOverOneRebid(ps);
			}
			var choices = new PositionCalls(ps);
			choices.AddRules(
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

				// If we have a 19 point balanced hand then better to show this with a rebid of 2NT
				// than a forcing jump shift or reverse.
				Nonforcing(Bid._2NT, Balanced, Points(Rebid2NT)),

				// With a big hand we need to make a forcing bid.  Reverse if possible.
				Forcing(Bid._2D, Reverse, MediumOrBetter),
				Forcing(Bid._2H, Reverse, MediumOrBetter),
				Forcing(Bid._2S, Reverse, MediumOrBetter),

		//		Forcing(3, Strain.Diamonds, NonJump, Reverse(), Maximum),
		//		Forcing(3, Strain.Hearts, NonJump, Reverse(), Maximum),
		//		Forcing(3, Strain.Spades, NonJump, Reverse(), Maximum),

				// TODO: What about minors.  This is bad. Think we want to fall through to 3NT...
                //Nonforcing(4, Strain.Clubs, DefaultPriority + 10, Fit(), ShowsTrump, Points(MediumOpener)),
                //Nonforcing(4, Strain.Diamonds, DefaultPriority + 10, Fit(), ShowsTrump, Points(MediumOpener)),

				// Show a new suit at an appropriate level...
	//			Nonforcing(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
    //            Nonforcing(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
                Nonforcing(Bid._2H, NewSuit, NotReverse, NotBalanced, Minimum, Shape(4, 6)),
                Nonforcing(Bid._2C, NewSuit, NotBalanced, CantJumpShift, Shape(4, 6)),
                Nonforcing(Bid._2D, NewSuit, NotReverse, NotBalanced, CantJumpShift, Shape(4, 6)),
        
				// Rebid a 6 card suit
				Nonforcing(Bid._2C, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2D, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2H, Rebid, Shape(6, 11), Minimum),
				Nonforcing(Bid._2S, Rebid, Shape(6, 11), Minimum),

				Nonforcing(Bid._3C, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3D, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3H, Rebid, Shape(6, 11), Medium),
				Nonforcing(Bid._3S, Rebid, Shape(6, 11), Medium),

	

				Nonforcing(Bid._2H, LastBid(Bid._1S), Shape(4, 6), Points(LessThanJumpShift)),
				Nonforcing(Bid._3H, LastBid(Bid._1S), Shape(4, 5), Points(JumpShift)),

				// TODO: Need to jump-shift only if this is the 2nd longest suit.  Perhaps this is good enough.  
				Forcing(Bid._2H, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),
				Forcing(Bid._2S, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),
				Forcing(Bid._3C, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),
				Forcing(Bid._3D, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),
				Forcing(Bid._3H, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),
				Forcing(Bid._3S, SingleJump, NewSuit, Shape(4, 6), Points(JumpShift)),

				// We have tried every possible way to show a strong hand by reversing or jump shifting.  If we get here
				// and have not found a bid but we are very strong then we just need to bid 3 or 4 of our suit.
				Nonforcing(Bid._4H, ExcellentPlusSuit, Shape(7, 11), Points(20, 21)),
				Nonforcing(Bid._3H, Shape(6, 11), Points(JumpShift)),
				Nonforcing(Bid._4S, ExcellentPlusSuit, Shape(7, 11), Points(20, 21)),
				Nonforcing(Bid._3S, Shape(6, 11), Points(JumpShift)),
				// TODO: Need to implement minors here too.  Long, strong minors need a backup if no reverse available.
				// 

				// TODO: Need to implement 3NT bid if long running minor.  Suits stopped????

				// Lowest priority if nothing else fits is bid NT
				Nonforcing(Bid._1NT, Balanced, Points(Rebid1NT))
            );
		// REMOVED THIS CRUTCH ---	choices.AddRules(Compete.CompBids(ps));
			return choices;
		}

		private static PositionCalls TwoOverOneRebid(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			var partnerSuit = (Suit)ps.Partner.Bid.Suit;
			choices.AddRules(
				// TODO: Need better responses for 2nd bid. PartnerBids(RespondBid2.SecondBid2Over1),
				Forcing(new Bid(3, partnerSuit), Fit(), ShowsTrump),

				Forcing(Bid._2NT, Balanced),

				Forcing(Bid._2D, Shape(6, 10), LongestSuit),
				Forcing(Bid._2H, Shape(6, 10), LongestSuit),
				Forcing(Bid._2S, Shape(6, 10), LongestSuit),
				Forcing(Bid._3C, Shape(6, 10), LongestSuit),

				Forcing(Bid._2D, NewSuit, Shape(4, 6)),
				Forcing(Bid._2H, NewSuit, Shape(4, 6)),
				Forcing(Bid._2S, NewSuit, Shape(4, 6)),
				Forcing(Bid._3C, NewSuit, Shape(4, 6)),
				Forcing(Bid._3D, NewSuit, Shape(4, 6))
			);
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



		public static PositionCalls SemiForcingNT(PositionState ps)
		{
			// TODO: Check for interferrence...  Bid or X.
			var choices = new PositionCalls(ps);
			choices.AddRules(
				Signoff(Call.Pass, Balanced, Points(12, 13)),

				Nonforcing(Bid._2NT, Balanced, Points(Rebid2NT)),

				Nonforcing(Bid._2C, NewSuit, Shape(4, 6), Points(12, 16)),
				Nonforcing(Bid._2D, NewSuit, Shape(4, 6), Points(12, 16)),
				Nonforcing(Bid._2H, NewSuit, Shape(4, 6), Points(12, 16)),

				Nonforcing(Bid._2C, Rebid, Shape(6, 11), Points(12, 16)),
				Nonforcing(Bid._2D, Rebid, Shape(6, 11), Points(12, 16)),
				Nonforcing(Bid._2H, Rebid, Shape(6, 11), Points(12, 16)),
				Nonforcing(Bid._2S, Rebid, Shape(6, 11), Points(12, 16))

			);
			return choices;
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

		public static PositionCalls ResponderBidNT(PositionState ps)
		{
			// TODO: Think about these NT bids.
			return ps.PairState.BiddingSystem.GetPositionCalls(ps);
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
