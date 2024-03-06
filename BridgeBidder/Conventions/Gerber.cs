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
				Convention(Bid._4C, UserText.Gerber),
				PartnerBids(Bid._4C, RespondAces, Gerber.Applies),
				Forcing(Bid._4C, Gerber.Applies, PairPoints(SlamOrBetter))
			};
		}
		public static IEnumerable<CallFeature> RespondAces(PositionState ps)
		{
			return new CallFeature[]
			{
				PartnerBids(PlaceContract),
				
				Forcing(Bid._4D,  Aces(0, 4)),
				Forcing(Bid._4H,  Aces(1)),
				Forcing(Bid._4S,  Aces(2)),
				Forcing(Bid._4NT, Aces(3)),
			};
		}
		// TODO: There needs to be somewhere that we ask for kings...
		public static IEnumerable<CallFeature> PlaceContract(PositionState ps)
		{
			// TODO: Need to ask about kings..... 
			return new CallFeature[]
			{
				Signoff(Bid._7NT, PairPoints(GrandSlam), PairAces(4)),
				Signoff(Bid._6NT, PairAces(3, 4)),
				Signoff(Bid._4NT, PairAces(0, 1, 2))
			};
		}

	}
}
