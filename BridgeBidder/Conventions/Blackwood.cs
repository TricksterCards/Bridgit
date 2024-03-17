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
			Suit suit;
			if (TryGetAgreedSuit(ps, out suit))
			{
				bids.Add(Properties(Bid._4NT, RespondKeyCards, convention: UserText.Blackwood, forcing1Round: true));
				bids.Add(Shows(Bid._4NT, PairPoints(suit, SlamOrBetter)));
				// TODO: Add DOPI and DEPO but for now just ignore double and punt on interference...
			}
			return bids;
		}

		private static bool TryGetAgreedSuit(PositionState ps, out Suit suit)
		{
			if (ps.PairState.LastShownSuit.HasValue)
			{
				suit = ps.PairState.LastShownSuit.Value;
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
					Properties(new Call[] { Bid._5C, Bid._5D, Bid._5H, Bid._5S }, PlaceContract, forcing1Round: true),
					Shows(Bid._5C, KeyCards(suit, 1, 4)),
					Shows(Bid._5D, KeyCards(suit, 0, 3)),
					Shows(Bid._5H, KeyCards(suit, 2, 5, hasQueen: false)),
					Shows(Bid._5S, KeyCards(suit, 2, 5, hasQueen: true))
				);
				return choices;
			}
			throw new System.Exception("This should never happen.  No agreed suiit.");
			
		}

		// TODO: Need to ask for queen if that is important...
		// TODO: There needs to be somewhere that we ask for kings...
		public static PositionCalls PlaceContract(PositionState ps)
		{
			Suit suit;
			if (TryGetAgreedSuit(ps, out suit))
			{
				return new PositionCalls(ps).AddRules(
					Properties(Bid._5NT, RespondKings, forcing1Round: true),
					Shows(Bid._5NT, PairKeyCards(suit, true, 5), PairPoints(GrandSlam)),

					Shows(new Bid(6, suit), PairPoints(SlamOrBetter), PairKeyCards(suit, null, 4, 5)),

					Shows(Call.Pass, ContractIsAgreedStrain, PairKeyCards(suit, null, 0, 1, 2, 3)),

					Shows(new Bid(5, suit), PairKeyCards(suit, null, 0, 1, 2, 3)),
					Shows(new Bid(6, suit), IsNonJump, PairKeyCards(suit, null, 0, 1, 2, 3))
				);
			}
			throw new System.Exception("This should not happen");
		}

		public static PositionCalls RespondKings(PositionState ps)
		{
			return new PositionCalls(ps).AddRules(
				Properties(new Bid[] { Bid._6C, Bid._6D, Bid._6H, Bid._6S }, TryGrandSlam, forcing1Round: true),
				Shows(Bid._6C, Kings(0, 4)),
				Shows(Bid._6D, Kings(1)),
				Shows(Bid._6H, Kings(2)),
				Shows(Bid._6S, Kings(3))
			);
		}

		public static PositionCalls TryGrandSlam(PositionState ps)
		{
			Suit suit;
			if (TryGetAgreedSuit(ps, out suit))
			{
				return new PositionCalls(ps).AddRules(
					Shows(new Bid(7, suit), PairKeyCards(suit, true, 5), PairKings(4)),

					Shows(Call.Pass, ContractIsAgreedStrain),

					Shows(new Bid(6, suit)),

					// Well, we blew it.  Gotta go to 7 anyway since nowhere else to go.  
					Shows(new Bid(7, suit))
				);
			}
			throw new System.Exception("This should not happen");
		}

	}
}
