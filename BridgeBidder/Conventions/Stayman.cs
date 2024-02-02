using System.Collections.Generic;

namespace BridgeBidding
{


    public class StaymanBidder : OneNoTrumpBidder
	{
 
        public StaymanBidder(NoTrumpDescription ntd) : base(ntd) { }

        public static CallFeaturesFactory InitiateConvention(NoTrumpDescription ntd)
        {
            return new StaymanBidder(ntd).Initiate;
        }

		private IEnumerable<CallFeature> Initiate(PositionState ps)
		{
            // TODO: REALLY THINK ABOUT WHO IS RESPONSIBLE FOR SYSTEMS "ON" OVER INTERFERRENCE!!!
            // If there is a bid then it can only be 2C..
            Call call = Bid._2C;
            if (ps.RHO.Bid is Bid rhoBid)
            {
                if (call.Equals(rhoBid))
                {
                    call = Call.Double; // Stolen bid
                }
                else 
                {
                    // TODO: Make sure calling code never calls this when it cant generate rules 
                    throw new System.Exception("INVALID STATE HERE...");
                }
            }
            return new CallFeature[] {
                Convention(UserText.Stayman),
                PartnerBids(call, Answer),

                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 0, 4), Flat(false), ShowsSuit(Suit.Hearts)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Spades, 4), Shape(Suit.Hearts, 0, 4), Flat(false), ShowsSuit(Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4), ShowsSuits(Suit.Hearts, Suit.Spades)),
                
                Forcing(call, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
            // TODO: Need to add rules for garbage stayman if that is on, and for 4-way transfers if that is on...
		}

       
        public IEnumerable<CallFeature> Answer(PositionState ps)
		{
            return new CallFeature[] {
                // TODO: Should we tag this with convention too???
                PartnerBids(Bid._2D, RespondTo2D),
				PartnerBids(Bid._2H,   p => RespondTo2M(p, Suit.Hearts)),
				PartnerBids(Bid._2S,   p => RespondTo2M(p, Suit.Spades)),

				// TODO: Deal with interferenceDefaultPartnerBids(goodThrough: Bid.Double, Explain),

				// TODO: Are these bids truly forcing?  Not if garbage stayman...
				Forcing(Bid._2D, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3), ShowsNoSuit()),
				Forcing(Bid._2H, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid._2S, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }


        public IEnumerable<CallFeature> RespondTo2D(PositionState ps)
        {
            var bids = new List<CallFeature>
            {
                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                PartnerBids(Bid._3H, p => GameNewMajor(p, Suit.Hearts)),
                PartnerBids(Bid._3S, p => GameNewMajor(p, Suit.Spades)),
                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid._3S, NTD.RR.GameOrBetter, Shape(5)),
				Forcing(Bid._3H, NTD.RR.GameOrBetter, Shape(5)),

                // These show invitational 5/4
                PartnerBids(Bid._2H,  p => PlaceConractNewMajor(p, Suit.Hearts)),
				PartnerBids(Bid._2S,  p => PlaceConractNewMajor(p, Suit.Spades)),
				Invitational(Bid._2H, NTD.RR.InviteGame, Shape(5)),
				Invitational(Bid._2S, NTD.RR.InviteGame, Shape(5)),

                PartnerBids(Bid._2NT, PlaceContract2NTInvite),
				Invitational(Bid._2NT, ShowsTrump, NTD.RR.InviteGame),

                Signoff(Bid._3NT, ShowsTrump, NTD.RR.Game),

                // TODO: Point ranges - Need to figure out where these...
                Invitational(Bid._4NT, ShowsTrump, PairPoints((30, 31)))
			};
            bids.AddRange(Gerber.InitiateConvention(ps));
            return bids;
        }

