using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BridgeBidding
{

    public class CallDetails
    {

        public Call Call { get; }

		public List<CallAnnotation> Annotations = new List<CallAnnotation>();

		private CallProperties _properties = null;


        private List<BidRule> _rules = new List<BidRule>();


        public bool HasRules {  get {  return _rules.Count > 0; } }

		public CallGroup Group { get; }

		public PositionState PositionState => Group.PositionState;
       

		public CallDetails(CallGroup group, Call call) 
        {
			this.Group = group;
            this.Call = call;
        }


		public void Add(CallFeature feature)
		{
			Debug.Assert(feature.Call.Equals(this.Call));
			if (feature is BidRule rule)
			{
				_rules.Add(rule);
			}
			else if (feature is CallProperties callProperties)
			{
				Debug.Assert(_properties == null);

				_properties = callProperties;
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
			if (_properties != null && _properties.PartnerBids != null)
			{
				return _properties.PartnerBids;
			}
			if (!this.Call.Equals(Call.Pass) && Group.PartnerCalls != null)
			{
				return Group.PartnerCalls.PartnerBids;
			}
			return null;
		}

/*
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
