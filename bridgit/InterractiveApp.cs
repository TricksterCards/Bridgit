using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;

using BridgeBidding;
using BridgeBidding.PBN;
using System.Text;

namespace bridgit;

public class InterractiveApp
{
    public static void Show()
    {
        Console.Clear();
        Console.Title = "Bridgit Command Line Test Tool";
        while (true)
        {
            Console.WriteLine("Your choices are:");
            Console.WriteLine("   Enter a number to bid random deals");
            Console.WriteLine("   Paste PBN text and run a test");
            Console.WriteLine("   \"Q\" to Quit");
            var input = Console.ReadLine();
            if (input == null) input = "";
            int numDeals;
            if (input.ToUpper().Equals("Q")) return;
            if (int.TryParse(input, out numDeals))
            {
                if (int.TryParse(input, out numDeals))
                {
                    for (int i = 0; i < numDeals; i++)
                    {
                        var game = new Game();
                        game.DealRandomHands();
                        game.BoardNumber = i+1;
                        BidDeal(game);
                    }
                }
            }
            else if (input.StartsWith("["))
            {
                ProcessPbnText(input);
            }
            else
            {
                Console.WriteLine($"Unknonwn command {input}");
            }
            Console.WriteLine();
        }
    }

    static void ProcessPbnText(string firstLine)
    {
        var gameText = new StringBuilder();
        if (!firstLine.StartsWith("[Event"))
        {
            gameText.Append("[Event \"\"]\n");
        }
        gameText.Append(firstLine);
        gameText.Append('\n');
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                var games = BridgeBidding.PBN.FromString.Games(gameText.ToString());
                Console.WriteLine("PARSED IT !!!");
                var editor = new TestEditor(games.First());
                editor.RunAuctionTest();
                return;
            }
            gameText.Append(line);
            gameText.Append('\n');
        }
    }

    public static void BidDeal(Game game)
    {
        Console.WriteLine($"bid --deal {game.Deal.ToString()}");
        var bidSystem = new TwoOverOneGameForce();
        var bs = new BiddingState(game, bidSystem, bidSystem);
        while (!bs.Contract.AuctionComplete)
        {
            var choices = bs.GetCallChoices();
            var callDetails = choices.BestCall;
            if (callDetails == null)
            {
                if (!bs.GetCallChoices().ContainsKey(Call.Pass))
                {
                    Console.WriteLine("*** OUCH!  NO PASS CHOICE AND NO BEST CALL!");
                }
                Console.WriteLine("ERROR:  No BestCall for call choices.");
                Console.WriteLine("Auction so far...");
                DisplayBiddingState(bs);
                choices = bs.DEBUG_ReEvaluateCallChoices();
                callDetails = choices.BestCall;
                if (callDetails == null)
                {
                    if (choices.ContainsKey(Call.Pass))
                    {
                        callDetails = choices[Call.Pass];
                    }
                    else 
                    {
                        choices.AddPassRule();
                        callDetails = choices[Call.Pass];
                    }
                }
            }
            bs.MakeCall(callDetails);
        }
        DisplayBiddingState(bs);
        Console.WriteLine();
        Console.WriteLine();
        /*


        Console.WriteLine(game.GetGameText());	
        Console.WriteLine("Declarer's know hand summary:");
        Console.WriteLine(bs.Contract.Declarer.PublicHandSummary.ToString());
        Console.WriteLine();
        Console.WriteLine("Dummy's known hand summary:");
        Console.WriteLine(bs.Contract.Declarer.Partner.PublicHandSummary.ToString());
        */                
    }

    private static void DisplayBiddingState(BiddingState bs)
    {
        var game = new Game();
        game.Update(bs);
        var testEditor = new TestEditor(game);
        game.Tags["Event"] = "Test new display game code";
        testEditor.DisplayGame();
        testEditor.DisplayAuction(game.GetAuction());
    }

}