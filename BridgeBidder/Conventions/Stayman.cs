using System.Collections.Generic;

namespace BridgeBidding
{


    public class StaymanBidder : OneNoTrumpBidder
	{
 
        public StaymanBidder(NoTrumpDescription ntd) : base(ntd) { }

        public static BidRulesFactory InitiateConvention(NoTrumpDescription ntd)
        {
            return new StaymanBidder(ntd).Initiate;
        }

		private IEnumerable<BidRule> Initiate(PositionState ps)
		{
            // If there is a bid then it can only be 2C..
            Bid bidStayman = Bid.TwoClubs;

            Call call = ps.RightHandOpponent.GetBidHistory(0).Equals(bidStayman) ? Bid.Double : bidStayman;
            return new BidRule[] {
                PartnerBids(call, Call.Double, Answer),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 0, 4), Flat(false), ShowsSuit(Suit.Hearts)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Spades, 4), Shape(Suit.Hearts, 0, 4), Flat(false), ShowsSuit(Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4), ShowsSuits(Suit.Hearts, Suit.Spades)),
                
                Forcing(call, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
            // TODO: Need to add rules for garbage stayman if that is on, and for 4-way transfers if that is on...
		}

       
        public IEnumerable<BidRule> Answer(PositionState ps)
		{
            return new BidRule[] {

                PartnerBids(Bid.TwoDiamonds, Call.Double, RespondTo2D),
				PartnerBids(Bid.TwoHearts, Call.Double, p => RespondTo2M(p, Suit.Hearts)),
				PartnerBids(Bid.TwoSpades, Call.Double, p => RespondTo2M(p, Suit.Spades)),

				// TODO: Deal with interferenceDefaultPartnerBids(goodThrough: Bid.Double, Explain),

				// TODO: Are these bids truly forcing?  Not if garbage stayman...
				Forcing(Bid.TwoDiamonds, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3), ShowsNoSuit()),
				Forcing(Bid.TwoHearts, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.TwoSpades, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }


        public IEnumerable<BidRule> RespondTo2D(PositionState ps)
        {
            var bids = new List<BidRule>
            {
                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                PartnerBids(Bid.ThreeHearts, Call.Double, p => GameNewMajor(p, Suit.Hearts)),
                PartnerBids(Bid.ThreeSpades, Call.Double, p => GameNewMajor(p, Suit.Spades)),
                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid.ThreeSpades, NTD.RR.GameOrBetter, Shape(5)),
				Forcing(Bid.ThreeHearts, NTD.RR.GameOrBetter, Shape(5)),

                // These show invitational 5/4
                PartnerBids(Bid.TwoHearts, Call.Double, p => PlaceConractNewMajor(p, Suit.Hearts)),
				PartnerBids(Bid.TwoSpades, Call.Double, p => PlaceConractNewMajor(p, Suit.Spades)),
				Invitational(Bid.TwoHearts, NTD.RR.InviteGame, Shape(5)),
				Invitational(Bid.TwoSpades, NTD.RR.InviteGame, Shape(5)),

                PartnerBids(Bid.TwoNoTrump, Call.Double, PlaceContract2NTInvite),
				Invitational(Bid.TwoNoTrump, ShowsTrump(), NTD.RR.InviteGame),

                Signoff(Bid.ThreeNoTrump, ShowsTrump(), NTD.RR.Game),

                // TODO: Point ranges - Need to figure out where these...
                Invitational(Bid.FourNoTrump, ShowsTrump(), PairPoints((30, 31)))
			};
            bids.AddRange(Gerber.InitiateConvention(ps));
            return bids;
        }

