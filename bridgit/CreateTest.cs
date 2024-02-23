using System;
using System.ComponentModel.Design;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BridgeBidding;
using BridgeBidding.PBN;

namespace bridgit;

public static class CreateTest
{


    private static Direction OffsetFrom(Direction d, int offset)
    {
        return (Direction)(((int)d + offset) % 4);
    }

    public static Game NewTest(int boardNumber, bool singleHand, List<Call> initialAuction,
                            HashSet<Call> desiredCalls)
    {
        while (true)
        {
            var game = new Game();
            game.SetStandardBoard(boardNumber);
            game.DealRandomHands();
            if (singleHand) 
            {
                Direction bidder = OffsetFrom(game.Dealer, initialAuction.Count);
                foreach (var d in Enum.GetValues<Direction>())
                {
                    if (d != bidder) game.Deal[d] = null;
                }
            }
            if (Matches(game, initialAuction, desiredCalls)) return game;
        }
    }

    private static bool Matches(Game game, List<Call> initialAuction, HashSet<Call> desiredCalls)
    {
        var bs = new BiddingState(game);
        // Here we simply replay the auction up to the point of the desired bid.  If
        // there is a hand, and the auction does not match that hand, then we need to start
        // over and try it again until it does.
        Direction d = game.Dealer;
        foreach (var call in initialAuction)
        {
            if (game.Deal[d] == null)
            {
                bs.MakeCall(call);
            }
            else
            {
                var choices = bs.GetCallChoices();
                var bestCall = choices.BestCall;
                // TODO: Need to kind of blow up if we don't have a bestcall...
                if (bestCall == null || !bestCall.Call.Equals(call))
                {
                    return false;
                }
                bs.MakeCall(bestCall);
            }
            d = d.LeftHandOpponent();
        }
        var finalCall = bs.GetCallChoices().BestCall;
        // TODO: finalCall == null is bad.  Need to do something more here...
        if (finalCall == null || 
            (desiredCalls.Count > 0 && !desiredCalls.Contains(finalCall.Call)))
        {
            return false;
        }
        bs.MakeCall(finalCall);
        return true;
    }
}
