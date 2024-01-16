using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace BridgeBidding
{
    public class Open: TwoOverOneGameForce
	{
        public static (int, int) OneLevel = (12, 21);
        public static (int, int) Minimum = (12, 16);
        public static (int, int) Medium = (17, 18);
        public static (int, int) Maximum = (19, 21);
		public static (int, int) MediumOrBetter = (17, 21);

        public static (int, int) Weak = (5, 11);
		public static (int, int) VeryWeak = (3, 11);
        public static (int, int) DontOpen = (0, 11);

        public static (int, int) Rebid1NT = (12, 14);
        public static (int, int) Rebid2NT = (18, 19);


		public static (int, int) ForthSeatOpen2 = (12, 15);

		public static (int, int) LessThanJumpShift = (12, 18);
		public static (int, int) JumpShift = (19, 21);



        public static new BidChoices GetBidChoices(PositionState ps)
        {
            var choices = new BidChoices(ps);

			choices.AddRules(SolidSuit.Bids);
            choices.AddRules(Strong2Clubs.Open);
            choices.AddRules(NoTrump.Open);
            choices.AddRules(OpenSuit);
			if (ps.Seat != 4)
			{
            	choices.AddRules(OpenSuitWeak);
			}
            choices.AddPassRule(Points(DontOpen));
            return choices;
        }

        public static IEnumerable<BidRule> OpenSuit(PositionState _)
		{
			return new List<BidRule>
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

				Nonforcing(Bid.TwoSpades,   Seat(4), Shape(6, 10), GoodSuit(), Points(ForthSeatOpen2)),
				Nonforcing(Bid.TwoHearts,   Seat(4), Shape(6, 10), GoodSuit(), Points(ForthSeatOpen2)),
				Nonforcing(Bid.TwoDiamonds, Seat(4), Shape(6, 10), GoodSuit(), Points(ForthSeatOpen2)),

				// In 4th seat we want to pass if the Rule of 15 does not apply.
				Nonforcing(Call.Pass, Seat(4), PassIn4thSeat()),

				// Special case 5 clubs and 4 diamonds with mimimum hand.  Bid diamonds to avoid reverse
				Nonforcing(Bid.OneDiamond, Points(Minimum), Shape(Suit.Clubs, 5), Shape(Suit.Diamonds, 4)),

				Nonforcing(Bid.OneClub, Points(MediumOrBetter), LongestSuit()),
				Nonforcing(Bid.OneClub, Points(OneLevel), LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneClub, Points(OneLevel), Shape(3), Shape(Suit.Diamonds, 0, 3), LongestMajor(4)),
				Nonforcing(Bid.OneClub, Points(OneLevel), Shape(4, 11), LongerThan(Suit.Diamonds), LongestMajor(4)),

				Nonforcing(Bid.OneDiamond, Points(MediumOrBetter), LongestSuit()),
				Nonforcing(Bid.OneDiamond, Points(OneLevel), LongestSuit(), Shape(Suit.Hearts, 0, 4)),
				Nonforcing(Bid.OneDiamond, Points(OneLevel), Shape(3), Shape(Suit.Clubs, 0, 2), LongestMajor(4)),
				Nonforcing(Bid.OneDiamond, Points(OneLevel), Shape(4, 10), LongerOrEqualTo(Suit.Clubs), LongestMajor(4)),

				// Special case longer hearts than spades, but not enough points to reverse.  Bid spades first.
				Nonforcing(Bid.OneSpade, Points(Minimum), Shape(5, 10), LongestSuit(Suit.Hearts)),

				Nonforcing(Bid.OneHeart, Points(OneLevel), Shape(5, 10), LongerThan(Suit.Spades)),

				Nonforcing(Bid.OneSpade, Points(OneLevel), Shape(5, 10), LongerOrEqualTo(Suit.Hearts)),
			};
		}

		// These rules should not be added in 4th seat - weak opens make no sense
		public static IEnumerable<BidRule> OpenSuitWeak(PositionState ps)
		{
			Debug.Assert(ps.Seat != 4);
			return new BidRule[]
			{
				PartnerBids(Respond.WeakOpen),

				// 2C can not be bid since strong opening.  Take care of great 6-card suits by bidding 3C
				Nonforcing(Bid.TwoDiamonds, Points(Weak), Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoHearts,   Points(Weak), Shape(6), GoodSuit()),
				Nonforcing(Bid.TwoSpades,   Points(Weak), Shape(6), GoodSuit()),

				Nonforcing(Bid.ThreeClubs,    Points(VeryWeak), Shape(6), ExcellentSuit()),
				Nonforcing(Bid.ThreeClubs,    Points(VeryWeak), Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeDiamonds, Points(VeryWeak), Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeHearts,   Points(VeryWeak), Shape(7), GoodSuit()),
				Nonforcing(Bid.ThreeSpades,   Points(VeryWeak), Shape(7), GoodSuit()),
				
                Nonforcing(Bid.FourClubs,    Points(VeryWeak), Shape(8), DecentSuit()),
				Nonforcing(Bid.FourDiamonds, Points(VeryWeak), Shape(8), DecentSuit()),
				Nonforcing(Bid.FourHearts,   Points(VeryWeak), Shape(8, 10), DecentSuit()),
				Nonforcing(Bid.FourSpades,   Points(VeryWeak), Shape(8, 10), DecentSuit()),

                Nonforcing(Bid.FiveClubs,    Points(VeryWeak), Shape(9, 10), DecentSuit()),
				Nonforcing(Bid.FiveDiamonds, Points(VeryWeak), Shape(9, 10), DecentSuit()),
			};
		}
	}
}
