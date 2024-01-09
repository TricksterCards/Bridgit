using System.Collections.Generic;


namespace BridgeBidding
{


    public class Blackwood : Bidder
	{
        private static (int, int) SlamOrBetter = (32, 100);
		private static (int, int) SmallSlam = (32, 35);
        private static (int, int) GrandSlam = (36, 100);

        public static IEnumerable<BidRule> InitiateConvention(PositionState ps)
		{
			var bids = new List<BidRule>();
			Strain? strain = ps.PairState.Agreements.AgreedStrain;
			if (strain == null)
			{
				strain = ps.PairState.Agreements.LastShownStrain;
			}
			if (strain != null && Call.StrainToSuit((Strain)strain) is Suit suit)
			{
				bids.Add(Forcing(Bid.FourNoTrump, ShowsTrump(suit), PairPoints(suit, SlamOrBetter)));
				bids.Add(PartnerBids(Bid.FourNoTrump, Call.Double, RespondAces));
				// TODO: Add DOPI and DEPO but for now just ignore double and punt on interference...
			}
			return bids;
		}
		public static IEnumerable<BidRule> RespondAces(PositionState ps)
		{
			return new BidRule[]
			{
				DefaultPartnerBids(goodThrough: Call.Double, PlaceContract),
				Forcing(Bid.FiveClubs, ShowsNoSuit(), Aces(0, 4)),
				Forcing(Bid.FiveDiamonds, ShowsNoSuit(), Aces(1)),
				Forcing(Bid.FiveHearts, ShowsNoSuit(), Aces(2)),
				Forcing(Bid.FiveSpades, ShowsNoSuit(), Aces(3)),
			};
		}
		// TODO: There needs to be somewhere that we ask for kings...
		public static IEnumerable<BidRule> PlaceContract(PositionState ps)
		{
			// If we are missing 2 or more aces return to trump suit if necessary (may just pass).
			// Otherwise 
			return new BidRule[]
			{
				PartnerBids(Bid.FiveNoTrump, Call.Double, RespondKings),
				Forcing(Bid.FiveNoTrump, PairAces(4), PairPoints(GrandSlam)),

				Signoff(Bid.SixClubs, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid.SixDiamonds, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid.SixHearts, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid.SixSpades, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),

				Signoff(Call.Pass, ContractIsAgreedStrain(), PairAces(0, 1, 2)),

                Signoff(Bid.FiveDiamonds, AgreedStrain(), PairAces(0, 1, 2)),
                Signoff(Bid.FiveHearts, AgreedStrain(), PairAces(0, 1, 2)),
                Signoff(Bid.FiveSpades, AgreedStrain(), PairAces(0, 1, 2)),

                Signoff(Bid.SixClubs,  Jump(0), AgreedStrain()),
                Signoff(Bid.SixDiamonds, Jump(0), AgreedStrain()),
                Signoff(Bid.SixHearts, Jump(0), AgreedStrain()),
            };
		}

		public static IEnumerable<BidRule> RespondKings(PositionState ps)
		{
			return new BidRule[]
			{
				DefaultPartnerBids(goodThrough: Call.Double, TryGrandSlam),
				Forcing(Bid.SixClubs, ShowsNoSuit(), Kings(0, 4)),
				Forcing(Bid.SixDiamonds, ShowsNoSuit(), Kings(1)),
				Forcing(Bid.SixHearts, ShowsNoSuit(), Kings(2)),
				Forcing(Bid.SixSpades, ShowsNoSuit(), Kings(3)),
			};
		}

		public static IEnumerable<BidRule> TryGrandSlam(PositionState ps)
		{
			return new BidRule[]
			{
				Signoff(Bid.SevenClubs, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid.SevenDiamonds, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid.SevenHearts, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid.SevenSpades, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),

				Signoff(Call.Pass, ContractIsAgreedStrain()),

				Signoff(Bid.SixDiamonds, AgreedStrain()),
				Signoff(Bid.SixHearts, AgreedStrain()),
				Signoff(Bid.SixSpades, AgreedStrain()),

				// We may have no choice but to go to 7.  Perhaps bid 6NT?  Otherwise gotta go 7 clubs->hearts
				Signoff(Bid.SevenClubs, AgreedStrain()),
				Signoff(Bid.SevenDiamonds, AgreedStrain()),
				Signoff(Bid.SevenHearts, AgreedStrain())
			};
		}

	}
}
