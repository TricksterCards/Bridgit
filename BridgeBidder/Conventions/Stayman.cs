using System.Collections.Generic;
using System.Net;

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
                Convention(call, UserText.Stayman),
                Properties(call, Answer, forcing1Round: true),

                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 0, 4), Flat(false)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Spades, 4), Shape(Suit.Hearts, 0, 4), Flat(false)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5)),
                Forcing(call, NTD.RR.InviteOrBetter, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4)),
                
                Forcing(call, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
            // TODO: Need to add rules for garbage stayman if that is on, and for 4-way transfers if that is on...
		}

       
        public PositionCalls Answer(PositionState ps)
		{
            return new PositionCalls(ps).AddRules(
                // TODO: Should we tag this with convention too???
                PartnerBids(Bid._2D, RespondTo2D),
				PartnerBids(Bid._2H, p => RespondTo2M(p, Suit.Hearts)),
				PartnerBids(Bid._2S, p => RespondTo2M(p, Suit.Spades)),

				Shows(Bid._2D, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),
				Shows(Bid._2H, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._2S, Shape(4, 5), LongerThan(Suit.Hearts))
            );
        }


        public PositionCalls RespondTo2D(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(
            
                // TODO: Points 0-7 defined as garbage range...
                Shows(Call.Pass, NTD.RR.LessThanInvite),

                Properties(Bid._3H, p => GameNewMajor(p, Suit.Hearts), forcing1Round: true),
                Properties(Bid._3S, p => GameNewMajor(p, Suit.Spades), forcing1Round: true),
                // If we have a 5 card suit and want to go to game then show that suit.
                Shows(Bid._3S, NTD.RR.GameOrBetter, Shape(5)),
				Shows(Bid._3H, NTD.RR.GameOrBetter, Shape(5)),

                // These show invitational 5/4
                PartnerBids(Bid._2H,  p => PlaceConractNewMajor(p, Suit.Hearts)),
				PartnerBids(Bid._2S,  p => PlaceConractNewMajor(p, Suit.Spades)),
				Shows(Bid._2H, NTD.RR.InviteGame, Shape(5)),
				Shows(Bid._2S, NTD.RR.InviteGame, Shape(5)),

                Properties(Bid._2NT, PlaceContract2NTInvite),
				Shows(Bid._2NT, NTD.RR.InviteGame),

                Shows(Bid._3NT, NTD.RR.Game),

                // TODO: Point ranges - Need to figure out where these...
                Shows(Bid._4NT, PairPoints((30, 31)))
            );
            choices.AddRules(Gerber.InitiateConvention(ps));
            return choices;
        }

        public PositionCalls RespondTo2M(PositionState ps, Suit major)
        {
            return new PositionCalls(ps).AddRules(
                Shows(Call.Pass, NTD.RR.LessThanInvite),

                Shows(new Bid(4, major), Shape(4, 5), NTD.RR.GameAsDummy),

                Properties(new Bid(3, major), p => PlaceContractMajorInvite(p, major)),
                Shows(new Bid(3, major), Shape(4, 5), NTD.RR.InviteAsDummy),

                PartnerBids(Bid._3NT, CheckOpenerSpadeGame),
                Shows(Bid._3NT, NTD.RR.Game, Shape(major, 0, 3)),

				PartnerBids(Bid._2NT, PlaceContract2NTInvite),
				Shows(Bid._2NT, NTD.RR.InviteGame, Shape(major, 0, 3))
            );
		}
        /*
        public IEnumerable<CallFeature> Explain(PositionState _)
        {
            return new CallFeature[] {
                DefaultPartnerBids(Bid.Double, PlaceContract), 

                // TODO: Points 0-7 defined as garbage range...
                Shows(Call.Pass, NTD.RR.LessThanInvite),   // Garbage stayman always passes...

                // If we have a 5 card suit and want to go to game then show that suit.
                Forcing(Bid._3S, NTD.RR.GameOrBetter, Shape(5), Partner(IsLastBid(Bid._2D))),
                Forcing(Bid._3H, NTD.RR.GameOrBetter, Shape(5), Partner(IsLastBid(Bid._2D))),


				// These show invitational 5/4
                Shows(Bid._2H, NTD.RR.InviteGame, Shape(5), Partner(IsLastBid(Bid._2D))),
                Shows(Bid._2S, NTD.RR.InviteGame, Shape(5), Partner(IsLastBid(Bid._2D))),

                Shows(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2D))),
                Shows(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid.TwoUnknown, ShowsTrump, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 0, 3)),


                Shows(Bid._3H, ShowsTrump, NTD.RR.InviteAsDummy, Partner(IsLastBid(Bid._2H)), Shape(4, 5)),
                Shows(Bid._3S, ShowsTrump, NTD.RR.InviteAsDummy, Partner(IsLastBid(Bid._2S)), Shape(4, 5)),


                // Prioritize suited contracts over 3NT bid by placing these rules first...
                Shows(4, Suit.Hearts, ShowsTrump, NTD.RR.GameAsDummy, Partner(IsLastBid(Bid._2H)), Shape(4, 5)),
                Shows(Bid._4S, ShowsTrump, NTD.RR.GameAsDummy, Partner(IsLastBid(Bid._2S)), Shape(4, 5)), 

                // TODO: After changeover is done an tests are working again, change all of these rules to simply
                // Shows(Bid.ThreeUnknown, ShowsTrump, Points(ResponderRange.Game), Fit(Suit.Hearts, false), Fit(Suit.Spades, false)),
                Shows(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(IsLastBid(Bid._2D))),
                Shows(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 0, 3)),
                Shows(Bid.ThreeUnknown, ShowsTrump, NTD.RR.Game, Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 0, 3)),

            };
        }
        */


        //******************** 2nd bid of opener.

        // Bid sequence was 1NT/2C/2X/
        public PositionCalls CheckOpenerSpadeGame(PositionState ps)
        {
            return new PositionCalls(ps).AddRules(
                Shows(Bid._4S, Fit8Plus),
                Shows(Call.Pass)
            );
        }

        public PositionCalls GameNewMajor(PositionState ps, Suit major)
        {
            return new PositionCalls(ps).AddRules(
                Shows(new Bid(4, major), Fit8Plus),
                Shows(Bid._3NT)
            );
        }

        public PositionCalls PlaceConractNewMajor(PositionState ps, Suit major)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(
                Shows(Call.Pass, NTD.OR.DontAcceptInvite, Fit(major)),    // TODO: Need to use dummy points here...
                Shows(Bid._2NT, NTD.OR.DontAcceptInvite),
                Shows(new Bid(4, major), Fit8Plus, NTD.OR.AcceptInvite),
                Shows(Bid._3NT, NTD.OR.AcceptInvite)
            );
            return choices;
        }
    
        public PositionCalls PlaceContract2NTInvite(PositionState ps)
        {
            return new PositionCalls(ps).AddRules(
				Properties(Bid._3S, CheckSpadeGame),
                // This is possible to know we have a fit if partner bid stayman, we respond hearts,
                Shows(Bid._3S, NTD.OR.DontAcceptInvite, Fit8Plus),

                Shows(Bid._4S, NTD.OR.AcceptInvite, Fit8Plus),

                Shows(Bid._3NT, NTD.OR.AcceptInvite),

                Shows(Call.Pass, NTD.OR.DontAcceptInvite)
            );
        }

        public PositionCalls PlaceContractMajorInvite(PositionState ps, Suit major)
        {
            return new PositionCalls(ps).AddRules(
				Shows(new Bid(4, major), NTD.OR.AcceptInvite, Fit8Plus),
                Shows(Call.Pass, NTD.OR.DontAcceptInvite)
            );
		}
		/*
        public IEnumerable<CallFeature> PlaceContract(PositionState _)
        {
            return new CallFeature[] {
				// These rules deal with a 5/4 invitational that we want to reject.  Leave contract in bid suit
				// if we have 3.  Otherwise put in NT
				Shows(Bid.Pass, NTD.OR.DontAcceptInvite,  // TODO: Should check for dummy points...
                                    Fit(Suit.Hearts), Partner(IsLastBid(Bid._2H))),
                Shows(Bid.Pass, NTD.OR.DontAcceptInvite,
                                    Fit(Suit.Spades), Partner(IsLastBid(Bid._2S))),

                Shows(Bid.TwoUnknown, NTD.OR.DontAcceptInvite),



                Shows(Bid.ThreeUnknown, NTD.OR.AcceptInvite, Partner(IsLastBid(Bid.TwoUnknown))),
                Shows(Bid.ThreeUnknown, IsLastBid(Bid._2D), Partner(IsLastBid(Bid._3H)),
                            Shape(Suit.Hearts, 2)),
                Shows(Bid.ThreeUnknown, NTD.OR.AcceptInvite, IsLastBid(Bid._2D),
                                    Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 2)),
                Shows(Bid.ThreeUnknown,  IsLastBid(Bid._2D), Partner(IsLastBid(Bid._3S)),
                            Shape(Suit.Spades, 2)),
                Shows(Bid.ThreeUnknown, NTD.OR.AcceptInvite, IsLastBid(Bid._2D),
                        Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 2)),



                Shows(4, Suit.Hearts, NTD.OR.AcceptInvite, Fit8Plus),
               //TODO: I think above rule ocvers itl.. Shows(4, Suit.Hearts, IsLastBid(Bid._2D), Partner(IsLastBid(Bid._3H)), Shape(3)),


                Shows(Bid._4S, NTD.OR.AcceptInvite, Partner(IsLastBid(Bid._3S)), Fit8Plus),
                Shows(Bid._4S, NTD.OR.AcceptInvite, Fit8Plus),
                Shows(Bid._4S, Partner(IsLastBid(Bid.ThreeUnknown)), Fit8Plus),
                Shows(Bid._4S, IsLastBid(Bid._2D), Partner(IsLastBid(Bid._3S)), Shape(3))
            };
        }
        */
		public PositionCalls CheckSpadeGame(PositionState ps)
        {
            return new PositionCalls(ps).AddRules(
                Shows(Bid._4S, NTD.RR.GameAsDummy, Shape(4, 5)),
                Shows(Call.Pass)
            );
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
                Properties(call, Answer, forcing1Round: true),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Spades, 4), Flat(false)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 4), Shape(Suit.Spades, 5)),
                Forcing(call, NTB.RespondGame, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 4))
                // TODO: The following rule is "Garbage Stayman"
                //Forcing(Bid._2C, Points(NTLessThanInvite), Shape(Suit.Diamonds, 4, 5), Shape(Suit.Hearts, 4), Shape(Suit.Spades, 4)),
            };
        }
        public PositionCalls Answer(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(
                PartnerBids(ResponderRebid),

                Forcing(Bid._3D, Shape(Suit.Hearts, 0, 3), Shape(Suit.Spades, 0, 3)),

                // If we are 4-4 then hearts bid before spades.  Can't be 5-5 or wouldn't be balanced.
                Shows(Bid._3H, Shape(4, 5), LongerOrEqualTo(Suit.Spades)),
                Shows(Bid._3S, Shape(4, 5), LongerThan(Suit.Hearts))
            );
            return choices;
        }

        public static IEnumerable<CallFeature> ResponderRebid(PositionState _)
        {
            return new CallFeature[] {
                Properties(new Bid[] { Bid._3H, Bid._3S }, OpenerRebid, forcing1Round: true),

                Shows(Bid._3H, Shape(5), Partner(IsLastBid(Bid._3D))),
                Shows(Bid._3S, Shape(5), Partner(IsLastBid(Bid._3D))),

                Shows(Bid._4H, Fit8Plus),
                Shows(Bid._4S, Fit8Plus),
                
                Shows(Bid._3NT),
            };
        }
    
        public static PositionCalls OpenerRebid(PositionState ps)
        { 
            return new PositionCalls(ps).AddRules(
        
                Shows(Bid._4H, Fit8Plus),
                Shows(Bid._4S, Fit8Plus),
                Shows(Bid._3NT, Fit(Suit.Hearts, false), Fit(Suit.Spades, false))
            );
        }
    }

}
