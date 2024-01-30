using System;
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

    public class PositionCalls: Dictionary<Call, CallDetails>
    {
        public PositionState PositionState { get; }
        public CallDetails BestCall { get; private set; } 

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
        }

        public PositionCalls(PositionState ps, CallFeaturesFactory rulesFactory) : this(ps)
        {
            AddRules(rulesFactory);
        }

        public PositionCalls(PositionState ps, IEnumerable<CallFeature> rules) : this(ps)
        {
            AddRules(rules);
        }

        public void AddRules(CallFeaturesFactory factory)
        {
            AddRules(factory(PositionState));
        }

        public void AddRules(IEnumerable<CallFeature> rules)
        {
            var group = CallGroup.Create(this, rules);
            foreach (var c in group)
            {
                Add(c.Key, c.Value);
            }
            if (BestCall == null)
            {
                BestCall = group.BestCall;
            }
        }

        // Method for adding a stand-alone pass rule.
        public void AddPassRule(params Constraint[] constraints)
        {
            AddRules(new CallFeature[] { Bidder.Nonforcing(Call.Pass, constraints) });
        }

        public void CreatePlaceholderCall(Call call)
        {
            AddRules(new CallFeature[] { Bidder.Nonforcing(call) });
        }
    }
}
