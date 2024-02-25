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

    public static bool IsValidAuction(List<Call> initialAuction, IEnumerable<Call> desiredCalls, out string error)
    {
        if (!ContractState.IsValidAuction(Direction.N, initialAuction, out error))
        {
            return false;
        }
        foreach (var call in desiredCalls)
        {
            var newAuction = new List<Call>(initialAuction);
            newAuction.Add(call);
            if (!ContractState.IsValidAuction(Direction.N, newAuction, out error))
            {
                return false;
            }
        }
        error = null;
        return true;
    }

    public static Game NewTest(int boardNumber, bool singleHand, List<Call> initialAuction,
                            Call? desiredCall, bool completeAuction)
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
            if (Matches(game, initialAuction, desiredCall, completeAuction)) return game;
        }
    }

    private static bool Matches(Game game, List<Call> initialAuction, Call? desiredCall, bool completeAuction)
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
            (desiredCall != null && !finalCall.Call.Equals(desiredCall)))
        {
            return false;
        }
        bs.MakeCall(finalCall);
        while (completeAuction && !bs.Contract.AuctionComplete)
        {
            var choices = bs.GetCallChoices();
            if (choices.BestCall == null)
            {
                // TODO: This is not good.  Need to do something more here...
                return false;
            }
            bs.MakeCall(choices.BestCall);
        }
        return true;
    }

    public static void CompleteAuction(Game game)
    {
        var bs = new BiddingState(game);
        while (!bs.Contract.AuctionComplete)
        {
            var choices = bs.GetCallChoices();
            if (choices.BestCall == null)
            {
                // TODO: Maybe don't throw here, but do something else...
                throw new Exception("No call available");
            }
            bs.MakeCall(choices.BestCall);
        }
    }
}
