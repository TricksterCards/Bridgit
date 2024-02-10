using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
	public class BiddingState
    {
        public Dictionary<Direction, PositionState> Positions { get; }

        public PositionState Dealer { get; }

        private PositionCalls _positionCalls = null;
        public PositionCalls GetCallChoices() 
        {
            if (_positionCalls == null)
            {
                _positionCalls = NextToAct.GetPositionCalls();
            }
            return _positionCalls;
        }

        // THID METHOD IS FOR DEBUGGING SO THE CALLER CAN OBSERVE THE CREATIION THE PositionCalls object.
        public PositionCalls DEBUG_ReEvaluateCallChoices()
        {
            _positionCalls = null;
            return GetCallChoices();
        }

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
                board.Deal.TryGetValue(d, out hand);
                PairState pairState = (d == Direction.N || d == Direction.S) ? ns : ew;
                this.Positions[d] = new PositionState(this, pairState, d, seat, hand);
                d = BridgeBidder.LeftHandOpponent(d);
            }
            this.Dealer = Positions[Board.Dealer];
            this.NextToAct = Dealer;
        }

        public void ReplayAuction(IEnumerable<Call> history)
        {
            foreach (var call in history)
            {
                MakeCall(call);
            }
        }

        // Note that this method will alawys make the call specified as long as the call is 
        public void MakeCall(Call call)
        {
            Contract.ValidateCall(call, NextToAct);
            var choices = GetCallChoices();
            if (!choices.ContainsKey(call))
            {
                // TODO: This is something strange.  A call we don't know
                // how to interpret.  Should bidding system get a crack at this
                // when we create the placeholder?
                choices.CreatePlaceholderCall(call);
            }
            MakeCall(choices[call]);
        }

        
        public void MakeCall(CallDetails callDetails)
        {
            Debug.Assert(NextToAct == callDetails.PositionState);
            
            if (callDetails.Group.PositionCalls != _positionCalls)
			{
				throw new System.Exception("MakeCall method called for CallDetails that is not part of current call choices");
			}

            // TODO: Carefully consider if contract should be updated before or after 
            // PositionState.MakeCall 
            callDetails.PositionState.MakeCall(callDetails);
            Contract.MakeCall(callDetails); // This also validates the call and will throw if a problem.

            if (this.OpeningBid == null && callDetails.Call is Bid bid)
            {
                this.OpeningBid = bid;
                this.Opener = NextToAct;
            }

            NextToAct = NextToAct.LeftHandOpponent;
            _positionCalls = null;  // Reset call choices
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
