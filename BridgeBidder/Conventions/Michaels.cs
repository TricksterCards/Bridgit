using System;

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
                Properties(new Bid[] { Bid._2C, Bid._2D }, RespondMajors, forcing1Round: true, convention: UserText.Michaels, onlyIf: IsCueBid),
                Shows(Bid._2C, IsCueBid, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5)),
                Shows(Bid._2D, IsCueBid, Shape(Suit.Hearts, 5), Shape(Suit.Spades, 5)),

                Properties(Bid._2H, p => ResopondMajorMinor(p, Suit.Spades), forcing1Round: true, convention: UserText.Michaels, onlyIf: IsCueBid),
                Shows(Bid._2H, IsCueBid, Shape(Suit.Spades, 5), Shape(Suit.Clubs, 5)),
                Shows(Bid._2H, IsCueBid, Shape(Suit.Spades, 5), Shape(Suit.Diamonds, 5)),

                Properties(Bid._2S, p => ResopondMajorMinor(p, Suit.Spades), forcing1Round: true, convention: UserText.Michaels, onlyIf: IsCueBid),
                Shows(Bid._2S, IsCueBid, Shape(Suit.Hearts, 5), Shape(Suit.Clubs, 5)),
                Shows(Bid._2S, IsCueBid, Shape(Suit.Hearts, 5), Shape(Suit.Diamonds, 5)),
             };
         }

        private static PositionCalls RespondMajors(PositionState ps)
        {
            return new PositionCalls(ps).AddRules(
                Shows(Bid._2H, BetterThan(Suit.Spades), Points((0, 5))),
                Shows(Bid._2S, BetterOrEqualTo(Suit.Hearts), Points((0, 5)))
            );
        }

        private static PositionCalls ResopondMajorMinor(PositionState ps, Suit majorSuit)
        {
            // TODO: Do something here ...
            return new PositionCalls(ps);
        }
    }
}