        public IEnumerable<BidRule> RespondTo2M(PositionState _, Suit major)
        {
            return new BidRule[]
            {

                Signoff(Call.Pass, NTD.RR.LessThanInvite),

                Signoff(new Bid(4, major), Shape(4, 5), NTD.RR.GameAsDummy, ShowsTrump()),
                PartnerBids(new Bid(3, major), Call.Double, p => PlaceContractMajorInvite(p, major)),
                Invitational(new Bid(3, major), Shape(4, 5), NTD.RR.InviteAsDummy, ShowsTrump()),

                PartnerBids(Bid.ThreeNoTrump, Call.Double, CheckOpenerSpadeGame),
                Signoff(Bid.ThreeNoTrump, NTD.RR.Game, Shape(major, 0, 3)),

				PartnerBids(Bid.TwoNoTrump, Call.Double, PlaceContract2NTInvite),
				Invitational(Bid.TwoNoTrump, NTD.RR.InviteGame, Shape(major, 0, 3))
			};
		}
        /*
        public IEnumerable<BidRule> Explain(PositionState _)
        {
            return new BidRule[] {
                DefaultPartnerBids(Bid.Double, PlaceContract), 

                // TODO: Points 0-7 defined as garbage range...
                Signoff(Call.Pass, NTD.RR.LessThanInvite),   // Garbage stayman always passes...

                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid.ThreeSpades, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),
                Forcing(Bid.ThreeHearts, NTD.RR.GameOrBetter, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),


				// These show invitational 5/4
                Invitational(Bid.TwoHearts, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),
                Invitational(Bid.TwoSpades, NTD.RR.InviteGame, Shape(5), Partner(LastBid(Bid.TwoDiamonds))),

                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoDiamonds))),
                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 0, 3)),
                Invitational(Bid.TwoUnknown, ShowsTrump(), NTD.RR.InviteGame, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 0, 3)),


                Invitational(Bid.ThreeHearts, ShowsTrump(), NTD.RR.InviteAsDummy, Partner(LastBid(Bid.TwoHearts)), Shape(4, 5)),
                Invitational(Bid.ThreeSpades, ShowsTrump(), NTD.RR.InviteAsDummy, Partner(LastBid(Bid.TwoSpades)), Shape(4, 5)),


                // Prioritize suited contracts over 3NT bid by placing these rules first...
                Signoff(4, Suit.Hearts, ShowsTrump(), NTD.RR.GameAsDummy, Partner(LastBid(Bid.TwoHearts)), Shape(4, 5)),
                Signoff(Bid.FourSpades, ShowsTrump(), NTD.RR.GameAsDummy, Partner(LastBid(Bid.TwoSpades)), Shape(4, 5)), 

                // TODO: After changeover is done an tests are working again, change all of these rules to simply
                // Signoff(Bid.ThreeUnknown, ShowsTrump(), Points(ResponderRange.Game), Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoDiamonds))),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 0, 3)),
                Signoff(Bid.ThreeUnknown, ShowsTrump(), NTD.RR.Game, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 0, 3)),

            };
        }
        */


        //******************** 2nd bid of opener.

        // Bid sequence was 1NT/2C/2X/
        public IEnumerable<BidRule> CheckOpenerSpadeGame(PositionState ps)
        {
            return new BidRule[]
            {
                Signoff(Bid.FourSpades, Fit(), ShowsTrump()),
                Signoff(Call.Pass)
            };
        }

        public IEnumerable<BidRule> GameNewMajor(PositionState ps, Suit major)
        {
            return new BidRule[]
            {
                Signoff(new Bid(4, major), Fit(), ShowsTrump()),
                Signoff(Bid.ThreeNoTrump)
            };
        }

        public IEnumerable<BidRule> PlaceConractNewMajor(PositionState ps, Suit major)
        {
            return new BidRule[]
            {
                Signoff(Call.Pass, NTD.OR.DontAcceptInvite, Fit(major)),    // TODO: Need to use dummy points here...
                Signoff(Bid.TwoNoTrump, NTD.OR.DontAcceptInvite),
                Signoff(new Bid(4, major), Fit(), ShowsTrump(), NTD.OR.AcceptInvite),
                Signoff(Bid.ThreeNoTrump, ShowsTrump(), NTD.OR.AcceptInvite)
            };
        }

