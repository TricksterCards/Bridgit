﻿using System;
using System.Collections.Generic;


namespace BridgeBidding
{

    public class NoTrumpDescription : Bidder
    {
        public struct OpenerRanges
        {
            public Constraint Open;
            public Constraint DontAcceptInvite;
            public Constraint AcceptInvite;
            public Constraint LessThanSuperAccept;
            public Constraint SuperAccept;
        }
        public struct ResponderRanges
        {

            public Constraint LessThanInvite;
            public Constraint InviteGame;
            public Constraint InviteOrBetter;
            public Constraint Game;
            public Constraint GameOrBetter;
            public Constraint GameIfSuperAccept;
            public Constraint InviteSlam;
            public Constraint SmallSlam;
            public Constraint GrandSlam;

            public Constraint GameAsDummy;
            public Constraint InviteAsDummy;
            public Constraint SmallSlamAsDummy;
            public Constraint GrandSlamAsDummy;
        }

        public String OpenType;

        public OpenerRanges OR;
        public ResponderRanges RR;

    }

    public class Open1NTDescription : NoTrumpDescription
    {
        public Open1NTDescription()
        {
            OpenType = "Open1NT";
            
            OR.Open = And(HighCardPoints(15, 17), Points(15, 18));
            
            OR.DontAcceptInvite = And(HighCardPoints(15, 15), Points(15, 16));
            OR.AcceptInvite = And(HighCardPoints(16, 17), Points(16, 18));
            OR.LessThanSuperAccept = And(HighCardPoints(15, 16), Points(15, 17));
            OR.SuperAccept = And(HighCardPoints(17, 17), Points(17, 18));
           

            RR.LessThanInvite = Points(0, 7);
            RR.InviteGame = Points(8, 9);
            RR.InviteOrBetter = Points(8, 40);
            RR.Game = Points(10, 15);
            RR.GameOrBetter = Points(10, 40);
            RR.GameIfSuperAccept = Points(6, 15);
            RR.InviteSlam = Points(16, 17);
            RR.SmallSlam = Points(18, 19);
            RR.GrandSlam = Points(20, 40);

            // TODO: This dummy stuff seems poorly thought out.  Perhaps just plain old points...
            RR.InviteAsDummy = DummyPoints(8, 9);
            RR.GameAsDummy = DummyPoints(10, 16);
            RR.SmallSlamAsDummy = DummyPoints(17, 20);
            RR.GrandSlamAsDummy = DummyPoints(21, 40);
        }
    }

	public class Overcall1NTDescription : NoTrumpDescription
	{
		public Overcall1NTDescription()
		{
			OpenType = "Overcall1NT";

			OR.Open =               And(HighCardPoints(15, 18), Points(15, 19));
			OR.DontAcceptInvite =   And(HighCardPoints(15, 15), Points(15, 16));
			OR.AcceptInvite =       And(HighCardPoints(16, 18), Points(16, 19));
			OR.LessThanSuperAccept= And(HighCardPoints(15, 16), Points(15, 17));
			OR.SuperAccept =        And(HighCardPoints(18, 19), Points(18, 20));


			RR.LessThanInvite =     Points(0, 7);
			RR.InviteGame =         Points(8, 9);
			RR.InviteOrBetter =     Points(8, 40);
			RR.Game =               Points(10, 15);
			RR.GameOrBetter =       Points(10, 40);
			RR.GameIfSuperAccept =  Points(6, 15);
            RR.InviteSlam =         Points(16, 17);
			RR.SmallSlam =          Points(18, 19);
			RR.GrandSlam =          Points(20, 40);

			RR.InviteAsDummy = DummyPoints(8, 9);
            RR.GameAsDummy = DummyPoints(10, 16);
            RR.SmallSlamAsDummy = DummyPoints(17, 20);
            RR.GrandSlamAsDummy = DummyPoints(21, 40);

		}
	}


