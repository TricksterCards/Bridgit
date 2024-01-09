namespace BridgeBidding
{

    public class Michaels : Bidder
    {
 
        public static BidRule[] InitiateConvention(PositionState ps)
        {
            return new BidRule[]
             {
                // TODO: Need some minimum points...
                PartnerBids(Bid.TwoClubs, Bid.Pass, RespondMajors),
                Forcing(Bid.TwoClubs, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid.TwoDiamonds, Bid.Pass, RespondMajors),
                Forcing(Bid.TwoDiamonds, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5), ShowsSuits(Suit.Hearts, Suit.Spades)),

                PartnerBids(Bid.TwoHearts, Bid.Pass, (PositionState _) => { return ResopondMajorMinor(Suit.Spades); }),
                Forcing(Bid.TwoHearts, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Spades, Suit.Clubs)),
                Forcing(Bid.TwoHearts, CueBid(), Shape(Suit.Spades, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Spades, Suit.Diamonds)),

                PartnerBids(Bid.TwoSpades, Bid.Pass, (PositionState _) => { return ResopondMajorMinor(Suit.Hearts); }),
                Forcing(Bid.TwoSpades, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Clubs, 5), ShowsSuits(Suit.Hearts, Suit.Clubs)),
                Forcing(Bid.TwoSpades, CueBid(), Shape(Suit.Hearts, 5), Shape(Suit.Diamonds, 5), ShowsSuits(Suit.Hearts, Suit.Diamonds)),

             };
         }

        private static BidRule[] RespondMajors(PositionState _)
        {
            return new BidRule[]
            {
                Signoff(Bid.TwoHearts, BetterThan(Suit.Spades), Points((0, 5))),
                Signoff(Bid.TwoSpades, BetterOrEqualTo(Suit.Hearts), Points((0, 5))),
            };
        }

        private static BidRule[] ResopondMajorMinor(Suit majorSuit)
        {
            // TODO: Do something here ...
            return new BidRule[0];
        }
    }
}