        public IEnumerable<CallFeature> RespondTo2M(PositionState _, Suit major)
        {
            return new CallFeature[]
            {

                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                Signoff(new Bid(4, major), Shape(4, 5), NTD.RR.GameAsDummy, ShowsTrump),
                PartnerBids(new Bid(3, major), p => PlaceContractMajorInvite(p, major)),
                Invitational(new Bid(3, major), Shape(4, 5), NTD.RR.InviteAsDummy, ShowsTrump),

                PartnerBids(Bid._3NT, CheckOpenerSpadeGame),
                Signoff(Bid._3NT, NTD.RR.Game, Shape(major, 0, 3)),

				PartnerBids(Bid._2NT, PlaceContract2NTInvite),
				Invitational(Bid._2NT, NTD.RR.InviteGame, Shape(major, 0, 3))
			};
		}
        /*
        public IEnumerable<CallFeature> Explain(PositionState _)
        {
            return new CallFeature[] {
                DefaultPartnerBids(Bid.Double, PlaceContract), 

                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),   // Garbage stayman always passes...

                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid._3S, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid._2D))),
                Forcing(Bid._3H, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid._2D))),


				// These show invitational 5/4
                Invitational(Bid._2H, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid._2D))),
                Invitational(Bid._2S, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid._2D))),

                Invitational(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(LastBid(Bid._2D))),
                Invitational(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 0, 3)),
                Invitational(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 0, 3)),


                Invitational(Bid._3H, ShowsTrump, NTD.RR.InviteAsDummy, Partner(LastBid(Bid._2H)), Shape(4, 5)),
                Invitational(Bid._3S, ShowsTrump, NTD.RR.InviteAsDummy, Partner(LastBid(Bid._2S)), Shape(4, 5)),


                // Prioritize suited contracts over 3NT bid by placing these rules first...
                Signoff(4, Suit.Hearts, ShowsTrump, NTD.RR.GameAsDummy, Partner(LastBid(Bid._2H)), Shape(4, 5)),
                Signoff(Bid._4S, ShowsTrump, NTD.RR.GameAsDummy, Partner(LastBid(Bid._2S)), Shape(4, 5)), 

                // TODO: After changeover is done an tests are working again, change all of these rules to simply
                // Signoff(Bid.ThreeUnknown, ShowsTrump, Points(ResponderRange.Game), Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(LastBid(Bid._2D))),
                Signoff(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 0, 3)),
                Signoff(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 0, 3)),

            };
        }
        */


        //******************** 2nd bid of opener.

        // Bid sequence was 1NT/2C/2X/
        public IEnumerable<CallFeature> CheckOpenerSpadeGame(PositionState ps)
        {
            return new CallFeature[]
            {
                Signoff(Bid._4S, Fit(), ShowsTrump),
                Signoff(Call.Pass)
            };
        }

        public IEnumerable<CallFeature> GameNewMajor(PositionState ps, Suit major)
        {
            return new CallFeature[]
            {
                Signoff(new Bid(4, major), Fit(), ShowsTrump),
                Signoff(Bid._3NT)
            };
        }

        public IEnumerable<CallFeature> PlaceConractNewMajor(PositionState ps, Suit major)
        {
            return new CallFeature[]
            {
                Signoff(Call.Pass, NTD.OR.DontAcceptInvite, Fit(major)),    // TODO: Need to use dummy points here...
                Signoff(Bid._2NT, NTD.OR.DontAcceptInvite),
                Signoff(new Bid(4, major), Fit(), ShowsTrump, NTD.OR.AcceptInvite),
                Signoff(Bid._3NT, ShowsTrump, NTD.OR.AcceptInvite)
            };
        }

        public IEnumerable<CallFeature> PlaceContract2NTInvite(PositionState ps)
        {
            return new CallFeature[]
            {
				PartnerBids(Bid._3S, CheckSpadeGame),
                // This is possible to know we have a fit if partner bid stayman, we respond hearts,
                Nonforcing(Bid._3S, NTD.OR.DontAcceptInvite, Fit(), ShowsTrump),


                Signoff(Bid._4S, NTD.OR.AcceptInvite, Fit(), ShowsTrump),

                Signoff(Bid._3NT, NTD.OR.AcceptInvite),

                Signoff(Call.Pass, NTD.OR.DontAcceptInvite)
			};

        }

