using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


namespace BridgeBidding
{
	public enum PositionRole { Opener, Overcaller, Responder, Advancer }




	public class PositionState
	{
		// When the first bid is made for a role, this variable is assigned to the length of _bids.  This allows
		// the property RoleRound to return the proper value.  For example, if a position has passed twice and then
		// places a bid as Advancer, the offset would be 2, indicating that advancer has 
		private int _roleAssignedOffset = 0;
		private bool _roleAssigned = false;

		private HandSummary _privateHandSummary;

		public bool HasHand => _privateHandSummary != null;

		private List<CallDetails> _bids;

		public PairState PairState { get; private set; }

		public BiddingState BiddingState { get; }

		public PositionRole Role { get; internal set; }

		public HandSummary PublicHandSummary { get; private set; }

		public bool Passed => _bids.Count == 0 || _bids.Last().Call.Equals(Call.Pass);

		public bool IsPassedHand => _bids.Count > 0 && _bids.First().Call.Equals(Call.Pass);

		public bool Doubled => _bids.Count > 0 && _bids.Last().Call.Equals(Call.Double);

		public Bid Bid 
		{
			get 
			{
				if (_bids.Count == 0) return null;
				return _bids.Last().Call as Bid;
			}
		}

		public Direction Direction { get; }

		public int Seat { get; }
		public bool IsVulnerable { 
			get {
				return PairState.AreVulnerable;
			}
		 }

		public bool IsOurContract
		{
			get { return BiddingState.Contract.IsOurs(this); }
		}

        public bool IsOpponentsContract
        {
			get { return BiddingState.Contract.IsOpponents(this); }
        }

        public Call GetBidHistory(int historyLevel)
		{
			if (_bids.Count <= historyLevel)
			{
				return null;
			}
			return _bids[_bids.Count - 1 - historyLevel].Call;
		}


		public int CallCount => _bids.Count;

		public CallDetails GetCallDetails(int index) => _bids[index];

		private Direction OffsetDirection(int offset)
		{
			return (Direction)(((int)Direction + offset) % 4);
		}

		public PositionState Partner => BiddingState.Positions[OffsetDirection(2)];
		public PositionState RightHandOpponent => BiddingState.Positions[OffsetDirection(3)];
		public PositionState LeftHandOpponent => BiddingState.Positions[OffsetDirection(1)];


		public PositionState RHO => RightHandOpponent;

		// TODO: Potentially LHO Interferred...  Maybe just in 


		public PositionState(BiddingState biddingState, PairState pairState, Direction direction, int seat, Hand hand)
		{
			Debug.Assert(seat >= 1 && seat <= 4);
			this.BiddingState = biddingState;
			this.Direction = direction;
			this.Seat = seat;
			this.Role = PositionRole.Opener;    // Best start for any position.  Will change with time.
			this.PublicHandSummary = new HandSummary();
			this.PairState = pairState;
			this._bids = new List<CallDetails>();

			if (hand != null)
			{
				var showHand = new HandSummary.ShowState();
				// TODO: This is where we would need to use a differnet implementation of HandSummary evaluator...
				StandardHandEvaluator.Evaluate(hand, showHand);
				this._privateHandSummary = showHand.HandSummary;
			}
			else
			{
				this._privateHandSummary = null;
			}
		}

		public int BidRound
		{
			get { return this._bids.Count + 1; }
		}

		public int RoleRound
		{
			get { return BidRound - _roleAssignedOffset; }
		}

		public Call LastCall { get { return GetBidHistory(0); } }


		public bool ForcedToBid
		{
			get
			{		
				return ((PairState.InGameForcingAuction || 
						(Partner._bids.Count > 0 &&
						Partner._bids.Last().BidForce == BidForce.Forcing1Round)) &&
						!RightHandOpponent._bids.Last().Equals(Call.Pass));
	
			}
		
		}
		
		public PositionCalls GetPositionCalls()
		{ 
			PositionCallsFactory bidFactory = Partner._bids.Count > 0 ? Partner._bids.Last().GetBidsFactory() : null;
			if (bidFactory != null) return bidFactory(this);
			return PairState.BiddingSystem.GetPositionCalls(this);
		}
	


