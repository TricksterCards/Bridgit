using System;
using BridgeBidding;



namespace Bridgit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(BridgeBidder.FullAuction(
                "N:KT9.KQJ7.KQ5.J97 5.T983.AJT4.A643 AQ432.654.32.Q85 J876.A2.9876.KT2", "None")
            );
        }
    }
}