using System.Collections.Generic;


namespace BridgeBidding
{
    public class ThreeNoTrump : Bidder
	{
		public Constraint OpenPoints = And(HighCardPoints(25, 27), Points(25, 28));
		public Constraint RespondNoSlam = Points(0, 5);	// TODO: More slam stuff...

		//    public static Constraint RespondGameOrBetter = Points(5, 40);

		public static ThreeNoTrump Open = new ThreeNoTrump();
		public static ThreeNoTrump After2COpen = new ThreeNoTrump();	



		public CallFeature[] Bids(PositionState ps)
		{

			return new CallFeature[]
			{
                // TODO: Systems on/off through here --- just like 1NT.....
                PartnerBids(Bid._3NT, Respond),
				Shows(Bid._3NT, OpenPoints, Balanced)
			};
		}


		private PositionCalls Respond(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(Gerber.InitiateConvention);
			//TODO: Stayman over 3NT is odd... choices.AddRules(new Stayman3NT(this).InitiateConvention);
			choices.AddRules(new Transfer3NT(this).InitiateConvention);
			choices.AddRules(new Natural3NT(this).Response);
			return choices;
		}

	}

	public class Natural3NT : Bidder
	{
		private ThreeNoTrump NTB;
		public Natural3NT(ThreeNoTrump ntb)
		{
			this.NTB = ntb;
		}

		public IEnumerable<CallFeature> Response(PositionState ps)
		{
			return new CallFeature[]
			{
			     // TODO: Perhaps bid BestSuit() of all the signoff suits... 
             	Shows(Bid._4H, NTB.RespondNoSlam, Shape(5, 11)),
				Shows(Bid._4S, NTB.RespondNoSlam, Shape(5, 11)),

				Shows(Bid.Pass, NTB.RespondNoSlam),
			};
		}
	}

}
