using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


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

        public ContractState Contract { get; }

        public Bid OpeningBid { get; private set; }

        public PositionState Opener { get; private set; }

        public Game Game { get; }

        public BiddingState(Game game)
        {
            this.Game = game;
            this.Positions = new Dictionary<Direction, PositionState>();
            this.Contract = new ContractState();
            var d = game.Dealer;
            var ns = new PairState(this, Pair.NS, GetBidSystem(game.BidSystemNS), game.Vulnerable);
            var ew = new PairState(this, Pair.EW, GetBidSystem(game.BidSystemEW), game.Vulnerable);
            for (int seat = 1; seat <= 4; seat++)
            {
                Hand hand;
                game.Deal.TryGetValue(d, out hand);
                this.Positions[d] = new PositionState(this, d.Pair() == Pair.NS ? ns : ew, d, seat, hand);
                d = d.LeftHandOpponent();
            }
            this.Dealer = Positions[Game.Dealer];
            this.NextToAct = Dealer;
            if (Game.Auction != null && Game.Auction.Count > 0)
            {
                var calls = Game.Auction.Calls;
                Game.Auction.Clear();
                foreach (var call in calls)
                {
                    MakeCall(call);
                }
            }
        }


        // TODO: In the furutre this code will examine the game's configuration for bidding systems.  
        // TODO: Perhaps it's more than just a string, but for now we allow only Null, Empty, or "LC-Basic"
        private static IBiddingSystem GetBidSystem(string bidSystem)
        {
            if (string.IsNullOrEmpty(bidSystem) || bidSystem == "LC-Basic")
            {
                return new LCStandard();
            }
            throw new ArgumentException($"Unknown bidding system {bidSystem}.");
        }
/*
        public void ReplayAuction(IEnumerable<Call> history)
        {
            foreach (var call in history)
            {
                MakeCall(call);
            }
        }
*/

        // Note that this method will alawys make the call specified as long as the call is valid
        public void MakeCall(Call call)
        {
            Contract.ValidateCall(call, NextToAct.Direction);
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
            Contract.MakeCall(callDetails.Call, callDetails.PositionState.Direction); // This also validates the call and will throw if a problem.

            if (this.OpeningBid == null && callDetails.Call is Bid bid)
            {
                this.OpeningBid = bid;
                this.Opener = NextToAct;
            }


            Game.Auction.Add(callDetails);
            if (Contract.AuctionComplete)
            {
                Game.Contract = Contract;
                if (!Contract.PassedOut)
                {
                    Game.Declarer = Contract.Declarer;
                }
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
