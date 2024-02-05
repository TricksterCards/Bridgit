using System;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BridgeBidding;
using BridgeBidding.PBN;

namespace bridgit;

public class TestGenerator
{
    public Dictionary<Call, List<Game>> Tests = new Dictionary<Call, List<Game>>();
    public string EventDescription;

    private IBiddingSystem _bidder = new TwoOverOneGameForce();

    private int _seat;

    public TestGenerator(string eventDescription, int seat, params Call[] calls)
    {
        this.EventDescription = eventDescription;
        this._seat = seat;
        Debug.Assert(seat >= 1 && seat <= 4);
        foreach (var call in calls)
        {
            Tests[call] = new List<Game>();
        }
    }

    public void GenerateTests(int count)
    {
        //   var auction = new List<Call>();
        var dealer = Direction.N;
        for (int s = 1; s < _seat; s++)
        {
    //        auction.Add(Call.Pass);
            dealer = BridgeBidder.RightHandOpponent(dealer);
        }
        int needed = Tests.Count * count;
        while (needed > 0)
        {
            bool goodTest = true;
            var board = new Board();
            board.Dealer = dealer;
            board.DealRandomHands();
        //    foreach (Direction noHand in Enum.GetValues<Direction>())
        //        if (noHand != Direction.N)
        //            board.Hands[noHand] = null;
            var bs = new BiddingState(board, _bidder, _bidder);
            for (int s = 1; s < _seat; s++)
            {
                var callDetails = bs.CallChoices.BestCall;
                if (callDetails.Call.Equals(Call.Pass))
                {
                    bs.MakeCall(callDetails);
                }
                else
                {
                    goodTest = false;
                }
            }
            if (goodTest)
            {
                CallDetails callDetails = bs.CallChoices.BestCall;
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
    }


    public Game[] GenerateSeatDependentTests(int count)
    {
        var games = new List<Game>();
        int needed = Tests.Count * count;
        while (games.Count < count)
        {
            var board = new Board();
            board.DealRandomHands();
            board.Hands[Direction.E] = null;
            board.Hands[Direction.S] = null;
            board.Hands[Direction.W] = null;
            if (CallsDifferBySeat(board))
            {
                board.Dealer = Direction.N;
                var seat = 1;
                var auction = new List<Call>();
                while (seat <= 4)
                {
                    foreach (var vul in Enum.GetValues<Vulnerable>())
                    {
                        board.Vulnerable = vul;
                        var bs = new BiddingState(board, _bidder, _bidder);
                        bs.ReplayAuction(auction);
                        bs.MakeCall(bs.CallChoices.BestCall);
                        var game = new Game();
                        game.Update(bs);
                        game.Tags["Event"] = $"Seat dependent, seat {seat}, vul {vul}";
                        games.Add(game);
                    }
                    seat++;
                    auction.Add(Call.Pass);
                    board.Dealer = BridgeBidder.RightHandOpponent(board.Dealer);
                }
            }
        }
        return games.ToArray();
    }

    private bool CallsDifferBySeat(Board board)
    {
        Debug.Assert(board.Dealer == Direction.N);
        var bs = new BiddingState(board, _bidder, _bidder);
        CallDetails lastCall = bs.CallChoices.BestCall;
        var auction = new List<Call>();
        while (board.Dealer != Direction.E)
        {

            board.Dealer = BridgeBidder.RightHandOpponent(board.Dealer);
            auction.Add(Call.Pass);
            foreach (var vul in Enum.GetValues<Vulnerable>())
            {
                board.Vulnerable = vul;
                bs = new BiddingState(board, _bidder, _bidder);
                bs.ReplayAuction(auction);
                CallDetails posCall = bs.CallChoices.BestCall;
                if (!posCall.Call.Equals(lastCall.Call) &&
                    (Tests.ContainsKey(posCall.Call) || Tests.ContainsKey(lastCall.Call)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string GamesToString(string eventName, Game[] games)
    {
        int boardNumber = 1;
        var sb = new StringBuilder();
        foreach (var game in games)
        {
            //game.Tags["Event"] = eventName;
            game.Tags["Board"] = boardNumber.ToString();
            sb.Append(game.GetGameText());
            var board = game.GetBoard();
            sb.Append(HandCommentary(board.Hands[Direction.N]));
            boardNumber++;
        }
        return sb.ToString();
    }

    public override string ToString()
    {
        int boardNumber = 1;
        var sb = new StringBuilder();
        foreach (var test in Tests)
        {
            var eventName = $"{EventDescription} seat {_seat} {test.Key}";
            foreach (var game in test.Value)
            {
                game.Tags["Event"] = eventName;
                game.Tags["Board"] = boardNumber.ToString();
                sb.Append(game.GetGameText());
        //         var board = game.GetBoard();
        //          sb.Append(HandCommentary(board.Hands[Direction.N]));
                sb.AppendLine();
                boardNumber++;
            }
        }
        return sb.ToString();
    }

    public static string HandCommentary(Hand hand)
    {
        var sb = new StringBuilder();
        var stringHand = hand.ToString();
        var suits = stringHand.Split('.');
        var showHand = new HandSummary.ShowState();
        StandardHandEvaluator.Evaluate(hand, showHand);
        var ha = showHand.HandSummary;
        sb.Append($";       S: {suits[0], -10} {ha.Suits[Suit.Spades].GetQuality().Min}\n");
        sb.Append($";       H: {suits[1], -10} {ha.Suits[Suit.Hearts].GetQuality().Min}\n");
        sb.Append($";       D: {suits[2], -10} {ha.Suits[Suit.Diamonds].GetQuality().Min}\n");
        sb.Append($";       C: {suits[3], -10} {ha.Suits[Suit.Clubs].GetQuality().Min}\n");
        sb.Append($";  Points: {ha.Points}\n");
        sb.Append($";  HCP:    {ha.HighCardPoints}\n");
        sb.Append("\n");
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