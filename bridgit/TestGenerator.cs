using System;
using System.Runtime.CompilerServices;
using System.Text;
using BridgeBidding;
using BridgeBidding.PBN;

namespace Bridgit
{
    public class TestGenerator
    {
        public Dictionary<Call, List<Game>> Tests = new Dictionary<Call, List<Game>>();
        public string EventDescription;

        public TestGenerator(string eventDescription, params Call[] calls)
        {
            this.EventDescription = eventDescription;
            foreach (var call in calls)
            {
                Tests[call] = new List<Game>();
            }
        }

        public void GenerateTests(int count)
        {
            var twoOverOne = new TwoOverOneGameForce();
            int needed = Tests.Count * count;
            while (needed > 0)
            {
                var board = new Board();
                board.DealRandomHands();
                board.Hands[Direction.E] = null;
                board.Hands[Direction.S] = null;
                board.Hands[Direction.W] = null;
                var bs = new BiddingState(board, twoOverOne, twoOverOne);
                Call call = bs.SuggestCall();
                if (Tests.ContainsKey(call) && Tests[call].Count < count)
                {
                    var game = new Game();
                    game.Update(bs);
                    Tests[call].Add(game);
                    needed--;
                }
            }
        }

        public override string ToString()
        {
            int boardNumber = 1;
            var sb = new StringBuilder();
            foreach (var test in Tests)
            {
                var eventName = $"{EventDescription} {test.Key}";
                foreach (var game in test.Value)
                {
                    game.Tags["Event"] = eventName;
                    game.Tags["Board"] = boardNumber.ToString();
                    sb.Append(game.GetGameText());
                    sb.Append("\n");
                    boardNumber++;
                }
            }
            return sb.ToString();
        }

    }
}