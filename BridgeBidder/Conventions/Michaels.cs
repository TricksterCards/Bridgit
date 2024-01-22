namespace BridgeBidding
{

    public class Michaels : Bidder
    {
 
        public static CallFeature[] InitiateConvention(PositionState ps)
        {
            return new CallFeature[]
             {
                // TODO: Response with interference???  Lots of work here...
                // TODO: Need some minimum points...
                PartnerBids(Bid.TwoClubs, RespondMajors),
                Forcing(Bid.TwoClubs, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid.TwoDiamonds, RespondMajors),
                Forcing(Bid.TwoDiamonds, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid.TwoHearts, (PositionState _) => { return ResopondMajorMinor(Suit.Spades); }),
                Forcing(Bid.TwoHearts, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Spades, Suit.Clubs)),
                Forcing(Bid.TwoHearts, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Spades, Suit.Diamonds)),

                PartnerBids(Bid.TwoSpades, (PositionState _) => { return ResopondMajorMinor(Suit.Hearts); }),
                Forcing(Bid.TwoSpades, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Hearts, Suit.Clubs)),
                Forcing(Bid.TwoSpades, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Hearts, Suit.Diamonds)),

             };
         }

        private static CallFeature[] RespondMajors(PositionState _)
        {
            return new CallFeature[]
            {
                Signoff(Bid.TwoHearts, BetterThan(Suit.Spades), Points((0, 5))),
                Signoff(Bid.TwoSpades, BetterOrEqualTo(Suit.Hearts), Points((0, 5))),
            };
        }

        private static CallFeature[] ResopondMajorMinor(Suit majorSuit)
        {
            // TODO: Do something here ...
            return new CallFeature[0];
        }
    }
}
