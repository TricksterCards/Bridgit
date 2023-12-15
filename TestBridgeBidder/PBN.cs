using System;
using System.Collections.Generic;
using System.Linq;

namespace TestBridgeBidder
{
    public class PBN
    {
        public const string Sides = "NESW";
        public const string SuitLetters = "SHDC";
        public const string UnknownCard = "0U";

        public static PBNTest[] ImportTests(string text)
        {
            var tests = new List<PBNTest>();
            var dealerSeat = 0;
            var hands = new List<string>();
            var history = new List<string>();
            var tags = TokenizeTags(text);
            var name = "";

            foreach (var tag in tags)
            {
                switch (tag.Name)
                {
                    case "Event":
                        name = tag.Description;
                        break;
                    case "Deal":
                        dealerSeat = Sides.IndexOf(tag.Description.Substring(0, 1).ToUpper());
                        hands = ImportHands(dealerSeat, tag.Description);
                        history = new List<string>();
                        break;
                    case "Auction":
                    {
                        dealerSeat = Sides.IndexOf(tag.Description.ToUpper());
                        var bids = ImportBids(tag.Data);
                        history = new List<string>();
                        for (var i = 0; i < bids.Count; i++)
                        {
                            var bid = bids[i];
                            var seat = (dealerSeat + i) % 4;
                            var hand = hands[seat];
                            var seatName = Sides[seat];
                            var bidNumber = 1 + i / 4;
                            if (!IsUnknownHand(hand))
                            {
                                tests.Add(
                                    new PBNTest
                                    {
                                        History = history.ToArray(),
                                        Hand = hand,
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

        private static List<string> ImportBids(List<string> bidLines)
        {
            return string.Join(" ", bidLines)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(bid => bid.Replace('S', '♠').Replace('H', '♥').Replace('D', '♦').Replace('C', '♣'))
                .ToList();
        }

        private static List<string> ImportHands(int dealerSeat, string handsString)
        {
            var hands = new List<string> { "", "", "", "" };
            var handStrings = handsString.Substring(2).Split(' ');
            for (var i = 0; i < handStrings.Length; i++)
            {
                var seat = (dealerSeat + i) % 4;
                var handString = handStrings[i];
                if (handString == "-")
                {
                    hands[seat] = string.Join("", Enumerable.Repeat(UnknownCard, 13));
                    continue;
                }

                var hand = "";
                var suits = handString.Split('.');

                for (var j = 0; j < suits.Length; j++)
                    foreach (var card in suits[j])
                        hand += $"{card}{SuitLetters[j]}";

                hands[seat] = hand;
            }

            // validate known hands are of the correct length with no shared cards
            var knownHands = hands.Where(h => !IsUnknownHand(h));
            foreach (var hand in knownHands)
            {
                if (hand.Length != 13 * 2)
                    throw new ArgumentException($"Hand without exactly 13 cards found in '{handsString}'");

                if (hands.Count(h => h == hand) > 1)
                    throw new ArgumentException($"Multiple identical hands found in '{handsString}'");

                for (var i = 0; i < hand.Length; i+=2)
                {
                    var card = hand.Substring(i, 2);
                    if (knownHands.Any(h => h != hand && h.Contains(card)))
                        throw new ArgumentException($"Multiple hands with {card} found in '{handsString}'");
                }
            }

            return hands;
        }

        private static bool IsUnknownHand(string hand)
        {
            return hand.Substring(0, 2) == UnknownCard;
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
