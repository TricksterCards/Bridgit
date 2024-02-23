using System;
using System.Diagnostics;
using BridgeBidding;
using BridgeBidding.PBN;





static class Display
{
    public static void Game(Game game)
    {
        Console.WriteLine($"Board {game.Board} - {game.Event}");
        Console.WriteLine($"{game.Dealer} deals, {game.Vulnerable} vulnerable");
        Console.WriteLine();
        Hands(game.Deal);
    }
    public static void Hands(Dictionary<Direction, Hand> hands)
    {
        var hcp = new Dictionary<Direction, int>();
        foreach (var d in Enum.GetValues<Direction>())
        {
            hcp[d] = hands[d] != null ? hands[d].HighCardPoints() : -1;
        }
        Display1Hand(10, hands[Direction.N]);
        Display2Hands(1, hands[Direction.W], 23, hands[Direction.E]);
        Display1Hand(10, hands[Direction.S], hcp);        
    }

    private static void DisplayHCP(int p)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write(p < 0 ? " -" : $"{p, 2}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void Display1Hand(int indent, Hand hand, Dictionary<Direction, int>? hcp = null)
    {
        var suitRanks = SuitRanks(hand);
        Debug.Assert(suitRanks.Length == Hand.SuitOrder.Length);
        for (var suitIndex = 0; suitIndex < Hand.SuitOrder.Length; suitIndex++)
        {
            var i = 0;
            if (hcp != null)
            {
                switch (suitIndex)
                {
                    case 1:
                        Console.Write("   ");
                        DisplayHCP(hcp[Direction.N]);
                        i = 5;
                        break;
                    case 2:
                        Console.Write(" ");
                        DisplayHCP(hcp[Direction.W]);
                        Console.Write("  ");
                        DisplayHCP(hcp[Direction.E]);
                        i = 7;
                        break;
                    case 3:
                        Console.Write("   ");
                        DisplayHCP(hcp[Direction.S]);
                        i = 5;
                        break;
                }
            }
            while (i < indent) 
            {
                Console.Write(" ");
                i++;
            }
            var s = Hand.SuitOrder[suitIndex];
            Console.WriteLine($"{s.ToSymbol()}: {suitRanks[suitIndex], -13}");
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
            var s = Hand.SuitOrder[suitIndex].ToSymbol();
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
        AuctionTitles(game.Vulnerable, showBidNumbers);
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

    public static void AuctionTitles(Vulnerable vul, bool showBidNumbers)
    {
        var titles = new String[] { "WEST", "NORTH", "EAST", "SOUTH" };
        var ewColor = vul == Vulnerable.All || vul == Vulnerable.EW ? ConsoleColor.Red : ConsoleColor.White;
        var nsColor = vul == Vulnerable.All || vul == Vulnerable.NS ? ConsoleColor.Red : ConsoleColor.White;
        int i = 0;
        foreach (var title in titles)
        {
            Console.BackgroundColor = (i % 2 == 0) ? ewColor : nsColor;
            if (showBidNumbers)
            {
                Console.Write($"{title, -9}");
            }
            else
            {
                Console.Write($"{title, -6}");
            }  
            i++;
        }
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine();
    }

}
