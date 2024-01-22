using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using static BridgeBidding.CallFeature;

namespace BridgeBidding
{
    public delegate IEnumerable<CallFeature> CallFeaturesFactory(PositionState ps);
  
    public delegate PositionCalls PositionCallsFactory(PositionState ps);



    public class PositionCalls 
    {
        public PositionState PositionState { get; }

        public HashSet<Call> Calls { get; }

        public Call BestCall => _bestCallGroup == null ? null : _bestCallGroup.BestCall;
      
        private CallGroup _bestCallGroup = null;
        private List<CallGroup> _callGroups = new List<CallGroup>();

        public static PositionCallsFactory FromCallFeaturesFactory(CallFeaturesFactory CallFeatures)
        {
            return (ps) => {
                if (ps.RHO.Passed || ps.RHO.Doubled) {
                    return new PositionCalls(ps, CallFeatures);
                }
                return ps.PairState.BiddingSystem.GetPositionCalls(ps);
            };
        }

        public PositionCalls(PositionState ps)
        {
            PositionState = ps;
            Calls = new HashSet<Call>();
        }

        public PositionCalls(PositionState ps, CallFeaturesFactory rulesFactory) : this(ps)
        {
            AddRules(rulesFactory);
        }

        public PositionCalls(PositionState ps, IEnumerable<CallFeature> rules) : this(ps)
        {
            AddRules(rules);
        }


        public CallDetails GetCallDetails(Call call)
        {
            // This is a small optimization to avoid searching groups if the BestCall has been selected
            if (call == BestCall)
                return _bestCallGroup[call];

            if (Calls.Contains(call))
            {
                foreach (var group in _callGroups)
                {
                    if (group.ContainsKey(call))
                    {
                        return group[call];
                    }
                }
                Debug.Fail("Should never get here.");
            }
            var fakeGroup = CallGroup.Create(PositionState, Calls, new CallFeature[] { Bidder.Nonforcing(call) });
            return fakeGroup[call];
        }

        public void AddRules(CallFeaturesFactory factory)
        {
            AddRules(factory(PositionState));
        }

        public void AddRules(IEnumerable<CallFeature> rules)
        {
            var group = CallGroup.Create(PositionState, Calls, rules);
            _callGroups.Add(group);
            Calls.UnionWith(group.Keys);
            if (_bestCallGroup == null && group.BestCall != null)
                _bestCallGroup = group;
        }

        // Method for adding a stand-alone pass rule.
        public void AddPassRule(params Constraint[] constraints)
        {
            AddRules(new CallFeature[] { Bidder.Nonforcing(Call.Pass, constraints) });
        }

    }
}
