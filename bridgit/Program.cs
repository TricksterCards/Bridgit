using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;

using BridgeBidding;
using BridgeBidding.PBN;
using System.Threading.Channels;
using System.Text;

namespace bridgit;

/*
Interesting deals
     "commandLineArgs": "bid --deal \"N:9643.T4.QT8.T542 AQ7.AKJ5.K2.AQ97 KT852.Q3.7643.K8 J.98762.AJ95.J63\""
                         bid --deal N:74.AKT542..98763 QT53.J87.AKQ6.AQ A92.9.JT9732.JT4 KJ86.Q63.854.K52
                         bid --deal N:J864.AT9543.Q92. Q2.KJ6.K5.KT7543 AKT753.72.A43.J8 9.Q8.JT876.AQ962 -- THIS ONE OPENS WEAK WITH 4 SPADES!
                         bid --deal N:QJ6.KQ743.A84.Q6 82.AJ9.Q732.KT97 AT74.2.KT95.J843 K953.T865.J6.A52 - passes unpassed responder
*/

public class Program
{
    static async Task<int> Main(string[] args)
    {
        // JUST FOR FUN...
       // EditGame.TryItOut();

      //  DoGameHack();

        var rootCommand = new RootCommand("Bridge bidding command line tool.");
        rootCommand.SetHandler(() =>
        {
            InterractiveApp.Show();

        });

        //var fileOption = new Option<FileInfo?>(
        //    name: "--file",
        //    description: "The file to read for bidding.");

        var dealOption = new Option<Game>(
            name: "--deal",
            description: "Full deal in PBN format.",
            parseArgument: result =>
            {
                return Game.Parse(result.Tokens.Single().ToString(), "None");
            });

        var vulnerableOption = new Option<Vulnerable>(
            name: "--vulnerable",
            description: "Vulnerability.  If not specified then None.",
            getDefaultValue: () => Vulnerable.None);

        var bidCommand = new Command("bid", "Bid the full auction for the specified deal or file.");
        bidCommand.Add(dealOption);
        bidCommand.Add(vulnerableOption);


        bidCommand.SetHandler((game, vulnerable) =>
            {
                game.Vulnerable = vulnerable;
                InterractiveApp.BidDeal(game);
            },
            dealOption, vulnerableOption);


        rootCommand.AddCommand(bidCommand);

        var eventOption = new Option<string>(
            name: "--event",
            description: "Event name to add to PBN output",
            getDefaultValue: () => "New tests"
        );

        var seatOption = new Option<int>(
            name: "--seat",
            description: "Seat for tests",
            getDefaultValue: () => 1
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


    private static void DoGameHack()
    {
        var game = new Game();
        Console.WriteLine("Here's an empty game:");
        Console.WriteLine(game.ToString());
        Console.WriteLine();

        game.Board = 5;
        game.Vulnerable = Vulnerable.EW;
        game.Dealer = Direction.E;
        game.Scoring = Scoring.IMP;
        game.DealRandomHands();

        Console.WriteLine("Board 5, random deal, E deals, EW vul, IMP scoring");
        Console.WriteLine(game.ToString());
        Console.WriteLine();

        game.Auction.Add(Call.Pass);
        game.Auction.Add(Call.Pass);
        game.Auction.Add(Bid._1H, "Open in 3rd seat");
        game.Auction.Add(Call.Pass);
        game.Auction.Add(Bid._2H);
        game.Auction.Add(Call.Pass);
        game.Auction.Add(Call.Pass);
        Console.WriteLine("Now have added:  Pass Pass 1H Pass 2H Pass Pass");
        Console.WriteLine(game.ToString());
        Console.WriteLine();

        game.Auction.Add(Call.Pass, "This ends the auction!");
        game.Contract = Contract.Parse("2NTXX");
        game.Declarer = Direction.W;
        Console.WriteLine("Final pass with a silly annotation.");
        Console.WriteLine(game.ToString());
        Console.WriteLine();        

        Game clone = game.Clone();
        Console.WriteLine("Here's the cloned game:");
        Console.WriteLine(clone.ToString());
        Console.WriteLine();


        Console.WriteLine("Press enter to continue...");
        Console.ReadLine();

        Console.WriteLine("NOW WE ARE GOING TO TRY TO SAVE SOME DIFFERENT GAMES AND PARSE THEM IN A LIST.");
        game.Event= "Original game";
        game.Board = 1;
        clone.Event = "Cloned game";
        clone.Board = 2;
        var thirdGame = new Game();
        thirdGame.Event = "Third game";
        thirdGame.Board = 3;

        var sb = new StringBuilder();
        sb.AppendLine(game.ToString());
        sb.AppendLine();
        sb.AppendLine(clone.ToString());
        sb.AppendLine();
        sb.AppendLine(thirdGame.ToString());
        string gameText = sb.ToString();

        Console.WriteLine("Here's the source multi-game file");
        Console.WriteLine(gameText);
        Console.WriteLine();

        Console.WriteLine("Now the extracted games from that text");
        var games = FromString.ParseGames(gameText);
        Console.WriteLine($"There are {games.Count} games in the file.");

        foreach (var g in games)
        {
            Console.WriteLine();
            Console.WriteLine($"{g}");
            Console.WriteLine();
        }


        Console.WriteLine("Press enter to continue...");
        Console.ReadLine();
    }

}