	public class Balancing1NTDescription : NoTrumpDescription
	{
		public Balancing1NTDescription()
		{
			OpenType = "Balancing1NT";

			OR.Open = And(HighCardPoints(13, 15), Points(13, 16));
			OR.DontAcceptInvite = And(HighCardPoints(13, 14), Points(13, 15));
			OR.AcceptInvite = And(HighCardPoints(15, 15), Points(15, 16));
            OR.LessThanSuperAccept = HighCardPoints(13, 15);    // NEVER super accept...
            OR.SuperAccept = HighCardPoints(40, 40);            // NEVER super accept




            // TODO: Review all of the balancing ranges..  They seem busted...

            RR.LessThanInvite = Points(0, 10);
            RR.InviteGame = Points(11, 12);
			RR.InviteOrBetter = Points(11, 40);

            // ALL OF THE FOLLOWINg WILL NEVER HAPPEN - PASSED HAND IMPOSSIBLE ....
			RR.Game = Points(13, 15);
			RR.GameOrBetter = Points(10, 40);
            RR.GameIfSuperAccept = Points(40, 40);
			RR.InviteSlam = Points(40, 40);
			RR.SmallSlam = Points(40, 40);
			RR.GrandSlam = Points(40, 40);

			RR.GameAsDummy = DummyPoints(13, 15);
			RR.InviteAsDummy = DummyPoints(11, 12);
            RR.SmallSlamAsDummy = DummyPoints(40, 40);
            RR.GrandSlamAsDummy = DummyPoints(40, 40);
		}
	}


	public class OneNoTrumpBidder : Bidder
    {

		public static OneNoTrumpBidder Open = new OneNoTrumpBidder(new Open1NTDescription());
        public static OneNoTrumpBidder Overcall = new OneNoTrumpBidder(new Overcall1NTDescription());
        public static OneNoTrumpBidder Balancing = new OneNoTrumpBidder(new Balancing1NTDescription());


        public NoTrumpDescription NTD;
        
        protected OneNoTrumpBidder(NoTrumpDescription ntd)
        { 
            NTD = ntd;
        }  

        public IEnumerable<CallFeature> Bids(PositionState ps)
        {
            if (ps.Role == PositionRole.Opener && ps.RoleRound == 1)
            {
                return new CallFeature[]
                {
                  //  Announce(Bid._1NT, UserText.OneNoTrumpRange),
                  //  PartnerBids(Bid._1NT, ConventionalResponses),
                  //  Shows(Bid._1NT, NTD.OR.Open, Balanced)
                    Properties(Bid._1NT, partnerBids: ConventionalResponses, announce: UserText.OneNoTrumpRange),
                    Shows(Bid._1NT, NTD.OR.Open, Balanced)   
                };
            }
            if (ps.Role == PositionRole.Overcaller && ps.RoleRound == 1)
            {
				if (ps.BiddingState.Contract.PassEndsAuction && NTD.OpenType == "Balancing1NT")
				{
                    return new CallFeature[]
                    {
                        PartnerBids(Bid._1NT, ConventionalResponses),
                        // TODO: Perhaps more rules here for balancing but for now this is fine -- Balanced is not necessary
                        Shows(Bid._1NT, NTD.OR.Open, IsFinalCall)
                    };
				}
                else if (NTD.OpenType == "Overcall1NT")
                {
                    return new CallFeature[]
                    {
                        PartnerBids(Bid._1NT, ConventionalResponses),
                        Shows(Bid._1NT, NTD.OR.Open, Balanced, OppsStopped, IsNotFinalCall)
                    };
                }
			}
            return new CallFeature[0];
		}



        private PositionCalls ConventionalResponses(PositionState ps)
        {

            if (ps.RHO.Bid is Bid rhoBid && !rhoBid.Equals(Bid._2C))
            {
                // TODO: Handle interfererence better than this...
                return ps.PairState.BiddingSystem.GetPositionCalls(ps);
            }
            // TODO: Interferrence?  Probably do something globally here...
            var choices = new PositionCalls(ps);
            choices.AddRules(StaymanBidder.InitiateConvention(NTD));
            choices.AddRules(TransferBidder.InitiateConvention(NTD));
            choices.AddRules(Gerber.InitiateConvention);
            // TODO: Should this actually happen? Natural probs never...
            choices.AddRules(Natural1NT.Respond(NTD));
            return choices;
        }

    }


    public class NoTrump : Bidder
    {

        public static IEnumerable<CallFeature> Open(PositionState ps)
        {
            var bids = new List<CallFeature>();

            bids.AddRange(OneNoTrumpBidder.Open.Bids(ps));
            bids.AddRange(TwoNoTrump.Open.Bids(ps));

            return bids;
        }


        public static IEnumerable<CallFeature> StrongOvercall(PositionState ps)
        {          
            return OneNoTrumpBidder.Overcall.Bids(ps);
        }

        public static IEnumerable<CallFeature> BalancingOvercall(PositionState ps)
        {
            return OneNoTrumpBidder.Balancing.Bids(ps);
        }

      
    }



