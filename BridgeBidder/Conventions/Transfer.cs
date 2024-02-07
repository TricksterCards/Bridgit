using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
    public class TransferBidder : OneNoTrumpBidder
	{
		public TransferBidder(NoTrumpDescription ntd) : base(ntd) { }


		public static CallFeaturesFactory InitiateConvention(NoTrumpDescription ntd)
		{
			return new TransferBidder(ntd).Initiate;
		}

		private IEnumerable<CallFeature> Initiate(PositionState _)
		{
			return new CallFeature[] {
				Convention(UserText.JacobyTransfer),
				Announce(Bid._2D, UserText.TransferToHearts),
				Announce(Bid._2H, UserText.TransferToSpades),
				Announce(Bid._2S, UserText.TransferToClubs),

				PartnerBids(AcceptTransfer),

				// For weak hands, transfer to longest major.
				// For invitational hands, 5/5 transfer to hearts then bid spades
				// For game-going hands 5/5 transfer to spades then bid 3H
				Forcing(Bid._2D, NTD.RR.LessThanInvite, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades), ShowsSuit(Suit.Hearts)),
				Forcing(Bid._2D, NTD.RR.InviteGame, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 5), ShowsSuit(Suit.Hearts)),
				Forcing(Bid._2D, NTD.RR.GameOrBetter, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 4), ShowsSuit(Suit.Hearts)),

				Forcing(Bid._2H, NTD.RR.LessThanInvite, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts), ShowsSuit(Suit.Spades)),
				Forcing(Bid._2H, NTD.RR.InviteGame, Shape(Suit.Spades, 5, 11), Shape(Suit.Hearts, 0, 4), ShowsSuit(Suit.Spades)),
				Forcing(Bid._2H, NTD.RR.GameOrBetter, Shape(Suit.Spades, 5, 11), ShowsSuit(Suit.Spades)),

				// TODO: Solid long minors are lots of tricks.  Need logic for those....
				// TODO: 4-way transfers...
				Forcing(Bid._2S, NTD.RR.LessThanInvite, Shape(Suit.Clubs, 6, 11)),
				Forcing(Bid._2S, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 6, 11))
			};
		}

		private PositionCalls AcceptTransfer(PositionState ps)
		{
			if (ps.RHO.Bid != null) {
				return ps.PairState.BiddingSystem.GetPositionCalls(ps);	
			}
			var choices = new PositionCalls(ps);
			if (ps.RHO.Doubled) {
				choices.AddRules(new CallFeature[] {
					PartnerBids(Call.Pass, OpenerShowsTwo),	// Explicit Pass rule here

					Nonforcing(Call.Pass, Partner(LastBid(2, Suit.Diamonds)), Shape(Suit.Hearts, 0, 2)),
					Nonforcing(Call.Pass, Partner(LastBid(2, Suit.Hearts)), Shape(Suit.Spades, 0, 2)),
				});
			}
			choices.AddRules(new CallFeature[] {
				PartnerBids(ExplainTransfer),

				Nonforcing(Bid._3H, ShowsTrump, Partner(LastBid(2, Suit.Diamonds)), NTD.OR.SuperAccept, Shape(4, 5)),
				Nonforcing(Bid._3S, ShowsTrump, Partner(LastBid(2, Suit.Hearts)), NTD.OR.SuperAccept, Shape(4, 5)),

				Nonforcing(Bid._2H, Partner(LastBid(2, Suit.Diamonds))),
				Nonforcing(Bid._2S, Partner(LastBid(2, Suit.Hearts))),

				Nonforcing(Bid._3C, Partner(LastBid(2, Suit.Spades)))
			});
			return choices;
		}


		// The opener passed after opponent's X showing only two of suit.
		// Now we need to determine if we want to play in NT or play in
		// our 7-card major.  Need ot have suit stopped.
		private IEnumerable<CallFeature> OpenerShowsTwo(PositionState ps)
		{
			// TODO: Opps stopped needs to understand lead-directing X
			// TODO: Point ranges are not reasonable. GameOrBetter does not
			// take slam into account...
			Debug.Assert(ps.RHO.Bid == null);
			return new CallFeature[] {
				PartnerBids(OpenerRebid),

				Nonforcing(Bid._2NT, NTD.RR.InviteGame, OppsStopped()),
				Nonforcing(Bid._2H, LastBid(Bid._2D), NTD.RR.InviteGame),
				Nonforcing(Bid._2S, LastBid(Bid._2H), NTD.RR.InviteGame),

				Nonforcing(Bid._3NT, NTD.RR.GameOrBetter, OppsStopped()),
				Nonforcing(Bid._4H, LastBid(Bid._2D), NTD.RR.GameOrBetter),
				Nonforcing(Bid._4S, LastBid(Bid._2H), NTD.RR.GameOrBetter),
			};
			
		}

		private IEnumerable<CallFeature> ExplainTransfer(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(OpenerRebid),

				// This can happen if we are 5/5 with invitational hand. Show Spades
				// TODDO: Higher prioiryt than other bids.  Seems reasonable...
				Invitational(Bid._2S, NTD.RR.InviteGame, Shape(5, 11)),

				Forcing(Bid._3H, NTD.RR.GameOrBetter, Partner(LastBid(Bid._2S)), Shape(5)),
				Signoff(Bid._4H, NTD.RR.GameIfSuperAccept, Partner(LastBid(Bid._3H))),
				Signoff(Bid._4S, NTD.RR.GameIfSuperAccept, Partner(LastBid(Bid._3S))),

				Invitational(Bid._2NT, NTD.RR.InviteGame, Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Invitational(Bid._2NT, NTD.RR.InviteGame, Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				Signoff(Bid._3D, Partner(LastBid(Bid._3C)), Shape(Suit.Diamonds, 6, 11)),


				Invitational(Bid._3H, NTD.RR.InviteGame, Partner(LastBid(Bid._2H)), Shape(6, 11), ShowsTrump),
				Invitational(Bid._3S, NTD.RR.InviteGame, Partner(LastBid(Bid._2S)), Shape(6, 11), ShowsTrump),

				Nonforcing(Bid._3NT, NTD.RR.Game, Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Nonforcing(Bid._3NT, NTD.RR.Game, Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				Signoff(Bid._4H, NTD.RR.Game, Partner(LastBid(Bid._2H)), Shape(6, 11), ShowsTrump),
				Signoff(Bid._4S, NTD.RR.Game, Partner(LastBid(Bid._2S)), Shape(6, 11), ShowsTrump),

				Invitational(Bid._4NT, NTD.RR.InviteSlam, Partner(LastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Invitational(Bid._4NT, NTD.RR.InviteSlam, Partner(LastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				// TODO: What about slam invite with 6+ of major.  Bid 5M?
				// TODO:  Grand slame, etc...
				Signoff(Call.Pass, NTD.RR.LessThanInvite)
            };
		}


		private IEnumerable<CallFeature> OpenerRebid(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(Bid._3H, PlaceGameContract, LastBid(Bid._2S)),
				PartnerBids(Bid._3S, PlaceGameContract, LastBid(Bid._2H)),

				// TODO: Make lower priority???  
				Signoff(Bid.Pass, LastBid(Bid._3C), Partner(LastBid(Bid._3D))),

				// If we have a 5 card suit then show it if invited.  
				Forcing(Bid._3H, NTD.OR.AcceptInvite, LastBid(Bid._2S), Shape(5), Shape(Suit.Spades, 2)),
				Forcing(Bid._3S, NTD.OR.AcceptInvite, LastBid(Bid._2H), Shape(5), Shape(Suit.Hearts, 2)),


				Signoff(Bid._3H, NTD.OR.DontAcceptInvite, LastBid(Bid._2H), Shape(3, 5)),
                Signoff(Bid._3S, NTD.OR.DontAcceptInvite, LastBid(Bid._2S), Shape(3, 5)),

				
				// TODO: Really want to work off of "Partner Shows" instead of PartnerBid...
                Signoff(Bid._4H, ShowsTrump, NTD.OR.AcceptInvite, Fit()),
				Signoff(Bid._4H, ShowsTrump, NTD.OR.AcceptInvite, LastBid(Bid._2H), Partner(LastBid(Bid._2NT)), Shape(3, 5)),
				Signoff(Bid._4H, ShowsTrump, LastBid(Bid._2H), Partner(LastBid(Bid._3NT)), Shape(3, 5)),
				Signoff(Bid._4H, ShowsTrump, LastBid(Bid._2S), Partner(LastBid(Bid._3H)), Shape(3, 5), BetterOrEqual(Suit.Hearts, Suit.Spades)),


				Signoff(Bid._4S, ShowsTrump, NTD.OR.AcceptInvite, Partner(LastBid(Bid._3S))),
				Signoff(Bid._4S, ShowsTrump, NTD.OR.AcceptInvite, LastBid(Bid._2S), Partner(LastBid(Bid._2NT)), Shape(3, 5)),
				Signoff(Bid._4S, ShowsTrump, LastBid(Bid._2S), Partner(LastBid(Bid._3NT)), Shape(3, 5)),
				Signoff(Bid._4S, ShowsTrump, LastBid(Bid._2H), Partner(LastBid(Bid._3S)), Shape(3, 5), Better(Suit.Spades, Suit.Hearts)),

				// TODO: Should this call add ShowsTrump since we don't want to play in 
				// any trump suit?  
                // Didn't fine a suit to play in, so bid game if we have the points...
                Signoff(Bid._3NT,  NTD.OR.AcceptInvite),

				Signoff(Bid._5H, ShowsTrump, Partner(LastBid(Bid._4NT)), Fit(), NTD.OR.DontAcceptInvite),
				Signoff(Bid._5S, ShowsTrump, Partner(LastBid(Bid._4NT)), Fit(), NTD.OR.DontAcceptInvite),


				Signoff(Bid._6H, ShowsTrump, Partner(LastBid(Bid._4NT)), Fit(), NTD.OR.AcceptInvite),
				Signoff(Bid._6S, ShowsTrump, Partner(LastBid(Bid._4NT)), Fit(), NTD.OR.AcceptInvite),

				// TODO: AGAIN- should this "SHOWTRUMP?"
				// Because this is lower priority it will only happen if there is not a fit
				// so bid 6NT if partner invited to slam with 4NT
				Signoff(Bid._6NT, Partner(LastBid(Bid._4NT)), NTD.OR.AcceptInvite),


                // TODO: SLAM BIDDING...
				// GERBER!  
                // I Think here we will just defer to competative bidding.  Then ranges don't matter.  We just look for 
                // shown values and shapes.  By this point everything is pretty clear.  The only thing is do we have a shown
                // fit or is it a known fit.  Perhaps competative bidding can handle this...  
				
			
                Signoff(Call.Pass)

			};
		}

		// Only a change of suit by opener after invitation will get to this code.  Figure out where to play
		private IEnumerable<CallFeature> PlaceGameContract(PositionState _)
		{
			return new CallFeature[] {
				// If partner has shown 5 hearts or 5 spades then this is game force contract so place
				// it in NT or 4 of their suit.

                Signoff(Bid._4H,  Fit(), ShowsTrump),
				Signoff(Bid._4S, Fit(), ShowsTrump),

				Signoff(Bid._3NT)
			};
		}
	}

	public class Transfer2NT : Bidder
	{
		private TwoNoTrump NTB;
		public Transfer2NT(TwoNoTrump ntb)
		{
			this.NTB = ntb;
		}

		public IEnumerable<CallFeature> InitiateConvention(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(AcceptTransfer),
				// TODO: Need to deal with 5/5 invite, etc.  For now just basic transfers work
				Forcing(Bid._3D, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades)),

				Forcing(Bid._3H, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts))

			};
		}
		private IEnumerable<CallFeature> AcceptTransfer(PositionState _)
		{
			// TODO: INTERFERRENCE.  DOUBLE...
			return new CallFeature[] {
				PartnerBids(ExplainTransfer),

				Nonforcing(Bid._3H, Partner(LastBid(Bid._3D))),
				Nonforcing(Bid._3S, Partner(LastBid(Bid._3H)))
			};
		}

		private IEnumerable<CallFeature> ExplainTransfer(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(PlaceContract),
				Signoff(Bid.Pass, NTB.RespondNoGame),

				Nonforcing(Bid._3NT, NTB.RespondGame, Partner(LastBid(Bid._3H)), Shape(Suit.Hearts, 5)),
				Nonforcing(Bid._3NT, NTB.RespondGame, Partner(LastBid(Bid._3S)), Shape(Suit.Spades, 5)),

				Signoff(Bid._4H, NTB.RespondGame, Partner(LastBid(Bid._3H)), Shape(6, 11), ShowsTrump),
				Signoff(Bid._4S, NTB.RespondGame, Partner(LastBid(Bid._3S)), Shape(6, 11), ShowsTrump)

			};
		}

		private static IEnumerable<CallFeature> PlaceContract(PositionState _)
		{
			return new CallFeature[] {
				Signoff(Bid._4H, Fit(), ShowsTrump),
				Signoff(Bid._4S, Fit(), ShowsTrump),
				Signoff(Bid.Pass)
			};
		}
	}

	public class Transfer3NT : Bidder
	{
		private ThreeNoTrump NTB;

		public Transfer3NT(ThreeNoTrump ntb)
		{
			this.NTB = ntb;
		}

		public IEnumerable<CallFeature> InitiateConvention(PositionState ps)
		{
			return new CallFeature[]
			{
				PartnerBids(Bid._4D, p => AcceptTransfer(p, Strain.Hearts)),
				PartnerBids(Bid._4H, p => AcceptTransfer(p, Strain.Spades)),

				Forcing(Bid._4D, Shape(Suit.Hearts, 5, 11)),
				Forcing(Bid._4H, Shape(Suit.Spades, 5, 11)),
			};
		}

		public IEnumerable<CallFeature> AcceptTransfer(PositionState ps, Strain strain)
		{
			return new CallFeature[]
			{
				Nonforcing(new Bid(4, strain))
			};
		}

	}
}
