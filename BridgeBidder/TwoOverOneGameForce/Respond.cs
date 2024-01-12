using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;


namespace BridgeBidding
{
    public class Respond : TwoOverOneGameForce
    {

        static protected (int, int) RespondPass = (0, 5);
        static protected (int, int) Respond1Level = (6, 40);
        static protected (int, int) Raise1 = (6, 10);

        static protected (int, int) NewSuit2Level = (13, 40);  
        static protected (int, int) RaiseTo2NT = (11, 12);
        static protected (int, int) SlamInterest = (17, 40);
        static protected (int, int) LimitRaise = (11, 12);
        static protected (int, int) LimitRaiseOrBetter = (11, 40);
        static protected (int, int) RaiseTo3NT = (13, 16);
        static protected (int, int) RaiseTo4M = (13, 16);
        static protected (int, int) Weak4Level = (0, 10);
        static protected (int, int) GameOrBetter = (13, 40);
        static protected (int, int) WeakJumpRaise = (0, 8); // TODO: Consider HCP vs DummyPoints...  For now this works.
        static protected (int, int) MinimumHand = (6, 10);
        static protected (int, int) MediumHand = (11, 13);
        static protected (int, int) ResponderRedouble = (10, 40);
        static protected (int, int) ResponderRedoubleHCP = (10, 40);

        


        //  ***** UPDATED TO 2/1 BETWEEN HERE AND NEXT ******** LINE
        static protected (int, int) WeakJumpShiftPoints = (0, 5);
        static protected (int, int) Weak5Level = (0, 10);

        static protected (int, int) Respond1NTOverMinor = (6, 10);
        static protected (int, int) Respond2NTOverMinor = (11, 12);
        static protected (int, int) Respond3NTOverClubs = (13, 17);

        static protected (int, int) Respond1NTOverMajor = (6, 12);
    


        public static IEnumerable<BidRule> WeakJumpShift(PositionState ps, params Suit[] suits)
        {
            var bids = new List<BidRule>();
            foreach (var suit in suits)
            {
                bids.Add(Signoff(new Bid (2, suit), Jump(1), Points(WeakJumpShiftPoints), Shape(6, 10), DecentSuit()));
                bids.Add(Signoff(new Bid (3, suit), Jump(1), Points(WeakJumpShiftPoints), Shape(6, 10), DecentSuit()));
            }
            return bids;
        }

        // Responses to 1 Club open.  No interference

