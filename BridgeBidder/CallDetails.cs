using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BridgeBidding
{

    public class CallDetails
    {

        public Call Call { get; }
		public BidForce BidForce { get; private set; }
		public List<CallAnnotation> Annotations = new List<CallAnnotation>();

		private PartnerCalls _partnerCalls = null;

        private List<BidRule> _rules = new List<BidRule>();


        public bool HasRules {  get {  return _rules.Count > 0; } }

		public CallGroup Group { get; }

		public PositionState PositionState => Group.PositionState;
       

		public CallDetails(CallGroup group, Call call) 
        {
			this.Group = group;
            this.Call = call;
			this.BidForce = BidForce.Unknown;
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
					if (rule.Force == BidForce.Forcing1Round)
						this.BidForce = BidForce.Forcing1Round;
				}
				_rules.Add(rule);
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


		public List<List<string>> GetCallDescriptions()
		{
			var descriptions = new List<List<string>>();
			foreach (var rule in _rules)
			{
				var d = rule.ConstraintDescriptions(PositionState);
				if (d != null)
				{
					descriptions.Add(d);
				}
			}
			return descriptions;
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


        public PairAgreements ShowAgreements()
        {  
			return _rules[0].ShowAgreements(PositionState);
			/*
            bool firstRule = true;
            foreach (var ri in _ruleInfo)
            {
                PairAgreements pa = ri.Rule.ShowAgreements(PositionState);
				ri.PairAgreements = pa;	// TODO: REMOVE THIS COMPLETELY??
				// TODO: This is right to save the state, but needs to be used later WITHOUT calling show.
                showAgreements.Combine(pa, firstRule ? State.CombineRule.Show : State.CombineRule.EqualOnly);
                firstRule = false;
            }
            return showAgreements.PairAgreements;
			*/
        }

		internal void Validate()
		{
			PairAgreements a = null;
			foreach (var rule in _rules)
			{
				var a2 = rule.ShowAgreements(PositionState);
				if (a == null)
				{
					a = a2;
				}
				else
				{
					Debug.Assert(a.Equals(a2));
				}
			}
		}


        public HandSummary ShowHand()
        {
			// TODO: This is a hack. Need to understand what's going on here.  But for now if empty rules
			// just return the current state...
			if (!HasRules) { return PositionState.PublicHandSummary; }

            var showHand = new HandSummary.ShowState();
            bool firstRule = true;
            foreach (var rule in _rules)
            {
                HandSummary hs = rule.ShowHand(PositionState);
				//ri.HandSummary = hs;	
				// TODO: This is right to save the state, but needs to be used later WITHOUT calling show.
                showHand.Combine(hs, firstRule ? State.CombineRule.Show : State.CombineRule.CommonOnly);
                firstRule = false;
            }
            return showHand.HandSummary;
        }


		public bool PruneRules(PositionState ps)
		{
			var rules = new List<BidRule>();
			foreach (var rule in _rules)
			{
				if (rule.SatisifiesHandConstraints(ps, ps.PublicHandSummary))
				{
					rules.Add(rule);
				}
			}
			if (rules.Count == _rules.Count) { return false; }
			_rules = rules;
			return true;
		}

    }

}
