using System.Collections.Generic;


namespace BridgeBidding
{
    public class Gerber : Bidder
	{
		// TODO: These values are wrong.  31 points seems too low....
		private static (int, int) SlamOrBetter = (31, 100);
	//	private static (int, int) SmallSlam = (31, 35);
		private static (int, int) GrandSlam = (36, 100);

		public static StaticConstraint Applies = new SimpleStaticConstraint((call, ps) => 
				ps.Partner.Bid is Bid partnerBid &&
				partnerBid.Strain == Strain.NoTrump && partnerBid.Level < 3, 
				logDescription: "partner's last bid was 1NT or 2NT");


		public static IEnumerable<CallFeature> InitiateConvention(PositionState ps)
		{
			return new CallFeature[]
			{
				Properties(Bid._4C, RespondAces, forcing1Round: true, convention: UserText.Gerber, onlyIf: Gerber.Applies),
				Shows(Bid._4C, Gerber.Applies, PairPoints(SlamOrBetter))
			};
		}
		public static PositionCalls RespondAces(PositionState ps)
		{
			return new PositionCalls(ps).AddRules(
				Properties(new Bid[] { Bid._4D, Bid._4H, Bid._4S, Bid._4NT }, PlaceContract, forcing1Round: true),
				
				Shows(Bid._4D,  Aces(0, 4)),
				Shows(Bid._4H,  Aces(1)),
				Shows(Bid._4S,  Aces(2)),
				Shows(Bid._4NT, Aces(3))
			);
		}
		// TODO: There needs to be somewhere that we ask for kings...
		public static PositionCalls PlaceContract(PositionState ps)
		{
			return new PositionCalls(ps).AddRules(
				Shows(Bid._7NT, PairPoints(GrandSlam), PairAces(4)),
				Shows(Bid._6NT, PairAces(3, 4)),
				Shows(Bid._4NT, PairAces(0, 1, 2))
			);
		}

	}
}
