﻿using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;

using BridgeBidding;
using BridgeBidding.PBN;
using Bridgit;

namespace bridgit;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Bridge bidding command line tool.");

        //var fileOption = new Option<FileInfo?>(
        //    name: "--file",
        //    description: "The file to read for bidding.");

        var dealOption = new Option<Deal>(
            name: "--deal",
            description: "Full deal in PBN format.",
            parseArgument: result =>
            {
                return Deal.Parse(result.Tokens.Single().ToString());
            });

        var vulnerableOption = new Option<Vulnerable>(
            name: "--vulnerable",
            description: "Vulnerability.  If not specified then None.",
            getDefaultValue: () => Vulnerable.None);

        var bidCommand = new Command("bid", "Bid the full auction for the specified deal or file.");
        bidCommand.Add(dealOption);
        bidCommand.Add(vulnerableOption);


        bidCommand.SetHandler((deal, vulnerable) =>
            {
                var board = new Board();
                board.Vulnerable = vulnerable;
                if (deal == null)
                {
                    board.DealRandomHands();
                    board.Dealer = Direction.N;
                }
                else 
                {
                    board.Hands = deal.Hands;
                    board.Dealer = deal.Dealer;
                }

                var bidSystem = new TwoOverOneGameForce();
                var bs = new BiddingState(board, bidSystem, bidSystem);
                while (!bs.Contract.AuctionComplete)
                {
                    var callDetails = bs.CallChoices.BestCall;
                    bs.MakeCall(callDetails);
                }
                var game = new Game();
			    game.Update(bs);
			    game.Tags["Event"] = "Full auction";

			    Console.WriteLine(game.GetGameText());	
                Console.WriteLine("Declarer's know hand summary:");
                Console.WriteLine(bs.Contract.Declarer.PublicHandSummary.ToString());
                Console.WriteLine();
                Console.WriteLine("Dummy's known hand summary:");
                Console.WriteLine(bs.Contract.Declarer.Partner.PublicHandSummary.ToString());                
            },
            dealOption, vulnerableOption);


        rootCommand.AddCommand(bidCommand);

        var eventOption = new Option<string>(
            name: "--event",
            description: "Event name to add to PBN output"
        );

        var seatOption = new Option<int>(
            name: "--seat",
            description: "Seat for tests"
        );

        var makeTestsCommand = new Command("maketests", "Create new test cases")
            {
                eventOption,
                seatOption
            };

        makeTestsCommand.SetHandler((eventName, seat) => 
            { 
                var tg = new TestGenerator(eventName, seat, Call.Pass, Bid._1C, Bid._1D, Bid._1H, Bid._1S, Bid._1NT,
                   Bid._2C, Bid._2D, Bid._2H, Bid._2S, Bid._2NT);
                tg.GenerateTests(20);
                Console.WriteLine(tg.ToString());
            },
            eventOption,
            seatOption);    
        rootCommand.AddCommand(makeTestsCommand);

        return await rootCommand.InvokeAsync(args);
            //new string[] { "maketests", "--event", "Open", "--seat", "1" });
            //new string[] {"--poop", "N:K43.QJ3.AK32.Q85 QJT9.K98.JT9.AJ4 765.752.654.T932 A82.AT64.Q87.K76"});
    }
/*
    static void Main(string[] args)
    {

    //     var s = BridgeBidder.FullAuction("N:KT9.KQJ7.KQ5.J97 5.T983.AJT4.A643 AQ432.654.32.Q85 J876.A2.9876.KT2", "None");
    //     Console.WriteLine(s);
    
    ///    var gen = new TestGenerator("Open", 4, Call.Pass, Bid._1C, Bid._1D, Bid._1H, Bid._1S, Bid._1NT,
        //           Bid._2C, Bid._2D, Bid._2H, Bid._2S, Bid._2NT);
        //   gen.GenerateTests(10);
        //   Console.WriteLine(gen);
    
        var gen = new TestGenerator("Seat-dependent", 1, Bid._3C);
        var games = gen.GenerateSeatDependentTests(20);
        Console.WriteLine(gen.GamesToString("Seat dependent", games));


    
    }


    public static void LoadGames(string subdirectory)
    {
        
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var files = Directory.GetFiles(Path.Combine(dir, subdirectory), "*.pbn");
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var text = File.ReadAllText(file);
            var games = BridgeBidding.PBN.FromString.Games(text);
            var board = games[0].GetBoard();
            var auction = games[0].GetAuction();
            Console.WriteLine("Worked");
        }
    }
    */
}
