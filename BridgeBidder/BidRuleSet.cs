using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{

    public class BidRuleSet
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

		private PartnerBidRule _partnerBidRule = null;

	//	private PartnerChoicesXXX _partnerChoices;


        private List<RuleInfo> _ruleInfo = new List<RuleInfo>();

        public bool HasRules {  get {  return _ruleInfo.Count > 0; } }

		public BidRuleGroup Parent { get; }
       

		public BidRuleSet(BidRuleGroup parent, Call call, BidForce bidForce) 
        {
			this.Parent = parent;
            this.Call = call;
			this.BidForce = bidForce;
            this._ruleInfo = new List<RuleInfo>();
        }


		public void AddRule(BidRule rule)
		{
			Debug.Assert(rule.Call.Equals(this.Call));
			if (rule.Force != this.BidForce)
			{
				// TODO: Need to do something about this.  For now if
				// either is forcing then we will say it is forcing.
				if (this.BidForce != BidForce.Forcing1Round &&
				    rule.Force == BidForce.Forcing1Round) 
				{
					this.BidForce = rule.Force;
				}
			}
			if (rule is PartnerBidRule partnerBidRule)
			{
				Debug.Assert(_partnerBidRule == null);
				_partnerBidRule = partnerBidRule;
			}
			else
			{
				_ruleInfo.Add(new RuleInfo(rule));
			}
	    }


		public BidChoicesFactory GetBidsFactory()
		{
			if (_partnerBidRule != null)
			{
				return _partnerBidRule.PartnerBids;
			}
			if (!this.Call.Equals(Call.Pass) && Parent.DefaultPartnerBidRule != null)
			{
				return Parent.DefaultPartnerBidRule.PartnerBids;
			}
			return null;		// TODO: Maybe always return default here... Clean up
		}

        //
        // This method makes sure that any rules that do not apply are removed.  If there are no rules that could apply
        // then the priority.
        /*
		public void Prune(HandSummary handSummary, PositionState positionState)
		{
			var conforming = new List<BidRule>();
			int priority = int.MinValue;
			foreach (BidRule rule in _rules)
			{
				if (handSummary != null && rule.Conforms(positionState, true))
				{
					priority = Math.Max(priority, rule.Priority);
					conforming.Append(rule);
				}
				else if (rule.Conforms(positionState, false))
				{
					conforming.Append(rule);
				}
			}
			this._rules = conforming;
			this.Priority = priority;
		}
		*/


        // Returns true if at least one rule conforms in this group.
        public bool Conforms(PositionState ps, HandSummary hs)
        {
            foreach (var ri in _ruleInfo)
            {
                if (ri.Rule.SatisifiesDynamicConstraints(ps, hs)) { return true; }
            }
            return false;
        }

        public (HandSummary, PairAgreements) ShowState(PositionState ps)
        {
			// TODO: This is a hack. Need to understand what's going on here.  But for now if empty rules
			// just return the current state...
			if (!HasRules) { return (ps.PublicHandSummary, ps.PairState.Agreements); }

            var showHand = new HandSummary.ShowState();
            var showAgreements = new PairAgreements.ShowState();
            bool firstRule = true;
            foreach (var ri in _ruleInfo)
            {
                (HandSummary hs, PairAgreements pa) newState = ri.Rule.ShowState(ps);
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

        
        /*
		public bool UpdateShownState(Direction direction, PairAgreements PairAgreements)
		{
			// TODO: Start with existing shown state
			// Then create a new shownState object
			ShownState compositeShown = new ShownState();
			bool isFirstRule = true;
			foreach (var rule in _rules)
			{
				var ruleShows = rule.ShownState(direction, PairAgreements);
				if (isFirstRule)
				{
					compositeShown = ruleShows;
					isFirstRule = false;
				} 
				else
				{
					compositeShown.Union(ruleShows);
				}
			}
			return false;	// TODO:   THIS IS ABSOLUTELY BUSTED -- NEED TO COMPARE TO LAST STATE KNOWN....

		}
		*/

    }

}
