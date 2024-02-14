using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Schema;
using BridgeBidding;
using BridgeBidding.PBN;

namespace bridgit;

public class TestEditor 
{
    public Game Game;
    public TestEditor(Game game)
    {
        this.Game = game;
    }

    private int ActionNextBoard = 0;
    private int ActionEditAuction = -1;

    public void RunAuctionTest()
    {
        Display.Game(Game);
        var calls = Game.Auction.Calls;
        Game.Auction.Clear();
        var bs = new BiddingState(Game);
        var bidIndex = 1;
        foreach (var call in calls)
        {
            var choices = bs.GetCallChoices();
            if (bs.NextToAct.HasHand)
            {
                if (choices.BestCall == null || !choices.BestCall.Call.Equals(call))
                {
                    Display.Auction(Game, false);
                    if (choices.BestCall == null) 
                    {
                        Console.WriteLine($" <- Invalid state.  No call suggested.  {call} expected");
                    }
                    else
                    {
                        Console.WriteLine($" {call} expected. bridgit suggested {choices.BestCall.Call}");
                    }
                    // NOTE:  PLACE A BREAKPOINT AT THE LINE FOLLOWING THIS COMMENT.  YOU CAN THEN TRACE
                    // THROUGH THE CREATION OF THE CHOICES AND THE SELECTION OF THE BEST CALL
                    choices = bs.DEBUG_ReEvaluateCallChoices();
                    Console.WriteLine($"Forcing {call} to be made and continuing");
                    bs.MakeCall(call);
                }
                else
                {
                    bs.MakeCall(choices.BestCall);
                }
            } 
            else 
            {
                // As long as the call is valid, we're happy
                bs.MakeCall(call);
            }
            bidIndex++;
        }
        Display.Auction(Game);
    }





    public int GetUserInput()
    {
        while (true)
        {
            int action = GetUserAction();
            if (action != ActionEditAuction)
            {
                return action;
            }
            EditAuction();
        }
    }


    private void EditAuction()
    {
        var auction = Game.Auction;
        Display.Auction(Game, true);
        Console.Write("Which bid would you like to change? ");

        var choice = Console.ReadLine();
        // If there is no space then it is a number that selects the bid.  If there is a single space
        // then it should be in the form "cur-bid new-call" where cur-bid is the current bid
        // and new-call is the value to replace that bid with.  Note that cur-bid must be a unique
        // bid in the auction so can't just be Pass or, if X is repeated that would simply replace the
        // first one.
        if (choice != null && choice.Split(" ").Length == 2)
        {
            var calls = choice.ToUpper().Split(" ");
            Call oldCall;
            Call newCall;
            if (Call.TryParse(calls[0], out oldCall) && Call.TryParse(calls[1], out newCall))
            {
                for (int i = 0; i < auction.Count; i++)
                {
                    if (auction[i].Call.Equals(oldCall))
                    {
                        auction[i].Call = newCall;
                        Game.UpdateContractFromAuction();
                        return;
                    }
                }
                Console.WriteLine($"ERROR: Did not find {oldCall}");
            }
            else
            {
                Console.WriteLine("ERROR: Unable to parse calls.");
            }
        }
        else
        {
            int bidIndex;
            if (int.TryParse(choice, out bidIndex) && bidIndex > 0 && bidIndex <= auction.Count)
            {
                Console.Write($"What bid should replace {auction[bidIndex-1].Call}? ");
                var newBid = Console.ReadLine();
                Call call;
                if (newBid != null && Call.TryParse(newBid.ToUpper(), out call))
                {
                    auction[bidIndex - 1].Call = call;
                    Game.UpdateContractFromAuction();
                }
                else
                {
                    Console.WriteLine("ERROR: Call value not recognized");
                }
            }
            else
            {
                Console.WriteLine("ERROR:  Input should be bid index or of form \"oldbid newbid\"");
            }
        }
    }

    private int GetUserAction()
    {
        while (true)
        {
            Console.Write("Press enter for next board, E to edit, or enter a board #: ");
            var c = Console.ReadLine();
            if (string.IsNullOrEmpty(c))
            {
                return ActionNextBoard;
            }
            int jumpToBoard;
            c = c.ToUpper();
            if (c.Equals("E"))
            {
                return ActionEditAuction;
            }
            else if (int.TryParse(c, out jumpToBoard) && jumpToBoard > 0)
            {
                return jumpToBoard;
            }
        }
    }
   






}
