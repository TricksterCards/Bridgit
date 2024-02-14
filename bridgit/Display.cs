using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Schema;
using BridgeBidding;
using BridgeBidding.PBN;

namespace bridgit;

static class Display
{
    public static void Game(Game game)
    {
        Console.WriteLine($"{game.Event}");
        Console.WriteLine($"{game.Dealer} deals, {game.Vulnerable} vulnerable");
        Hands(game.Deal);
    }
    public static void Hands(Dictionary<Direction, Hand> hands)
    {
        Display1Hand(10, hands[Direction.N]);
        Display2Hands(1, hands[Direction.W], 23, hands[Direction.E]);
        Display1Hand(10, hands[Direction.S]);        
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

    private static string[] SuitRanks(Hand hand)
    {
        if (hand == null)
        {
            return new string[] { "-", "-", "-", "-" };
        }
        return hand.ToString().Split(".");
    }

    public static void Auction(Game game, bool showBidNumbers = false, int stopAtIndex = int.MaxValue)
    {
        Console.WriteLine(showBidNumbers ? "   West     North    East     South" : "West  North East  South");
        var direction = Direction.W;
        int col = 0;
        int bidIndex = 1;
        while (direction != game.Dealer)
        {
            Console.Write(showBidNumbers? "         " : "      ");
            col += 1;
            direction = BridgeBidder.LeftHandOpponent(direction);
        }
        foreach (var call in game.Auction.Calls)
        {
            Console.Write(showBidNumbers? $"{bidIndex, 2}:{call, -6}" : $"{call, -6}");
            if (bidIndex == stopAtIndex) return;
            col ++;
            bidIndex ++;
            if (col % 4 == 0) Console.WriteLine();
        }
        if (col % 4 != 0) Console.WriteLine();
    }

}
