using System;
using System.Configuration.Assemblies;
using System.Diagnostics;
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

        private IBiddingSystem _bidder = new TwoOverOneGameForce();

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
            int needed = Tests.Count * count;
            while (needed > 0)
            {
                var board = new Board();
                board.DealRandomHands();
                board.Hands[Direction.E] = null;
                board.Hands[Direction.S] = null;
                board.Hands[Direction.W] = null;
                var bs = new BiddingState(board, _bidder, _bidder);
                CallDetails callDetails = bs.SuggestCall();
                var call = callDetails.Call;
                if (Tests.ContainsKey(call) && Tests[call].Count < count)
                {
                    bs.MakeCall(callDetails);
                    var game = new Game();
                    game.Update(bs);
                    Tests[call].Add(game);
                    needed--;
                }
            }
        }

        private bool CallsDifferBySeat(Board board, IBiddingSystem bidder)
        {
            Debug.Assert(board.Dealer == Direction.N);
            var bs = new BiddingState(board, bidder, bidder);
            CallDetails lastCall = bs.SuggestCall();
            var auction = new List<Call>();
            while (board.Dealer != Direction.E)
            {
                board.Dealer = BridgeBidder.RightHandOpponent(board.Dealer);
                auction.Add(Call.Pass);
                bs = new BiddingState(board, bidder, bidder);
                bs.ReplayAuction(auction.ToArray());
                CallDetails posCall = bs.SuggestCall();
                if (!posCall.Call.Equals(lastCall.Call))
                {
                    return true;
                }
            }
            return false;
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
                    var board = game.GetBoard();
                    var hand = board.Hands[Direction.N];
                    var stringHand = BridgeBidding.PBN.ToString.Hand(hand);
                    var suits = stringHand.Split('.');
                	var showHand = new HandSummary.ShowState();
					StandardHandEvaluator.Evaluate(hand, showHand);
                    var ha = showHand.HandSummary;
                    sb.Append($"%       S: {suits[0]}\n");
                    sb.Append($"%       H: {suits[1]}\n");
                    sb.Append($"%       D: {suits[2]}\n");
                    sb.Append($"%       C: {suits[3]}\n");
                    sb.Append($"%  Points: {ha.Points}\n");
                    sb.Append($"%  HCP:    {ha.HighCardPoints}\n");
                    sb.Append("\n");
                    boardNumber++;
                }
            }
            return sb.ToString();
        }

        public void FullyRandom(int count)
        {
            for (int i = 0; i < count; i ++)
            {
                var board = new Board();
                board.DealRandomHands();
                var bs = new BiddingState(board, _bidder, _bidder);
                var auction = bs.GetAuction();
            }
        }

    }
}