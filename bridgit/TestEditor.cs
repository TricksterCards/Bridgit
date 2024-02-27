using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Schema;
using BridgeBidding;
using BridgeBidding.PBN;

using static BridgeBidding.PositionCalls;

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

    public static Game[] FailingTests(IEnumerable<Game> games)
    {
        var failures = new List<Game>();
        foreach (var game in games)
        {
            if (!AuctionPasses(game))
            {
                failures.Add(game);
            }
        }
        return failures.ToArray();
    }

    public static bool AuctionPasses(Game game)
    {
        var testGame = game.Clone();
        testGame.Auction.Clear();
        var bs = new BiddingState(testGame);
        foreach (var call in game.Auction.Calls)
        {
            var choices = bs.GetCallChoices();
            if (bs.NextToAct.HasHand)
            {
                if (choices.BestCall == null || !choices.BestCall.Call.Equals(call))
                {
                    return false;
                }
            }
            bs.MakeCall(call);
        }
        return true;
    }

    private class CallRecord
    {
        public PositionCalls PositionCalls;
        public Call DesiredCall;
        public CallDetails? CallDetails = null;
        public bool Failed => CallDetails == null || !CallDetails.Call.Equals(DesiredCall);
        public string Error;
        public int Index;
    }

    public void RunAuctionTest(bool verbose, bool allCallDetails = false)
    {
        var callRecords = new List<CallRecord>();
        Display.Game(Game);
        var calls = Game.Auction.Calls;
        Game.Auction.Clear();
        var bs = new BiddingState(Game);
        var bidIndex = 1;
        foreach (var call in calls)
        {
            var choices = bs.GetCallChoices();
            var callRecord = new CallRecord { PositionCalls = choices, DesiredCall = call, Index = bidIndex };
            if (bs.NextToAct.HasHand)
            {
                if (choices.BestCall == null)
                {
                    callRecord.Error = "No call available";
                }
                else
                {
                    callRecord.CallDetails = choices.BestCall;
                    if (!choices.BestCall.Call.Equals(call))
                    {
                        callRecord.Error = $"suggested {choices.BestCall.Call}";
                    }
                }
            }
            else 
            {
                if (choices.ContainsKey(call))
                {
                    callRecord.CallDetails = choices[call];
                }
                else
                {
                    callRecord.Error = $"No rule for {call}";
                }
            }
            callRecords.Add(callRecord);
            bs.MakeCall(call);
            bidIndex++;
        }
        var failingCalls = callRecords.Where(cr => cr.Failed).ToList();
        Display.Auction(Game, true, failingCalls.Select(c => c.Index));
        if (failingCalls.Count > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Auction fails");
            Console.ResetColor();
            if (verbose)
            {
                foreach (var cr in callRecords)
                {
                    ShowCallRecord(cr, allCallDetails);
                }
            }
            else 
            {
                foreach (var fc in failingCalls)
                {
                    ShowCallRecord(fc, allCallDetails);
                }
            }
        }
        else if (verbose)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Auction passes");
            Console.ResetColor();
            foreach (var cr in callRecords)
            {
                ShowCallRecord(cr, allCallDetails);
            }
        }
    }

    static void ShowCallRecord(CallRecord cr, bool allCallDetails)
    {
        if (cr.Failed)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  {cr.Index, 2}: Expected {cr.DesiredCall} - {cr.Error}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  {cr.Index, 2}: {cr.DesiredCall}");
        }
        Console.ResetColor();
        Console.WriteLine($"      Rule from {Path.GetFileName(cr.PositionCalls.CallerSourceFilePath)}, {cr.PositionCalls.CallerMemberName}, line: {cr.PositionCalls.CallerSourceLineNumber}");
        foreach (var le in cr.PositionCalls.BidRuleLog)
        {
            if (allCallDetails || le.BidRule.Call.Equals(cr.DesiredCall) || (cr.CallDetails != null && le.BidRule.Call.Equals(cr.CallDetails.Call)))
            {
                DisplayLogEntry(le, cr.PositionCalls.PositionState);
            }
        }
    }


    private static void DisplayLogEntry(LogEntry le, PositionState ps)
    {
        switch (le.Action)
        {
            case LogAction.Illegal:
            case LogAction.Duplicate:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogAction.Rejected:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogAction.Accepted:
            case LogAction.Chosen:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogAction.NotChosen:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
        }
        if (le.Action == LogAction.Chosen || le.Action == LogAction.Accepted)
        {
            Console.WriteLine($"      {le.BidRule.Call} {le.Action} - {le.BidRule.GetDescription(ps)}");
        }
        else if (le.Action == LogAction.Illegal || le.Action == LogAction.Duplicate)
        {
            Console.WriteLine($"      {le.BidRule.Call} {le.Action}");
        }
        else
        {
            Console.Write($"      {le.BidRule.Call} {le.Action} - Not conforming: ");
            foreach (var constraint in le.FailingConstraints)
            {
                Console.Write($"{constraint.GetLogDescription(le.BidRule.Call, ps)} ");
            }
            Console.WriteLine();
        }
        Console.ResetColor();
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


    public void EditAuction()
    {
        while (true)
        {
            Console.Clear();
            Display.Game(Game);
            var auction = Game.Auction;
            Display.Auction(Game, true);
            Console.WriteLine();
            Console.Write("Enter bid #, (oldbid) (newbid), +call, -bid# or enter to quit: ");

            var choice = Console.ReadLine();
            if (string.IsNullOrEmpty(choice) || choice == "Q" || choice == "q") return;
            choice = choice.ToUpper();
            if (choice.StartsWith("+"))
            {
                Call newCall;
                if (Call.TryParse(choice.Substring(1), out newCall))
                {
                    TryUpdateAuction(Game.Auction.Count, newCall);
                }
            }
            // If there is no space then it is a number that selects the bid.  If there is a single space
            // then it should be in the form "cur-bid new-call" where cur-bid is the current bid
            // and new-call is the value to replace that bid with.  Note that cur-bid must be a unique
            // bid in the auction so can't just be Pass or, if X is repeated that would simply replace the
            // first one.
            else if (choice.Split(" ").Length == 2)
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
                            TryUpdateAuction(i, newCall);
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
                if (int.TryParse(choice, out bidIndex) && bidIndex != 0 && Math.Abs(bidIndex) <= auction.Count)
                {
                    if (bidIndex < 0)
                    {
                        Game.Auction.RemoveAt(-bidIndex - 1);
                        // TODO: Need to update contract if we remove a bid
                    }
                    else
                    {
                        Console.Write($"What bid should replace {bidIndex}:{auction[bidIndex-1].Call}? ");
                        var newBid = Console.ReadLine();
                        Call call;
                        if (newBid != null && Call.TryParse(newBid.ToUpper(), out call))
                        {
                            TryUpdateAuction(bidIndex - 1, call);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("ERROR:  Input should be bid index or of form \"oldbid newbid\"");
                }
            }
        }
    }

    private bool TryUpdateAuction(int index, Call newCall)
    {
        if (index < 0 || index > Game.Auction.Count) return false;
        var calls = Game.Auction.Calls;
        if (index == Game.Auction.Count)
        {
            calls.Add(newCall);
        }
        else
        {
            calls[index] = newCall;
        }
        string error;
        if (!ContractState.IsValidAuction(Game.Dealer, calls, out error))
        {
            Console.WriteLine($"Unable to update auction: {error}");
            Console.Write("Press enter to continue: ");
            Console.ReadLine();
            return false;
        }
        if (index == Game.Auction.Count)
        {
            Game.Auction.Add(newCall);
        }
        else
        {
            Game.Auction[index] = new Auction.AnnotatedCall { Call = newCall, Note = null };
        }
        Game.UpdateContractFromAuction();
        return true;
    }

    private int GetUserAction()
    {
        while (true)
        {
            Console.Write("Press enter for next board, E to edit, or enter a bid # to examine: ");
            var c = Console.ReadLine();
            if (string.IsNullOrEmpty(c))
            {
                return ActionNextBoard;
            }
            int bidIndex;
            c = c.ToUpper();
            if (c.Equals("E"))
            {
                return ActionEditAuction;
            }
            else if (int.TryParse(c, out bidIndex) && bidIndex > 0)
            {
                return bidIndex;
            }
        }
    }
   






}