        public static BidChoices OneClub(PositionState ps)
        {
            var choices = new BidChoices(ps);
            // TODO: Need to do different bids if passed hand....
            choices.AddRules(new BidRule[]
            {
				DefaultPartnerBids(Call.Pass, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoClubs,   Call.Pass, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.ThreeClubs, Call.Pass, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.FourClubs,  Call.Pass, OpenBid2.ResponderRaisedMinor),
                PartnerBids(Bid.FiveClubs,  Call.Pass, OpenBid2.ResponderRaisedMinor),

                // Bids
				Forcing(Bid.OneDiamond, Points(Respond1Level), Shape(5, 10), LongestMajor(3)),

                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Inverted minors...
                Invitational(Bid.TwoClubs, ShowsTrump(), Points(Raise1), Shape(5), LongestMajor(3)),

				Invitational(Bid.ThreeClubs, ShowsTrump(), Points(LimitRaise), Shape(5), LongestMajor(3)),
                
                Signoff(Bid.FiveClubs, ShowsTrump(), Points(Weak5Level), Shape(7, 10)),

                Signoff(Bid.FourClubs, ShowsTrump(), Points(Weak4Level), Shape(6)),
            });
            choices.AddRules(NoTrumpResponsesToMinor(Suit.Clubs));
            choices.AddRules(WeakJumpShift(ps, Suit.Diamonds, Suit.Hearts, Suit.Spades));
            choices.AddRules(SolidSuit.Bids(ps));
            choices.AddRules(new BidRule[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }


        // Responses to 1 Diamond open.  No interference
        public static BidChoices OneDiamond(PositionState ps)
        {
            var choices = new BidChoices(ps);
            // TODO: Need to do different bids if passed hand....
            choices.AddRules(SolidSuit.Bids(ps));
            choices.AddRules(new BidRule[]
            {
				DefaultPartnerBids(Call.Pass, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoDiamonds,   Call.Pass, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.ThreeDiamonds, Call.Pass, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.FourDiamonds,  Call.Pass, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.FiveDiamonds,  Call.Pass, OpenBid2.ResponderRaisedMinor),

                // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                // with game going values even if we have a 4 card major.
                ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), Shape(4, 10), LongestMajor(4)),

                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Inverted minors...
                Invitational(Bid.TwoDiamonds, ShowsTrump(), Points(Raise1), Shape(5), LongestMajor(3)),

				Invitational(Bid.ThreeDiamonds, ShowsTrump(), Points(LimitRaise), Shape(5), LongestMajor(3)),
                
                Signoff(Bid.FiveDiamonds, ShowsTrump(), Points(Weak5Level), Shape(7, 10)),

                Signoff(Bid.FourDiamonds, ShowsTrump(), Points(Weak4Level), Shape(6)),

            });
            choices.AddRules(NoTrumpResponsesToMinor(Suit.Diamonds));
            choices.AddRules(WeakJumpShift(ps, Suit.Clubs, Suit.Hearts, Suit.Spades));
            choices.AddRules(new BidRule[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }


        // Responses to 1 Heart open.  No interference
        public static BidChoices OneHeart(PositionState ps)
        {
            var choices = new BidChoices(ps);
            // TODO: Need to do different bids if passed hand....
            choices.AddRules(SolidSuit.Bids(ps));
            choices.AddRules(Jacoby2NT.InitiateConvention);
            choices.AddRules(new BidRule[]
            {
				DefaultPartnerBids(Call.Pass, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoHearts,   Call.Pass, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.ThreeHearts, Call.Pass, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.FourHearts,  Call.Pass, OpenBid2.ResponderRaisedMajor),

                // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                // with game going values even if we have 4 spades.
                // TODO: Is this right? 
                // TODO: Is BetterMinor() logic right?  Choose clubs over diamonds if both less than 4
                // Is this right to ever bid with less than 4?
                ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), BetterMinor(), LongestMajor(4)),
                ForcingToGame(Bid.TwoDiamonds, Points(GameOrBetter), BetterMinor(), LongestMajor(4)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10)),

                Invitational(Bid.TwoHearts, Points(Raise1), Shape(3, 5)),
                Invitational(Bid.ThreeHearts, Points(MediumHand), Shape(4, 5)),
                // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                Signoff(Bid.FourHearts, Points(Weak4Level), Shape(5, 10)),

                PartnerBids(Bid.OneNoTrump, Call.Pass, OpenBid2.OneNTOverMajorOpen),
                Semiforcing(Bid.OneNoTrump, Points(Respond1NTOverMajor), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),
            });
            choices.AddRules(WeakJumpShift(ps, Suit.Clubs, Suit.Diamonds, Suit.Spades));

            choices.AddRules(new BidRule[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }

        // Responses to 1 Spade open.  No interference
        public static BidChoices OneSpade(PositionState ps)
        {
            var choices = new BidChoices(ps);
            // TODO: Need to do different bids if passed hand....
            choices.AddRules(SolidSuit.Bids(ps));
            choices.AddRules(Jacoby2NT.InitiateConvention);
            choices.AddRules(new BidRule[]
            {
				DefaultPartnerBids(Call.Pass, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoSpades,   Call.Pass, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.ThreeSpades, Call.Pass, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.FourSpades,  Call.Pass, OpenBid2.ResponderRaisedMajor),

                // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                // with game going values even if we have 4 spades.
                // TODO: Is this right? 
                // TODO: Is BetterMinor() logic right?  Choose clubs over diamonds if both less than 4
                // Is this right to ever bid with less than 4?
                ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), BetterMinor(), LongestMajor(4)),
                ForcingToGame(Bid.TwoDiamonds, Points(GameOrBetter), BetterMinor(), LongestMajor(4)),
                ForcingToGame(Bid.TwoHearts, Shape(5, 10), Points(GameOrBetter)),

                Invitational(Bid.TwoSpades, Points(Raise1), Shape(3, 5)),
                Invitational(Bid.ThreeSpades, Points(MediumHand), Shape(4, 5)),
                // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                Signoff(Bid.FourSpades, Points(Weak4Level), Shape(5, 10)),
            
                PartnerBids(Bid.OneNoTrump, Call.Pass, OpenBid2.OneNTOverMajorOpen),
                Semiforcing(Bid.OneNoTrump, Points(Respond1NTOverMajor), Shape(Suit.Spades, 0, 3)),
            });
            choices.AddRules(WeakJumpShift(ps, Suit.Clubs, Suit.Diamonds, Suit.Hearts));
            choices.AddRules(new BidRule[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }


        private static IEnumerable<BidRule> NTResponseToMinor(Suit minor, int level, (int, int) pointRange, BidRulesFactory partnerBids)
        {
            var bid = new Bid(level, Strain.NoTrump);
            var rule = Invitational(bid, Points(pointRange), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3));
            if (minor == Suit.Clubs) rule.AddConstraint(Shape(Suit.Diamonds, 0, 4));
            return new BidRule[]
            {
                PartnerBids(bid, Bid.Double, partnerBids),
                rule
            };
        }

