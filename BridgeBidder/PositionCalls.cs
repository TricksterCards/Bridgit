﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;


namespace BridgeBidding
{


    public delegate IEnumerable<CallFeature> CallFeaturesFactory(PositionState ps);
  
    public delegate PositionCalls PositionCallsFactory(PositionState ps);

    public class PositionCalls: Dictionary<Call, CallDetails>
    {
    // TODO: Move this to another file when it gets nailed down.

        public class BidRuleLog : List<BidRuleLog.Entry>
        {
            public string CallerMemberName { get; }
            public string CallerSourceFilePath { get; }
            public int CallerSourceLineNumber { get; }
            public enum Action { Illegal, Duplicate, Rejected, Accepted, Chosen, NotChosen };
            public class Entry
            {
                public Action Action;

                public BidRule BidRule;

                public List<Constraint> FailingConstraints = new List<Constraint>();

                public string GetDescription(PositionState ps)
                {
                    if (Action == Action.Accepted || Action == Action.Chosen || Action == Action.NotChosen)
                    {
                        return $"{BidRule.Call} {Action} {BidRule.GetDescription(ps)}";
                    }
                    var descriptions = new List<string>();
                    foreach (var constraint in FailingConstraints)
                    {
                        descriptions.Add(constraint.GetDescription(ps));
                    }
                    return $"{BidRule.Call} {Action} {string.Join(", ", descriptions)}";
                }
            }

            public BidRuleLog(string callerMemberName, string callerSourceFilePath, int callerSourceLineNumber)
            {
                CallerMemberName = callerMemberName;
                CallerSourceFilePath = callerSourceFilePath;
                CallerSourceLineNumber = callerSourceLineNumber;
            }
        }



        public PositionState PositionState { get; }
        public CallDetails BestCall { get; private set; } 

        public BidRuleLog Log { get; }

        public static PositionCallsFactory FromCallFeaturesFactory(CallFeaturesFactory CallFeatures)
        {
            return (ps) => {
                if (ps.RHO.Passed || ps.RHO.Doubled) {
                    var calls = new PositionCalls(ps);
                    // TODO: It is unfortunate that we don't get the call features class or line number.
                    // Think about this and see if there is a way to get this information.
                    calls.AddRules(CallFeatures(ps));
                    return calls;
                }
                return ps.PairState.BiddingSystem.GetPositionCalls(ps);
            };
        }

        public PositionCalls(PositionState ps,
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            PositionState = ps;
            Log = new BidRuleLog(memberName, sourceFilePath, sourceLineNumber);
        }

/*        public PositionCalls(PositionState ps, CallFeaturesFactory rulesFactory) : this(ps)
        {
            AddRules(rulesFactory);
        }

        public PositionCalls(PositionState ps, IEnumerable<CallFeature> rules) : this(ps)
        {
            AddRules(rules);
        }
*/
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
