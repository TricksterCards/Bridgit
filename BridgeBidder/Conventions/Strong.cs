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
                Properties(Bid._2C, Respond, forcing1Round: true, convention: UserText.Strong),

                // TODO: Other reasons for 2-club opening...
                Shows(Bid._2C, Points(StrongOpenRange))
            };
    
        }

        private static PositionCalls Respond(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            if (ps.RHO.Passed) {
                choices.AddRules(
                    Properties(new[] { Bid._2H, Bid._2S, Bid._2NT, Bid._3C, Bid._3D }, 
                               partnerBids: OpenerRebidPositiveResponse, forcingToGame: true),
         
                    Shows(Bid._2H,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Shows(Bid._2S,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Shows(Bid._2NT, Points(PositiveResponse), Balanced),
                    Shows(Bid._3C,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),
                    Shows(Bid._3D,  Points(PositiveResponse), Shape(5, 11), GoodPlusSuit),

                    Properties(Bid._2D, OpenerRebidWaiting, forcing1Round: true), 
                    Shows(Bid._2D,  Points(Waiting))
                );
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

        private static PositionCalls OpenerRebidWaiting(PositionState ps)
        {
            // TODO: Interferrence...
            var choices = new PositionCalls(ps);
            choices.AddRules(TwoNoTrump.After2COpen.Bids(ps));
            // TODO: There should probably never be a 3NT bid after 2C.  This is AG stuff...
            choices.AddRules(ThreeNoTrump.After2COpen.Bids(ps));
            choices.AddRules(
                Properties(new[] { Bid._2H, Bid._2S, Bid._3C, Bid._3D }, Responder2ndBid, forcing1Round: true),

                Shows(Bid._2H, Shape(5, 11)),
                Shows(Bid._2S, Shape(5, 11)),
                Shows(Bid._3C, Shape(5, 11)),
                Shows(Bid._3D, Shape(5, 11))
            );
            return choices;
            // TODO: Next state, more bids, et.....
        }

        private static PositionCalls OpenerRebidPositiveResponse(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(Blackwood.InitiateConvention);
            choices.AddRules(Gerber.InitiateConvention);
            choices.AddRules(
                // Highest priority is to support responder's suit...
                PartnerBids(Responder2ndBid),

                Shows(Bid._3H, Fit8Plus),
                Shows(Bid._3S, Fit8Plus),
                Shows(Bid._4C, Fit8Plus),
                Shows(Bid._4D, Fit8Plus),

				Shows(Bid._2S, Shape(5, 11)),
	// TODO: What about 2NT??			Shows(Bid.TwoUnknown, Balanced, Points(Rebid2NT)),
				Shows(Bid._3C, Shape(5, 11)),
                Shows(Bid._3D, Shape(5, 11)),
                Shows(Bid._3H, Shape(5, 11)),
                Shows(Bid._3S, IsNonJump, Shape(5, 11)),

                Shows(Bid._3NT, Balanced),

              // TODO: 3 NT>>>  Shows(Bid.ThreeUnknown, NonJump),
                Shows(Bid._4C, Shape(5, 11), IsNonJump)

			);
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

                Shows(Bid._3H, Fit8Plus),
                Shows(Bid._3S, Fit8Plus),
                Shows(Bid._4C, Fit8Plus),
                Shows(Bid._4D, Fit8Plus),

                // Now show a bust hand by bidding cheapest minor with less 0-4 points
                Properties(Bid._3C, PartnerIsBust, forcing1Round: true),
                Properties(Bid._3D, PartnerIsBust, forcing1Round: true, onlyIf: Partner(IsLastBid(Bid._3C))),
                Shows(Bid._3C, Points(RespondBust)),
                Shows(Bid._3D, Partner(IsLastBid(Bid._3C)), Points(RespondBust)),

                // Show a 5 card major if we have one.
                Shows(Bid._2S, Shape(5, 11), Points(RespondSuitNotBust)),
                Shows(Bid._3H, Shape(5, 11), Points(RespondSuitNotBust)),
                Shows(Bid._3S, IsNonJump, Shape(5, 11), Points(RespondSuitNotBust)),

                // TODO: What about minors?  3D could be natural if opener doesn't bid 3C...

                // Final bid if we're 
                Shows(Bid._3NT, Points(RespondNTNotBust)) 
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
				Shows(Bid._4H, Fit8Plus),  // TODO: Limit points...???
				Shows(Bid._4S, Fit8Plus),
				Shows(Bid._4C, Fit8Plus),
				Shows(Bid._4D, Fit8Plus),
                Shows(Bid._3NT),
                Shows(Call.Pass)  // If we get here then we are already in game...
			});
            return bids;
        }
		private static PositionCalls PartnerIsBust(PositionState ps)
		{
			var choices = new PositionCalls(ps);
			// Bust does not need this -> bids.AddRange(Blackwood.InitiateConvention(ps));
			// TODO: Perhaps gerber too???  Not sure...
			choices.AddRules(
			
				Shows(Bid._4H, IsRebid, Points(GameInHand)),
				Shows(Bid._4S, IsRebid, Points(GameInHand)),
                Shows(Bid._5C, IsRebid, Shape(7, 11), Points(GameInHand)),
                Shows(Bid._5D, IsRebid, Shape(7, 11), Points(GameInHand)),

                Shows(Bid._3NT, Points(GameInHand)),

                // Bust partner so return to or original suit...
                Shows(Bid._3H, IsRebid),
                Shows(Bid._3S, IsRebid),
                Shows(Bid._4C, IsRebid),
                Shows(Bid._4D, IsRebid)
			);
			return choices;
		}

	}

}

