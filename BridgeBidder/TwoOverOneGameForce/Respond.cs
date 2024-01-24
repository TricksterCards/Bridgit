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
        static protected (int, int) LimitRaise = (11, 12);
        static protected (int, int) NewSuit2Level = (13, 40);  
        static protected (int, int) SlamInterest = (17, 40);

        static protected (int, int) LimitRaiseOrBetter = (11, 40);
        static protected (int, int) RaiseTo3NT = (13, 16);
        static protected (int, int) RaiseTo4M = (13, 16);
        static protected (int, int) Weak4Level = (0, 10);
        static protected (int, int) GameOrBetter = (13, 40);
        static protected (int, int) WeakJumpRaise = (0, 8); // TODO: Consider HCP vs DummyPoints...  For now this works.
        static protected (int, int) MinimumHand = (6, 10);
        static protected (int, int) MediumHand = (11, 13);

        static protected (int, int) RespondRedouble = (10, 40);
        static protected (int, int) RespondX1Level = (6, 9);
        static protected (int, int) RespondXJump = (0, 6);      


        //  ***** UPDATED TO 2/1 BETWEEN HERE AND NEXT ******** LINE

        static protected (int, int) MaxPassed = (10, 11);
        static protected (int, int) WeakJumpShiftPoints = (0, 5);
        static protected (int, int) Weak5Level = (0, 10);

        static protected (int, int) Respond1NTOverMinor = (6, 10);
        static protected (int, int) Respond2NTOverMinor = (11, 12);
        static protected (int, int) Respond3NTOverClubs = (13, 17);

        static protected (int, int) Respond1NTOverMajor = (6, 12);

        static protected (int, int) Respond1NTPassedHand = (6, 11);
    


        public static IEnumerable<CallFeature> WeakJumpShift(Suit openSuit)
        {
            var bids = new List<CallFeature>();
            foreach (var suit in Card.Suits)
            {
                if (suit != openSuit)
                {
                    bids.Add(Signoff(new Bid (2, suit), Jump(1), Points(WeakJumpShiftPoints), Shape(6, 10), DecentSuit()));
                    bids.Add(Signoff(new Bid (3, suit), Jump(1), Points(WeakJumpShiftPoints), Shape(6, 10), DecentSuit()));
                }
            }
            return bids;
        }

        // Responses to 1 Club open.

        public static PositionCalls OneClub(PositionState ps)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, Suit.Clubs);

            var choices = new PositionCalls(ps);
            if (ps.IsPassedHand)
            {
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),
                    PartnerBids(Bid.TwoClubs,   OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.ThreeClubs, OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.FourClubs,  OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.FiveClubs,  OpenBid2.ResponderRaisedMinor),

                
                    Nonforcing(Bid.OneDiamond, Points(Respond1Level), Shape(5, 10), LongestMajor(3)),

                    Nonforcing(Bid.OneHeart, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                    Nonforcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                    Nonforcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                    // TODO: Inverted minors...
                    Invitational(Bid.TwoClubs, ShowsTrump(), Points(Raise1), Shape(5), LongestMajor(3)),
                    Invitational(Bid.ThreeClubs, ShowsTrump(), Points(LimitRaise), Shape(5), LongestMajor(3)),                
                    Signoff(Bid.FiveClubs, ShowsTrump(), Points(Weak5Level), Shape(7, 10)),
                    Signoff(Bid.FourClubs, ShowsTrump(), Points(Weak4Level), Shape(6)),
                });
            }
            else
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),
                    PartnerBids(Bid.TwoClubs,   OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.ThreeClubs, OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.FourClubs,  OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid.FiveClubs,  OpenBid2.ResponderRaisedMinor),

                    Forcing(Bid.OneDiamond, Points(Respond1Level), Shape(5, 10), LongestMajor(3)),
                    // TODO: Should we bid "up the line" with 11+ points?
                    Forcing(Bid.OneDiamond, Points(LimitRaiseOrBetter), Shape(5, 10), LongerThan(Suit.Hearts), LongerThan(Suit.Spades)),

                    Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                    Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                    Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                    // TODO: Inverted minors...
                    Invitational(Bid.TwoClubs, ShowsTrump(), Points(Raise1), Shape(5), LongestMajor(3)),
                    Invitational(Bid.ThreeClubs, ShowsTrump(), Points(LimitRaise), Shape(5), LongestMajor(3)),                
                    Signoff(Bid.FiveClubs, ShowsTrump(), Points(Weak5Level), Shape(7, 10)),
                    Signoff(Bid.FourClubs, ShowsTrump(), Points(Weak4Level), Shape(6)),
                });
            }
            choices.AddRules(NoTrumpResponsesToMinor(Suit.Clubs));
            choices.AddRules(WeakJumpShift(Suit.Clubs));

            choices.AddPassRule(Points(RespondPass));
            return choices;
        }


        // Responses to 1 Diamond open.  No interference
        public static PositionCalls OneDiamond(PositionState ps)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, Suit.Diamonds);
            var choices = new PositionCalls(ps);
            // TODO: Need to do different bids if passed hand....
            choices.AddRules(SolidSuit.Bids);
            choices.AddRules(new CallFeature[]
            {
				PartnerBids(OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoDiamonds,   OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.ThreeDiamonds, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.FourDiamonds,  OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.FiveDiamonds,  OpenBid2.ResponderRaisedMinor),

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
            choices.AddRules(WeakJumpShift(Suit.Diamonds));
            choices.AddRules(new CallFeature[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }


        // Responses to 1 Heart open.  No interference
        public static PositionCalls OneHeart(PositionState ps)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, Suit.Hearts);

            var choices = new PositionCalls(ps);
            if (ps.IsPassedHand)
            {
                choices.AddRules(new CallFeature[]
                {
                    // TODO: Is this OK
                    PartnerBids(OpenBid2.ResponderChangedSuits),

                    // TODO: Are these OK?  Need a "passed" version?
                    PartnerBids(Bid.TwoHearts,   OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.ThreeHearts, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.FourHearts,  OpenBid2.ResponderRaisedMajor),

                    Nonforcing(Bid.OneSpade, Shape(4, 10), Points(Respond1Level)),

                    // TODO: Rules for best suit if 5-5
                    Invitational(Bid.TwoClubs, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid.TwoDiamonds, Points(MaxPassed), Shape(5, 10)),

                    Invitational(Bid.TwoHearts, Points(Raise1), Shape(3, 5)),
                    Invitational(Bid.ThreeHearts, Points(MediumHand), Shape(3, 5)),
                    // TODO: When would we bid 4?
                    Signoff(Bid.FourHearts, Points(Weak4Level), Shape(5, 10)),
                
                  // TODO: Where  PartnerBids(Bid.OneNoTrump, OpenBid2.OneNTOverMajorOpen),
                    // TODO: Points range name wrong!
                    Semiforcing(Bid.OneNoTrump, Points(Respond1NTPassedHand), Shape(Suit.Hearts, 0, 2)),
                });
            }
            else 
            {
                // TODO: Need to do different bids if passed hand....
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(Jacoby2NT.InitiateConvention);
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),
                    PartnerBids(Bid.TwoHearts,   OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.ThreeHearts, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.FourHearts,  OpenBid2.ResponderRaisedMajor),

                    // TODO: LARRY CONFIRM - With 2/1 game force values bid a minor with 4 spades. 
                    // more spades bid spades regardless of points
                    // TODO: If we have a fit skip spades even with 5????  Always show 5? 

                    ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, DummyPoints(Suit.Hearts, GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, DummyPoints(Suit.Hearts, GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),

                    ForcingToGame(Bid.TwoDiamonds, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid.TwoDiamonds, DummyPoints(Suit.Hearts, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),

                    Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 10)),

                    Invitational(Bid.TwoHearts, Points(Raise1), Shape(3, 5)),
                    Invitational(Bid.ThreeHearts, Points(MediumHand), Shape(4, 5)),
                    // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                    // TODO: LARRY - we have discussed 3-card limit raises.  Should we do that?
                    Signoff(Bid.FourHearts, Points(Weak4Level), Shape(5, 10)),

                    PartnerBids(Bid.OneNoTrump, OpenBid2.OneNTOverMajorOpen),
                    Semiforcing(Bid.OneNoTrump, Points(Respond1NTOverMajor), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),
                    
                    // TODO: Any follow-up to this?  Slam? - or just use compete logic?  
                    Signoff(Bid.ThreeNoTrump, Flat(), Points(RaiseTo3NT))
                });
                choices.AddRules(WeakJumpShift(Suit.Hearts));
            }
            choices.AddPassRule(Points(RespondPass));
            return choices;
        }

        // Responses to 1 Spade open.  No interference
        public static PositionCalls OneSpade(PositionState ps)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, Suit.Spades);

            var choices = new PositionCalls(ps);
            if (ps.IsPassedHand)
            {
                // TODO: This is where we would put Drury
                choices.AddRules(new CallFeature[]
                {
                    // TODO: Is this OK
                    PartnerBids(OpenBid2.ResponderChangedSuits),

                    // TODO: Are these OK?  Need a "passed" version?
                    PartnerBids(Bid.TwoSpades,   OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.ThreeSpades, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.FourSpades,  OpenBid2.ResponderRaisedMajor),

                    // TODO: Rules for best suit if 5-5
                    Invitational(Bid.TwoClubs, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid.TwoDiamonds, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid.TwoHearts, Points(MaxPassed), Shape(5, 10)),

                    Invitational(Bid.TwoSpades, Points(Raise1), Shape(3, 5)),
                    Invitational(Bid.ThreeSpades, Points(MediumHand), Shape(3, 5)),
                    // TODO: When would we bid 4?
                    Signoff(Bid.FourSpades, Points(Weak4Level), Shape(5, 10)),
                
                  // TODO: Where  PartnerBids(Bid.OneNoTrump, OpenBid2.OneNTOverMajorOpen),
                    // TODO: Points range name wrong!
                    Semiforcing(Bid.OneNoTrump, Points(Respond1NTPassedHand), Shape(Suit.Spades, 0, 2)),
                });
            }
            else 
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(Jacoby2NT.InitiateConvention);
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),

                    PartnerBids(Bid.TwoSpades,   OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.ThreeSpades, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid.FourSpades,  OpenBid2.ResponderRaisedMajor),

                    // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                    // with game going values even if we have 4 spades.
                    // TODO: Is this right? 
                    // TODO: Is BetterMinor() logic right?  Choose clubs over diamonds if both less than 4
                    // Is this right to ever bid with less than 4?

                    ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, Points(GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, DummyPoints(Suit.Spades, GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid.TwoClubs, DummyPoints(Suit.Spades, GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),

                    ForcingToGame(Bid.TwoDiamonds, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid.TwoDiamonds, DummyPoints(Suit.Spades, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),

                    ForcingToGame(Bid.TwoHearts, Shape(5, 10), Points(GameOrBetter)),

                    Invitational(Bid.TwoSpades, Points(Raise1), Shape(3, 5)),
                    Invitational(Bid.ThreeSpades, Points(MediumHand), Shape(4, 5)),
                    // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                    // LARRY: Same issue here - 3-card supprot limit raises or 1NT relay?
                    Signoff(Bid.FourSpades, Points(Weak4Level), Shape(5, 10)),
                
                    PartnerBids(Bid.OneNoTrump, OpenBid2.OneNTOverMajorOpen),
                    Semiforcing(Bid.OneNoTrump, Points(Respond1NTOverMajor), Shape(Suit.Spades, 0, 3)),

                    // TODO: Partner bids for this- slam or show 4M
                    Signoff(Bid.ThreeNoTrump, Flat(), Points(RaiseTo3NT))
                });

            }
            // Weak jump shift for passed and unpassed hands
            choices.AddRules(WeakJumpShift(Suit.Spades));
            choices.AddPassRule(Points(RespondPass));
            return choices;
        }


        private static IEnumerable<CallFeature> NTResponseToMinor(Suit minor, int level, (int, int) pointRange, PositionCallsFactory partnerBids)
        {
            var bid = new Bid(level, Strain.NoTrump);
            var rule = Invitational(bid, Points(pointRange), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3));
            if (minor == Suit.Clubs) rule.AddConstraint(Shape(Suit.Diamonds, 0, 4));
            return new CallFeature[]
            {
                PartnerBids(bid, partnerBids),
                rule
            };
        }

        protected static IEnumerable<CallFeature> NoTrumpResponsesToMinor(Suit minor)
        {
            var bids = new List<CallFeature>();
            bids.AddRange(NTResponseToMinor(minor, 1, Respond1NTOverMinor, OpenBid2.OneNTOverMinorOpen));
            bids.AddRange(NTResponseToMinor(minor, 2, Respond2NTOverMinor, OpenBid2.TwoNTOverMinorOpen));
            if (minor == Suit.Clubs)
            {
			    bids.AddRange(NTResponseToMinor(minor, 3, Respond3NTOverClubs, OpenBid2.ThreeNTOverClubOpen));
            }
            return bids;
        }


        private static PositionCalls OppsInterferred(PositionState ps, Suit openSuit)
        {
            if (ps.RHO.Doubled)
                return OppsDoubled(ps, openSuit);
            else
                return OppsOvercalled(ps, openSuit, ps.RHO.Bid);
        }
 



        // *************************** END OF 2/1 - OLD SAYC CODE ********************************


        public static IEnumerable<CallFeature> ForthSeat2Open(PositionState ps)
        {
            return new CallFeature[]
            {
                // TODO: What are the right bids after a strongish 2-level open?
                Signoff(Call.Pass)
            };
        }

        public static IEnumerable<CallFeature> WeakOpen(PositionState ps)
        {
            return new CallFeature[]
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
                
                Signoff(Call.Pass)

			};
        }


        // TODO: THIS IS SUPER HACKED AND INCOMPLETE - NO TRUMP BIDS FOR ONE THING!
        public static PositionCalls OppsOvercalled(PositionState ps, Suit openSuit, Bid rhoBid)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(SolidSuit.Bids);
            choices.AddRules(NegativeDouble.InitiateConvention);
            choices.AddRules(WeakJumpShift(openSuit));
            choices.AddRules(new CallFeature[]
            {
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Perhaps show new 5+ card suit forcing here?  Only if not passed.

				// Now cuebid raises are next in priority - RaisePartner calls ShowTrump()
                Forcing(Bid.TwoDiamonds, CueBid(), RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Forcing(Bid.TwoHearts, CueBid(), RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Forcing(Bid.TwoSpades, CueBid(), RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),

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

                Signoff(Bid.Pass),  // May have enought points to respond but no good call, so can't specify points.

            });
            // TODO: Need to have opponents stopped?  Maybe those bids go higher up ...
// TODO: NO TRUMP...            choices.AddRules(NoTrumpResponses(ps));

            return choices;
        }


        

        public static PositionCalls OppsDoubled(PositionState ps, Suit openSuit)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(SolidSuit.Bids);
          // TODO: Maybe we want this???  choices.AddRules(WeakJumpShift(openSuit));
            choices.AddRules(new CallFeature[] 
            {
                Forcing(Call.Redouble, Points(RespondRedouble)),
				// TODO: Here we need to make all bids reflect that they are less than 10 points...

				Nonforcing(Bid.OneHeart, Points(RespondX1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Nonforcing(Bid.OneHeart, Points(RespondX1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Nonforcing(Bid.OneSpade, Points(RespondX1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Nonforcing(Bid.OneSpade, Points(RespondX1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                Nonforcing(Bid.OneDiamond, Shape(4, 11), Points(RespondX1Level)),

                //
                // If we have a good fit but a week hand then time to jump.
                //
                Nonforcing(new Bid(3, openSuit), Fit(9), ShowsTrump(), Points(RespondXJump)),
                
                Nonforcing(Bid.TwoClubs, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoClubs, Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoDiamonds, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoDiamonds, Jump(0), Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoHearts, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid.TwoHearts, Jump(0), Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid.TwoSpades, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),

				// TODO: Perhaps higer priority than raise of a minor???
                Nonforcing(Bid.OneNoTrump, Points(RespondX1Level)),

                // TODO: Is this correct about RespondPass points?  Are there scenerios where you just don't have
                // the right shape, but do have enought points?  I guess we'd always bid 1NT
                Signoff(Bid.Pass, Points(RespondPass))

            });

            return choices;
        }

    }
}
