using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class RandomStress
    {
        [TestMethod]
        public void Test10000Deals()
        {
            for (int i = 0; i < 10000; i++)
            {
                var game = new Game();
                game.DealRandomHands();
                var biddingState = new BiddingState(game);  
                while (!biddingState.Contract.AuctionComplete)
                {
                    var callChoices = biddingState.GetCallChoices();
                    var call = callChoices.BestCall;
                    // TODO: This should be removed in the future, but for now
                    // we will allow Pass to be added and it will work.
                    if (call == null)
                    {
                        callChoices.AddPassRule();
                        call = callChoices[Call.Pass];
                    }
                    biddingState.MakeCall(call);
                }
            }
        }

    }
}