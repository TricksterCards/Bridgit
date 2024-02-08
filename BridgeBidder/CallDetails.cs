using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{

    public class CallDetails
    {
		protected class RuleInfo
		{
			public RuleInfo(BidRule rule)
			{
				this.Rule = rule;
				this.HandSummary = null;
				this.PairAgreements = null;
			}
			public BidRule Rule;
			public HandSummary HandSummary;
			public PairAgreements PairAgreements;
		}
        public Call Call { get; }
		public BidForce BidForce { get; private set; }
		public List<CallAnnotation> Annotations = new List<CallAnnotation>();

		private PartnerCalls _partnerCalls = null;

        private List<RuleInfo> _ruleInfo = new List<RuleInfo>();


        public bool HasRules {  get {  return _ruleInfo.Count > 0; } }

		public CallGroup Group { get; }

		public PositionState PositionState => Group.PositionState;
       

		public CallDetails(CallGroup group, Call call) 
        {
			this.Group = group;
            this.Call = call;
			this.BidForce = BidForce.Unknown;
            this._ruleInfo = new List<RuleInfo>();
        }


		public void Add(CallFeature feature)
		{
			Debug.Assert(feature.Call.Equals(this.Call));
			if (feature is BidRule rule)
			{
				if (BidForce == BidForce.Unknown)
				{
					this.BidForce = rule.Force;
				}
				else
				{
					/*
					if (this.BidForce == BidForce.Forcing1Round && rule.Force != BidForce.Forcing1Round)

						throw new System.Exception($"{rule.Call} not forcing but prior rule was");
					if (rule.Force == BidForce.Forcing1Round && this.BidForce != BidForce.Forcing1Round)
						throw new System.Exception($"{rule.Call} is forcing but prior rule was not");
					// TODO: This may blow up in some cases.  If so then deal with it!!!
					// TODO: FIX!! Debug.Assert(this.BidForce == rule.Force);
					*/
					// TODO: Need to turn the rules above back on and catch all the errors
					// For now just try not to blow up.
					if (rule.Force == BidForce.Forcing1Round)
						this.BidForce = BidForce.Forcing1Round;
				}
				_ruleInfo.Add(new RuleInfo(rule));
			}
			else if (feature is PartnerCalls partnerCalls)
			{
				Debug.Assert(_partnerCalls == null);
				_partnerCalls = partnerCalls;
			}
			else if (feature is CallAnnotation annotation)
			{
				Annotations.Add(annotation);
			}
	    }


		public List<List<string>> GetDescriptions()
		{
			throw new NotImplementedException();
		}

		public PositionCallsFactory GetBidsFactory()
		{
			if (_partnerCalls != null)
			{
				return _partnerCalls.PartnerBids;
			}
			if (!this.Call.Equals(Call.Pass) && Group.PartnerCalls != null)
			{
				return Group.PartnerCalls.PartnerBids;
			}
			return null;
		}


        public (HandSummary, PairAgreements) ShowState()
        {
			// TODO: This is a hack. Need to understand what's going on here.  But for now if empty rules
			// just return the current state...
			if (!HasRules) { return (PositionState.PublicHandSummary, PositionState.PairState.Agreements); }

            var showHand = new HandSummary.ShowState();
            var showAgreements = new PairAgreements.ShowState();
            bool firstRule = true;
            foreach (var ri in _ruleInfo)
            {
                (HandSummary hs, PairAgreements pa) newState = ri.Rule.ShowState(PositionState);
				ri.HandSummary = newState.hs;
				ri.PairAgreements = newState.pa;	
				// TODO: This is right to save the state, but needs to be used later WITHOUT calling show.
                showHand.Combine(newState.hs, firstRule ? State.CombineRule.Show : State.CombineRule.CommonOnly);
                showAgreements.Combine(newState.pa, firstRule ? State.CombineRule.Show : State.CombineRule.CommonOnly);
                firstRule = false;
            }
            // After all of the possible shapes of suits have been unioned we can trim the max length of suits
           // TODO: NEED TO CALL ON HAND EVEALUATOR HERE... handSummary.TrimShape();
            return (showHand.HandSummary, showAgreements.PairAgreements);
        }


		public bool PruneRules(PositionState ps)
		{
			var rules = new List<RuleInfo>();
			foreach (var ri in _ruleInfo)
			{
				if (ri.Rule.SatisifiesDynamicConstraints(ps, ps.PublicHandSummary))
				{
					rules.Add(ri);
				}
			}
			if (rules.Count == _ruleInfo.Count) { return false; }
			_ruleInfo = rules;
			return true;
		}

    }

}
