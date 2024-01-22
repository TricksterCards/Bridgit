using System;
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
                PartnerBids(Bid.TwoClubs, Respond),

                // TODO: Other reasons for 2-club opening...
                Forcing(Bid.TwoClubs, Points(StrongOpenRange), ShowsNoSuit())
            };
    
        }

        private static PositionCalls Respond(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            if (ps.RHO.Passed) {
                choices.AddRules(new CallFeature[] {
                    PartnerBids(OpenerRebidPositiveResponse),
                    PartnerBids(Bid.TwoDiamonds, OpenerRebidWaiting), 

                    Forcing(Bid.TwoHearts, Points(PositiveResponse), Shape(5, 11), Quality(SuitQuality.Good, SuitQuality.Solid)),
                    Forcing(Bid.TwoSpades, Points(PositiveResponse), Shape(5, 11), Quality(SuitQuality.Good, SuitQuality.Solid)),
                    Forcing(Bid.TwoNoTrump, Points(PositiveResponse), Balanced()),
                    Forcing(Bid.ThreeClubs, Points(PositiveResponse), Shape(5, 11), Quality(SuitQuality.Good, SuitQuality.Solid)),
                    Forcing(Bid.ThreeDiamonds, Points(PositiveResponse), Shape(5, 11), Quality(SuitQuality.Good, SuitQuality.Solid)),

                    Forcing(Bid.TwoDiamonds, Points(Waiting), ShowsNoSuit()),
                });
            }
            else if (ps.RHO.Doubled)
            {
                // TODO: Redouble is SOS, what about suit?
                throw new NotImplementedException();
            }
            else
            {
                // TODO: What here??? Larry???
                throw new NotImplementedException();
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

                Forcing(Bid.TwoHearts, Shape(5, 11)),
                Forcing(Bid.TwoSpades, Shape(5, 11)),
                Forcing(Bid.ThreeClubs, Shape(5, 11)),
                Forcing(Bid.ThreeDiamonds, Shape(5, 11))
            });
            return bids;
            // TODO: Next state, more bids, et.....
        }

        private static IEnumerable<CallFeature> OpenerRebidPositiveResponse(PositionState ps)
        {
            var bids = new List<CallFeature>();
            bids.AddRange(Blackwood.InitiateConvention(ps));
            bids.AddRange(new CallFeature[]
            {
                // Highest priority is to support responder's suit...
                PartnerBids(Responder2ndBid),

                Forcing(Bid.ThreeHearts, Fit(), ShowsTrump()),
                Forcing(Bid.ThreeSpades, Fit(), ShowsTrump()),
                Forcing(Bid.FourClubs, Fit(), ShowsTrump()),
                Forcing(Bid.FourDiamonds, Fit(), ShowsTrump()),

				Forcing(Bid.TwoSpades, Shape(5, 11)),
	// TODO: What about 2NT??			Forcing(Bid.TwoUnknown, Balanced(), Points(Rebid2NT)),
				Forcing(Bid.ThreeClubs, Shape(5, 11)),
                Forcing(Bid.ThreeDiamonds, Shape(5, 11)),
                Forcing(Bid.ThreeHearts, Shape(5, 11)),
                Forcing(Bid.ThreeSpades, Jump(0), Shape(5, 11)),
              // TODO: 3 NT>>>  Forcing(Bid.ThreeUnknown, Jump(0)),
                Forcing(Bid.FourClubs, Shape(5, 11), Jump(0)),
              

			});
            return bids;
        }

        private static PositionCalls Responder2ndBid(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(Blackwood.InitiateConvention);
            choices.AddRules(new CallFeature[]
            {
                PartnerBids(OpenerPlaceContract),
                Forcing(Bid.ThreeHearts, Fit(), ShowsTrump()),
                Forcing(Bid.ThreeSpades, Fit(), ShowsTrump()),
                Forcing(Bid.FourClubs, Fit(), ShowsTrump()),
                Forcing(Bid.FourDiamonds, Fit(), ShowsTrump()),

                // Now show a bust hand by bidding cheapest minor with less 0-4 points
                PartnerBids(Bid.ThreeClubs, PartnerIsBust),
                PartnerBids(Bid.ThreeDiamonds, PartnerIsBust, Partner(LastBid(Bid.ThreeClubs))),
                Forcing(Bid.ThreeClubs, ShowsNoSuit(), Points(RespondBust)),
                Forcing(Bid.ThreeDiamonds, Partner(LastBid(Bid.ThreeClubs)), ShowsNoSuit(), Points(RespondBust)),

                // Show a 5 card major if we have one.
                Forcing(Bid.ThreeHearts, Shape(5, 11), Points(RespondSuitNotBust)),
                Forcing(Bid.ThreeSpades, Shape(5, 11), Points(RespondSuitNotBust)),

                // Final bid if we're 
                Signoff(Bid.ThreeNoTrump, Points(RespondNTNotBust)) 

            });
            return choices;
        }

        private static IEnumerable<CallFeature> OpenerPlaceContract(PositionState ps)
        {
            var bids = new List<CallFeature>();
            bids.AddRange(Blackwood.InitiateConvention(ps));
            // TODO: Perhaps gerber too???  Not sure...
            bids.AddRange( new CallFeature[] 
            {
				Signoff(Bid.FourHearts, Fit(), ShowsTrump()),  // TODO: Limit points...???
				Signoff(Bid.FourSpades, Fit(), ShowsTrump()),
				Forcing(Bid.FourClubs, Fit(), ShowsTrump()),
				Forcing(Bid.FourDiamonds, Fit(), ShowsTrump()),
                Signoff(Bid.ThreeNoTrump),
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
				Signoff(Bid.FourHearts, LastBid(Bid.TwoHearts), Points(GameInHand)),
				Signoff(Bid.FourSpades, LastBid(Bid.TwoSpades), Points(GameInHand)),
                Signoff(Bid.FiveClubs, LastBid(Bid.ThreeClubs), Shape(7, 11), Points(GameInHand)),
                Signoff(Bid.FiveDiamonds, LastBid(Bid.ThreeDiamonds), Shape(7, 11), Points(GameInHand)),

                Signoff(Bid.ThreeNoTrump, Points(GameInHand)),

                // Bust partner so return to or original suit...
                Signoff(Bid.ThreeHearts, Rebid()),
                Signoff(Bid.ThreeSpades, Rebid()),
                Signoff(Bid.FourClubs, Rebid()),
                Signoff(Bid.FourDiamonds, Rebid())
			});
			return bids;
		}

	}

}

