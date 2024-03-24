using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;

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

				Properties(Bid._2D, forcing1Round: true, announce: UserText.TransferToHearts),
				Properties(Bid._2H, forcing1Round: true, announce: UserText.TransferToSpades),
				Properties(Bid._2S, forcing1Round: true, announce: UserText.TransferToClubs),

				PartnerBids(AcceptTransfer),

				// For weak hands, transfer to longest major.
				// For invitational hands, 5/5 transfer to hearts then bid spades
				// For game-going hands 5/5 transfer to spades then bid 3H
				Shows(Bid._2D, NTD.RR.LessThanInvite, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades)),
				Shows(Bid._2D, NTD.RR.InviteGame, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 5)),
				Shows(Bid._2D, NTD.RR.GameOrBetter, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 4)),

				Shows(Bid._2H, NTD.RR.LessThanInvite, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts)),
				Shows(Bid._2H, NTD.RR.InviteGame, Shape(Suit.Spades, 5, 11), Shape(Suit.Hearts, 0, 4)),
				Shows(Bid._2H, NTD.RR.GameOrBetter, Shape(Suit.Spades, 5, 11)),

				// TODO: Solid long minors are lots of tricks.  Need logic for those....
				// TODO: 4-way transfers...
				Shows(Bid._2S, NTD.RR.LessThanInvite, Shape(Suit.Clubs, 6, 11)),
				Shows(Bid._2S, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 6, 11))
			};
		}

		private PositionCalls AcceptTransfer(PositionState ps)
		{
			if (ps.RHO.Bid != null) {
				return ps.PairState.BiddingSystem.GetPositionCalls(ps);	
			}
			var choices = new PositionCalls(ps);
			if (ps.RHO.Doubled) {			
				choices.AddRules(
					PartnerBids(Call.Pass, OpenerShowsTwo),	// Explicit Pass rule here

					Shows(Call.Pass, Partner(IsLastBid(2, Suit.Diamonds)), Shape(Suit.Hearts, 0, 2)),
					Shows(Call.Pass, Partner(IsLastBid(2, Suit.Hearts)), Shape(Suit.Spades, 0, 2)),

					Shows(Bid._2H, Partner(IsLastBid(2, Suit.Diamonds)), Shape(3, 5)),
					Shows(Bid._2S, Partner(IsLastBid(2, Suit.Hearts)), Shape(3, 5))

				);
			}
			choices.AddRules(
				PartnerBids(ExplainTransfer),

				Shows(Bid._3H, Partner(IsLastBid(2, Suit.Diamonds)), NTD.OR.SuperAccept, Shape(4, 5)),
				Shows(Bid._3S, Partner(IsLastBid(2, Suit.Hearts)), NTD.OR.SuperAccept, Shape(4, 5)),

				Shows(Bid._2H, Partner(IsLastBid(2, Suit.Diamonds))),
				Shows(Bid._2S, Partner(IsLastBid(2, Suit.Hearts))),

				Shows(Bid._3C, Partner(IsLastBid(2, Suit.Spades)))
			);
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
			var suit = ps.LastCall == Bid._2D ? Suit.Hearts : Suit.Spades;
			return new CallFeature[] {
				PartnerBids(OpenerRebid),

				Shows(new Bid(4, suit), NTD.RR.GameOrBetter, OppsNotStopped),
				Shows(new Bid(4, suit), Shape(6, 10), NTD.RR.GameOrBetter),
				Shows(Bid._3NT, NTD.RR.GameOrBetter, OppsStopped),

				Shows(new Bid(3, suit), NTD.RR.InviteGame, OppsNotStopped),
				Shows(new Bid(3, suit), Shape(6, 10), NTD.RR.InviteGame),
				Shows(Bid._2NT, NTD.RR.InviteGame, OppsStopped),

				Shows(new Bid(2, suit))

			};
			
		}

		private IEnumerable<CallFeature> ExplainTransfer(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(OpenerRebid),

				// This can happen if we are 5/5 with invitational hand. Show Spades
				// TODDO: Higher prioiryt than other bids.  Seems reasonable...
				// TODO: This changes when we do 4-way tansferrers...
				Shows(Bid._2S, NTD.RR.InviteGame, Shape(5, 11)),

				Properties(Bid._3H, forcing1Round: true),
				Shows(Bid._3H, NTD.RR.GameOrBetter, Partner(IsLastBid(Bid._2S)), Shape(5)),
				Shows(Bid._4H, NTD.RR.GameIfSuperAccept, Partner(IsLastBid(Bid._3H))),
				Shows(Bid._4S, NTD.RR.GameIfSuperAccept, Partner(IsLastBid(Bid._3S))),

				Shows(Bid._2NT, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Shows(Bid._2NT, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				Shows(Bid._3D, Partner(IsLastBid(Bid._3C)), Shape(Suit.Diamonds, 6, 11)),


				Shows(Bid._3H, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2H)), Shape(6, 11)),
				Shows(Bid._3S, NTD.RR.InviteGame, Partner(IsLastBid(Bid._2S)), Shape(6, 11)),

				Shows(Bid._3NT, NTD.RR.Game, Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Shows(Bid._3NT, NTD.RR.Game, Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				Shows(Bid._4H, NTD.RR.Game, Partner(IsLastBid(Bid._2H)), Shape(6, 11)),
				Shows(Bid._4S, NTD.RR.Game, Partner(IsLastBid(Bid._2S)), Shape(6, 11)),

				Shows(Bid._4NT, NTD.RR.InviteSlam, Partner(IsLastBid(Bid._2H)), Shape(Suit.Hearts, 5)),
				Shows(Bid._4NT, NTD.RR.InviteSlam, Partner(IsLastBid(Bid._2S)), Shape(Suit.Spades, 5)),

				// TODO: What about slam invite with 6+ of major.  Bid 5M?
				// TODO:  Grand slame, etc...
				Shows(Call.Pass, NTD.RR.LessThanInvite)
            };
		}


		private IEnumerable<CallFeature> OpenerRebid(PositionState _)
		{
			return new CallFeature[] {
				// TODO: Make lower priority???  
				Shows(Bid.Pass, IsLastBid(Bid._3C), Partner(IsLastBid(Bid._3D))),
	
				Properties(Bid._3H, PlaceGameContract, forcing1Round: true, onlyIf: IsLastBid(Bid._2S)),
				Properties(Bid._3S, PlaceGameContract, forcing1Round: true, onlyIf: IsLastBid(Bid._2H)),

				// If we have a 5 card suit then show it if invited.  
				Shows(Bid._3H, NTD.OR.AcceptInvite, IsLastBid(Bid._2S), Shape(5), Shape(Suit.Spades, 2)),
				Shows(Bid._3S, NTD.OR.AcceptInvite, IsLastBid(Bid._2H), Shape(5), Shape(Suit.Hearts, 2)),


				Shows(Bid._3H, NTD.OR.DontAcceptInvite, IsLastBid(Bid._2H), Shape(3, 5)),
                Shows(Bid._3S, NTD.OR.DontAcceptInvite, IsLastBid(Bid._2S), Shape(3, 5)),

				
				// TODO: Really want to work off of "Partner Shows" instead of PartnerBid...
                Shows(Bid._4H, NTD.OR.AcceptInvite, Fit()),
				Shows(Bid._4H, NTD.OR.AcceptInvite, IsLastBid(Bid._2H), Partner(IsLastBid(Bid._2NT)), Shape(3, 5)),
				Shows(Bid._4H, IsLastBid(Bid._2H), Partner(IsLastBid(Bid._3NT)), Shape(3, 5)),
				Shows(Bid._4H, IsLastBid(Bid._2S), Partner(IsLastBid(Bid._3H)), Shape(3, 5), BetterOrEqual(Suit.Hearts, Suit.Spades)),


				Shows(Bid._4S, NTD.OR.AcceptInvite, Partner(IsLastBid(Bid._3S))),
				Shows(Bid._4S, NTD.OR.AcceptInvite, IsLastBid(Bid._2S), Partner(IsLastBid(Bid._2NT)), Shape(3, 5)),
				Shows(Bid._4S, IsLastBid(Bid._2S), Partner(IsLastBid(Bid._3NT)), Shape(3, 5)),
				Shows(Bid._4S, IsLastBid(Bid._2H), Partner(IsLastBid(Bid._3S)), Shape(3, 5), Better(Suit.Spades, Suit.Hearts)),

				// TODO: Should this call add ShowsTrump since we don't want to play in 
				// any trump suit?  
                // Didn't fine a suit to play in, so bid game if we have the points...
                Shows(Bid._3NT,  NTD.OR.AcceptInvite),

				Shows(Bid._5H, Partner(IsLastBid(Bid._4NT)), Fit(), NTD.OR.DontAcceptInvite),
				Shows(Bid._5S, Partner(IsLastBid(Bid._4NT)), Fit(), NTD.OR.DontAcceptInvite),


				Shows(Bid._6H, Partner(IsLastBid(Bid._4NT)), Fit(), NTD.OR.AcceptInvite),
				Shows(Bid._6S, Partner(IsLastBid(Bid._4NT)), Fit(), NTD.OR.AcceptInvite),

				// TODO: AGAIN- should this "SHOWTRUMP?"
				// Because this is lower priority it will only happen if there is not a fit
				// so bid 6NT if partner invited to slam with 4NT
				Shows(Bid._6NT, Partner(IsLastBid(Bid._4NT)), NTD.OR.AcceptInvite),


                // TODO: SLAM BIDDING...
				// GERBER!  
                // I Think here we will just defer to competative bidding.  Then ranges don't matter.  We just look for 
                // shown values and shapes.  By this point everything is pretty clear.  The only thing is do we have a shown
                // fit or is it a known fit.  Perhaps competative bidding can handle this...  
				
			
                Shows(Call.Pass)

			};
		}

		// Only a change of suit by opener after invitation will get to this code.  Figure out where to play
		private PositionCalls PlaceGameContract(PositionState ps)
		{
			return new PositionCalls(ps).AddRules(
                Shows(Bid._4H, Fit8Plus),
				Shows(Bid._4S, Fit8Plus),

				Shows(Bid._3NT)
			);
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
				Properties(Bid._3D, AcceptTransfer, forcing1Round: true, announce: UserText.TransferToHearts),
				Properties(Bid._3H, AcceptTransfer, forcing1Round: true, announce: UserText.TransferToSpades),

				// TODO: Need to deal with 5/5 invite, etc.  For now just basic transfers work
				Shows(Bid._3D, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades)),

				Shows(Bid._3H, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts))

			};
		}
		private PositionCalls AcceptTransfer(PositionState ps)
		{
			// TODO: INTERFERRENCE.  DOUBLE...
			return new PositionCalls(ps).AddRules(
				PartnerBids(ExplainTransfer),

				Shows(Bid._3H, Partner(IsLastBid(Bid._3D))),
				Shows(Bid._3S, Partner(IsLastBid(Bid._3H)))
			);
		}

		private IEnumerable<CallFeature> ExplainTransfer(PositionState _)
		{
			return new CallFeature[] {
				PartnerBids(PlaceContract),
				Shows(Bid.Pass, NTB.RespondNoGame),

				Shows(Bid._3NT, NTB.RespondGame, Partner(IsLastBid(Bid._3H)), Shape(Suit.Hearts, 5)),
				Shows(Bid._3NT, NTB.RespondGame, Partner(IsLastBid(Bid._3S)), Shape(Suit.Spades, 5)),

				Shows(Bid._4H, NTB.RespondGame, Partner(IsLastBid(Bid._3H)), Shape(6, 11)),
				Shows(Bid._4S, NTB.RespondGame, Partner(IsLastBid(Bid._3S)), Shape(6, 11))

			};
		}

		private static IEnumerable<CallFeature> PlaceContract(PositionState _)
		{
			return new CallFeature[] {
				Shows(Bid._4H, Fit8Plus),
				Shows(Bid._4S, Fit8Plus),
				Shows(Bid.Pass)
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
				Properties(Bid._4D, p => AcceptTransfer(p, Strain.Hearts)),
				PartnerBids(Bid._4H, p => AcceptTransfer(p, Strain.Spades)),

				Shows(Bid._4D, Shape(Suit.Hearts, 5, 11)),
				Shows(Bid._4H, Shape(Suit.Spades, 5, 11)),
			};
		}

		public PositionCalls AcceptTransfer(PositionState ps, Strain strain)
		{
			return new PositionCalls(ps).AddRules(
				Shows(new Bid(4, strain))
			);
		}

	}
}