        public IEnumerable<CallFeature> PlaceContractMajorInvite(PositionState ps, Suit major)
        {
			return new CallFeature[]
            {
				Signoff(new Bid(4, major), NTD.OR.AcceptInvite, Fit(), ShowsTrump),
                Signoff(Call.Pass, NTD.OR.DontAcceptInvite)
            };

		}
		/*
        public IEnumerable<CallFeature> PlaceContract(PositionState _)
        {
            return new CallFeature[] {
				// These rules deal with a 5/4 invitational that we want to reject.  Leave contract in bid suit
				// if we have 3.  Otherwise put in NT
				Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,  // TODO: Should check for dummy points...
                                    Fit(Suit.Hearts), Partner(LastBid(Bid._2H))),
                Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,
                                    Fit(Suit.Spades), Partner(LastBid(Bid._2S))),

                Signoff(Bid.TwoUnknown, NTD.OR.DontAcceptInvite),



                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, Partner(LastBid(Bid.TwoUnknown))),
                Signoff(Bid.ThreeUnknown, LastBid(Bid._2D), Partner(LastBid(Bid._3H)),
                            Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid._2D),
                                    Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown,  LastBid(Bid._2D), Partner(LastBid(Bid._3S)),
                            Shape(Suit.Spades, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid._2D),
                        Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 2)),



                Signoff(4, Suit.Hearts, NTD.OR.AcceptInvite, Fit()),
               //TODO: I think above rule ocvers itl.. Signoff(4, Suit.Hearts, LastBid(Bid._2D), Partner(LastBid(Bid._3H)), Shape(3)),


                Signoff(Bid._4S, NTD.OR.AcceptInvite, Partner(LastBid(Bid._3S)), Fit()),
                Signoff(Bid._4S, NTD.OR.AcceptInvite, Fit()),
                Signoff(Bid._4S, Partner(LastBid(Bid.ThreeUnknown)), Fit()),
                Signoff(Bid._4S, LastBid(Bid._2D), Partner(LastBid(Bid._3S)), Shape(3))
            };
        }
        */
		public IEnumerable<CallFeature> CheckSpadeGame(PositionState _)
        {
            return new CallFeature[] {
                Signoff(Bid._4S, ShowsTrump, NTD.RR.GameAsDummy, Shape(4, 5)),
                Signoff(Call.Pass)
            };
		}
	}


    //*********************************************************************************************

    // TODO: Maybe move thse 2NT stayman...
    public class Stayman2NT: Bidder
    {
        private TwoNoTrump NTB;

        public Stayman2NT(TwoNoTrump ntb)
        {
            this.NTB = ntb;
        }

        public IEnumerable<CallFeature> InitiateConvention(PositionState ps) 
        {
            // If there is a bid then it can only be 3C..
            Bid bidStayman = Bid._3C;

            // TODO: This is no longer possible unless convert this to PositionCalls...
            Call call = ps.RightHandOpponent.GetBidHistory(0).Equals(bidStayman) ? Bid.Double : bidStayman;
            return new CallFeature[] {
                PartnerBids(call, Answer),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Spades, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4))
                // TODO: The following rule is "Garbage Stayman"
                //Forcing(Bid._2C, Points(NTLessThanInvite), Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
        }
        public IEnumerable<CallFeature> Answer(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(ResponderRebid),

                Forcing(Bid._3D, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),

                // If we are 4-4 then hearts bid before spades.  Can't be 5-5 or wouldn't be balanced.
                Forcing(Bid._3H, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid._3S, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }

        public static IEnumerable<CallFeature> ResponderRebid(PositionState _)
        {
            return new CallFeature[] {
                PartnerBids(Bid._3H, OpenerRebid),
                PartnerBids(Bid._3S, OpenerRebid),

                Forcing(Bid._3H, Shape(5), Partner(LastBid(Bid._3D))),
                Forcing(Bid._3S, Shape(5), Partner(LastBid(Bid._3D))),

                Signoff(Bid._4H, Fit()),
                Signoff(Bid._4S, Fit()),
                
                Signoff(Bid._3NT),
            };
        }
    
        public static IEnumerable<CallFeature> OpenerRebid(PositionState _)
        { 
            return new CallFeature[] {
                Signoff(Bid._3NT, Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid._4H, Fit()),
                Signoff(Bid._4S, Fit()),
            };
        }
    }

}
