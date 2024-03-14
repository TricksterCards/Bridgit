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
				strain = ps.PairState.LastBidStrain();
			}
			if (strain != null && ((Strain)strain).ToSuit() is Suit suit)
			{
				bids.Add(Convention(Bid._4NT, UserText.Blackwood));
				bids.Add(Properties(Bid._4NT, RespondKeyCards, forcing1Round: true));
				bids.Add(Forcing(Bid._4NT, PairPoints(suit, SlamOrBetter)));
				// TODO: Add DOPI and DEPO but for now just ignore double and punt on interference...
			}
			return bids;
		}

		private static bool TryGetAgreedSuit(PositionState ps, out Suit suit)
		{
			Strain? strain = ps.PairState.Agreements.AgreedStrain;
			if (strain is Strain st && st.ToSuit() is Suit s)
			{
				suit = s;
				return true;
			}
			suit = Suit.Clubs;
			return false;
		}
		public static PositionCalls RespondKeyCards(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			Suit suit;
			if (TryGetAgreedSuit(ps, out suit))
			{
				choices.AddRules(
					PartnerBids(PlaceContract),
					Forcing(Bid._5C, KeyCards(suit, 1, 4)),
					Forcing(Bid._5D, KeyCards(suit, 0, 3)),
					Forcing(Bid._5H, KeyCards(suit, 2, 5, hasQueen: false)),
					Forcing(Bid._5S, KeyCards(suit, 2, 5, hasQueen: true))
				);
				return choices;
			}
			throw new System.Exception("This should never happen.  No agreed suiit.");
			
		}
		// TODO: There needs to be somewhere that we ask for kings...
		public static IEnumerable<CallFeature> PlaceContract(PositionState ps)
		{
			Suit suit;
			if (TryGetAgreedSuit(ps, out suit))
			{
				return new CallFeature[]
				{
					PartnerBids(Bid._5NT, RespondKings),
					Forcing(Bid._5NT, PairKeyCards(suit, true, 5), PairPoints(GrandSlam)),

					Shows(new Bid(6, suit), PairPoints(SlamOrBetter), PairKeyCards(suit, null, 4, 5)),

					Shows(Call.Pass, ContractIsAgreedStrain, PairKeyCards(suit, null, 0, 1, 2, 3)),

					Shows(new Bid(5, suit), PairKeyCards(suit, null, 0, 1, 2, 3)),
					Shows(new Bid(6, suit), IsNonJump, PairKeyCards(suit, null, 0, 1, 2, 3))
/*
					Shows(Bid._6C, IsAgreedStrain, PairPoints(SlamOrBetter), PairKeyCards(suit, null, 4, 5)),
					Shows(Bid._6D, IsAgreedStrain, PairPoints(SlamOrBetter), PairAces(3, 4)),
					Shows(Bid._6H, IsAgreedStrain, PairPoints(SlamOrBetter), PairAces(3, 4)),
					Shows(Bid._6S, IsAgreedStrain, PairPoints(SlamOrBetter), PairAces(3, 4)),

					Shows(Call.Pass, ContractIsIsAgreedStrain, PairAces(0, 1, 2)),

					Shows(Bid._5D, IsAgreedStrain, PairAces(0, 1, 2)),
					Shows(Bid._5H, IsAgreedStrain, PairAces(0, 1, 2)),
					Shows(Bid._5S, IsAgreedStrain, PairAces(0, 1, 2)),

					Shows(Bid._6C, NonJump, IsAgreedStrain),
					Shows(Bid._6D, NonJump, IsAgreedStrain),
					Shows(Bid._6H, NonJump, IsAgreedStrain),
					*/
				};
			}
			throw new System.Exception("This should not happen");
		}

		public static IEnumerable<CallFeature> RespondKings(PositionState ps)
		{
			return new CallFeature[]
			{
				PartnerBids(TryGrandSlam),
				Forcing(Bid._6C, Kings(0, 4)),
				Forcing(Bid._6D, Kings(1)),
				Forcing(Bid._6H, Kings(2)),
				Forcing(Bid._6S, Kings(3)),
			};
		}

		public static IEnumerable<CallFeature> TryGrandSlam(PositionState ps)
		{
			return new CallFeature[]
			{
				Shows(Bid._7C, IsAgreedStrain, PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Shows(Bid._7D, IsAgreedStrain, PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Shows(Bid._7H, IsAgreedStrain, PairPoints(GrandSlam), PairAces(4), PairKings(4)),
				Shows(Bid._7S, IsAgreedStrain, PairPoints(GrandSlam), PairAces(4), PairKings(4)),

				Shows(Call.Pass, ContractIsAgreedStrain),

				Shows(Bid._6D, IsAgreedStrain),
				Shows(Bid._6H, IsAgreedStrain),
				Shows(Bid._6S, IsAgreedStrain),

				// We may have no choice but to go to 7.  Perhaps bid 6NT?  Otherwise gotta go 7 clubs->hearts
				Shows(Bid._7C, IsAgreedStrain),
				Shows(Bid._7D, IsAgreedStrain),
				Shows(Bid._7H, IsAgreedStrain)
			};
		}

	}
}
