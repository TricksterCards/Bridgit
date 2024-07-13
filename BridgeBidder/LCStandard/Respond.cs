using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Xml.Serialization;


namespace BridgeBidding
{
    public class Respond : LCStandard
    {

        static readonly HandConstraint PointsPass = Points(0, 5);
        static protected (int, int) Respond1Level = (6, 40);
        static readonly HandConstraint DummyPointsRaise1 = DummyPoints(6, 10);
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
    


        private static PositionCalls MinorOpen(PositionState ps, Suit openSuit, CallFeaturesFactory unpassedCalls, CallFeaturesFactory passedCalls)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, openSuit);
            var choices = new PositionCalls(ps);
            if (ps.IsPassedHand)
            {
                choices.AddRules(passedCalls);
            }
            else
            {
                choices.AddRules(SolidSuit.Bids);
                choices.AddRules(unpassedCalls);
            }
            choices.AddRules(NoTrumpResponsesToMinor(openSuit));
            choices.AddRules(WeakJumpShift(openSuit));
            choices.AddPassRule(PointsPass);
            return choices;
        }

        // Responses to 1 Club open.

        public static PositionCalls OneClub(PositionState ps)
        {
            return MinorOpen(ps, Suit.Clubs, OneClubUnpassedHand, OneClubPassedHand);
        }

        private static CallFeature[] OneClubUnpassedHand(PositionState _)
        {      
            return new CallFeature[]
            {
                Properties(new[] { Bid._1D, Bid._1H, Bid._1S }, OpenBid2.ResponderChangedSuits, forcing1Round: true),

                Shows(Bid._1D, Points(6, 40), Shape(5, 10), LongestMajor(3)),
                Shows(Bid._1D, Points(11, 40), Shape(4), LongestMajor(4)),
                Shows(Bid._1D, Points(11, 40), Shape(5, 10), LongerThan(Suit.Hearts), LongerThan(Suit.Spades)),

                Shows(Bid._1H, Points(6, 40), Shape(4), Shape(Suit.Spades, 0, 4)),
                Shows(Bid._1H, Points(6, 40), Shape(5, 10), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(6, 40), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Should these be based on dummy points?  Seems dangerous...
                Properties(Bid._2C, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorStrong),
                Properties(Bid._3C, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorWeak),
                Shows(Bid._2C, Points(10, 40), Shape(5, 10), LongestMajor(3)),
                Shows(Bid._3C, Points(0, 7), Shape(5, 10), LongestMajor(3)),
            };
        }

        private static CallFeature[] OneClubPassedHand(PositionState _)
        {
            // TODO: Do we always want to show a 4 card major first, even if we have a 5 card minor?  With 11 points???
            return new CallFeature[]
            {
                Properties(new[] { Bid._1D, Bid._1H, Bid._1S }, OpenBid2.ResponderChangedSuits, forcing1Round: false),

                Shows(Bid._1D, Points(6, 11), Shape(5, 10), LongestMajor(3)),

                Shows(Bid._1H, Points(6, 11), Shape(4), Shape(Suit.Spades, 0, 4)),
                Shows(Bid._1H, Points(6, 11), Shape(5, 10), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(6, 11), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Should these be based on dummy points?  Seems dangerous...
                Properties(Bid._2C, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorStrong),
                Properties(Bid._3C, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorWeak),
                Shows(Bid._2C, Points(10, 40), Shape(5, 10), LongestMajor(3)),
                Shows(Bid._3C, Points(0, 7), Shape(5, 10), LongestMajor(3))           
            };
        }


        // Responses to 1 Diamond open. 
        public static PositionCalls OneDiamond(PositionState ps)
        {
            return MinorOpen(ps, Suit.Diamonds, OneDiamondUnpassedHand, OneDiamondPassedHand);
        }


        private static CallFeature[] OneDiamondUnpassedHand(PositionState _)
        {      
            return new CallFeature[]
            {
                // TODO: When do we show the major?  How many clubs do we need to have?  Should this take priority over
                // inverted minors if we have a minor fit?  
                Properties(Bid._2C, OpenBid2.TwoOverOne, forcingToGame: true),
                Shows(Bid._2C, Points(12, 40), Shape(4, 10), LongestMajor(4)),

                Properties(new[] { Bid._1H, Bid._1S }, OpenBid2.ResponderChangedSuits, forcing1Round: true),

                Shows(Bid._1H, Points(6, 40), Shape(4), Shape(Suit.Spades, 0, 4)),
                Shows(Bid._1H, Points(6, 40), Shape(5, 10), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(6, 40), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Should these be based on dummy points?  Seems dangerous...
                Properties(Bid._2D, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorStrong),
                Properties(Bid._3D, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorWeak),
                Shows(Bid._2D, Points(10, 40), Shape(5, 10), LongestMajor(3)),
                Shows(Bid._3D, Points(0, 7), Shape(5, 10), LongestMajor(3)),
            };
        }

        private static CallFeature[] OneDiamondPassedHand(PositionState _)
        {
            // TODO: Do we always want to show a 4 card major first, even if we have a 5 card minor?  With 11 points???
            return new CallFeature[]
            {
                Properties(new[] { Bid._1H, Bid._1S }, OpenBid2.ResponderChangedSuits, forcing1Round: false),

                Shows(Bid._1H, Points(6, 11), Shape(4), Shape(Suit.Spades, 0, 4)),
                Shows(Bid._1H, Points(6, 11), Shape(5, 10), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(6, 11), Shape(4, 10), LongerOrEqualTo(Suit.Hearts)),

                // TODO: Should these be based on dummy points?  Seems dangerous...
                Properties(Bid._2D, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorStrong),
                Properties(Bid._3D, OpenBid2.ResponderRaisedMinor, agreeTrump: true, alert: UserText.InvertedMinorWeak),
                Shows(Bid._2D, Points(10, 40), Shape(5, 10), LongestMajor(3)),
                Shows(Bid._3D, Points(0, 7), Shape(5, 10), LongestMajor(3))           
            };
        }



        // ********** MAJOR SUIT OPENINGS ****************
        private static PositionCalls MajorOpen(PositionState ps, Suit openSuit, CallFeaturesFactory unpassedCalls, CallFeaturesFactory passedCalls)
        {
            if (!ps.RHO.Passed)
                return OppsInterferred(ps, openSuit);

            var choices = new PositionCalls(ps);
            if (ps.IsPassedHand)
            {
                choices.AddRules(Drury.InitiateConvention(ps, openSuit));
                choices.AddRules(passedCalls);
            }
            else
            {
                choices.AddRules(SolidSuit.Bids);
                // Splinters should come before Jacoby 2NT.  They are limited in point range.
                choices.AddRules(Splinter.InitiateConvention(ps, openSuit));
                choices.AddRules(Jacoby2NT.InitiateConvention);
                choices.AddRules(unpassedCalls);
            }
            choices.AddRules(WeakJumpShift(openSuit));
            choices.AddPassRule(Points(0, 5));
            return choices;
        }

        public static PositionCalls OneHeart(PositionState ps)
        {
            return MajorOpen(ps, Suit.Hearts, OneHeartUnpassedHand, OneHeartPassedHand);
        }


        private static CallFeature[] OneHeartUnpassedHand(PositionState _)
        {
            return new CallFeature[] {
                    PartnerBids(OpenBid2.ResponderChangedSuits),

                    // TODO: LARRY CONFIRM - With 2/1 game force values bid a minor with 4 spades. 
                    // more spades bid spades regardless of points
                    // TODO: If we have a fit skip spades even with 5????  Always show 5? 
                    Properties(new Bid[] { Bid._2C, Bid._2D }, OpenBid2.TwoOverOne, forcingToGame: true),

                    Shows(Bid._2C, Points(GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    Shows(Bid._2C, Points(GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    Shows(Bid._2C, DummyPoints(Suit.Hearts, GameOrBetter), LongerThan(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),
                    Shows(Bid._2C, DummyPoints(Suit.Hearts, GameOrBetter), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Spades, 0, 4)),

                    Shows(Bid._2D, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),
                    Shows(Bid._2D, DummyPoints(Suit.Hearts, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Spades, 0, 4)),


                // What about the hole where we have 3 hearts and a limit raise?  1NT then 2H.  Need to make sure that
                // logic is correct. 
                    Properties(new Bid[] { Bid._2H, Bid._3H, Bid._4H }, OpenBid2.ResponderRaisedMajor, agreeTrump: true),
                    
                    Shows(Bid._2H, DummyPointsRaise1, Shape(3, 5)),
                    Shows(Bid._3H, DummyPoints(MediumHand), Shape(4, 5)),
                    Shows(Bid._4H, Points(Weak4Level), Shape(5, 10)),

                    Properties(Bid._1S, forcing1Round: true),
                    Shows(Bid._1S, Points(Respond1Level), Shape(4, 10), Shape(Suit.Hearts, 0, 3)),

                    Properties(Bid._1NT, partnerBids: OpenBid2.SemiForcingNT, announce: UserText.SemiForcing),
                    Shows(Bid._1NT, Points(Respond1NTOverMajor), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),
                    
                    // TODO: Any follow-up to this?  Slam? - or just use compete logic?  
                    Shows(Bid._3NT, Flat, Points(RaiseTo3NT))
            };
        }
        private static CallFeature[] OneHeartPassedHand(PositionState _)
        {
            return new CallFeature[]
            {
                // Drury artifical bid of 2C will take care of any raise > 2H
                Properties(Bid._2H, OpenBid2.ResponderRaisedMajor, agreeTrump: true),
                Shows(Bid._2H, DummyPointsRaise1, Shape(3, 7)),
            
                Shows(Bid._1S, Shape(4, 10), Points(Respond1Level), Shape(Suit.Hearts, 0, 3)),

                Properties(Bid._2D, OpenBid2.ResponderChangedSuits, forcing1Round: false),
                Shows(Bid._2D, Points(11, 11), Shape(5, 10)),

                // TODO: Ever bid 2NT here?  Under what circumstances?  Long runnins source of tricks?            
                Properties(Bid._1NT, OpenBid2.PassedHandResponded1NT, forcing1Round: false),
                Shows(Bid._1NT, Points(6, 11),  Shape(Suit.Hearts, 0, 2), Shape(Suit.Spades, 0, 3)),
            };
        }

        public static PositionCalls OneSpade(PositionState ps)
        {
            return MajorOpen(ps, Suit.Spades, OneSpadeUnpassedHand, OneSpadePassedHand);
        }


        private static CallFeature[] OneSpadeUnpassedHand(PositionState _)
        {
            return new CallFeature[] 
            {

                PartnerBids(OpenBid2.ResponderChangedSuits),

                // 2/1 game force is the highet priority if we can make it.  It is OK to bid this
                // with game going values even if we have 4 spades.
        
                Properties(new Bid[] { Bid._2C, Bid._2D, Bid._2H }, partnerBids: OpenBid2.TwoOverOne, forcingToGame: true),
                
                Shows(Bid._2C, Points(12, 40), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                Shows(Bid._2C, Points(12, 40), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                Shows(Bid._2C, DummyPoints(Suit.Spades, 12, 40), LongerThan(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),
                Shows(Bid._2C, DummyPoints(Suit.Spades, 12, 40), Shape(4), LongerOrEqualTo(Suit.Diamonds), Shape(Suit.Hearts, 0, 4)),

                Shows(Bid._2D, Points(GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),
                Shows(Bid._2D, DummyPoints(Suit.Spades, GameOrBetter), LongerOrEqualTo(Suit.Clubs), Shape(Suit.Hearts, 0, 4)),

                Shows(Bid._2H, Shape(5, 10), Points(GameOrBetter)),

                Properties(new Bid[] { Bid._2S, Bid._3S, Bid._4S }, OpenBid2.ResponderRaisedMajor, agreeTrump: true),

                Shows(Bid._2S, DummyPoints(6, 10), Shape(3, 5)),
                Shows(Bid._3S, DummyPoints(11, 12), Shape(4, 5)),
                Shows(Bid._4S, Points(Weak4Level), Shape(5, 10)),
            
                Properties(Bid._1NT, OpenBid2.SemiForcingNT, announce: UserText.SemiForcing),
                Shows(Bid._1NT, Points(Respond1NTOverMajor), Shape(Suit.Spades, 0, 3)),

                // TODO: Partner bids for this- slam or show 4M
                Shows(Bid._3NT, Flat, Points(RaiseTo3NT))
            };
        }

        private static CallFeature[] OneSpadePassedHand(PositionState _)
        {
            return new CallFeature[] 
            {
                // TODO: Is this OK
                PartnerBids(OpenBid2.ResponderChangedSuits),

                Properties(new Bid[] { Bid._2S, Bid._3S, Bid._4S} , OpenBid2.ResponderRaisedMajor, agreeTrump: true),

                Shows(Bid._2S, DummyPoints(6, 10),  Shape(3, 5)),
                Shows(Bid._3S, DummyPoints(11, 11), Shape(3, 5)),
                Shows(Bid._4S, Points(4, 11), Shape(5, 10)),
            
                Shows(Bid._2C, Points(10, 11), Shape(5, 10)),
                Shows(Bid._2D, Points(10, 11), Shape(5, 10)),
                Shows(Bid._2H, Points(10, 11), Shape(5, 10)),

                Shows(Bid._1NT, Points(6, 11),  Shape(Suit.Spades, 0, 2)),
                // TODO: Ever bid 2NT here?
            };
        }
        private static IEnumerable<CallFeature> NTResponseToMinor(Suit minor, int level, (int, int) pointRange, PositionCallsFactory partnerBids)
        {
            var bid = new Bid(level, Strain.NoTrump);
            var rule = Shows(bid, Points(pointRange), Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3));
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



        public static IEnumerable<CallFeature> WeakJumpShift(Suit openSuit)
        {
            var bids = new List<CallFeature>();
            foreach (var suit in Card.Suits)
            {
                if (suit != openSuit)
                {
                    bids.Add(Shows(new Bid (2, suit), IsSingleJump, Points(WeakJumpShiftPoints), Shape(6, 10), DecentPlusSuit));
                    bids.Add(Shows(new Bid (3, suit), IsSingleJump, Points(WeakJumpShiftPoints), Shape(6, 10), DecentPlusSuit));
                }
            }
            return bids;
        }



        // *************************** END OF 2/1 - OLD SAYC CODE ********************************


        public static IEnumerable<CallFeature> ForthSeat2Open(PositionState ps)
        {
            return new CallFeature[]
            {
                // TODO: What are the right bids after a strongish 2-level open?
                Shows(Call.Pass)
            };
        }

        public static IEnumerable<CallFeature> WeakOpen(PositionState ps)
        {
            return new CallFeature[]
            {

                // TODO: Artificial inquiry 2NT...
                Shows(Bid._4H, Fit8Plus, RuleOf17()),
                Shows(Bid._4H, Fit(10)),
                Shows(Bid._4S, Fit8Plus, RuleOf17()),
                Shows(Bid._4S, Fit(10)),
				// TODO: Pass???

                Shows(Bid._3D, Fit9Plus),
                Shows(Bid._3H, Fit9Plus),
                Shows(Bid._3S, Fit9Plus),

				// TODO: NT Bids
				// TODO: Minor bids???
                
                Shows(Call.Pass)

			};
        }


     
        public static PositionCalls OppsOvercalledSuit(PositionState ps, Suit openSuit, int rhoBidLevel, Suit rhoBidSuit)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(SolidSuit.Bids);
            choices.AddRules(NegativeDouble.InitiateConvention);
            choices.AddRules(WeakJumpShift(openSuit));

            var raisePartner = ps.BiddingState.Contract.NextAvailableBid(openSuit);
            var cueBidRaise = new Bid(rhoBidLevel + 1, rhoBidSuit);
            var weakRaise = new Bid(raisePartner.Level + 1, openSuit);
            var weakFit = openSuit.IsMinor() ? Fit8Plus : Fit9Plus;
            PositionCallsFactory raiseHandler = OpenBid2.ResponderRaisedMajor;
            if (openSuit.IsMinor())
            {
                raiseHandler = OpenBid2.ResponderRaisedMinor;
            }
            List<Suit> suits = new List<Suit>(Card.Suits);
            suits.Remove(openSuit);
            suits.Remove(rhoBidSuit);
            var lowerUnbid = suits.First();
            var higherUnbid = suits.Last();
            var bidNew1 = ps.BiddingState.Contract.NextAvailableBid(lowerUnbid);
            var bidNew2 = ps.BiddingState.Contract.NextAvailableBid(higherUnbid);
            bool newSuitForcing = !ps.IsPassedHand;

            choices.AddRules(
                // TODO: Perhaps ResponderChangedSuitsInComp is better here?
                PartnerBids(OpenBid2.ResponderChangedSuits),
               
                Properties(new[] { Bid._1NT, Bid._2NT, Bid._3NT }, OpenBid2.ResponderBidNT),

                // Negative double may have made these bids irrelevant
           // TODO: Duplicates new1 and new2     Properties(Bid._1H, OpenBid2.ResponderChangedSuits, forcing1Round: true),
                Shows(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

          // TODO: New1 and New2 do this...  Redundant logic      Properties(Bid._1S, OpenBid2.ResponderChangedSuits, forcing1Round: true),
                Shows(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                Properties(new Bid[] { raisePartner, weakRaise }, raiseHandler, trump: openSuit),
                Properties(cueBidRaise, raiseHandler, forcing1Round: true, trump: openSuit), 
                Shows(raisePartner, Fit8Plus, DummyPointsRaise1),
                Shows(cueBidRaise, Fit(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Shows(weakRaise, weakFit, DummyPoints(WeakJumpRaise)),
            
                Shows(Bid._1NT, OppsStopped, Points(6, 10)),
                Shows(Bid._2NT, OppsStopped, Points(11, 12)),
                // TODO: Still lots more...  3NT.  Bid majors first if they must be bid at 2-level etc.

                Properties(new Bid[] { bidNew1, bidNew2 }, OpenBid2.ResponderChangedSuits, forcing1Round: newSuitForcing),
                Shows(bidNew1, Shape(4), Shape(higherUnbid, 0, 4), Points(NewSuit2Level)),
                Shows(bidNew2, Shape(5, 10), LongerThan(higherUnbid), Points(NewSuit2Level)),
                Shows(bidNew2, Shape(4, 10), Shape(lowerUnbid, 0, 3), Points(NewSuit2Level)),

                PartnerBids(Call.Pass, OpenBid2.ResponderPassedInCompetition),
                Shows(Bid.Pass)  // May have enought points to respond but no good call, so can't specify points
                );
/*
            choices.AddRules(new CallFeature[]
            {
                PartnerBids(OpenBid2.ResponderBidInCompetition),

                // Negative double may have made these bids irrelevant
                Shows(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),
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
                Shows(Bid._1H, Points(Respond1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._1H, Points(Respond1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(Respond1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid._1S, Points(Respond1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

      
                // TODO: Perhaps show new 5+ card suit forcing here?  Only if not passed.

				// Now cuebid raises are next in priority - RaisePartner calls ShowTrump()
                Shows(Bid._2D, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Shows(Bid._2H, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Shows(Bid._2S, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),
                Shows(Bid._3C, CueBid, RaisePartner(openSuit), DummyPoints(openSuit, LimitRaiseOrBetter)),

                // TODO: Weak jumps here take precedence over simple raise
               
				Shows(Bid._3H, Fit9Plus, SingleJump, DummyPoints(WeakJumpRaise)),
                Shows(Bid._3S, Fit9Plus, SingleJump, DummyPoints(WeakJumpRaise)),


                // Now time for invitational bids.
                Shows(Bid._2C, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Shows(Bid._2C, OppsStopped(false), NotCueBid, RaisePartner(fit: 7), DummyPoints(Raise1)),

                Shows(Bid._2D, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Shows(Bid._2D, OppsStopped(false), NotCueBid, RaisePartner(fit: 7), DummyPoints(Raise1)),

                Shows(Bid._2H, NotCueBid, RaisePartner(), DummyPoints(Raise1)),
                Shows(Bid._2S, NotCueBid, RaisePartner(), DummyPoints(Raise1)),

				// TODO: Still need lots and lots more bid levels here.  But decent start...
		
				// TODO: This is all common wacky bids from thsi point on.  Need to append at the bottom of this function

				Shows(Bid._4H, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),
                Shows(Bid._4S, RaisePartner(raise: 3, fit: 10), DummyPoints(Weak4Level)),

                Shows(Bid._1NT, OppsStopped, Points(Raise1)),
                Shows(Bid._2NT, OppsStopped, Points(LimitRaise)),

                PartnerBids(Call.Pass, OpenBid2.ResponderPassedInCompetition),
                Shows(Bid.Pass),  // May have enought points to respond but no good call, so can't specify points.

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
                // TODO: Need partner bids here.  Not finished at all.
                Properties(Call.Redouble, forcing1Round: true),
                Shows(Call.Redouble, Points(RespondRedouble)),
				// TODO: Here we need to make all bids reflect that they are less than 10 points...

				Shows(Bid._1H, Points(RespondX1Level), Shape(4), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._1H, Points(RespondX1Level), Shape(5, 11), LongerThan(Suit.Spades)),

                Shows(Bid._1S, Points(RespondX1Level), Shape(4), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid._1S, Points(RespondX1Level), Shape(5, 11), LongerOrEqualTo(Suit.Hearts)),

                Shows(Bid._1D, Shape(4, 11), Points(RespondX1Level)),

                //
                // If we have a good fit but a week hand then time to jump.
                //
                Shows(new Bid(3, openSuit), Fit9Plus, Points(RespondXJump)),
                
                Shows(Bid._2C, Partner(HasShownSuit()), Fit8Plus, Points(RespondX1Level)),
                Shows(Bid._2C, Shape(5, 11), Points(RespondX1Level)),

                Shows(Bid._2D, Partner(HasShownSuit()), Fit8Plus, Points(RespondX1Level)),
                Shows(Bid._2D, IsNonJump, Shape(5, 11), Points(RespondX1Level)),

                Shows(Bid._2H, Partner(HasShownSuit()), Fit8Plus, Points(RespondX1Level)),
                Shows(Bid._2H, IsNonJump, Shape(5, 11), Points(RespondX1Level)),

                Shows(Bid._2S, Partner(HasShownSuit()), Fit8Plus, Points(RespondX1Level)),

				// TODO: Perhaps higer priority than raise of a minor???
                Shows(Bid._1NT, Points(RespondX1Level)),

                // TODO: Is this correct about RespondPass points?  Are there scenerios where you just don't have
                // the right shape, but do have enought points?  I guess we'd always bid 1NT
                Shows(Bid.Pass, PointsPass)

            });

            return choices;
        }

    }
}
