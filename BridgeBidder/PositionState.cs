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
		// places a bid as Advancer, the offset would be 2, indicating that the position became the Advancer on the
		// third round of bidding.  This allows us to property compute the "RoleRound" property.
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
			get { return BiddingState.Contract.IsOurs(this.Direction); }
		}

        public bool IsOpponentsContract
        {
			get { return BiddingState.Contract.IsOpponents(this.Direction); }
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



		public PositionState Partner => BiddingState.Positions[Direction.Partner()];
		public PositionState RightHandOpponent => BiddingState.Positions[Direction.RightHandOpponent()];
		public PositionState LeftHandOpponent => BiddingState.Positions[Direction.LeftHandOpponent()];


		public PositionState RHO => RightHandOpponent;


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
				return ((PairState.Agreements.ForcingToGame || 
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
	

		internal void MakeCall(CallDetails callDetails)
		{
			BiddingState.Contract.ValidateCall(callDetails.Call, this.Direction);
            if (!callDetails.Call.Equals(Call.Pass) && !this._roleAssigned)
			{
				if (Role == PositionRole.Opener)
				{
					AssignRole(PositionRole.Opener);
					Partner.AssignRole(PositionRole.Responder);
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

			// Now show any state changes to the PairAgreements.  This only happens once per call
			// and does not update dynamically like the PublicHandSummary.
			// TODO: Should this happen here?  
			//var showAgreements = new PairAgreements.ShowState(PairState.Agreements);
			//showAgreements.Combine(callDetails.ShowAgreements(), State.CombineRule.Merge);
			PairState.Agreements = callDetails.ShowAgreements();

		///	if (callDetails.BidForce == BidForce.ForcingToGame)
		//	{
		//		PairState.InGameForcingAuction = true;
	//		}
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

				var showHand = new HandSummary.ShowState(PublicHandSummary);
				showHand.Combine(callDetails.ShowHand(), State.CombineRule.Merge);

				if (this.PublicHandSummary.Equals(showHand.HandSummary)) 
				{ 
					return stateChanged;
				}
				stateChanged = true;
				PairState.UpdateShownSuits(callDetails.Call, this, showHand.HandSummary);
				PublicHandSummary = showHand.HandSummary;
			}
			Debug.Assert(false); // This is bad - we had over 1000 state changes.  Infinite loop time...
			return false;	// Seems the best thing to do to avoid repeated
		}

		// TODO: Perhaps remove this.  Looks like combinations of other 
		// constraints are used instead.
		public bool IsOpenerJumpShift(Call call)
		{
			return (this.Role == PositionRole.Opener &&
                    this.BiddingState.OpeningBid is Bid openingBid &&
                    call is Bid thisBid &&
                    thisBid.Strain != Strain.NoTrump && 
                    openingBid.Strain != Strain.NoTrump &&
                    thisBid.JumpOver(openingBid) == 1 &&
                    PairState.FirstToShow((Suit)thisBid.Suit) == null);
		}


		public bool IsReverse(Call call)
		{
			if (call is Bid bid && bid.Suit is Suit bidSuit)
			{
				foreach (CallDetails callDetails in _bids.Reverse<CallDetails>())
				{
					if (callDetails.Call is Bid lastBid)
					{
						return (lastBid.Level < bid.Level && lastBid.Suit != null && lastBid.Suit < bidSuit &&
							PairState.FirstToShow(bidSuit) == null);
					}
				}
			}
			return false;
		}


		// TODO: This logic is spread out across several classes.  Think about how to consolidate it.
		public bool PrivateHandConforms(BidRule rule)
		{
			return HasHand ? rule.SatisifiesHandConstraints(this, this._privateHandSummary) : false;
		}

		// Returns a list of dynamic constraints that do not conform to the private hand.
		// If the list is empty, then the hand conforms.  Any caller should check HasHand first.
		public List<Constraint> PrivateHandFailingConstraints(BidRule rule)
		{
			Debug.Assert(HasHand);
			return rule.FailingHandConstraints(this, this._privateHandSummary);
		}

		public bool IsValidNextCall(Call call)
		{
			return BiddingState.Contract.IsValid(call, this.Direction);
		} 
	}
}
