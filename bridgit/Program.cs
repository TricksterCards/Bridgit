using System;
using BridgeBidding;
using System.Reflection;
using BridgeBidding.PBN;



namespace Bridgit
{
    internal class Program
    {
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
    }
}