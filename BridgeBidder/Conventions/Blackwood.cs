using System.Collections.Generic;


namespace BridgeBidding
{


    public class Blackwood : Bidder
	{
        private static (int, int) SlamOrBetter = (32, 100);
	//	private static (int, int) SmallSlam = (32, 35);
        private static (int, int) GrandSlam = (36, 100);

        public static IEnumerable<CallFeature> InitiateConvention(PositionState ps)
		{
			var bids = new List<CallFeature>();
			Strain? strain = ps.PairState.Agreements.AgreedStrain;
			if (strain == null)
			{
				strain = ps.PairState.Agreements.LastShownStrain;
			}
			if (strain != null && Call.StrainToSuit((Strain)strain) is Suit suit)
			{
				bids.Add(Forcing(Bid._4NT, ShowsTrumpSuit(suit), PairPoints(suit, SlamOrBetter)));
				bids.Add(PartnerBids(Bid._4NT, RespondAces));
				// TODO: Add DOPI and DEPO but for now just ignore double and punt on interference...
			}
			return bids;
		}
		public static IEnumerable<CallFeature> RespondAces(PositionState ps)
		{
			return new CallFeature[]
			{
				PartnerBids(PlaceContract),
				Forcing(Bid._5C, ShowsNoSuit, Aces(0, 4)),
				Forcing(Bid._5D, ShowsNoSuit, Aces(1)),
				Forcing(Bid._5H, ShowsNoSuit, Aces(2)),
				Forcing(Bid._5S, ShowsNoSuit, Aces(3)),
			};
		}
		// TODO: There needs to be somewhere that we ask for kings...
		public static IEnumerable<CallFeature> PlaceContract(PositionState ps)
		{
			// If we are missing 2 or more aces return to trump suit if necessary (may just pass).
			// Otherwise 
			return new CallFeature[]
			{
				PartnerBids(Bid._5NT, RespondKings),
				Forcing(Bid._5NT, PairAces(4), PairPoints(GrandSlam)),

				Signoff(Bid._6C, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid._6D, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid._6H, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),
				Signoff(Bid._6S, AgreedStrain(), PairPoints(SlamOrBetter), PairAces(3, 4)),

				Signoff(Call.Pass, ContractIsAgreedStrain(), PairAces(0, 1, 2)),

                Signoff(Bid._5D, AgreedStrain(), PairAces(0, 1, 2)),
                Signoff(Bid._5H, AgreedStrain(), PairAces(0, 1, 2)),
                Signoff(Bid._5S, AgreedStrain(), PairAces(0, 1, 2)),

                Signoff(Bid._6C,  Jump(0), AgreedStrain()),
                Signoff(Bid._6D, Jump(0), AgreedStrain()),
                Signoff(Bid._6H, Jump(0), AgreedStrain()),
            };
		}

		public static IEnumerable<CallFeature> RespondKings(PositionState ps)
		{
			return new CallFeature[]
			{
				PartnerBids(TryGrandSlam),
				Forcing(Bid._6C, ShowsNoSuit, Kings(0, 4)),
				Forcing(Bid._6D, ShowsNoSuit, Kings(1)),
				Forcing(Bid._6H, ShowsNoSuit, Kings(2)),
				Forcing(Bid._6S, ShowsNoSuit, Kings(3)),
			};
		}

		public static IEnumerable<CallFeature> TryGrandSlam(PositionState ps)
		{
			return new CallFeature[]
			{
				Signoff(Bid._7C, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid._7D, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid._7H, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Signoff(Bid._7S, AgreedStrain(), PairPoints(GrandSlam), PairAces(4), PairKings(4)),

				Signoff(Call.Pass, ContractIsAgreedStrain()),

				Signoff(Bid._6D, AgreedStrain()),
				Signoff(Bid._6H, AgreedStrain()),
				Signoff(Bid._6S, AgreedStrain()),

				// We may have no choice but to go to 7.  Perhaps bid 6NT?  Otherwise gotta go 7 clubs->hearts
				Signoff(Bid._7C, AgreedStrain()),
				Signoff(Bid._7D, AgreedStrain()),
				Signoff(Bid._7H, AgreedStrain())
			};
		}

	}
}