        protected static IEnumerable<BidRule> NoTrumpResponsesToMinor(Suit minor)
        {
            var bids = new List<BidRule>();
            bids.AddRange(NTResponseToMinor(minor, 1, Respond1NTOverMinor, OpenBid2.OneNTOverMinorOpen));
            bids.AddRange(NTResponseToMinor(minor, 2, Respond2NTOverMinor, OpenBid2.TwoNTOverMinorOpen));
            if (minor == Suit.Clubs)
            {
			    bids.AddRange(NTResponseToMinor(minor, 3, Respond3NTOverClubs, OpenBid2.ThreeNTOverClubOpen));
            }
            return bids;
        }



        // *************************** END OF 2/1 - OLD SAYC CODE ********************************


/*
        protected static BidRule[] NewMinorSuit2Level(Suit openersSuit)
        {
            return new BidRule[]
            {

                Forcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(4, 5), Shape(Suit.Diamonds, 0, 4)),
                Forcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(6), Shape(Suit.Diamonds, 0, 5)),
                Forcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(7, 11)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, LimitRaise), Shape(3), Shape(openersSuit, 3), Shape(Suit.Diamonds, 0, 3)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, LimitRaise), Shape(4, 5), Shape(openersSuit, 3), Shape(Suit.Diamonds, 0, 4)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, LimitRaise), Shape(6), Shape(openersSuit, 3)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, GameOrBetter), Shape(3), Shape(openersSuit, 3, 11), Shape(Suit.Diamonds, 0, 3)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, GameOrBetter), Shape(4, 5), Shape(openersSuit, 3, 11), Shape(Suit.Diamonds, 0, 4)),
                Forcing(Bid.TwoClubs, DummyPoints(openersSuit, GameOrBetter), Shape(6, 11), Shape(openersSuit, 3, 11)),


                Forcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(4), Shape(Suit.Clubs, 0, 3)),
                Forcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(5), Shape(Suit.Clubs, 0, 5)),
                Forcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(6), Shape(Suit.Clubs, 0, 6)),
                Forcing(Bid.TwoDiamonds, Points(NewSuit2Level), Shape(7, 11)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, LimitRaise), Shape(3), Shape(openersSuit, 3), Shape(Suit.Clubs, 0, 2)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, LimitRaise), Shape(4), Shape(openersSuit, 3), Shape(Suit.Clubs, 0, 3)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, LimitRaise), Shape(5), Shape(openersSuit, 3), Shape(Suit.Clubs, 0, 5)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, LimitRaise), Shape(6, 11), Shape(openersSuit, 3)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, GameOrBetter), Shape(3), Shape(openersSuit, 3, 11), Shape(Suit.Clubs, 0, 2)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, GameOrBetter), Shape(4), Shape(openersSuit, 3, 11), Shape(Suit.Clubs, 0, 3)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, GameOrBetter), Shape(5), Shape(openersSuit, 3, 11), Shape(Suit.Clubs, 0, 5)),
                Forcing(Bid.TwoDiamonds, DummyPoints(openersSuit, GameOrBetter), Shape(6, 11), Shape(openersSuit, 3, 11)),
            };
        }
*/


        public static IEnumerable<BidRule> ForthSeat2Open(PositionState ps)
        {
            return new BidRule[]
            {
                // TODO: What are the right bids after a strongish 2-level open?
                Signoff(Call.Pass)
            };
        }

        public static IEnumerable<BidRule> WeakOpen(PositionState ps)
        {
            return new BidRule[]
            {

                // TODO: Artificial inquiry 2NT...
                Signoff(Bid.FourHearts, Fit(), RuleOf17()),
                Signoff(Bid.FourHearts, Fit(10)),
                Signoff(Bid.FourSpades, Fit(), RuleOf17()),
                Signoff(Bid.FourSpades, Fit(10)),
				// TODO: Pass???

                Signoff(Bid.ThreeDiamonds, Fit(9)),
                Signoff(Bid.ThreeHearts,   Fit(9)),
                Signoff(Bid.ThreeSpades,   Fit(9)),

				// TODO: NT Bids
				// TODO: Minor bids???
			};
        }


