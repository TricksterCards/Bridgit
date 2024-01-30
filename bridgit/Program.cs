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

            var s = BridgeBidder.FullAuction("N:KT9.KQJ7.KQ5.J97 5.T983.AJT4.A643 AQ432.654.32.Q85 J876.A2.9876.KT2", "None");
            Console.WriteLine(s);
       /*
            var gen = new TestGenerator("Open", Call.Pass, Bid.OneClub, Bid.OneDiamond, Bid.OneHeart, Bid.OneSpade, Bid.OneNoTrump,
                    Bid.TwoClubs, Bid.TwoDiamonds, Bid.TwoHearts, Bid.TwoSpades, Bid.TwoNoTrump);
            gen.GenerateTests(10);
            Console.WriteLine(gen);
      */
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