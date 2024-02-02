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
                PartnerBids(Bid._2C, RespondMajors),
                Forcing(Bid._2C, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid._2D, RespondMajors),
                Forcing(Bid._2D, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid._2H, (PositionState _) => { return ResopondMajorMinor(Suit.Spades); }),
                Forcing(Bid._2H, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Spades, Suit.Clubs)),
                Forcing(Bid._2H, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Spades, Suit.Diamonds)),

                PartnerBids(Bid._2S, (PositionState _) => { return ResopondMajorMinor(Suit.Hearts); }),
                Forcing(Bid._2S, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Hearts, Suit.Clubs)),
                Forcing(Bid._2S, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Hearts, Suit.Diamonds)),

             };
         }

        private static CallFeature[] RespondMajors(PositionState _)
        {
            return new CallFeature[]
            {
                Signoff(Bid._2H, BetterThan(Suit.Spades), Points((0, 5))),
                Signoff(Bid._2S, BetterOrEqualTo(Suit.Hearts), Points((0, 5))),
            };
        }

        private static CallFeature[] ResopondMajorMinor(Suit majorSuit)
        {
            // TODO: Do something here ...
            return new CallFeature[0];
        }
    }
}