        // TODO: THIS IS SUPER HACKED NOW TO JUST 
        public static BidChoices OppsOvercalled(PositionState ps)
        {
            var choices = new BidChoices(ps);
            // TODO:  Need to do better thann this for bid rules.
            choices.DefaultPartnerBids.AddFactory(Call.Double, (p) => { return new BidChoices(p, Compete.CompBids); });

            choices.AddRules(NegativeDouble.InitiateConvention);
            choices.AddRules(new BidRule[]
            {
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Perhaps show new 5+ card suit forcing here?  Only if not passed.

				// Now cuebid raises are next in priority - RaisePartner calls ShowTrump()
                Forcing(Bid.TwoDiamonds, CueBid(), RaisePartner(Suit.Clubs), DummyPoints(Suit.Clubs, LimitRaiseOrBetter)),

                Forcing(Bid.TwoHearts, CueBid(), RaisePartner(Suit.Clubs), DummyPoints(Suit.Clubs, LimitRaiseOrBetter)),
                Forcing(Bid.TwoHearts, CueBid(), RaisePartner(Suit.Diamonds), DummyPoints(Suit.Diamonds, LimitRaiseOrBetter)),


                Forcing(Bid.TwoSpades, CueBid(), RaisePartner(Suit.Clubs), DummyPoints(Suit.Clubs, LimitRaiseOrBetter)),
                Forcing(Bid.TwoSpades, CueBid(), RaisePartner(Suit.Diamonds), DummyPoints(Suit.Diamonds, LimitRaiseOrBetter)),
                Forcing(Bid.TwoSpades, CueBid(), RaisePartner(Suit.Hearts), DummyPoints(Suit.Hearts, LimitRaiseOrBetter)),


                // TODO: Weak jumps here take precedence over simple raise
               
				Nonforcing(Bid.ThreeHearts, Fit(9), Jump(1), DummyPoints(WeakJumpRaise)),
                Nonforcing(Bid.ThreeSpades, Fit(9), Jump(1), DummyPoints(WeakJumpRaise)),


                // Now time for invitational bids.
                Invitational(Bid.TwoClubs, CueBid(false), RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid.TwoClubs, OppsStopped(false), CueBid(false), RaisePartner(fit: 7), DummyPoints(Raise1)),

                Invitational(Bid.TwoDiamonds, CueBid(false), RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid.TwoDiamonds, OppsStopped(false), CueBid(false), RaisePartner(fit: 7), DummyPoints(Raise1)),

                Invitational(Bid.TwoHearts, CueBid(false), RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid.TwoSpades, CueBid(false), RaisePartner(), DummyPoints(Raise1)),

				// TODO: Still need lots and lots more bid levels here.  But decent start...
		
				// TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function

				Signoff(Bid.FourHearts, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),
                Signoff(Bid.FourSpades, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),

                Signoff(Bid.Pass, Points(RespondPass)),

            });
            // TODO: Need to have opponents stopped?  Maybe those bids go higher up ...
// TODO: NO TRUMP...            choices.AddRules(NoTrumpResponses(ps));

            return choices;
        }

        static protected (int, int) RespondRedouble = (10, 40);
        static protected (int, int) RespondX1Level = (6, 9);
        static protected (int, int) RespondXJump = (0, 6);
        

        public static IEnumerable<BidRule> OppsDoubled(PositionState ps)
        {
            var bids = new List<BidRule>
            {
                Forcing(Call.Redouble, Points(RespondRedouble)),
				// TODO: Here we need to make all bids reflect that they are less than 10 points...

				Nonforcing(Bid.OneHeart, Points(RespondX1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Nonforcing(Bid.OneHeart, Points(RespondX1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Nonforcing(Bid.OneSpade, Points(RespondX1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Nonforcing(Bid.OneSpade, Points(RespondX1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                Nonforcing(Bid.OneDiamond, Shape(4, 11), Points(RespondX1Level)),

                //
                // If we have a good fie but a week hand then time to jump.
                //
                Nonforcing(Bid.ThreeClubs,    Partner(HasShownSuit()), Fit(9), ShowsTrump(), Points(RespondXJump)),
                Nonforcing(Bid.ThreeDiamonds, Partner(HasShownSuit()), Fit(9), ShowsTrump(), Points(RespondXJump)),
                Nonforcing(Bid.ThreeHearts,   Partner(HasShownSuit()), Fit(9), ShowsTrump(), Points(RespondXJump)),
                Nonforcing(Bid.ThreeSpades,   Partner(HasShownSuit()), Fit(9), ShowsTrump(), Points(RespondXJump)),

                Nonforcing(Bid.TwoClubs, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoClubs, Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoDiamonds, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoDiamonds, Jump(0), Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoHearts, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoHearts, Jump(0), Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoSpades, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),

				// TODO: Perhaps higer priority than raise of a minor???
                Nonforcing(Bid.OneNoTrump, Points(RespondX1Level)),

                Signoff(Bid.Pass, Points(RespondPass))

            };

            return bids;
        }

    }
}
