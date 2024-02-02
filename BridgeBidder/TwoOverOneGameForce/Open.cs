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
		
	//	protected static Constraint Open2In4thSeat = And(Seat(4), Points(12, 15), Shape(6, 10), GoodPlusSuit); 
	//	protected static Constraint OpenIn3rdBal = And(Seat(3), Points(11, 11), Balanced(), DecentPlusSuit);
	
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


		public static Constraint ForthSeatOpen2 = Points(12, 15);

		public static (int, int) LessThanJumpShift = (12, 18);
		public static (int, int) JumpShift = (19, 21);

        public static new PositionCalls GetPositionCalls(PositionState ps)
        {
            var choices = new PositionCalls(ps);

			choices.AddRules(SolidSuit.Bids);
            choices.AddRules(Strong2Clubs.Open);
            choices.AddRules(NoTrump.Open);
			choices.AddRules(OpenSuitWeak);
            choices.AddRules(OpenSuit);
			if (ps.Seat != 4)
			{
            	choices.AddPassRule(DontOpen);
			} 
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

/*
				// 2-level opening in 4th seat is strong, like a 1 level
				// TODO: Rename this function - sometimes opens 2-level
				PartnerBids(Bid.TwoDiamonds, Respond.ForthSeat2Open, Seat(4)),
				PartnerBids(Bid.TwoHearts,   Respond.ForthSeat2Open, Seat(4)),
				PartnerBids(Bid.TwoSpades,   Respond.ForthSeat2Open, Seat(4)),

				Nonforcing(Bid.TwoSpades,   Seat(4), Shape(6, 10), GoodPlusSuit, BetterOrEqualTo(Suit.Hearts), ForthSeatOpen2),
				Nonforcing(Bid.TwoHearts,   Seat(4), Shape(6, 10), GoodPlusSuit, BetterThan(Suit.Spades), ForthSeatOpen2),
				Nonforcing(Bid.TwoDiamonds, Seat(4), Shape(6, 10), GoodPlusSuit, ForthSeatOpen2),
*/
				// In 4th seat we want to pass if the Rule of 15 does not apply.
				Nonforcing(Call.Pass, Seat(4), PassIn4thSeat()),
			};
			// If in 3rd seat then give priority to opening good 4 card majors with a minimum hand
			if (ps.Seat == 3)
			{
				// No special case for vulnerable & balanced.  Same as 1st and 2nd seats...
				bids.AddRange(ThirdSeat4CardMajor(And(IsVul,    Balanced(false), Points(11, 13))));
				bids.AddRange(ThirdSeat4CardMajor(And(IsNotVul, Balanced(true),  Points(11, 13))));
				bids.AddRange(ThirdSeat4CardMajor(And(IsNotVul, Balanced(false), Points(10, 13))));
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
				bids.AddRange(ThirdSeatWeak(And(IsVul, Balanced(false), Points(11, 11))));
				bids.AddRange(ThirdSeatWeak(And(IsNotVul, Balanced(true), DecentPlusSuit, Points(11, 11))));
				bids.AddRange(ThirdSeatWeak(And(IsNotVul, Balanced(false), Points(10, 11))));
			}
			// Since this group contians a rule for Pass in 4th seat, we have to define one here 
			// at the end of the group to catch the cases where we don't want to open because of
			// just plain-old lack of points.
			bids.Add(Nonforcing(Call.Pass, Seat(4), DontOpen));
			return bids;
		}

		private static CallFeature[] ThirdSeat4CardMajor(Constraint range)
		{
			return new CallFeature[]
			{
				Nonforcing(Bid.OneSpade, range, GoodPlusSuit, Shape(4), BetterOrEqualTo(Suit.Hearts)),
				Nonforcing(Bid.OneHeart, range, GoodPlusSuit, Shape(4), BetterThan(Suit.Spades))
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
/*
		// These rules should not be added in 4th seat - weak opens make no sense
		public static IEnumerable<CallFeature> OpenSuitWeak(PositionState ps)
		{
			Debug.Assert(ps.Seat != 4);
			return new CallFeature[]
			{
				PartnerBids(Respond.WeakOpen),

				// 2C can not be bid since strong opening.  Take care of great 6-card suits by bidding 3C
				Nonforcing(Bid.TwoDiamonds, Weak, Shape(6), GoodPlusSuit),
				Nonforcing(Bid.TwoHearts,   Weak, Shape(6), GoodPlusSuit),
				Nonforcing(Bid.TwoSpades,   Weak, Shape(6), GoodPlusSuit),

				Nonforcing(Bid.ThreeClubs,    VeryWeak, Shape(6), ExcellentPlusSuit),
				Nonforcing(Bid.ThreeClubs,    VeryWeak, Shape(7), GoodPlusSuit),
				Nonforcing(Bid.ThreeDiamonds, VeryWeak, Shape(7), GoodPlusSuit),
				Nonforcing(Bid.ThreeHearts,   VeryWeak, Shape(7), GoodPlusSuit),
				Nonforcing(Bid.ThreeSpades,   VeryWeak, Shape(7), GoodPlusSuit),
				
                Nonforcing(Bid.FourClubs,    VeryWeak, Shape(8), DecentPlusSuit),
				Nonforcing(Bid.FourDiamonds, VeryWeak, Shape(8), DecentPlusSuit),
				Nonforcing(Bid.FourHearts,   VeryWeak, Shape(8, 10), DecentPlusSuit),
				Nonforcing(Bid.FourSpades,   VeryWeak, Shape(8, 10), DecentPlusSuit),

                Nonforcing(Bid.FiveClubs,    VeryWeak, Shape(9, 10), DecentPlusSuit),
				Nonforcing(Bid.FiveDiamonds, VeryWeak, Shape(9, 10), DecentPlusSuit),
			};
		}
*/
		private static List<CallFeature> OpenSuitWeak(PositionState ps)
		{
			var rules = new List<CallFeature>();
			rules.Add(PartnerBids(Respond.WeakOpen));
			switch (ps.Seat)
			{
				case 1:
					AddWeakRules(rules, And(IsFavVul,   Points(4, 11)));
					AddWeakRules(rules, And(IsFavVul,   Points(8, 11), Shape(5), ExcellentPlusSuit), onlyLevel: 2);
					AddWeakRules(rules, And(BothNotVul, Points(5, 11), DecentPlusSuit));
					AddWeakRules(rules, And(IsVul,      Points(7, 11), GoodPlusSuit));
					break;

				case 2:
					AddWeakRules(rules, And(IsNotVul, Points(6, 11), DecentPlusSuit));
					AddWeakRules(rules, And(IsVul,    Points(8, 11), GoodPlusSuit));
					break;

				case 3:
					AddWeakRules(rules, And(IsFavVul,   Points(2, 13)));
					AddWeakRules(rules, And(IsFavVul,   Points(2, 13), Shape(5), GoodPlusSuit), onlyLevel: 2);
					AddWeakRules(rules, And(BothNotVul, Points(4, 13), DecentPlusSuit));
					AddWeakRules(rules, And(BothNotVul, Points(4, 13), Shape(5), ExcellentPlusSuit), onlyLevel: 2);
					AddWeakRules(rules, And(IsVul,      Points(6, 13), GoodPlusSuit));
					break;

				case 4:
					AddWeakRules(rules, And(Points(10, 15), DecentPlusSuit));
					break;

				default:
					Debug.Fail("Seat not 1-4!");
					break;
			}
			return rules;
		}

		// If a level is specified (level != 0) then the constraint must contain ALL of the information about
		// the shape of the suit - That is, it is specific to only one level (which is really just level 2).
		// If no level is given then bids will be generated for 6 card suits at level 2.  7 at level 3, with a
		// special exceptoin for a 6-card club suit at level 3.  8 card suits at level 4.
		public static void AddWeakRules(List<CallFeature> rules, Constraint constraint, int onlyLevel = 0)
		{
			int minLevel = onlyLevel == 0 ? 2 : onlyLevel;
			int maxLevel = onlyLevel == 0 ? 4 : onlyLevel;
			for (int level = minLevel; level <= maxLevel; level++)
			{
				Constraint levelConstraint = constraint;
				if (onlyLevel == 0)
				{
					levelConstraint = And(constraint, Shape(level + 4));
				}
				foreach (var suit in Card.Suits)
				{
					var bid = new Bid(level, suit);
					if (!bid.Equals(Bid.TwoClubs))
					{
						if (suit != Suit.Hearts && suit != Suit.Spades)
						{
							rules.Add(Nonforcing(bid, levelConstraint, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)));
						}
						if (suit == Suit.Hearts)
						{
							rules.Add(Nonforcing(bid, levelConstraint, Shape(Suit.Spades, 0, 3)));
						}
						else 
						{
							rules.Add(Nonforcing(bid, levelConstraint, Shape(Suit.Hearts, 4, 5), IsBadSuit(Suit.Hearts)));
						}
						if (suit == Suit.Spades)
						{
							rules.Add(Nonforcing(bid, levelConstraint, Shape(Suit.Hearts, 0, 3)));
						}
						else 
						{
							rules.Add(Nonforcing(bid, levelConstraint, Shape(Suit.Spades, 4, 5), IsBadSuit(Suit.Spades)));
						}
					}
				}
			}
		}
	}
}
