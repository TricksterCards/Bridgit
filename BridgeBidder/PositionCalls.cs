using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


namespace BridgeBidding
{


    public delegate IEnumerable<CallFeature> CallFeaturesFactory(PositionState ps);
  
    public delegate PositionCalls PositionCallsFactory(PositionState ps);

    public class PositionCalls: Dictionary<Call, CallDetails>
    {
        public enum LogAction { Illegal, Duplicate, Rejected, Accepted, Chosen, NotChosen };
        public class LogEntry
        {
            public PositionCalls PositionCalls { get; }
            public BidRule BidRule { get; }
            public LogAction Action;

            public List<Constraint> FailingConstraints = null;

            internal LogEntry(PositionCalls positionCalls, BidRule bidRule)
            {
                PositionCalls = positionCalls;
                BidRule = bidRule;
            }

            public override string ToString()
            {
                string id = LogID.GetID(BidRule);
                string call = id == null ? $"{BidRule.Call, 3}" : $"{BidRule.Call, 3} {id}";
                switch (Action)
                {
                    case LogAction.Illegal:
                    case LogAction.Duplicate:
                        return $"{call} {Action}";
                    
                    case LogAction.Chosen:
                    case LogAction.Accepted:
                        return $"{call} {Action}: {BidRule.GetDescription(PositionCalls.PositionState)}";

                    default:    // This is the case for LogAction.Rejected and LogAction.NotChosen
                        return $"{call} {Action}, not comforming: {string.Join(", ", FailingConstraints.Select(c => c.GetLogDescription(BidRule.Call, PositionCalls.PositionState)))}";
                }
            
            }
        }


        public PositionState PositionState { get; }
        public CallDetails BestCall { get; private set; } 

        // The following fields are only used for diagnoitic purposes.
        public List<LogEntry> BidRuleLog { get; } = new List<LogEntry>();

        public string CallerMemberName { get; }
        public string CallerSourceFilePath { get; }
        public int CallerSourceLineNumber { get; }



        // TODO: Work on plubming the calling member name, source file path, and line number to the PositionCalls constructor.
        public static PositionCallsFactory FromCallFeaturesFactory(CallFeaturesFactory CallFeatures)
        {
            return (ps) => {
                if (ps.RHO.Passed || ps.RHO.Doubled) {
                    var calls = new PositionCalls(ps);
                    calls.AddRules(CallFeatures(ps));
                    return calls;
                }
                return ps.PairState.BiddingSystem.GetPositionCalls(ps);
            };
        }

        public PositionCalls(PositionState ps,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            PositionState = ps;
            CallerMemberName = memberName;
            CallerSourceFilePath = sourceFilePath;
            CallerSourceLineNumber = sourceLineNumber;
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

        public void AddRules(params CallFeature[] rules)
        {
            AddRules(rules.AsEnumerable());
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


        // IF this feature is a BidRule then examine all of the things that the selection code will and
        // log the results.  In the future we may want to disalbe logging globally, so this function could
        // check a flag in BiddingState before doing all this workl
        internal void LogBidRule(BidRule rule)
        {
            var entry = new LogEntry (this, rule);
            if (!PositionState.IsValidNextCall(rule.Call))
            {
                entry.Action = LogAction.Illegal;
            }
            else if (ContainsKey(rule.Call))
            {
                entry.Action = LogAction.Duplicate;
            }
            else
            {
                entry.FailingConstraints = rule.FailingStaticConstraints(PositionState);
                entry.Action = entry.FailingConstraints.Count > 0 ? LogAction.Rejected : LogAction.Accepted;
                if (entry.Action == LogAction.Accepted && PositionState.HasHand)
                {
                    entry.FailingConstraints = PositionState.PrivateHandFailingConstraints(rule);
                    entry.Action = entry.FailingConstraints.Count > 0 ? LogAction.NotChosen : LogAction.Chosen;
                }
            }
            BidRuleLog.Add(entry);
        }
    }
}
