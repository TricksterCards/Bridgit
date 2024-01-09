using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;


namespace BridgeBidding
{
    public class Respond : TwoOverOneGameForce
    {

        static protected (int, int) RespondPass = (0, 5);
        static protected (int, int) Respond1Level = (6, 40);
        static protected (int, int) Raise1 = (6, 10);
        static protected (int, int) Respond1NT = (6, 12);
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
				DefaultPartnerBids(Call.Double, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoClubs, Call.Double, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.ThreeClubs, Bid.Double, OpenBid2.ResponderRaisedMinor),

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
            choices.AddRules(NoTrumpResponses(ps, Suit.Diamonds, Suit.Hearts, Suit.Spades));
            choices.AddRules(WeakJumpShift(ps, Suit.Diamonds, Suit.Hearts, Suit.Spades));
            choices.AddRules(SolidSuit.Bids(ps));
            choices.AddRules(new BidRule[] {  Signoff(Bid.Pass, Points(RespondPass))});
            return choices;
        }



        // *************************** END OF 2/1 - OLD SAYC CODE ********************************



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

        private static BidRule[] RespondNT(int level, params Suit[] denies)
        {
            var rule = Invitational(new Bid(level, Strain.NoTrump));
            if (level == 1)
            {
                foreach (Suit suit in denies)
                {
                    rule.AddConstraint(Shape(suit, 0, 3));
                }
                rule.AddConstraint(Points(Respond1NT));
            }
            else
            {
                // TODO: Is this right?  I think a 5+ card suit should always be bid...
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    rule.AddConstraint(Shape(suit, 0, 4));
                }
                rule.AddConstraint(Points(level == 2 ? RaiseTo2NT : RaiseTo3NT));
            }
            return new BidRule[]
            {
                PartnerBids(new Bid(level, Strain.NoTrump), Bid.Double, p => OpenBid2.ResponderBidNT(p, level)),
                rule
            };
        }

        protected static IEnumerable<BidRule> NoTrumpResponses(PositionState ps, params Suit[] denies)
        {
            var bids = new List<BidRule>();
			bids.AddRange(RespondNT(1, denies));
            //  TODO: Jacoby 2NT would preclude this bid...
			bids.AddRange(RespondNT(2, denies));
			bids.AddRange(RespondNT(3, denies));
            return bids;
        }

        public static IEnumerable<BidRule> ForthSeat2Open(PositionState ps)
        {
            return new BidRule[]
            {
                // TODO: What are the right bids after a strongish 2-level open?
                Signoff(Call.Pass)
            };
        }


        
        public static IEnumerable<BidRule> Diamond(PositionState ps)
        {
            var bids = new List<BidRule>
            {
				DefaultPartnerBids(Bid.Double, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoDiamonds, Bid.Double, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid.ThreeDiamonds, Bid.Double, OpenBid2.ResponderRaisedMinor),

				// TODO: More formal redouble???
				Forcing(Bid.Redouble, Points((10, 100)), HighCardPoints((10, 100))),

                Invitational(Bid.ThreeDiamonds, DummyPoints(LimitRaise), Shape(5, 11), LongestMajor(3)),
                Invitational(Bid.TwoDiamonds, Points(Raise1), Shape(5, 11), LongestMajor(2)),

				// TODO: Only forcing if not a passed hand...
				Forcing(Bid.OneHeart, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.OneHeart, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

//				Nonforcing(1, Suit.Unknown, Points(Respond1NT), Balanced(), LongestMajor(3)),


				Forcing(Bid.TwoClubs, Points(NewSuit2Level), Shape(5, 11), LongestMajor(3)),

                Forcing(Bid.TwoHearts, Points(SlamInterest), Shape(5, 11)),

                Forcing(Bid.TwoSpades, Points(SlamInterest), Shape(5, 11)),

                // TODO: Really balanced?  This would only be the case for 4333 given current rules.  Maybe so...
              //  Invitational(2, Suit.Unknown, Points(RaiseTo2NT), LongestMajor(3), Balanced()),


//				Signoff(3, Suit.Unknown, Points(RaiseTo3NT), LongestMajor(3)),

				Signoff(Bid.FourDiamonds, Points(Weak4Level), Shape(6, 11)),

                // TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function

                Signoff(Bid.FourHearts, Points(Weak4Level), Shape(7, 11)),

                Signoff(Bid.FourSpades, Points(Weak4Level), Shape(7, 11)),


                Signoff(Call.Pass, Points(RespondPass)),
            };
            bids.AddRange(NoTrumpResponses(ps,Suit.Hearts, Suit.Spades));
            return bids;
        }
        public static IEnumerable<BidRule> Heart(PositionState ps)
        {
            var bids = new List<BidRule>
            {
				DefaultPartnerBids(Call.Double, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoHearts,   Call.Double, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.ThreeHearts, Call.Double, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.FourHearts,  Call.Double, OpenBid2.ResponderRaisedMajor),

                // TODO: Need higher priority bids showing spades when bid hand ---

				Invitational(Bid.TwoHearts, DummyPoints(Raise1), Shape(3, 8), ShowsTrump()),
                Invitational(Bid.TwoHearts,DummyPoints(LimitRaise), Shape(4, 8), ShowsTrump()),
				Signoff(Bid.FourHearts, DummyPoints(Suit.Hearts, Weak4Level), Shape(5, 8)),

                // TODO: This is wrong.  Need weak bid. not this one...
				Forcing(Bid.TwoSpades, Points(SlamInterest), Shape(5, 11)),

                Forcing(Bid.OneSpade, Points(Respond1Level), Shape(4, 11), Shape(Suit.Hearts, 0, 2)),
                Forcing(Bid.OneSpade, DummyPoints(Suit.Hearts, LimitRaise), Shape(4, 11), Shape(Suit.Hearts, 3)),
                Forcing(Bid.OneSpade, DummyPoints(Suit.Hearts, GameOrBetter), Shape(4, 11), Shape(Suit.Hearts, 3, 8)),



                // TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function


                Signoff(Bid.FourSpades, Points(Weak4Level), Shape(7, 11)),

                Signoff(Bid.Pass,Points(RespondPass)),

            };
            bids.AddRange(NewMinorSuit2Level(Suit.Hearts));
            bids.AddRange(NoTrumpResponses(ps, Suit.Spades));
            return bids;
        }

        public static IEnumerable<BidRule> Spade(PositionState ps)
        {
            var bids = new List<BidRule>
            {
                DefaultPartnerBids(Bid.Double, OpenBid2.ResponderChangedSuits),
				PartnerBids(Bid.TwoSpades, Bid.Double, OpenBid2.ResponderRaisedMajor),
				PartnerBids(Bid.ThreeSpades, Bid.Double, OpenBid2.ResponderRaisedMajor),
                PartnerBids(Bid.FourSpades, Bid.Double, OpenBid2.ResponderRaisedMajor),

				// Highest priority is to show support...
                Invitational(Bid.ThreeSpades, DummyPoints(LimitRaise), Shape(4, 8), ShowsTrump()),
                Invitational(Bid.TwoSpades, DummyPoints(Raise1), Shape(3, 8), ShowsTrump()),
				Signoff(Bid.FourSpades, DummyPoints(Weak4Level), Shape(5, 8)),

                // Two level minor bids are handled by NewMinorSuit2Level...
                // THIS IS HIGHER PRIORITY THAN SHOWING MINORS NO MATTER WHAT THE LENGTH...
				Forcing(Bid.TwoHearts, Points(NewSuit2Level), Shape(5, 11)),


                // TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function

                Signoff(Bid.FourHearts, Points(Weak4Level), Shape(7, 11)),


                Signoff(Call.Pass, Points(RespondPass)),

            };
            bids.AddRange(NewMinorSuit2Level(Suit.Spades));
            bids.AddRange(NoTrumpResponses(ps));
            return bids;
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
            choices.AddRules(NoTrumpResponses(ps));

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

        public static IEnumerable<BidRule> Rebid(PositionState ps)
        {
            var bids = new List<BidRule>
            {
                DefaultPartnerBids(Call.Double, OpenBid3.ThirdBid),

                // Opener could have bid 1S.  Support at the right level...
                Nonforcing(Bid.TwoSpades, RaisePartner(), Points(MinimumHand)),
                Nonforcing(Bid.ThreeSpades, RaisePartner(2), Points(MediumHand)),
                Signoff(Bid.FourSpades, RaisePartner(3), Points(RaiseTo4M)),

                Nonforcing(Bid.TwoClubs, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoDiamonds, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoHearts, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoSpades, Shape(6, 11), Points(MinimumHand)),


				// TODO: Make these dependent on pair points.
                Invitational(Bid.ThreeClubs, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeDiamonds, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeHearts, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeSpades, Shape(6, 11), Points(MediumHand)),


                Nonforcing(Bid.OneNoTrump, Points(MinimumHand)),
                
             /// TODO: MORE PASSING MORE OFTEN...   Signoff(Call.Pass, Points(MinimumHand), ForcedToBid(false), )
                Signoff(Bid.TwoClubs, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoDiamonds, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoHearts, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoSpades, Fit(), ForcedToBid(), Points(MinimumHand)),

                Signoff(Bid.ThreeClubs, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeDiamonds, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeHearts, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeSpades, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand))


            };
            bids.AddRange(Compete.CompBids(ps));
            return bids;
        }

        public static IEnumerable<BidRule> OpenerInvitedGame(PositionState ps)
        {
            var bids = new List<BidRule>()
            {
                Signoff(Bid.FourHearts, Fit(), PairPoints(PairGame)),
                Signoff(Bid.FourSpades, Fit(), PairPoints(PairGame))
            };
            // TODO: Competative bids here too?  Seems silly since restricted raise
            return bids;
        }
    }
}