        public IEnumerable<BidRule> PlaceContract2NTInvite(PositionState ps)
        {
            return new BidRule[]
            {
				PartnerBids(Bid.ThreeSpades, Bid.Double, CheckSpadeGame),
                // This is possible to know we have a fit if partner bid stayman, we respond hearts,
                Nonforcing(Bid.ThreeSpades, Break(false, "3NT"), NTD.OR.DontAcceptInvite, Fit(), ShowsTrump()),


                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Fit(), ShowsTrump()),

                Signoff(Bid.ThreeNoTrump, NTD.OR.AcceptInvite)
			};

        }

        public IEnumerable<BidRule> PlaceContractMajorInvite(PositionState ps, Suit major)
        {
			return new BidRule[]
            {
				Signoff(new Bid(4, major), NTD.OR.AcceptInvite, Fit(), ShowsTrump()),
            };

		}
		/*
        public IEnumerable<BidRule> PlaceContract(PositionState _)
        {
            return new BidRule[] {
				// These rules deal with a 5/4 invitational that we want to reject.  Leave contract in bid suit
				// if we have 3.  Otherwise put in NT
				Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,  // TODO: Should check for dummy points...
                                    Fit(Suit.Hearts), Partner(LastBid(Bid.TwoHearts))),
                Signoff(Bid.Pass, NTD.OR.DontAcceptInvite,
                                    Fit(Suit.Spades), Partner(LastBid(Bid.TwoSpades))),

                Signoff(Bid.TwoUnknown, NTD.OR.DontAcceptInvite),



                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, Partner(LastBid(Bid.TwoUnknown))),
                Signoff(Bid.ThreeUnknown, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeHearts)),
                            Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid.TwoDiamonds),
                                    Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 2)),
                Signoff(Bid.ThreeUnknown,  LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeSpades)),
                            Shape(Suit.Spades, 2)),
                Signoff(Bid.ThreeUnknown, NTD.OR.AcceptInvite, LastBid(Bid.TwoDiamonds),
                        Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 2)),



                Signoff(4, Suit.Hearts, NTD.OR.AcceptInvite, Fit()),
               //TODO: I think above rule ocvers itl.. Signoff(4, Suit.Hearts, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeHearts)), Shape(3)),


                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Partner(LastBid(Bid.ThreeSpades)), Fit()),
                Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Fit()),
                Signoff(Bid.FourSpades, Partner(LastBid(Bid.ThreeUnknown)), Fit()),
                Signoff(Bid.FourSpades, LastBid(Bid.TwoDiamonds), Partner(LastBid(Bid.ThreeSpades)), Shape(3))
            };
        }
        */
		public IEnumerable<BidRule> CheckSpadeGame(PositionState _)
        {
            return new BidRule[] {
                Signoff(Bid.FourSpades, ShowsTrump(), NTD.RR.GameAsDummy, Shape(4, 5))
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

        public IEnumerable<BidRule> InitiateConvention(PositionState ps) 
        {
            // If there is a bid then it can only be 3C..
            Bid bidStayman = Bid.ThreeClubs;

            Call call = ps.RightHandOpponent.GetBidHistory(0).Equals(bidStayman) ? Bid.Double : bidStayman;
            return new BidRule[] {
                PartnerBids(call, Bid.Double, Answer),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Spades, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4))
                // TODO: The following rule is "Garbage Stayman"
                //Forcing(Bid.TwoClubs, Points(NTLessThanInvite), Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
        }
        public IEnumerable<BidRule> Answer(PositionState _)
        {
            return new BidRule[] {
                DefaultPartnerBids(Call.Double, ResponderRebid),

                Forcing(Bid.ThreeDiamonds, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),

                // If we are 4-4 then hearts bid before spades.  Can't be 5-5 or wouldn't be balanced.
                Forcing(Bid.ThreeHearts, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Forcing(Bid.ThreeSpades, Shape(4, 5), LongerThan(Suit.Hearts))
            };
        }

        public static IEnumerable<BidRule> ResponderRebid(PositionState _)
        {
            return new BidRule[] {
                DefaultPartnerBids(Call.Double, OpenerRebid),

                Forcing(Bid.ThreeHearts, Shape(5), Partner(LastBid(Bid.ThreeDiamonds))),
                Forcing(Bid.ThreeSpades, Shape(5), Partner(LastBid(Bid.ThreeDiamonds))),

                Signoff(Bid.ThreeNoTrump, Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.FourHearts, Fit()),
                Signoff(Bid.FourSpades, Fit())
            };
        }
    
        public static IEnumerable<BidRule> OpenerRebid(PositionState _)
        { 
            return new BidRule[] {
                Signoff(Bid.ThreeNoTrump, Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Signoff(Bid.FourHearts, Fit()),
                Signoff(Bid.FourSpades, Fit())
            };
        }
    }

}
