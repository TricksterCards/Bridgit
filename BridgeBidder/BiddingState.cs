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


        public BiddingState(Dictionary<Direction, Hand> hands, Direction dealer, HashSet<Pair> vulPairs, IBiddingSystem nsSystem, IBiddingSystem ewSystem)
        {
            this.Positions = new Dictionary<Direction, PositionState>();
            this.Contract = new Contract();
            Debug.Assert(hands.Count == 4);
            var d = dealer;
            var ns = new PairState(Pair.NorthSouth, nsSystem, vulPairs);
            var ew = new PairState(Pair.EastWest, ewSystem, vulPairs);
            for (int seat = 1; seat <= hands.Count; seat++)
            {
                PairState pairState = (d == Direction.North || d == Direction.South) ? ns : ew;
                this.Positions[d] = new PositionState(this, pairState, d, seat, hands[d]);
                d = BridgeBidder.LeftHandOpponent(d);
            }
            this.Dealer = Positions[dealer];
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
