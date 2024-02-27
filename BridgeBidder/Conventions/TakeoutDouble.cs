using System.Collections.Generic;


namespace BridgeBidding
{
    public class TakeoutDouble: Bidder
    {
        private static (int, int) TakeoutRange = (12, 16);
        private static (int, int) StrongTakeout = (17, 100);
        private static (int, int) MinimumTakeout = (11, 16);
        private static (int, int) MediumTakeout = (17, 19);
        private static (int, int) MaximumTakeout = (20, 100);



        public static IEnumerable<CallFeature> InitiateConvention(PositionState ps)
        {
            var bids = new List<CallFeature>();
            if (ps.IsOpponentsContract && ps.BiddingState.OpeningBid.Strain != Strain.NoTrump)
            {
                var contractBid = ps.BiddingState.Contract.Bid;
                if (contractBid.Level <= 3)
                {
                    bids.AddRange(Takeout(ps, contractBid.Level));
                }
            }
            return bids;
        }


        private static IEnumerable<CallFeature> Takeout(PositionState ps, int Level)
        {
            var bids = new List<CallFeature>
            {
                Convention(Call.Double, UserText.TakeoutDouble),
                PartnerBids(Call.Double, Respond),
                Forcing(Bid.Double, Points(StrongTakeout))
			};


			var rule = Forcing(Bid.Double, Points(TakeoutRange), BidAvailable(4, Suit.Clubs));
			var oppsSummary = PairSummary.Opponents(ps);
			foreach (Suit s in Card.Suits)
			{
                if (oppsSummary.ShownSuits.Contains(s))
                {
                    rule.AddConstraint(Shape(s, 0, 4));
                }
                else
                {
					rule.AddConstraint(Shape(s, 3, 4));
				}
			}
			bids.Add(rule);
              
            // TODO: Should this be 2 or 1 for MinBidLevel?  Or is this really based on opponent bids?
            // If opponenets have shown a weak hand...
            // If opponents have shown a strong hand...
            // If opponents have bid twice...

 
            return bids;

        }

        public static (int, int) MinLevel = (0, 8);
        public static (int, int) NoTrump1 = (6, 10);
        public static (int, int) NoTrump2 = (11, 12);
        public static (int, int) InviteLevel = (9, 11);
        public static (int, int) GameLevel = (12, 40);
        public static (int, int) Game3NT = (13, 40);

        private static PositionCalls Respond(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            choices.AddRules(new CallFeature[]
            {
                PartnerBids(DoublerRebid),

                Signoff(Call.Pass, RuleOf9()),
                // TODO: FOR NOW WE WILL JUST BID AT THE NEXT LEVEL REGARDLESS OF POINTS...
                // TODO: Need LongestSuit...
                // TODO: Should this be TakeoutSuit()...
                Nonforcing(Bid._1D, TakeoutSuit(), Points(MinLevel)),
                Nonforcing(Bid._1H, TakeoutSuit(), Points(MinLevel)),
                Nonforcing(Bid._1S, TakeoutSuit(), Points(MinLevel)),


                Nonforcing(Bid._1NT, Balanced(), OppsStopped(), Points(NoTrump1)),

                Nonforcing(Bid._2C, TakeoutSuit(), Points(MinLevel)),
                Nonforcing(Bid._2D, TakeoutSuit(), Jump(0), Points(MinLevel)),
                Nonforcing(Bid._2D, TakeoutSuit(), Jump(1), Points(InviteLevel)),
                Nonforcing(Bid._2H, TakeoutSuit(), Jump(0), Points(MinLevel)),
                Nonforcing(Bid._2H, TakeoutSuit(), Jump(1), Points(InviteLevel)),
                Nonforcing(Bid._2S, TakeoutSuit(), Jump(0), Points(MinLevel)),
                Nonforcing(Bid._2S, TakeoutSuit(), Jump(1), Points(InviteLevel)),


                Nonforcing(Bid._2NT, Balanced(), OppsStopped(), Points(NoTrump2)),

                // TODO: Game bids
                Signoff(Bid._4H, TakeoutSuit(), Points(GameLevel)),
                Signoff(Bid._4S, TakeoutSuit(), Points(GameLevel)),

                Signoff(Bid._3NT, Balanced(), OppsStopped(), Points(Game3NT)),

                Nonforcing(Call.Pass, Not(RHO(LastBid(Call.Pass))))
            }) ;
            // Many strong bids can be done with pure competition.
            // TODO: Think through this - is this really what we want?
           //  FOR NOW TAKE THIS OUT AND TRY TO COVER THE BASES... choices.AddRules(Compete.CompBids);
            return choices;         
        }




        private static IEnumerable<CallFeature> DoublerRebid(PositionState ps)
        {
            return new CallFeature[]
            {
     
                PartnerBids(AdvancerRebid),


                // TODO: Clean this up... For now just majors...  Clean up range...
                Signoff(Bid._4H, Fit(), PairPoints((25, 30))),
                Signoff(Bid._4S, Fit(), PairPoints((25, 30))),

                // CANT BE - Invitational(Bid._2C, RaisePartner(), Points(MediumTakeout)),
                Invitational(Bid._2D, RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._2H,   RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._2S,   RaisePartner(), DummyPoints(MediumTakeout)),

                Invitational(Bid._3C,    RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._3D, RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._3D, RaisePartner(2), DummyPoints(MaximumTakeout)),
                Invitational(Bid._3H,   RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._3H,   RaisePartner(2), DummyPoints(MaximumTakeout)),
                Invitational(Bid._3S,   RaisePartner(), DummyPoints(MediumTakeout)),
                Invitational(Bid._3S,   RaisePartner(2), DummyPoints(MaximumTakeout)),
                // TODO: Bid new suits for strong hands...  Bid NT?  

                // TODO: Forcing?  What to do here...
                // TODO: What is the lowest suit we could do here?  1C X Pass 1D is all I can think of...
                Invitational(Bid._1H, Shape(5, 11), Points(MediumTakeout)),
                Invitational(Bid._1S, Shape(5, 11), Points(MediumTakeout)),
                Invitational(Bid._2C, Shape(5, 11), Points(MediumTakeout)),
                Invitational(Bid._2D, Shape(5, 11), Points(MediumTakeout)),
                Invitational(Bid._2H, Jump(0), Shape(5, 11), Points(MediumTakeout)),
                Invitational(Bid._2S, Jump(0), Shape(5, 11), Points(MediumTakeout)),

                // TODO: Need stronger bids here...

                Signoff(Call.Pass, Points(MinimumTakeout)),

                // TODO: THIS IS WHERE I START OFF - BB2 - DEAL 21 NEEDS STRONG RESPONSE...
            };
        }

        private static IEnumerable<CallFeature> AdvancerRebid(PositionState ps)
        {
            return Compete.CompBids(ps);
        }
        // TODO: Interference...
        /*
        private static PrescribedBids RespondWithInterference()
        {
            var pb = new PrescribedBids();
            pb.CallFeatures.Add(Signoff(Bid.Pass, new Constraint[0]));   // TODO: Do something here
            return pb;
        }
        */
    }
}
