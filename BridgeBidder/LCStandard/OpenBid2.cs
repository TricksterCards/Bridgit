using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace BridgeBidding
{
    public class OpenBid2: Open
	{

        public static PositionCalls ResponderChangedSuits(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(
				PartnerBids(RespondBid2.SecondBid),

				// Responder bid a major suits and we have a fit.  Support at appropriate level.
				// RaisePartner() requires a known 8+ card fit.  If the selected, the rule shows trump
				Shows(Bid._2H, RaisePartner(), DummyMinimum),
				Shows(Bid._2S, RaisePartner(), DummyMinimum),
				Shows(Bid._3H, RaisePartner(jump: 1), DummyMedium),
				Shows(Bid._3S, RaisePartner(jump: 1), DummyMedium),
                Shows(Bid._4H, RaisePartner(jump: 2), DummyMaximum),
				Shows(Bid._4S, RaisePartner(jump: 2), DummyMaximum),

				// TODO: There is the possibility that slam will push us beyond
				// "maximum" - need to handle in Blackwood...

				// We can't raise partner's suit.  
				// TODO: Here is where welsh bidding would happen...  1NT or 2NT
				Shows(Bid._1H, Shape(4, 6)),
				Shows(Bid._1S, Shape(4, 6)),

				// TODO: These need to be lower priority...
				Shows(Bid._2D, RaisePartner(), Minimum),
				Shows(Bid._3D, RaisePartner(jump: 1), Medium),

				// If we have a 19 point balanced hand then better to show this with a rebid of 2NT
				// than a forcing jump shift or reverse.
				Shows(Bid._2NT, Balanced, Points(Rebid2NT)),

				// With a big hand we need to make a forcing bid.  Reverse if possible.
				Properties(new Bid[] { Bid._2D, Bid._2H, Bid._2S }, forcing1Round: true, onlyIf: IsReverseBid),
				Shows(Bid._2D, IsReverseBid, ReverseShape, MediumOrBetter),
				Shows(Bid._2H, IsReverseBid, ReverseShape, MediumOrBetter),
				Shows(Bid._2S, IsReverseBid, ReverseShape, MediumOrBetter),


				// TODO: What about minors.  This is bad. Think we want to fall through to 3NT...
                //Shows(4, Strain.Clubs, DefaultPriority + 10, Fit8Plus, ShowsTrump, Points(MediumOpener)),
                //Shows(4, Strain.Diamonds, DefaultPriority + 10, Fit8Plus, ShowsTrump, Points(MediumOpener)),

				// Show a new suit at an appropriate level...
	//			Shows(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
    //            Shows(Bid._2C, Balanced(false), Points(MinimumOpener), LongestUnbidSuit()),
                Shows(Bid._2H, IsNewSuit, IsNotReverse, NotBalanced, Minimum, Shape(4, 6)),
                Shows(Bid._2C, IsNewSuit, NotBalanced, CantJumpShift, Shape(4, 6)),
                Shows(Bid._2D, IsNewSuit, IsNotReverse, NotBalanced, CantJumpShift, Shape(4, 6)),
        
				// Rebid a 6 card suit
				Shows(Bid._2C, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2D, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2H, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2S, IsRebid, Shape(6, 11), Minimum),

				Shows(Bid._3C, IsRebid, Shape(6, 11), Medium),
				Shows(Bid._3D, IsRebid, Shape(6, 11), Medium),
				Shows(Bid._3H, IsRebid, Shape(6, 11), Medium),
				Shows(Bid._3S, IsRebid, Shape(6, 11), Medium),

				Shows(Bid._2H, IsLastBid(Bid._1S), Shape(4, 6), Points(LessThanJumpShift)),
				Shows(Bid._3H, IsLastBid(Bid._1S), Shape(4, 5), Points(JumpShift)),

				Properties(new Bid[] { Bid._2H, Bid._2S, Bid._3C, Bid._3D, Bid._3H, Bid._3S }, forcingToGame: true, onlyIf: IsJumpShift),
				// TODO: Need to jump-shift only if this is the 2nd longest suit.  Perhaps this is good enough.  
				Shows(Bid._2H, IsJumpShift, Shape(4, 6), Points(JumpShift)),
				Shows(Bid._2S, IsJumpShift, Shape(4, 6), Points(JumpShift)),
				Shows(Bid._3C, IsJumpShift, Shape(4, 6), Points(JumpShift)),
				Shows(Bid._3D, IsJumpShift, Shape(4, 6), Points(JumpShift)),
				Shows(Bid._3H, IsJumpShift, Shape(4, 6), Points(JumpShift)),
				Shows(Bid._3S, IsJumpShift, Shape(4, 6), Points(JumpShift)),

				// We have tried every possible way to show a strong hand by reversing or jump shifting.  If we get here
				// and have not found a bid but we are very strong then we just need to bid 3 or 4 of our suit.
				Shows(Bid._4H, IsRebid, ExcellentPlusSuit, Shape(7, 11), Points(20, 21)),
				Shows(Bid._3H, IsRebid, Shape(6, 11), Points(17, 19)),
				Shows(Bid._4S, IsRebid, ExcellentPlusSuit, Shape(7, 11), Points(20, 21)),
				Shows(Bid._3S, IsRebid, Shape(6, 11), Points(17, 19)),
				// TODO: Need to implement minors here too.  Long, strong minors need a backup if no reverse available.
				// 

				// TODO: Need to implement 3NT bid if long running minor.  Suits stopped????

				// Lowest priority if nothing else fits is bid NT
				Shows(Bid._1NT, Balanced, Points(Rebid1NT))
            );
		// REMOVED THIS CRUTCH ---	choices.AddRules(Compete.CompBids(ps));
			return choices;
		}

		public static PositionCalls TwoOverOne(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			var partnerSuit = ps.Partner.Bid.Suit.Value;
			choices.AddRules(
				PartnerBids(RespondBid2.SecondBid2Over1),

				// These show a trump suit.  Responder must play in this.
				// TODO: Should the idea of a "forced" trump suit be remembered in the bidding system?
				// This would allow competative bids to use this information to place the contract...
				// TODO: What is the length requirements and quality requirements for these bids?
				Properties(Bid._3H, agreeTrump: true, onlyIf: IsRebid),
				Properties(Bid._3S, agreeTrump: true, onlyIf: IsRebid),
				Shows(Bid._3H, IsRebid, Shape(7), ExcellentPlusSuit),
				Shows(Bid._3H, IsRebid, Shape(8), GoodPlusSuit),
				Shows(Bid._3H, IsRebid, Shape(9, 11)),
				Shows(Bid._3S, IsRebid, Shape(7), ExcellentPlusSuit),
				Shows(Bid._3S, IsRebid, Shape(8), GoodPlusSuit),
				Shows(Bid._3S, IsRebid, Shape(9, 11)),

				// If partner has shown hearts after we bid spades then agree on that suit.
				Properties(new[] {Bid._3H, Bid._4H }, agreeTrump: true, onlyIf: IsPartnersSuit),
				Shows(Bid._3H, IsPartnersSuit, Shape(3, 7), Points(14, 40)),
				Shows(Bid._4H, IsPartnersSuit, Shape(3, 7), Points(12, 13)),

				// Need to show any 4+ card major that is a new suit.  Reverses don't exist
				Shows(Bid._2H, IsNewSuit, Shape(4, 6)),
				Shows(Bid._2S, IsNewSuit, Shape(4, 6)),

				// TODO: If partner shows a minor then do we want to raise?  Or bid NT?  Or bid a new major if
				// we have one?  One D/2C/ Now shouldn't we show majors?  Or bid NT?
				// TODO:  If it was 1S/2H and we have 3+H then raise to 3 or 4 hearts...  Only special case.
				Shows(new Bid(3, partnerSuit), Fit8Plus),

				Shows(Bid._2NT, Balanced),

				Shows(Bid._2D, IsRebid, Shape(6, 11), LongestSuit),
				Shows(Bid._2H, IsRebid, Shape(6, 8), LongestSuit),
				Shows(Bid._2S, IsRebid, Shape(6, 8), LongestSuit),
				Shows(Bid._3C, IsRebid, Shape(6, 11), LongestSuit),

				Shows(Bid._2D, IsNewSuit, Shape(4, 6)),
				Shows(Bid._3C, IsNewSuit, Shape(4, 6)),
				Shows(Bid._3D, IsNewSuit, IsNonJump, Shape(4, 6))
			);
			return choices;
		}

		public static PositionCalls ResponderPassedInCompetition(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(
				// TODO: This is way not finished.  Also I think that perhaps min-medium
				// would just rebid at the cheapest level??? Competition...
				// Rebid a 6 card suit
				Shows(Bid._2C, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2D, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2H, IsRebid, Shape(6, 11), Minimum),
				Shows(Bid._2S, IsRebid, Shape(6, 11), Minimum),

				Shows(Bid._3C, IsRebid, Shape(6, 11), MediumOrBetter),
				Shows(Bid._3D, IsRebid, Shape(6, 11), MediumOrBetter),
				Shows(Bid._3H, IsRebid, Shape(6, 11), MediumOrBetter),
				Shows(Bid._3S, IsRebid, Shape(6, 11), MediumOrBetter),

				Shows(Call.Pass)
			);
			return choices;
		}



		public static PositionCalls SemiForcingNT(PositionState ps)
		{
			// TODO: Check for interferrence...  Bid or X.
			var choices = new PositionCalls(ps);
			choices.AddRules(
				Shows(Call.Pass, Balanced, Points(12, 13)),

				Shows(Bid._2NT, Balanced, HighCardPoints(18, 19), Points(19, 20)),

				Shows(Bid._2C, IsNewSuit, Shape(4, 6), Points(12, 16)),
				Shows(Bid._2D, IsNewSuit, Shape(4, 6), Points(12, 16)),
				Shows(Bid._2H, IsNewSuit, Shape(4, 6), Points(12, 16)),

				Shows(Bid._2C, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2D, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2H, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2S, IsRebid, Shape(6, 11), Points(12, 16))

			);
			return choices;
		}
		public static PositionCalls PassedHandResponded1NT(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(
				Shows(Call.Pass, Balanced, Points(12, 14)),

				Shows(Bid._2NT, Balanced, HighCardPoints(18, 19), Points(19, 20)),

				Shows(Bid._2C, IsNewSuit, Shape(4, 6), Points(12, 16)),
				Shows(Bid._2D, IsNewSuit, Shape(4, 6), Points(12, 16)),
				Shows(Bid._2H, IsNewSuit, Shape(4, 6), Points(12, 16)),

				Shows(Bid._2C, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2D, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2H, IsRebid, Shape(6, 11), Points(12, 16)),
				Shows(Bid._2S, IsRebid, Shape(6, 11), Points(12, 16))
			);
			return choices;
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

		public static PositionCalls ResponderRaisedMinor(PositionState ps)
		{
			// TODO: More to do here...
			var choices = new PositionCalls(ps);
			choices.AddRules(Compete.CompBids(ps));
			return choices;
		}

		public static PositionCalls ResponderRaisedMajor(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(Blackwood.InitiateConvention);
			choices.AddRules(
				// TODO: These are not reall game invitations...
				PartnerBids(Bid._3H, RespondBid2.OpenerInvitedGame),
				PartnerBids(Bid._3S, RespondBid2.OpenerInvitedGame),

				// TODO: Game invitation shoudl always be help suit...  At least if that convention
				// is in use.  
				Shows(Bid._3H, Fit8Plus, PairPoints(PairGameInvite)),
				Shows(Bid._3S, Fit8Plus, PairPoints(PairGameInvite)),

                Shows(Bid._4H, Fit8Plus, PairPoints(PairGame)),
				Shows(Bid._4S, Fit8Plus, PairPoints(PairGame))
			);

			return choices;
		}
	

	}
}
