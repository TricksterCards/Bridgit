using System;
using System.Collections.Generic;

namespace BridgeBidding
{
	public class TwoNoTrump : Bidder
	{
		public Constraint OpenPoints { get; private set; }
		public Constraint RespondNoGame { get; private set; }
		public Constraint RespondGame { get; private set; }
		//    public static Constraint RespondGameOrBetter = Points(5, 40);

		public static TwoNoTrump Open = new TwoNoTrump(20, 21);
		public static TwoNoTrump After2COpen = new TwoNoTrump(22, 24);

		private TwoNoTrump(int min, int max)
		{
			OpenPoints = And(HighCardPoints(min, max), Points(min, max + 1));
			RespondNoGame = Points(0, Math.Max(0, 25 - min - 1));
			RespondGame = Points(Math.Max(0, 25 - min), 31 - min);
			// TODO: More 

		}
		public CallFeature[] Bids(PositionState ps)
		{

			return new CallFeature[]
			{
                // TODO: Systems on/off through here --- just like 1NT.....
                PartnerBids(Bid.TwoNoTrump, Respond),
				Nonforcing(Bid.TwoNoTrump, OpenPoints, Balanced())
			};
		}


		private PositionCalls Respond(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			choices.AddRules(new Stayman2NT(this).InitiateConvention);
			choices.AddRules(new Transfer2NT(this).InitiateConvention);
			choices.AddRules(new Natural2NT(this).Response);
			return choices;
		}

	}

	public class Natural2NT : Bidder
	{
		private TwoNoTrump NTB;
		public Natural2NT(TwoNoTrump ntb)
		{
			this.NTB = ntb;
		}

		public IEnumerable<CallFeature> Response(PositionState ps)
		{
			return new CallFeature[]
			{
			     // TODO: Perhaps bid BestSuit() of all the signoff suits... 
                Signoff(Bid.ThreeClubs, NTB.RespondNoGame, Shape(5, 11), LongestMajor(4)),
				Signoff(Bid.ThreeDiamonds, NTB.RespondNoGame, Shape(5, 11), LongestMajor(4)),
				Signoff(Bid.ThreeHearts, NTB.RespondNoGame, Shape(5, 11)),
				Signoff(Bid.ThreeSpades, NTB.RespondNoGame, Shape(5, 11)),

				Signoff(Bid.Pass, NTB.RespondNoGame),

				Signoff(Bid.ThreeNoTrump, NTB.RespondGame, LongestMajor(4)),

				Signoff(Bid.FourHearts, NTB.RespondGame, Shape(5, 11), BetterThan(Suit.Spades)),
				Signoff(Bid.FourSpades, NTB.RespondGame, Shape(5, 11), BetterOrEqualTo(Suit.Hearts)),
			};
		}
	}
}
