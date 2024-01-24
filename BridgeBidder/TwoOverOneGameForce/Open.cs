using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


namespace BridgeBidding
{
    public class Open: TwoOverOneGameForce
	{
		
	//	protected static Constraint Open2In4thSeat = And(Seat(4), Points(12, 15), Shape(6, 10), GoodSuit()); 
	//	protected static Constraint OpenIn3rdBal = And(Seat(3), Points(11, 11), Balanced(), DecentSuit());
	
	//	protected static Constraint OpenIn3rdUnbalanced = And(Seat(3), Balanced(false), IfElse(IsVul(), Points(9, 11), Points(10, 11));

	//	protected static Constraint OpenIn3rdUnbalVul = (And(Seat(3)))

		// Think this through.  If we are in 3rd seat we want to open some suits lighter depengind
		// on vulnerablity and shape of the hand.  


        public static DynamicConstraint OneLevel = Points(12, 21);
        public static DynamicConstraint Minimum = Points(12, 16);
		public static DynamicConstraint DummyMinimum = DummyPoints(12, 16);
        public static DynamicConstraint Medium = Points(17, 18);
		public static DynamicConstraint DummyMedium = DummyPoints(17, 18);
        public static DynamicConstraint Maximum = Points(19, 21);
		public static DynamicConstraint DummyMaximum = DummyPoints(19, 12);
		public static DynamicConstraint MediumOrBetter = Points(17, 21);

        public static DynamicConstraint Weak = Points(5, 11);
		public static DynamicConstraint VeryWeak = Points(3, 11);
        public static DynamicConstraint DontOpen = Points(0, 11);

		// Rebid values.
        public static (int, int) Rebid1NT = (12, 14);
        public static (int, int) Rebid2NT = (18, 19);


		public static (int, int) ForthSeatOpen2 = (12, 15);

		public static (int, int) LessThanJumpShift = (12, 18);
		public static (int, int) JumpShift = (19, 21);

        public static new PositionCalls GetPositionCalls(PositionState ps)
        {
            var choices = new PositionCalls(ps);

			choices.AddRules(SolidSuit.Bids);
            choices.AddRules(Strong2Clubs.Open);
            choices.AddRules(NoTrump.Open);
            choices.AddRules(OpenSuit);
			if (ps.Seat != 4)
			{
            	choices.AddRules(OpenSuitWeak);
			}
            choices.AddPassRule(DontOpen);
            return choices;
        }

        public static IEnumerable<CallFeature> OpenSuit(PositionState ps)
		{
			var bids = new List<CallFeature>
			{
                PartnerBids(Bid.OneClub,    Respond.OneClub),
				PartnerBids(Bid.OneDiamond, Respond.OneDiamond),
				PartnerBids(Bid.OneHeart,   Respond.OneHeart),
				PartnerBids(Bid.OneSpade,   Respond.OneSpade),

				// 2-level opening in 4th seat is strong, like a 1 level
				// TODO: Rename this function - sometimes opens 2-level
				PartnerBids(Bid.TwoDiamonds, Respond.ForthSeat2Open, Seat(4)),
				PartnerBids(Bid.TwoHearts,   Respond.ForthSeat2Open, Seat(4)),
				PartnerBids(Bid.TwoSpades,   Respond.ForthSeat2Open, Seat(4)),

				Nonforcing(Bid.TwoSpades,   Seat(4), Shape(6, 10), GoodSuit(), BetterOrEqualTo(Suit.Hearts), Points(ForthSeatOpen2)),
				Nonforcing(Bid.TwoHearts,   Seat(4), Shape(6, 10), GoodSuit(), BetterThan(Suit.Spades), Points(ForthSeatOpen2)),
				Nonforcing(Bid.TwoDiamonds, Seat(4), Shape(6, 10), GoodSuit(), Points(ForthSeatOpen2)),

				// In 4th seat we want to pass if the Rule of 15 does not apply.
				Nonforcing(Call.Pass, Seat(4), PassIn4thSeat()),
			};
			// If in 3rd seat then give priority to opening good 4 card majors with a minimum hand
			if (ps.Seat == 3)
			{
				// No special case for vulnerable & balanced.  Same as 1st and 2nd seats...
				bids.AddRange(ThirdSeat4CardMajor(And(IsVul(), Balanced(false), Points(11, 16))));
				bids.AddRange(ThirdSeat4CardMajor(And(Not(IsVul()), Balanced(true), Points(11, 16))));
				bids.AddRange(ThirdSeat4CardMajor(And(Not(IsVul()), Balanced(false), Points(10, 16))));
			}
			bids.AddRange(new CallFeature[] 
			{
				// For medium+ hands we will always bid the longest suit first.
				Nonforcing(Bid.OneClub, MediumOrBetter, Shape(4, 10), LongestSuit()),				
				Nonforcing(Bid.OneDiamond, MediumOrBetter, Shape(4, 10), LongestSuit()),

				// Special case 5 clubs and 4 diamonds with mimimum hand.  Bid diamonds to avoid reverse
				Nonforcing(Bid.OneDiamond, Minimum, Shape(Suit.Clubs, 5), Shape(Suit.Diamonds, 4)),

				Nonforcing(Bid.OneClub, OneLevel, LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneClub, OneLevel, Shape(3), Shape(Suit.Diamonds, 0, 3), LongestMajor(4)),
				Nonforcing(Bid.OneClub, OneLevel, Shape(4, 11), LongerThan(Suit.Diamonds), LongestMajor(4)),

				Nonforcing(Bid.OneDiamond, OneLevel, LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneDiamond, OneLevel, Shape(3), Shape(Suit.Clubs, 0, 2), LongestMajor(4)),
				Nonforcing(Bid.OneDiamond, OneLevel, Shape(4, 10), LongerOrEqualTo(Suit.Clubs), LongestMajor(4)),

				// Special case longer hearts than spades, but not enough points to reverse.  Bid spades first.
				Nonforcing(Bid.OneSpade, Minimum, Shape(5, 10), LongestSuit(Suit.Hearts)),

				Nonforcing(Bid.OneHeart, OneLevel, Shape(5, 10), LongerThan(Suit.Spades)),

				Nonforcing(Bid.OneSpade, OneLevel, Shape(5, 10), LongerOrEqualTo(Suit.Hearts)),
			});
			// Now handle cases where we are willing to open weak in 3rd seat.  Already 4-card
			// majors have beein bid as a higher priority.  Now open with lower points.
			if (ps.Seat == 3)
			{
				// No special case for vulnerable & balanced.  Same as 1st and 2nd seats...
				bids.AddRange(ThirdSeatWeak(And(IsVul(), Balanced(false), Points(11, 16))));
				bids.AddRange(ThirdSeatWeak(And(Not(IsVul()), Balanced(true), DecentSuit(), Points(11, 16))));
				bids.AddRange(ThirdSeatWeak(And(Not(IsVul()), Balanced(false), Points(10, 16))));
			}
			return bids;
		}

