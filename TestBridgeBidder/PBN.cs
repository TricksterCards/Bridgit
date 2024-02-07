using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BridgeBidding.PBN;
using BridgeBidding;

namespace TestBridgeBidder
{
    public class PBN
    {
     
        // This function must ensure that test names are unique.  The testing infrastructure blows up if test names
        // are duplicated.
        public static PBNTest[] ImportTests(string text)
        {
            var tests = new List<PBNTest>();
            var games = BridgeBidding.PBN.FromString.Games(text);
            foreach (var game in games)
            {
                var auction = Auction.FromGame(game).Calls;
                var board = game.GetBoard();
                var bidHistory = "";
                var direction = board.Dealer;
                for (var i = 0; i < auction.Length; i++)
                {
                    var call = auction[i];
                    if (board.Hands[direction] != null)
                    {
                        var bidNumber = 1 + i / 4;
                        string testName = game.Tags["Event"];
                        if (board.Number != null)
                        {
                            testName += $" Board {board.Number}";

                        }
                        tests.Add(
                                new PBNTest
                                {
                                    Auction = bidHistory,
                                    Deal = new Deal(board).ToString(),
                                    Vulnerable = board.Vulnerable.ToString(),
                                    ExpectedCall = call.ToString(),
                                    Name = $"{testName} (Seat {direction}, Bid {bidNumber})"
                                }
                                );
                    }
                    direction = BridgeBidder.LeftHandOpponent(direction);
                    bidHistory += $"{call} ";
                }
            }
            return tests.ToArray();
        }
    }
}
        /*
        public const string Sides = "NESW";


        public static PBNTest[] ImportTests(string text)
        {
            var tests = new List<PBNTest>();
            var dealerSeat = 0;
            string deal = null;
            var knownHands = new HashSet<int>();
            string vulnerable = "None"; // If tag is missing we will assume no vul
            var tags = TokenizeTags(text);
            var name = "";

            foreach (var tag in tags)
            {
                switch (tag.Name)
                {
                    case "Vulnerable":
                        vulnerable = tag.Description;
                        break;
                    case "Event":
                        name = tag.Description;
                        break;
                    case "Deal":
                        dealerSeat = Sides.IndexOf(tag.Description.Substring(0, 1).ToUpper());
                        deal = tag.Description;
                        knownHands = DetermineKnownHands(dealerSeat, deal.Substring(2));
                        break;
                    case "Auction":
                    {
                        var bids = ImportAuction(tag.Data);
                        var history = new List<string>();
                        for (var i = 0; i < bids.Count; i++)
                        {
                            var bid = bids[i];
                            var seat = (dealerSeat + i) % 4;
                            if (knownHands.Contains(seat))
                            {
                                var bidNumber = 1 + i / 4;
                                var seatName = Sides[seat];
                                tests.Add(
                                    new PBNTest
                                    {
                                        Auction = string.Join(" ", history),
                                        Deal = deal,
                                        Vulnerable = vulnerable,
                                        Bid = bid,
                                        Name = $"{name} (Seat {seatName}, Bid {bidNumber})"
                                    }
                                );
                            }
                            history.Add(bid);
                        }
                        break;
                    }
                }
            }

            // Ignore all other tags
            return tests.ToArray();
        }



        private static HashSet<int> DetermineKnownHands(int dealerSeat, string hands)
        {
            var knownHands = new HashSet<int>();
            var handStrings = hands.Split(' ');
            for (var i = 0; i < handStrings.Length; i++)
            {
                var seat = (dealerSeat + i) % 4;
                var handString = handStrings[i];
                if (handString != "-")
                {
                    // Do no validation on hands.  Should be caught by bidder code.
                    knownHands.Add(seat);
                }
            }
            return knownHands;
        }

        private static List<string> ImportAuction(List<string> bidLines)
        {
            return string.Join(" ", bidLines)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }


        private static List<PBNTag> TokenizeTags(string text)
        {
            var tag = new PBNTag { Data = new List<string>() };
            var tags = new List<PBNTag>();
            var lines = text.Split(new[] { "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('%'));
            foreach (var line in lines)
                if (line.StartsWith('['))
                {
                    tag = new PBNTag { Data = new List<string>() };
                    tags.Add(tag);
                    tag.Name = line.Substring(1, line.IndexOf(' ') - 1);
                    var start = line.IndexOf('"') + 1;
                    var end = line.LastIndexOf('"') - start;
                    tag.Description = line.Substring(start, end);
                }
                else
                {
                    tag.Data.Add(line);
                }

            return tags;
        }

        private class PBNTag
        {
            public List<string> Data { get; set; }
            public string Description { get; set; }
            public string Name { get; set; }
        }
    }
}
*/