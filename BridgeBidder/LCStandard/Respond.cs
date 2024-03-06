using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Xml.Serialization;


namespace BridgeBidding
{
    public class Respond : LCStandard
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
                    bids.Add(Signoff(new Bid (2, suit), SingleJump, Points(WeakJumpShiftPoints), Shape(6, 10), DecentPlusSuit));
                    bids.Add(Signoff(new Bid (3, suit), SingleJump, Points(WeakJumpShiftPoints), Shape(6, 10), DecentPlusSuit));
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
                    PartnerBids(Bid._2C,   OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._3C, OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._4C,  OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._5C,  OpenBid2.ResponderRaisedMinor),

                
                    Nonforcing(Bid._1D, Points(Respond1Level), Shape(5, 10), LongestMajor(3)),

                    Nonforcing(Bid._1H, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                    Nonforcing(Bid._1H, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                    Nonforcing(Bid._1S, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                    // TODO: Inverted minors...
                    Invitational(Bid._2C, ShowsTrump, Points(Raise1), Shape(5), LongestMajor(3)),
                    Invitational(Bid._3C, ShowsTrump, Points(LimitRaise), Shape(5), LongestMajor(3)),                
                    Signoff(Bid._5C, ShowsTrump, Points(Weak5Level), Shape(7, 10)),
                    Signoff(Bid._4C, ShowsTrump, Points(Weak4Level), Shape(6)),
                });
            }
            else
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),
                    PartnerBids(Bid._2C,   OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._3C, OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._4C,  OpenBid2.ResponderRaisedMinor),
                    PartnerBids(Bid._5C,  OpenBid2.ResponderRaisedMinor),

                    Forcing(Bid._1D, Points(Respond1Level), Shape(5, 10), LongestMajor(3)),
                    // TODO: Should we bid "up the line" with 11+ points?
                    Forcing(Bid._1D, Points(LimitRaiseOrBetter), Shape(5, 10), LongerThan(Suit.Hearts), LongerThan(Suit.Spades)),

                    Forcing(Bid._1H, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                    Forcing(Bid._1H, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                    Forcing(Bid._1S, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                    // TODO: Inverted minors...
                    Invitational(Bid._2C, ShowsTrump, Points(Raise1), Shape(5), LongestMajor(3)),
                    Invitational(Bid._3C, ShowsTrump, Points(LimitRaise), Shape(5), LongestMajor(3)),                
                    Signoff(Bid._5C, ShowsTrump, Points(Weak5Level), Shape(7, 10)),
                    Signoff(Bid._4C, ShowsTrump, Points(Weak4Level), Shape(6)),
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
				PartnerBids(Bid._2D,   OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid._3D, OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid._4D,  OpenBid2.ResponderRaisedMinor),
				PartnerBids(Bid._5D,  OpenBid2.ResponderRaisedMinor),

                // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                // with game going values even if we have a 4 card major.
                ForcingToGame(Bid._2C, Points(GameOrBetter), Shape(4, 10), LongestMajor(4)),

                Forcing(Bid._1H, Points(Respond1Level), Shape(4), Shape(Suit.Spades, 0, 4)),
                Forcing(Bid._1H, Points(Respond1Level), Shape(5, 10), LongerThan(Suit.Spades)),

                Forcing(Bid._1S, Points(Respond1Level), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Inverted minors...
                Invitational(Bid._2D, ShowsTrump, Points(Raise1), Shape(5), LongestMajor(3)),

				Invitational(Bid._3D, ShowsTrump, Points(LimitRaise), Shape(5), LongestMajor(3)),
                
                Signoff(Bid._5D, ShowsTrump, Points(Weak5Level), Shape(7, 10)),

                Signoff(Bid._4D, ShowsTrump, Points(Weak4Level), Shape(6)),

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
                    PartnerBids(Bid._2H,   OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._3H, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._4H,  OpenBid2.ResponderRaisedMajor),


                    Invitational(Bid._2H, DummyPoints(Raise1), Shape(3, 5)),
                    Invitational(Bid._3H, DummyPoints(MediumHand), Shape(3, 5)),
                    // TODO: When would we bid 4?
                    Signoff(Bid._4H, Points(Weak4Level), Shape(5, 10)),
                
                    Nonforcing(Bid._1S, Shape(4, 10), Points(Respond1Level), Shape(Suit.Hearts, 0, 3)),

                    // TODO: Rules for best suit if 5-5
                    Invitational(Bid._2C, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid._2D, Points(MaxPassed), Shape(5, 10)),
                  // TODO: Where  PartnerBids(Bid._1NT, OpenBid2.OneNTOverMajorOpen),
                    // TODO: Points range name wrong!
                
                    Invitational(Bid._1NT, Points(6, 10), Shape(Suit.Hearts, 0, 2)),
                    Invitational(Bid._2NT, Points(11, 12), Shape(Suit.Hearts, 0, 2)),
                });
            }
            else 
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(Jacoby2NT.InitiateConvention);
                choices.AddRules(new CallFeature[]
                {
                    PartnerBids(OpenBid2.ResponderChangedSuits),
                    PartnerBids(Bid._2H, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._3H, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._4H, OpenBid2.ResponderRaisedMajor),

                    // TODO: LARRY CONFIRM - With 2/1 game force values bid a minor with 4 spades. 
                    // more spades bid spades regardless of points
                    // TODO: If we have a fit skip spades even with 5????  Always show 5? 

                    ForcingToGame(Bid._2C, Points(GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid._2C, Points(GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid._2C, DummyPoints(Suit.Hearts, GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid._2C, DummyPoints(Suit.Hearts, GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),

                    ForcingToGame(Bid._2D, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),
                    ForcingToGame(Bid._2D, DummyPoints(Suit.Hearts, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),


                    Invitational(Bid._2H, DummyPoints(Raise1), Shape(3, 5)),
                    Invitational(Bid._3H, DummyPoints(MediumHand), Shape(4, 5)),
                    // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                    // TODO: LARRY - we have discussed 3-card limit raises.  Should we do that?
                    Signoff(Bid._4H, Points(Weak4Level), Shape(5, 10)),

                    Forcing(Bid._1S, Points(Respond1Level), Shape(4, 10), Shape(Suit.Hearts, 0, 3)),

                    PartnerBids(Bid._1NT, OpenBid2.SemiForcingNT),
                    Announce(Bid._1NT, UserText.SemiForcing),
                    Semiforcing(Bid._1NT, Points(Respond1NTOverMajor), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),
                    
                    // TODO: Any follow-up to this?  Slam? - or just use compete logic?  
                    Signoff(Bid._3NT, Flat(), Points(RaiseTo3NT))
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
                    PartnerBids(Bid._2S, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._3S, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._4S, OpenBid2.ResponderRaisedMajor),



                    Invitational(Bid._2S, DummyPoints(Raise1), Shape(3, 5)),
                    Invitational(Bid._3S, DummyPoints(MediumHand), Shape(3, 5)),
                    // TODO: When would we bid 4?
                    Signoff(Bid._4S, Points(Weak4Level), Shape(5, 10)),
                
                  // TODO: Where  PartnerBids(Bid._1NT, OpenBid2.OneNTOverMajorOpen),
                    // TODO: Points range name wrong!
                    // TODO: Rules for best suit if 5-5
                    Invitational(Bid._2C, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid._2D, Points(MaxPassed), Shape(5, 10)),
                    Invitational(Bid._2H, Points(MaxPassed), Shape(5, 10)),

                    Invitational(Bid._1NT, Points(6, 10),  Shape(Suit.Spades, 0, 2)),
                    Invitational(Bid._2NT, Points(11, 12), Shape(Suit.Spades, 0, 2)),
                });
            }
            else 
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(Jacoby2NT.InitiateConvention);
                choices.AddRules(
                    PartnerBids(OpenBid2.ResponderChangedSuits),

                    PartnerBids(Bid._2S, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._3S, OpenBid2.ResponderRaisedMajor),
                    PartnerBids(Bid._4S, OpenBid2.ResponderRaisedMajor),

                    // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                    // with game going values even if we have 4 spades.
                    // TODO: Is this right? 
                    // TODO: Is BetterMinor() logic right?  Choose clubs over diamonds if both less than 4
                    // Is this right to ever bid with less than 4?

                    ForcingToGame(Bid._2C, Points(GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid._2C, Points(GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid._2C, DummyPoints(Suit.Spades, GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid._2C, DummyPoints(Suit.Spades, GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),

                    ForcingToGame(Bid._2D, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),
                    ForcingToGame(Bid._2D, DummyPoints(Suit.Spades, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),

                    ForcingToGame(Bid._2H, Shape(5, 10), Points(GameOrBetter)),

                    Invitational(Bid._2S, DummyPoints(Raise1), Shape(3, 5)),
                    Invitational(Bid._3S, DummyPoints(MediumHand), Shape(4, 5)),
                    // NOTE: Medium hand with 3-card support will be handled with 1NT followed by raise...
                    // LARRY: Same issue here - 3-card supprot limit raises or 1NT relay?
                    Signoff(Bid._4S, Points(Weak4Level), Shape(5, 10)),
                
                    PartnerBids(Bid._1NT, OpenBid2.SemiForcingNT),
                    Announce(Bid._1NT, UserText.SemiForcing),
                    Semiforcing(Bid._1NT, Points(Respond1NTOverMajor), Shape(Suit.Spades, 0, 3)),

                    // TODO: Partner bids for this- slam or show 4M
                    Signoff(Bid._3NT, Flat(), Points(RaiseTo3NT))
                );
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
            {
                return OppsDoubled(ps, openSuit);
            }
            else
            {
                if (ps.RHO.Bid.Suit is Suit suit)
                {
                    return OppsOvercalledSuit(ps, openSuit, ps.RHO.Bid.Level, suit);
                }
                return OppsOvercalledNT(ps, openSuit, ps.RHO.Bid.Level);
            }
        }
 
        // TODO: Actually do something intelligent here...
        public static PositionCalls OppsOvercalledNT(PositionState ps, Suit openSuit, int rhoBidLevel)
        {
            return ps.PairState.BiddingSystem.GetPositionCalls(ps);
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
                Signoff(Bid._4H, Fit(), RuleOf17()),
                Signoff(Bid._4H, Fit(10)),
                Signoff(Bid._4S, Fit(), RuleOf17()),
                Signoff(Bid._4S, Fit(10)),
				// TODO: Pass???

                Signoff(Bid._3D, Fit(9)),
                Signoff(Bid._3H,   Fit(9)),
                Signoff(Bid._3S,   Fit(9)),

				// TODO: NT Bids
				// TODO: Minor bids???
                
                Signoff(Call.Pass)

			};
        }


     
        public static PositionCalls OppsOvercalledSuit(PositionState ps, Suit openSuit, int rhoBidLevel, Suit rhoBidSuit)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(SolidSuit.Bids);
            choices.AddRules(NegativeDouble.InitiateConvention);
            choices.AddRules(WeakJumpShift(openSuit));

            var raisePartner = ps.BiddingState.Contract.NextAvailableBid(openSuit);
            var limitRaise = new Bid(rhoBidLevel + 1, rhoBidSuit);
            var weakRaise = new Bid(raisePartner.Level + 1, openSuit);
            CallFeaturesFactory raiseHandler =  OpenBid2.ResponderRaisedMajor;
            if (openSuit.IsMinor())
            {
                raiseHandler = OpenBid2.ResponderRaisedMinor;
            }
            List<Suit> suits = new List<Suit> { Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades };
            suits.Remove(openSuit);
            suits.Remove(rhoBidSuit);
            var lowerUnbid = suits.First();
            var higherUnbid = suits.Last();
            var bidNew1 = ps.BiddingState.Contract.NextAvailableBid(lowerUnbid);
            var bidNew2 = ps.BiddingState.Contract.NextAvailableBid(higherUnbid);


            choices.AddRules(
                // TODO: Perhaps ResponderChangedSuitsInComp is better here?
                PartnerBids(OpenBid2.ResponderChangedSuits),
                PartnerBids(raisePartner, raiseHandler),
                PartnerBids(limitRaise, raiseHandler),
                PartnerBids(weakRaise, raiseHandler),
                PartnerBids(Bid._1NT, OpenBid2.ResponderBidNT),
                PartnerBids(Bid._2NT, OpenBid2.ResponderBidNT),
                PartnerBids(Bid._3NT, OpenBid2.ResponderBidNT),

                // Negative double may have made these bids irrelevant
                Forcing(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),
                
                Nonforcing(raisePartner, Fit(), DummyPoints(Raise1)),
                Forcing(limitRaise, Fit(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Nonforcing(weakRaise, Fit(9), DummyPoints(WeakJumpRaise)),
            
                Nonforcing(Bid._1NT, OppsStopped(), Points(Raise1)),
                Nonforcing(Bid._2NT, OppsStopped(), Points(11, 12)),
                // TODO: Still lots more...  3NT.  Bid majors first if they must be bid at 2-level etc.

                Forcing(bidNew1, Shape(4), Shape(higherUnbid, 0, 4), Points(NewSuit2Level)),
                Forcing(bidNew2, Shape(5, 10), LongerThan(higherUnbid), Points(NewSuit2Level)),
                Forcing(bidNew2, Shape(4, 10), Shape(lowerUnbid, 0, 3), Points(NewSuit2Level)),

                PartnerBids(Call.Pass, OpenBid2.ResponderPassedInCompetition),
                Signoff(Bid.Pass)  // May have enought points to respond but no good call, so can't specify points
                );
/*
            choices.AddRules(new CallFeature[]
            {
                PartnerBids(OpenBid2.ResponderBidInCompetition),

                // Negative double may have made these bids irrelevant
                Forcing(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(SolidSuit.Bids);
            choices.AddRules(NegativeDouble.InitiateConvention);
            choices.AddRules(WeakJumpShift(openSuit));
            var oppsSuit = rhoBid.Suit;
            var raisePartner = new Bid(2, openSuit);
            var limitRaise = new Bid(2, (Suit)oppsSuit);

            choices.AddRules(new CallFeature[]
            {
                PartnerBids(OpenBid2.ResponderBidInCompetition),

                // Negative double may have made these bids irrelevant
                Forcing(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Forcing(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Forcing(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

      
                // TODO: Perhaps show new 5+ card suit forcing here?  Only if not passed.

				// Now cuebid raises are next in priority - RaisePartner calls ShowTrump()
                Forcing(Bid._2D, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Forcing(Bid._2H, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Forcing(Bid._2S, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Forcing(Bid._3C, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),

                // TODO: Weak jumps here take precedence over simple raise
               
				Nonforcing(Bid._3H, Fit(9), SingleJump, DummyPoints(WeakJumpRaise)),
                Nonforcing(Bid._3S, Fit(9), SingleJump, DummyPoints(WeakJumpRaise)),


                // Now time for invitational bids.
                Invitational(Bid._2C, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid._2C, OppsStopped(false), NotCueBid, RaisePartner(fit: 7), DummyPoints(Raise1)),

                Invitational(Bid._2D, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid._2D, OppsStopped(false), NotCueBid, RaisePartner(fit: 7), DummyPoints(Raise1)),

                Invitational(Bid._2H, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Invitational(Bid._2S, NotCueBid, RaisePartner(), DummyPoints(Raise1)),

				// TODO: Still need lots and lots more bid levels here.  But decent start...
		
				// TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function

				Signoff(Bid._4H, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),
                Signoff(Bid._4S, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),

                Nonforcing(Bid._1NT, OppsStopped(), Points(Raise1)),
                Nonforcing(Bid._2NT, OppsStopped(), Points(LimitRaise)),

                PartnerBids(Call.Pass, OpenBid2.ResponderPassedInCompetition),
                Signoff(Bid.Pass),  // May have enought points to respond but no good call, so can't specify points.

            });
            // TODO: Need to have opponents stopped?  Maybe those bids go higher up ...
// TODO: NO TRUMP...            choices.AddRules(NoTrumpResponses(ps));
*/
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

				Nonforcing(Bid._1H, Points(RespondX1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Nonforcing(Bid._1H, Points(RespondX1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Nonforcing(Bid._1S, Points(RespondX1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Nonforcing(Bid._1S, Points(RespondX1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                Nonforcing(Bid._1D, Shape(4, 11), Points(RespondX1Level)),

                //
                // If we have a good fit but a week hand then time to jump.
                //
                Nonforcing(new Bid(3, openSuit), Fit(9), ShowsTrump, Points(RespondXJump)),
                
                Nonforcing(Bid._2C, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid._2C, Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid._2D, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid._2D, NonJump, Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid._2H, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),
                Nonforcing(Bid._2H, NonJump, Shape(5, 11), Points(RespondX1Level)),

                Nonforcing(Bid._2S, Partner(HasShownSuit()), Fit(), Points(RespondX1Level)),

				// TODO: Perhaps higer priority than raise of a minor???
                Nonforcing(Bid._1NT, Points(RespondX1Level)),

                // TODO: Is this correct about RespondPass points?  Are there scenerios where you just don't have
                // the right shape, but do have enought points?  I guess we'd always bid 1NT
                Signoff(Bid.Pass, Points(RespondPass))

            });

            return choices;
        }

    }
}
