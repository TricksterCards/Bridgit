using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static BridgeBidding.BidRule;

namespace BridgeBidding
{

    public delegate IEnumerable<BidRule> BidRulesFactory(PositionState ps);
  
    public delegate BidChoices BidChoicesFactory(PositionState ps);


/*
    public class BidChoiceFactories : IBidder
    {
        private BidChoices _parent;
        private Call? _call;
        private BidChoicesFactory _bidChoicesFactory;
        private BidChoicesCompetativeFacotry _bidChoicesCompetativeFactory;

        public BidChoiceFactories(BidChoices parent, Call? call, BidChoicesFactory bcf, BidChoicesCompetativeFacotry bccf)
        {
            _parent = parent;
            _call = call;
            _bidChoicesFactory = bcf;
            _bidChoicesCompetativeFactory = bccf;
        }
        public BidChoices GetBidChoices(PositionState ps)
        {   
            if (_bidChoicesFactory is BidChoicesFactory bcf)
                return bcf(ps);
            return _parent.GetBidder(_call).GetBidChoices(ps);
        }

        public BidChoices GetBidChoices(PositionState ps, Call lhoCall)
        {
            if (_bidChoicesFactory is BidChoicesFactory bcf)
                return bcf(ps);
            return _parent.GetBidder(_call).GetBidChoices(ps);        }
    }


    public class PartnerChoicesXXX
    {
        private SortedList<Call, BidChoicesFactory> _choices;

        public PartnerChoicesXXX()
        {
            _choices = new SortedList<Call, BidChoicesFactory>();
        }

        public void AddFactory(Call goodThrough, BidChoicesFactory partnerFactory)
        {
            _choices.Add(goodThrough, partnerFactory);
        }

        public BidChoicesFactory GetPartnerBidsFactory(PositionState ps)
        {
            var lhoBid = ps.LeftHandOpponent.GetBidHistory(0);
            foreach (KeyValuePair<Call, BidChoicesFactory> choice in _choices)
            {
                if (choice.Key.CompareTo(lhoBid) >= 0) return choice.Value;
            }
            return null;
        }


        public void Merge(PartnerChoicesXXX other)
        {
            if (_choices.Count == 0)
            {
                _choices = other._choices;  // TODO: Should I copy this???
            }
            else
            {
                Call highestGoodThrough = _choices.Last().Key;
                foreach (KeyValuePair<Call, BidChoicesFactory> choice in other._choices)
                {
                    if (highestGoodThrough.CompareTo(choice.Key) < 0)
                    {
                        AddFactory(choice.Key, choice.Value);
                    }
                }
            }
        }
    }
*/

    public class BidRuleGroup 
    {
        private Dictionary<Call, BidRuleSet> _choices;
        public PartnerBidRule DefaultPartnerBidRule  { get; private set; }
        private BidChoices Parent { get;}

        public BidRuleGroup(BidChoices parent)
        {
            Parent = parent;
            _choices = new Dictionary<Call, BidRuleSet>();

        }

        public bool Contains(Call call) => _choices.ContainsKey(call);

        public BidRuleSet GetBidRuleSet(Call call) => _choices[call];

        public IEnumerable<Call> AddRules(PositionState ps, IEnumerable<BidRule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.Call == null)
                {
                    if (rule is PartnerBidRule partnerBids)
                    {
                        if (rule.SatisifiesStaticConstraints(ps))
                        {
                            Debug.Assert(DefaultPartnerBidRule == null);
                            DefaultPartnerBidRule = partnerBids;
                        }
                    }
                    else
                    {
                        Debug.Fail("Rules for Null bid must be default bid factory rules only");
                    }

                }
                else
                {
                    if (ps.IsValidNextCall(rule.Call))
                    {
                        if (rule.SatisifiesStaticConstraints(ps))
                        {
                            if (!_choices.ContainsKey(rule.Call))
                            {
                                _choices[rule.Call] = new BidRuleSet(this, rule.Call, rule.Force);
                            }
                            // TODO: This is an ugly way to do this.. Hack side-effect.  Clean it up
                            _choices[rule.Call].AddRule(rule);
                            if (Parent.BestCall == null && !(rule is PartnerBidRule) && ps.PrivateHandConforms(rule))
                            {
                                Parent.BestCall = _choices[rule.Call];
                            }
                        }
                    }
                }
            }
            var added = _choices.Keys;  // We are going to modify keys inside the loop so initialize enumerator ourside of loop
            foreach (var call in added)
            {
                if (!_choices[call].HasRules)
                {
                    _choices.Remove(call);
                }
            }
            return _choices.Keys;
        }

    }

    

    public class BidChoices : Bidder // Inherit from bidder so can use static functions...
    {


        // TODO: Protected set.  Group a sub-class
        public BidRuleSet BestCall  { get; set; }

        private PositionState _ps;

        private List<BidRuleGroup> _bidGroups;

        private HashSet<Call> _definedCalls = new HashSet<Call>();
      //  private Dictionary<Call, BidRuleSet> _choices;

  //      public PartnerChoicesXXX DefaultPartnerBids { get; private set; }


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
         //   BestCall = null;
            _ps = ps;
       //     _choices = new Dictionary<Call, BidRuleSet>();
            _bidGroups = new List<BidRuleGroup>();
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
            if (_definedCalls.Contains(call))
            {
                foreach (var group in _bidGroups)
                {
                    if (group.Contains(call))
                    {
                        return group.GetBidRuleSet(call);
                    }
                }
                Debug.Fail("Should never get here.");
            }
            var newGroup = new BidRuleGroup(this);
            newGroup.AddRules(_ps, new BidRule[] { Nonforcing(call) });
            return newGroup.GetBidRuleSet(call);
        }

        public void AddRules(BidRulesFactory factory)
        {
            AddRules(factory(_ps));
        }

        public void AddRules(IEnumerable<BidRule> rules)
        {
            var group = new BidRuleGroup(this);
            _bidGroups.Add(group);
            _definedCalls.UnionWith(group.AddRules(_ps, rules));
        }

        // Method for adding a stand-alone pass rule.
        public void AddPassRule(params Constraint[] constraints)
        {
            AddRules(new BidRule[] { Nonforcing(Call.Pass, constraints) });
        }

    }
}
