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
                        game.Board = i+1;
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
                var game = Game.Parse(gameText.ToString());
                Console.WriteLine("PARSED IT !!!");
                var editor = new TestEditor(game);
                editor.RunAuctionTest();
                return;
            }
            gameText.Append(line);
            gameText.Append('\n');
        }
    }

    public static void BidDeal(Game game)
    {
        Console.WriteLine($"bid --deal {game.Deal}");
        Display.Game(game);
        var bs = new BiddingState(game);
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
            Console.WriteLine($"{bs.NextToAct.Direction}: {callDetails.Call}");
            foreach (var a in callDetails.Annotations)
            {
                Console.WriteLine($"   {a.Type}: {a.Text}");
            }
            var desc = callDetails.GetCallDescriptions();
            foreach (var descList in desc)
            {
                var description = string.Join(", ", descList);
                Console.WriteLine($"      {description}");
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
        Display.Game(bs.Game);
        Display.Auction(bs.Game);
    }

}