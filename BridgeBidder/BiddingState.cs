using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
	public class BiddingState
    {
        public Dictionary<Direction, PositionState> Positions { get; }

        public PositionState Dealer { get; }

        public PositionState NextToAct { get; private set; }

        public Contract Contract { get; }

        public Bid OpeningBid { get; private set; }

        public PositionState Opener { get; private set; }

        public Board Board { get; }

        public BiddingState(Board board, IBiddingSystem nsSystem, IBiddingSystem ewSystem)
        {
            this.Board = board;
            this.Positions = new Dictionary<Direction, PositionState>();
            this.Contract = new Contract();
            var d = board.Dealer;
            var ns = new PairState(Pair.NS, nsSystem, board.Vulnerable);
            var ew = new PairState(Pair.EW, ewSystem, board.Vulnerable);
            for (int seat = 1; seat <= 4; seat++)
            {
                Hand hand = null;
                board.Hands.TryGetValue(d, out hand);
                PairState pairState = (d == Direction.N || d == Direction.S) ? ns : ew;
                this.Positions[d] = new PositionState(this, pairState, d, seat, hand);
                d = BridgeBidder.LeftHandOpponent(d);
            }
            this.Dealer = Positions[Board.Dealer];
            this.NextToAct = Dealer;
        }

        public void ReplayAuction(Call[] history)
        {
            foreach (var call in history)
            {
                Contract.ValidateCall(call, NextToAct);
                MakeCall(call, NextToAct.GetPositionCalls());
            }
        }

        public Call SuggestCall()
        {
            if (!NextToAct.HasHand)
            {
                throw new AuctionException(Call.Pass, NextToAct, Contract, $"{NextToAct.Direction} does not have a known hand so can not suggest a bid.");
            }
            if (Contract.AuctionComplete)
            {
                throw new AuctionException(Call.Pass, NextToAct, Contract, "Auction is final.  No more bids can be made");
            }
            var choices = NextToAct.GetPositionCalls();
            if (choices.BestCall == null)
            {
                throw new Exception("No BestCall for auction.");
            }
            MakeCall(choices.BestCall, choices);
            return choices.BestCall;
        }


        private void MakeCall(Call call, PositionCalls choices)
        {
            Debug.Assert(Contract.IsValid(call, NextToAct));
            NextToAct.MakeCall(choices.GetCallDetails(call));
            if (this.OpeningBid == null && call is Bid bid)
            {
                this.OpeningBid = bid;
                this.Opener = NextToAct;
            }
            NextToAct = NextToAct.LeftHandOpponent;
        }

        public List<CallDetails> GetAuction()
        {
            var bidder = Dealer;
            var bidIndex = 0;
            var calls = new List<CallDetails>();
            while (bidIndex < bidder.CallCount)
            {
                calls.Add(bidder.GetCallDetails(bidIndex));
                if (calls.Count % 4 == 0) bidIndex += 1;
                bidder = bidder.LeftHandOpponent;
            }
            return calls;
        }



        // TODO: Really think through all of the issues with Hand State vs Agreements
        // and when each should be updated....
        internal void UpdateStateFromFirstBid()
        {
            for (int i = 0; i < 50; i++)
            {
                var position = Dealer;
                var bidIndex = 0;
                bool someStateChanged = false;
                bool posStateChanged;
                while (position.UpdateBidIndex(bidIndex, out posStateChanged))
                {
                    someStateChanged |= posStateChanged;
                    position = position.LeftHandOpponent;
                    if (position == Dealer) { bidIndex++; }
                }
                if (!someStateChanged) { return; }
            }
            throw new Exception("Unable to resolve to a stable state.  Giving up");   
        }
    }
}
