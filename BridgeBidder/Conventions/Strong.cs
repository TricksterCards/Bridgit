using System.Collections.Generic;

namespace BridgeBidding
{
    public class Strong2Clubs : Bidder
    {

        protected static (int, int) StrongOpenRange = (22, 40);
        protected static (int, int) GameInHand = (25, 40);
        protected static (int, int) PositiveResponse = (8, 18);
        protected static (int, int) Waiting = (0, 18);

        protected static (int, int) RespondBust = (0, 4);
        protected static (int, int) RespondSuitNotBust = (5, 7);
        protected static (int, int) RespondNTNotBust = (5, 9);  // TODO: Is this point range right???




        public static IEnumerable<CallFeature> Open(PositionState _)

        {
            return new CallFeature[] {
                Convention(Bid._2C, UserText.Strong),
                PartnerBids(Bid._2C, Respond),

                // TODO: Other reasons for 2-club opening...
                Forcing(Bid._2C, Points(StrongOpenRange), ShowsNoSuit)
            };
    
        }

        private static PositionCalls Respond(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            if (ps.RHO.Passed) {
                choices.AddRules(new CallFeature[] {
                    PartnerBids(OpenerRebidPositiveResponse),
                    PartnerBids(Bid._2D, OpenerRebidWaiting), 

                    Forcing(Bid._2H,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Forcing(Bid._2S,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Forcing(Bid._2NT, Points(PositiveResponse), Balanced()),
                    Forcing(Bid._3C,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Forcing(Bid._3D,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),

                    Forcing(Bid._2D,  Points(Waiting), ShowsNoSuit),
                });
            }
            else if (ps.RHO.Doubled)
            {
                // TODO: Redouble is SOS, what about suit?
                choices.AddPassRule();
            }
            else
            {
                // TODO: What here??? Larry???
                choices.AddPassRule();
            }
            return choices;
        }

        private static IEnumerable<CallFeature> OpenerRebidWaiting(PositionState ps)
        {
            
            var bids = new List<CallFeature>();
            bids.AddRange(TwoNoTrump.After2COpen.Bids(ps));
            bids.AddRange(ThreeNoTrump.After2COpen.Bids(ps));
            bids.AddRange(new CallFeature[]
            {
                PartnerBids(Responder2ndBid),

                Forcing(Bid._2H, Shape(5, 11)),
                Forcing(Bid._2S, Shape(5, 11)),
                Forcing(Bid._3C, Shape(5, 11)),
                Forcing(Bid._3D, Shape(5, 11))
            });
            return bids;
            // TODO: Next state, more bids, et.....
        }

        private static PositionCalls OpenerRebidPositiveResponse(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(Blackwood.InitiateConvention);
            choices.AddRules(Gerber.InitiateConvention);
            choices.AddRules(new CallFeature[]
            {
                // Highest priority is to support responder's suit...
                PartnerBids(Responder2ndBid),

                Forcing(Bid._3H, Fit(), ShowsTrump),
                Forcing(Bid._3S, Fit(), ShowsTrump),
                Forcing(Bid._4C, Fit(), ShowsTrump),
                Forcing(Bid._4D, Fit(), ShowsTrump),

				Forcing(Bid._2S, Shape(5, 11)),
	// TODO: What about 2NT??			Forcing(Bid.TwoUnknown, Balanced(), Points(Rebid2NT)),
				Forcing(Bid._3C, Shape(5, 11)),
                Forcing(Bid._3D, Shape(5, 11)),
                Forcing(Bid._3H, Shape(5, 11)),
                Forcing(Bid._3S, NonJump, Shape(5, 11)),

                Nonforcing(Bid._3NT, Balanced(true)),

              // TODO: 3 NT>>>  Forcing(Bid.ThreeUnknown, NonJump),
                Forcing(Bid._4C, Shape(5, 11), NonJump),
              

			});
            return choices;
        }

        private static PositionCalls Responder2ndBid(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(Blackwood.InitiateConvention);
            choices.AddRules(Gerber.InitiateConvention);
            choices.AddRules(new CallFeature[]
            {
                PartnerBids(OpenerPlaceContract),
                Forcing(Bid._3H, Fit(), ShowsTrump),
                Forcing(Bid._3S, Fit(), ShowsTrump),
                Forcing(Bid._4C, Fit(), ShowsTrump),
                Forcing(Bid._4D, Fit(), ShowsTrump),

                // Now show a bust hand by bidding cheapest minor with less 0-4 points
                PartnerBids(Bid._3C, PartnerIsBust),
                PartnerBids(Bid._3D, PartnerIsBust, Partner(LastBid(Bid._3C))),
                Forcing(Bid._3C, ShowsNoSuit, Points(RespondBust)),
                Forcing(Bid._3D, Partner(LastBid(Bid._3C)), ShowsNoSuit, Points(RespondBust)),

                // Show a 5 card major if we have one.
                Forcing(Bid._3H, Shape(5, 11), Points(RespondSuitNotBust)),
                Forcing(Bid._3S, Shape(5, 11), Points(RespondSuitNotBust)),

                // Final bid if we're 
                Signoff(Bid._3NT, Points(RespondNTNotBust)) 
            });
            choices.AddPassRule();
            return choices;
        }

        private static IEnumerable<CallFeature> OpenerPlaceContract(PositionState ps)
        {
            var bids = new List<CallFeature>();
            bids.AddRange(Blackwood.InitiateConvention(ps));
            // TODO: Perhaps gerber too???  Not sure...
            bids.AddRange( new CallFeature[] 
            {
				Signoff(Bid._4H, Fit(), ShowsTrump),  // TODO: Limit points...???
				Signoff(Bid._4S, Fit(), ShowsTrump),
				Forcing(Bid._4C, Fit(), ShowsTrump),
				Forcing(Bid._4D, Fit(), ShowsTrump),
                Signoff(Bid._3NT),
                Signoff(Call.Pass)  // If we get here then we are already in game...
			});
            return bids;
        }
		private static IEnumerable<CallFeature> PartnerIsBust(PositionState ps)
		{
			var bids = new List<CallFeature>();
			bids.AddRange(Blackwood.InitiateConvention(ps));
			// TODO: Perhaps gerber too???  Not sure...
			bids.AddRange(new CallFeature[]
			{
				Signoff(Bid._4H, LastBid(Bid._2H), Points(GameInHand)),
				Signoff(Bid._4S, LastBid(Bid._2S), Points(GameInHand)),
                Signoff(Bid._5C, LastBid(Bid._3C), Shape(7, 11), Points(GameInHand)),
                Signoff(Bid._5D, LastBid(Bid._3D), Shape(7, 11), Points(GameInHand)),

                Signoff(Bid._3NT, Points(GameInHand)),

                // Bust partner so return to or original suit...
                Signoff(Bid._3H, Rebid),
                Signoff(Bid._3S, Rebid),
                Signoff(Bid._4C, Rebid),
                Signoff(Bid._4D, Rebid)
			});
			return bids;
		}

	}

}