		private static CallFeature[] ThirdSeat4CardMajor(Constraint range)
		{
			return new CallFeature[]
			{
				Nonforcing(Bid.OneSpade, range, GoodSuit(), Shape(4), BetterOrEqualTo(Suit.Hearts)),
				Nonforcing(Bid.OneHeart, range, GoodSuit(), Shape(4), BetterThan(Suit.Spades))
			};
		}

		private static CallFeature[] ThirdSeatWeak(Constraint range)
		{
			return new CallFeature[]
			{
				// Don't open a 3-card suit weak.  If standard open rules dont apply then dont open
				Nonforcing(Bid.OneClub, range, LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneClub, range, Shape(4, 11), LongerThan(Suit.Diamonds), LongestMajor(4)),

				Nonforcing(Bid.OneDiamond, range, LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneDiamond, range, Shape(4, 10), LongerOrEqualTo(Suit.Clubs), LongestMajor(4)),

				Nonforcing(Bid.OneHeart, range, Shape(5, 10), LongerThan(Suit.Spades)),
				Nonforcing(Bid.OneSpade, range, Shape(5, 10), LongerOrEqualTo(Suit.Hearts)),

			};
		}

		// These rules should not be added in 4th seat - weak opens make no sense
		public static IEnumerable<CallFeature> OpenSuitWeak(PositionState ps)
		{
			Debug.Assert(ps.Seat != 4);
			return new CallFeature[]
			{
				PartnerBids(Respond.WeakOpen),

				// 2C can not be bid since strong opening.  Take care of great 6-card suits by bidding 3C
				Nonforcing(Bid.TwoDiamonds, Weak, Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoHearts,   Weak, Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoSpades,   Weak, Shape(6), GoodSuit()),

				Nonforcing(Bid.ThreeClubs,    VeryWeak, Shape(6), ExcellentSuit()),
				Nonforcing(Bid.ThreeClubs,    VeryWeak, Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeDiamonds, VeryWeak, Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeHearts,   VeryWeak, Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeSpades,   VeryWeak, Shape(7), GoodSuit()),
				
                Nonforcing(Bid.FourClubs,    VeryWeak, Shape(8), DecentSuit()),
				Nonforcing(Bid.FourDiamonds, VeryWeak, Shape(8), DecentSuit()),
				Nonforcing(Bid.FourHearts,   VeryWeak, Shape(8, 10), DecentSuit()),
				Nonforcing(Bid.FourSpades,   VeryWeak, Shape(8, 10), DecentSuit()),

                Nonforcing(Bid.FiveClubs,    VeryWeak, Shape(9, 10), DecentSuit()),
				Nonforcing(Bid.FiveDiamonds, VeryWeak, Shape(9, 10), DecentSuit()),
			};
		}
	}
}