		// THIS IS AN INTERNAL FUNCITON:
		internal void MakeCall(CallDetails callDetails)
		{
			BiddingState.Contract.ValidateCall(callDetails.Call, this);
            if (!callDetails.Call.Equals(Call.Pass) && !this._roleAssigned)
			{
				if (Role == PositionRole.Opener)
				{
					AssignRole(PositionRole.Opener);
					Partner.AssignRole(PositionRole.Responder);
					// The opponenents are now 
					LeftHandOpponent.Role = PositionRole.Overcaller;
					RightHandOpponent.Role = PositionRole.Overcaller;
				}
				else if (this.Role == PositionRole.Overcaller)
				{
					AssignRole(PositionRole.Overcaller);
					Partner.AssignRole(PositionRole.Advancer);
				}
			}
			_bids.Add(callDetails);
			if (callDetails.BidForce == BidForce.ForcingToGame)
			{
				PairState.InGameForcingAuction = true;
			}
			// Now we prune any rules that do not 

			if (RepeatUpdatesUntilStable(callDetails))
			{
				BiddingState.UpdateStateFromFirstBid();
			}
		}

		private void AssignRole(PositionRole role)
		{
			Debug.Assert(_roleAssigned == false);
			Role = role;
			_roleAssigned = true;
			_roleAssignedOffset = _bids.Count;
		}

		internal bool UpdateBidIndex(int bidIndex, out bool updateHappened)
		{
			if (bidIndex >= _bids.Count)
			{
				updateHappened = false;
				return false;
			}
			updateHappened = RepeatUpdatesUntilStable(this._bids[bidIndex]);
			return true;
		}

		internal bool RepeatUpdatesUntilStable(CallDetails callDetails)
		{
			Debug.Assert(callDetails.PositionState == this);

			bool stateChanged = false;
			for (int i = 0; i < 1000; i++)
			{
                stateChanged |= callDetails.PruneRules(this);

                (HandSummary hs, PairAgreements pa) newState = callDetails.ShowState();

				var showHand = new HandSummary.ShowState(PublicHandSummary);
				var showAgreements = new PairAgreements.ShowState(PairState.Agreements);

				showHand.Combine(newState.hs, State.CombineRule.Merge);
				showAgreements.Combine(newState.pa, State.CombineRule.Merge);


				if (this.PublicHandSummary.Equals(showHand.HandSummary) &&
					this.PairState.Agreements.Equals(showAgreements.PairAgreements)) 
				{ 
					return stateChanged;
				}
				stateChanged = true;
				this.PublicHandSummary = showHand.HandSummary;
				this.PairState.Agreements = showAgreements.PairAgreements;
			}
			Debug.Assert(false); // This is bad - we had over 1000 state changes.  Infinite loop time...
			return false;	// Seems the best thing to do to avoid repeated
		}


        public bool IsOpenerReverseBid(Call call)
        {
			// TODO: For now only 2-level bids are considered a reverse
			// When in competition, perhaps allow reverses at higher levels...
            return (this.Role == PositionRole.Opener &&
                    this.BiddingState.OpeningBid is Bid openingBid &&
                	call is Bid thisBid &&
                    thisBid.Level == 2 &&
                    thisBid.Strain != Strain.NoTrump && 
                    openingBid.Strain != Strain.NoTrump &&
                    thisBid.Suit > openingBid.Suit &&
                    !this.PairState.Agreements.Strains[thisBid.Strain].Shown);
        }

		public bool IsOpenerJumpShift(Call call)
		{
			return (this.Role == PositionRole.Opener &&
                    this.BiddingState.OpeningBid is Bid openingBid &&
                    call is Bid thisBid &&
                    thisBid.Strain != Strain.NoTrump && 
                    openingBid.Strain != Strain.NoTrump &&
                    thisBid.JumpOver(openingBid) == 1 &&
                    !this.PairState.Agreements.Strains[thisBid.Strain].Shown);
		}


		public bool PrivateHandConforms(BidRule rule)
		{
			return (this._privateHandSummary == null) ? false : rule.SatisifiesDynamicConstraints(this, this._privateHandSummary);
		}


		public bool IsValidNextCall(Call call)
		{
			return BiddingState.Contract.IsValid(call, this);
		}

 
	}
}
