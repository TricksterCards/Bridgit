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
        DisplayGame();
        var bidSystem = new TwoOverOneGameForce();
        var bs = new BiddingState(Game, bidSystem, bidSystem);
        var auction = Game.GetAuction();
        var bidIndex = 1;
        foreach (var a in auction)
        {
            var choices = bs.GetCallChoices();
            if (bs.NextToAct.HasHand)
            {
                if (choices.BestCall == null || !choices.BestCall.Call.Equals(a.Call))
                {
                    DisplayAuction(auction, false, bidIndex);
                    if (choices.BestCall == null) 
                    {
                        Console.WriteLine(" <- Invalid state.  No call suggested");
                    }
                    else
                    {
                        Console.WriteLine($" <- bridgit suggested {choices.BestCall.Call}");
                    }
                    // NOTE:  PLACE A BREAKPOINT AT THE LINE FOLLOWING THIS COMMENT.  YOU CAN THEN TRACE
                    // THROUGH THE CREATION OF THE CHOICES AND THE SELECTION OF THE BEST CALL
                    choices = bs.DEBUG_ReEvaluateCallChoices();
                    Console.WriteLine($"Forcing {a.Call} to be made and continuing");
                    bs.MakeCall(a.Call);
                }
                else
                {
                    bs.MakeCall(choices.BestCall);
                }
            } 
            else 
            {
                // As long as the call is valid, we're happy
                bs.MakeCall(a.Call);
            }
            bidIndex++;
        }
        DisplayAuction(auction);
    }


    public void DisplayGame()
    {
        Console.WriteLine($"{Game.Tags["Event"]}");
        Console.WriteLine($"{Game.Dealer} deals, {Game.Vulnerable} vulnerable");
        DisplayHands(Game.Deal);
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
        var auction = Auction.FromGame(Game);
        DisplayAuction(auction, true);
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
                        auction.UpdateGame(Game);
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
                    auction.UpdateGame(Game);
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
    public void DisplayAuction(Auction auction, bool showBidNumbers = false, int stopAtIndex = int.MaxValue)
    {
        Console.WriteLine(showBidNumbers ? "   West     North    East     South" : "West  North East  South");
        var direction = Direction.W;
        int col = 0;
        int bidIndex = 1;
        while (direction != auction.FirstToAct)
        {
            Console.Write(showBidNumbers? "         " : "      ");
            col += 1;
            direction = BridgeBidder.LeftHandOpponent(direction);
        }
        foreach (var call in auction.Calls)
        {
            Console.Write(showBidNumbers? $"{bidIndex, 2}:{call, -6}" : $"{call, -6}");
            if (bidIndex == stopAtIndex) return;
            col ++;
            bidIndex ++;
            if (col % 4 == 0) Console.WriteLine();
        }
        if (col % 4 != 0) Console.WriteLine();
    }


    public void DisplayHands(Dictionary<Direction, Hand> hands)
    {
        Display1Hand(10, hands[Direction.N]);
        Display2Hands(1, hands[Direction.W], 23, hands[Direction.E]);
        Display1Hand(10, hands[Direction.S]);        
    }

    public static string[] SuitRanks(Hand hand)
    {
        if (hand == null)
        {
            return new string[] { "-", "-", "-", "-" };
        }
        return hand.ToString().Split(".");
    }

    public static void Display1Hand(int indent, Hand hand)
    {
        var suitRanks = SuitRanks(hand);
        Debug.Assert(suitRanks.Length == Hand.SuitOrder.Length);
        for (var suitIndex = 0; suitIndex < Hand.SuitOrder.Length; suitIndex++)
        {
            for (int i = 0; i < indent; i++) Console.Write(" ");
            var s = Hand.SuitOrder[suitIndex].ToString().Substring(0, 1);
            Console.WriteLine($"{s}: {suitRanks[suitIndex]}");
        }
    }

    public static void Display2Hands(int indentWest, Hand westHand, int indentEast, Hand eastHand)
    {
        var westSuitRanks = SuitRanks(westHand);
        var eastSuitRanks = SuitRanks(eastHand);
        Debug.Assert(eastSuitRanks.Length == westSuitRanks.Length);
        Debug.Assert(westSuitRanks.Length == Hand.SuitOrder.Length);
        for (var suitIndex = 0; suitIndex < Hand.SuitOrder.Length; suitIndex++)
        {
            for (int i = 0; i < indentWest; i++) Console.Write(" ");
            var s = Hand.SuitOrder[suitIndex].ToString().Substring(0, 1);
            Console.Write($"{s}: {westSuitRanks[suitIndex]}");
            var curIndent = indentWest + 3 + westSuitRanks[suitIndex].Length;
            while (curIndent < indentEast)
            {
                Console.Write(" ");
                curIndent++;
            }
            Console.WriteLine($"{s}: {eastSuitRanks[suitIndex]}");
        }
    }


}
