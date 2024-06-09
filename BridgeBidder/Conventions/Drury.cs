using System.Collections.Generic;


namespace BridgeBidding
{
    public class Drury : Bidder
	{
        public static IEnumerable<CallFeature> InitiateConvention(PositionState ps, Suit agreedSuit) 
        {
            return new CallFeature[] {
                Properties(Bid._2C, OpenBid2.ResponderRaisedMajor, convention: UserText.Drury, forcing1Round: true),
                // TODO: What is the approporiate range for Drury?  Is is dummy points?  Splinter ever with passed hand or always drury?
                Shows(Bid._2C, Shape(agreedSuit, 3, 7), DummyPoints(agreedSuit, 11, 20))
            };
        }
    }
}