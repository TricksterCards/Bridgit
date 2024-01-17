using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using static BridgeBidding.BidRule;

namespace BridgeBidding
{

    public delegate IEnumerable<BidRule> BidRulesFactory(PositionState ps);
  
    public delegate BidChoices BidChoicesFactory(PositionState ps);




    public class BidRuleGroup : Dictionary<Call, BidRuleSet>
    {
       // private Dictionary<Call, BidRuleSet> RuleSets = new Dictionary<Call, BidRuleSet>();
        public PartnerBidRule PartnerBidRule  { get; private set; }
        public Call BestCall = null;



        public static BidRuleGroup Create(PositionState ps, HashSet<Call> existingCalls, IEnumerable<BidRule> rules)
        {
            var group = new BidRuleGroup();
            group.AddRules(ps, existingCalls, rules);
            return group;
        }

        private BidRuleGroup() {}

    //    public bool Contains(Call call) => _choices.ContainsKey(call);

    //    public IEnumerable<Call> Calls => _choices.Keys;


      //  public BidRuleSet GetBidRuleSet(Call call) => _choices[call];

        public void AddRules(PositionState ps, 
                            HashSet<Call> existingCalls,
                            IEnumerable<BidRule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.Call == null)
                {
                    if (rule is PartnerBidRule partnerBids)
                    {
                        if (rule.SatisifiesStaticConstraints(ps))
                        {
                            Debug.Assert(PartnerBidRule == null);
                            PartnerBidRule = partnerBids;
                        }
                    }
                    else
                    {
                        Debug.Fail("Rules for Null bid must be default bid factory rules only");
                    }

                }
                else
                {
                    if (ps.IsValidNextCall(rule.Call) && !existingCalls.Contains(rule.Call))
                    {
                        if (rule.SatisifiesStaticConstraints(ps))
                        {
                            if (!this.ContainsKey(rule.Call))
                            {
                                this[rule.Call] = new BidRuleSet(this, rule.Call, rule.Force);
                            }
                            this[rule.Call].AddRule(rule);

                            if (BestCall == null && 
                               !((rule is PartnerBidRule) || (rule is BidAnnotation)) && 
                               ps.PrivateHandConforms(rule))
                            {
                                BestCall = rule.Call;
                            }
                        }
                    }
                }
            }
            var calls = Keys;  // We are going to modify keys inside the loop so initialize enumerator ourside of loop
            foreach (var call in calls)
            {
                if (!this[call].HasRules)
                {
                    Debug.Assert(call != BestCall);
                    this.Remove(call);
                }
           }
        }

    }

    

    public class BidChoices 
    {
        private BidRuleGroup _bestCallGroup = null;
        private PositionState _ps;

        private List<BidRuleGroup> _bidGroups = new List<BidRuleGroup>();

        public HashSet<Call> Calls = new HashSet<Call>();

        public Call BestCall => _bestCallGroup == null ? null : _bestCallGroup.BestCall;
      
        public static BidChoicesFactory FromBidRulesFactory(BidRulesFactory bidRules)
        {
            return (ps) => {
                if (ps.RHO.Passed || ps.RHO.Doubled) {
                    return new BidChoices(ps, bidRules);
                }
                return ps.PairState.BiddingSystem.GetBidChoices(ps);
            };
        }


        public BidChoices(PositionState ps)
        {
            _ps = ps;
        }




        public BidChoices(PositionState ps, BidRulesFactory rulesFactory) : this(ps)
        {
            AddRules(rulesFactory);
        }

        public BidChoices(PositionState ps, IEnumerable<BidRule> rules) : this(ps)
        {
            AddRules(rules);
        }


        public BidRuleSet GetBidRuleSet(Call call)
        {
            // This is a small optimization to avoid searching groups if the BestCall has been selected
            if (call == BestCall)
                return _bestCallGroup[call];

            if (Calls.Contains(call))
            {
                foreach (var group in _bidGroups)
                {
                    if (group.ContainsKey(call))
                    {
                        return group[call];
                    }
                }
                Debug.Fail("Should never get here.");
            }
            var fakeGroup = BidRuleGroup.Create(_ps, Calls, new BidRule[] { Bidder.Nonforcing(call) });
            return fakeGroup[call];
        }

        public void AddRules(BidRulesFactory factory)
        {
            AddRules(factory(_ps));
        }

        public void AddRules(IEnumerable<BidRule> rules)
        {
            var group = BidRuleGroup.Create(_ps, Calls, rules);
            _bidGroups.Add(group);
            Calls.UnionWith(group.Keys);
            if (_bestCallGroup == null && group.BestCall != null)
                _bestCallGroup = group;
        }

        // Method for adding a stand-alone pass rule.
        public void AddPassRule(params Constraint[] constraints)
        {
            AddRules(new BidRule[] { Bidder.Nonforcing(Call.Pass, constraints) });
        }

    }
}
