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

    public void DisplayGame()
    {
        Console.WriteLine($"{Game.Tags["Event"]}");
        var board = Game.GetBoard();
        Console.WriteLine($"{board.Dealer} deals, {board.Vulnerable} vulnerable");
        DisplayHands(board.Hands);
        Console.WriteLine();
        var auction = Auction.FromGame(Game);
        DisplayAuction(auction);
        Console.WriteLine();

        Console.Write("Press Y to accept, N to edit: ");
        bool haveValidInput = false;
        bool needToEdit = false;
        while (!haveValidInput)
        {
            var c = Console.ReadLine();
            c = c.ToLower();
            if (c.Equals("n"))
            {
                haveValidInput = true;
                needToEdit = true;
            }
            else if (c.Equals("y"))
            {
                haveValidInput = true;
            }
        }
        Console.WriteLine();
        if (needToEdit) Console.WriteLine("*** NEED TO IMPLEMENT EDIITNG ***");
    }

    public void DisplayAuction(Auction auction)
    {
        Console.WriteLine("West  North East  South");
        var direction = Direction.W;
        int col = 0;
        while (direction != auction.FirstToAct)
        {
            Console.Write("      ");
            col += 1;
            direction = BridgeBidder.LeftHandOpponent(direction);
        }
        foreach (var call in auction.Calls)
        {
            Console.Write($"{call, -6}");
            col += 1;
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
