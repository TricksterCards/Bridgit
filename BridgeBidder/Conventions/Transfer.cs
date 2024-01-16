using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    public class TransferBidder : OneNoTrumpBidder
	{
		public TransferBidder(NoTrumpDescription ntd) : base(ntd) { }


		public static BidRulesFactory InitiateConvention(NoTrumpDescription ntd)
		{
			return new TransferBidder(ntd).Initiate;
		}

		private IEnumerable<BidRule> Initiate(PositionState _)
		{
			return new BidRule[] {
				PartnerBids(AcceptTransfer),
				// For weak hands, transfer to longest major.
				// For invitational hands, 5/5 transfer to hearts then bid spades
				// For game-going hands 5/5 transfer to spades then bid 3H
				Forcing(Bid.TwoDiamonds, NTD.RR.LessThanInvite, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades), ShowsSuit(Suit.Hearts)),
				Forcing(Bid.TwoDiamonds, NTD.RR.InviteGame, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 5), ShowsSuit(Suit.Hearts)),
				Forcing(Bid.TwoDiamonds, NTD.RR.GameOrBetter, Shape(Suit.Hearts, 5, 11), Shape(Suit.Spades, 0, 4), ShowsSuit(Suit.Hearts)),

				Forcing(Bid.TwoHearts, NTD.RR.LessThanInvite, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts), ShowsSuit(Suit.Spades)),
				Forcing(Bid.TwoHearts, NTD.RR.InviteGame, Shape(Suit.Spades, 5, 11), Shape(Suit.Hearts, 0, 4), ShowsSuit(Suit.Spades)),
				Forcing(Bid.TwoHearts, NTD.RR.GameOrBetter, Shape(Suit.Spades, 5, 11), ShowsSuit(Suit.Spades)),

				// TODO: Solid long minors are lots of tricks.  Need logic for those....
				// TODO: 4-way transfers...
				Forcing(Bid.TwoSpades, NTD.RR.LessThanInvite, Shape(Suit.Clubs, 6, 11)),
				Forcing(Bid.TwoSpades, NTD.RR.LessThanInvite, Shape(Suit.Diamonds, 6, 11))
			};
		}

		private BidChoices AcceptTransfer(PositionState ps)
		{
			if (ps.RHO.Bid != null) {
				// TOOD: Better interferrence....
				return ps.PairState.BiddingSystem.GetBidChoices(ps);	
			}
			var choices = new BidChoices(ps);
			if (ps.RHO.Doubled) {
				choices.AddRules(new BidRule[] {
					PartnerBids(Call.Pass, OpenerShowsTwo),	// Explicit Pass rule here

					Nonforcing(Call.Pass, Partner(LastBid(2, Suit.Diamonds)), Shape(Suit.Hearts, 0, 2)),
					Nonforcing(Call.Pass, Partner(LastBid(2, Suit.Hearts)), Shape(Suit.Spades, 0, 2)),
				});
			}
			choices.AddRules(new BidRule[] {
				PartnerBids(ExplainTransfer),

				Nonforcing(Bid.ThreeHearts, ShowsTrump(), Partner(LastBid(2, Suit.Diamonds)), NTD.OR.SuperAccept, Shape(4, 5)),
				Nonforcing(Bid.ThreeSpades, ShowsTrump(), Partner(LastBid(2, Suit.Hearts)), NTD.OR.SuperAccept, Shape(4, 5)),

				Nonforcing(Bid.TwoHearts, Partner(LastBid(2, Suit.Diamonds))),
				Nonforcing(Bid.TwoSpades, Partner(LastBid(2, Suit.Hearts))),

				Nonforcing(Bid.ThreeClubs, Partner(LastBid(2, Suit.Spades)))
			});
			return choices;
		}


		private IEnumerable<BidRule> OpenerShowsTwo(PositionState ps)
		{
			// TODO: Need to either bid our suit or NT if stopped...  Partner (opener) has passed transfer
			// showing exactly two of the major being transferred to...
			throw new NotImplementedException();
		}

		private IEnumerable<BidRule> ExplainTransfer(PositionState _)
		{
			return new BidRule[] {
				PartnerBids(OpenerRebid),

				// This can happen if we are 5/5 with invitational hand. Show Spades
				// TODDO: Higher prioiryt than other bids.  Seems reasonable...
				Invitational(Bid.TwoSpades, NTD.RR.InviteGame, Shape(5, 11)),

				Forcing(Bid.ThreeHearts, NTD.RR.GameOrBetter, Partner(LastBid(Bid.TwoSpades)), Shape(5)),
				Signoff(Bid.FourHearts, NTD.RR.GameIfSuperAccept, Partner(LastBid(Bid.ThreeHearts))),
				Signoff(Bid.FourSpades, NTD.RR.GameIfSuperAccept, Partner(LastBid(Bid.ThreeSpades))),

				Invitational(Bid.TwoNoTrump, NTD.RR.InviteGame, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 5)),
				Invitational(Bid.TwoNoTrump, NTD.RR.InviteGame, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 5)),

				Signoff(Bid.ThreeDiamonds, Partner(LastBid(Bid.ThreeClubs)), Shape(Suit.Diamonds, 6, 11)),


				Invitational(Bid.ThreeHearts, NTD.RR.InviteGame, Partner(LastBid(Bid.TwoHearts)), Shape(6, 11)),
				Invitational(Bid.ThreeSpades, NTD.RR.InviteGame, Partner(LastBid(Bid.TwoSpades)), Shape(6, 11)),

				Signoff(Bid.ThreeNoTrump, NTD.RR.Game, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 5)),
				Signoff(Bid.ThreeNoTrump, NTD.RR.Game, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 5)),

				Signoff(Bid.FourHearts, NTD.RR.Game, Partner(LastBid(Bid.TwoHearts)), Shape(6, 11)),


				Signoff(Bid.FourSpades, NTD.RR.Game, Partner(LastBid(Bid.TwoSpades)), Shape(6,11)),

				Invitational(Bid.FourNoTrump, NTD.RR.InviteSlam, Partner(LastBid(Bid.TwoHearts)), Shape(Suit.Hearts, 5)),
				Invitational(Bid.FourNoTrump, NTD.RR.InviteSlam, Partner(LastBid(Bid.TwoSpades)), Shape(Suit.Spades, 5)),

				// TODO: What about slam invite with 6+ of major.  Bid 5M?
				// TODO:  Grand slame, etc...
				Signoff(Call.Pass, NTD.RR.LessThanInvite)
            };
		}


		private IEnumerable<BidRule> OpenerRebid(PositionState _)
		{
			return new BidRule[] {
				PartnerBids(Bid.ThreeHearts, PlaceGameContract, LastBid(Bid.TwoSpades)),
				PartnerBids(Bid.ThreeSpades, PlaceGameContract, LastBid(Bid.TwoHearts)),

				// TODO: Make lower priority???  
				Signoff(Bid.Pass, LastBid(Bid.ThreeClubs), Partner(LastBid(Bid.ThreeDiamonds))),

				// If we have a 5 card suit then show it if invited.  
				Forcing(Bid.ThreeHearts, NTD.OR.AcceptInvite, LastBid(Bid.TwoSpades), Shape(5), Shape(Suit.Spades, 2)),
				Forcing(Bid.ThreeSpades, NTD.OR.AcceptInvite, LastBid(Bid.TwoHearts), Shape(5), Shape(Suit.Hearts, 2)),


				Signoff(Bid.ThreeHearts, NTD.OR.DontAcceptInvite, LastBid(Bid.TwoHearts), Shape(3, 5)),
                Signoff(Bid.ThreeSpades, NTD.OR.DontAcceptInvite, LastBid(Bid.TwoSpades), Shape(3, 5)),

				
				// TODO: Really want to work off of "Partner Shows" instead of PartnerBid...
                Signoff(Bid.FourHearts, NTD.OR.AcceptInvite, Fit()),
				//Signoff(Bid.FourHearts, Points(OpenerRange.AcceptInvite), LastBid(Bid.TwoHearts), Partner(LastBid(Bid.TwoNoTrump)), Shape(3, 5)),
				//Signoff(Bid.FourHearts, LastBid(Bid.TwoHearts), Partner(LastBid(Bid.ThreeNoTrump)), Shape(3, 5)),
				Signoff(Bid.FourHearts, LastBid(Bid.TwoSpades), Partner(LastBid(Bid.ThreeHearts)), Shape(3, 5), BetterOrEqual(Suit.Hearts, Suit.Spades)),


				Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, Partner(LastBid(Bid.ThreeSpades))),
				Signoff(Bid.FourSpades, NTD.OR.AcceptInvite, LastBid(Bid.TwoSpades), Partner(LastBid(Bid.TwoNoTrump)), Shape(3, 5)),
				Signoff(Bid.FourSpades, LastBid(Bid.TwoSpades), Partner(LastBid(Bid.ThreeNoTrump)), Shape(3, 5)),
				Signoff(Bid.FourSpades, LastBid(Bid.TwoHearts), Partner(LastBid(Bid.ThreeSpades)), Shape(3, 5), Better(Suit.Spades, Suit.Hearts)),


                // Didn't fine a suit to play in, so bid game if we have the points...
                Signoff(Bid.ThreeNoTrump,  NTD.OR.AcceptInvite),

				Signoff(Bid.FiveHearts, Partner(LastBid(Bid.FourNoTrump)), Fit(), NTD.OR.DontAcceptInvite),
				Signoff(Bid.FiveSpades, Partner(LastBid(Bid.FourNoTrump)), Fit(), NTD.OR.DontAcceptInvite),


				Signoff(Bid.SixHearts, Partner(LastBid(Bid.FourNoTrump)), Fit(), NTD.OR.AcceptInvite),
				Signoff(Bid.SixSpades, Partner(LastBid(Bid.FourNoTrump)), Fit(), NTD.OR.AcceptInvite),

				// Because this is lower priority it will only happen if there is not a fit
				// so bid 6NT if partner invited to slam with 4NT
				Signoff(Bid.SixNoTrump, Partner(LastBid(Bid.FourNoTrump)), NTD.OR.AcceptInvite),


                // TODO: SLAM BIDDING...
				// GERBER!  
                // I Think here we will just defer to competative bidding.  Then ranges don't matter.  We just look for 
                // shown values and shapes.  By this point everything is pretty clear.  The only thing is do we have a shown
                // fit or is it a known fit.  Perhaps competative bidding can handle this...  
				
			
                Signoff(Call.Pass)

			};
		}

		// Only a change of suit by opener after invitation will get to this code.  Figure out where to play
		private IEnumerable<BidRule> PlaceGameContract(PositionState _)
		{
			return new BidRule[] {
				// If partner has shown 5 hearts or 5 spades then this is game force contract so place
				// it in NT or 4 of their suit.

                Signoff(Bid.FourHearts,  Fit(), ShowsTrump()),
				Signoff(Bid.FourSpades, Fit(), ShowsTrump()),

				Signoff(Bid.ThreeNoTrump)
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

		public IEnumerable<BidRule> InitiateConvention(PositionState _)
		{
			return new BidRule[] {
				PartnerBids(AcceptTransfer),
				// TODO: Need to deal with 5/5 invite, etc.  For now just basic transfers work
				Forcing(Bid.ThreeDiamonds, Shape(Suit.Hearts, 5, 11), Better(Suit.Hearts, Suit.Spades)),

				Forcing(Bid.ThreeHearts, Shape(Suit.Spades, 5, 11), BetterOrEqual(Suit.Spades, Suit.Hearts))

			};
		}
		private IEnumerable<BidRule> AcceptTransfer(PositionState _)
		{
			// TODO: INTERFERRENCE.  DOUBLE...
			return new BidRule[] {
				PartnerBids(ExplainTransfer),

				Nonforcing(Bid.ThreeHearts, Partner(LastBid(Bid.ThreeDiamonds))),
				Nonforcing(Bid.ThreeSpades, Partner(LastBid(Bid.ThreeHearts)))
			};
		}

		private IEnumerable<BidRule> ExplainTransfer(PositionState _)
		{
			return new BidRule[] {
				PartnerBids(PlaceContract),
				Signoff(Bid.Pass, NTB.RespondNoGame),

				Nonforcing(Bid.ThreeNoTrump, NTB.RespondGame, Partner(LastBid(Bid.ThreeHearts)), Shape(Suit.Hearts, 5)),
				Nonforcing(Bid.ThreeNoTrump, NTB.RespondGame, Partner(LastBid(Bid.ThreeSpades)), Shape(Suit.Spades, 5)),

				Signoff(Bid.FourHearts, NTB.RespondGame, Partner(LastBid(Bid.ThreeHearts)), Shape(6, 11)),
				Signoff(Bid.FourSpades, NTB.RespondGame, Partner(LastBid(Bid.ThreeSpades)), Shape(6, 11))

			};
		}

		private static IEnumerable<BidRule> PlaceContract(PositionState _)
		{
			return new BidRule[] {
				Signoff(Bid.FourHearts, Fit()),
				Signoff(Bid.FourSpades, Fit()),
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

		public IEnumerable<BidRule> InitiateConvention(PositionState ps)
		{
			return new BidRule[]
			{
				PartnerBids(Bid.FourDiamonds, p => AcceptTransfer(p, Strain.Hearts)),
				PartnerBids(Bid.FourHearts, p => AcceptTransfer(p, Strain.Spades)),

				Forcing(Bid.FourDiamonds, Shape(Suit.Hearts, 5, 11)),
				Forcing(Bid.FourHearts, Shape(Suit.Spades, 5, 11)),
			};
		}

		public IEnumerable<BidRule> AcceptTransfer(PositionState ps, Strain strain)
		{
			return new BidRule[]
			{
				Nonforcing(new Bid(4, strain))
			};
		}

	}
}