    public class Natural1NT : OneNoTrumpBidder
    {
        public Natural1NT(NoTrumpDescription ntd) : base(ntd)
        {
        }

        public static IEnumerable<CallFeature> Respond(NoTrumpDescription ntd)
        {
            return new Natural1NT(ntd).NaturalResponse();
        }



        private IEnumerable<CallFeature> NaturalResponse()
        {
            return new CallFeature[]
            {
                PartnerBids(OpenerRebid),
                PartnerBids(Bid._4NT, Compete.CompBids), // TODO: Handle slam invite better???  Maybe this is ok?

                Shows(Bid._2C, Shape(5, 11), NTD.RR.LessThanInvite),
                Shows(Bid._2D, Shape(5, 11), NTD.RR.LessThanInvite),
                Shows(Bid._2H, Shape(5, 11), NTD.RR.LessThanInvite),
                Shows(Bid._2S, Shape(5, 11), NTD.RR.LessThanInvite),

                Shows(Bid._2NT, NTD.RR.InviteGame, LongestMajor(4)),
                // TODO: These natural bids are not exactly right....

                Properties(Bid._3H, forcing1Round: true),
                Properties(Bid._3S, forcing1Round: true),
                Shows(Bid._3H, NTD.RR.GameOrBetter, Shape(5, 11)),
                Shows(Bid._3S, NTD.RR.GameOrBetter, Shape(5, 11)),

                Shows(Bid._3NT, NTD.RR.Game, LongestMajor(4)),

                Shows(Bid._4NT, NTD.RR.InviteSlam), // TODO: Any shape stuff here???

                Shows(Bid._6NT, Flat, NTD.RR.SmallSlam),
                Shows(Bid._6NT, Balanced, Shape(Suit.Hearts, 2, 3), Shape(Suit.Spades, 2, 3), NTD.RR.SmallSlam),

                Shows(Bid.Pass, NTD.RR.LessThanInvite),


            };
        }

        private IEnumerable<CallFeature> OpenerRebid(PositionState _)
        {
            return new CallFeature[]
            {
                PartnerBids(ResponderRebid),

                Shows(Call.Pass, Partner(IsLastBid(Bid._3NT))),
                Shows(Call.Pass, NTD.OR.DontAcceptInvite, Partner(IsLastBid(Bid._2NT))),
                Shows(Call.Pass, Partner(IsLastBid(Bid._2C))),
                Shows(Call.Pass, Partner(IsLastBid(Bid._2D))),
                Shows(Call.Pass, Partner(IsLastBid(Bid._2H))),
                Shows(Call.Pass, Partner(IsLastBid(Bid._2S))),
                
                Properties(Bid._3H, forcing1Round: true),
                Properties(Bid._3S, forcing1Round: true),
                Shows(Bid._3H, Partner(IsLastBid(Bid._2NT)), NTD.OR.AcceptInvite, Shape(5)),
                Shows(Bid._3S, Partner(IsLastBid(Bid._2NT)), NTD.OR.AcceptInvite, Shape(5)),

                Shows(Bid._3NT, NTD.OR.AcceptInvite, Partner(IsLastBid(Bid._2NT))),
                Shows(Bid._3NT, Partner(IsLastBid(Bid._3H)), Shape(Suit.Hearts, 0, 2)),
                Shows(Bid._3NT, Partner(IsLastBid(Bid._3S)), Shape(Suit.Spades, 0, 2)),

                Shows(Bid._4H, Partner(IsLastBid(Bid._3H)), Shape(3, 5)),
                Shows(Bid._4S, Partner(IsLastBid(Bid._3S)), Shape(3, 5))
            };
        }
        private IEnumerable<CallFeature> ResponderRebid(PositionState _)
        {
            return new CallFeature[]
            {
                // TODO: Ideally this would be "Parther(ShowsShape(Hearts, 5)" Better than lastbid...
                Shows(Bid._3NT, Partner(IsLastBid(Bid._3H)), Shape(Suit.Hearts, 0, 2)),
                Shows(Bid._3NT, Partner(IsLastBid(Bid._3S)), Shape(Suit.Spades, 0, 2)),

                Shows(Bid._4H, Partner(IsLastBid(Bid._3H)), Shape(3, 4)),
                Shows(Bid._4S, Partner(IsLastBid(Bid._3S)), Shape(3, 4)),

                Shows(Call.Pass)
            };
        }
    }

  
}
