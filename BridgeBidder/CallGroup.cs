using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using static BridgeBidding.PositionCalls.BidRuleLog;

namespace BridgeBidding
{


    public class CallGroup : Dictionary<Call, CallDetails>
    {
        public PositionCalls PositionCalls { get; }

        public PositionState PositionState => PositionCalls.PositionState;

        public PartnerCalls PartnerCalls  { get; private set; }
        public List<CallAnnotation> Annotations { get; }
        public CallDetails BestCall = null;




        public static CallGroup Create(PositionCalls positionCalls, IEnumerable<CallFeature> rules)
        {
            var group = new CallGroup(positionCalls);
            group.AddRules(rules);
            return group;
        }

        private CallGroup(PositionCalls positionCalls)
        {
            PositionCalls = positionCalls;
            Annotations = new List<CallAnnotation>();
        }
       
        // Add only calls that are not already defined in the PositionCalls
        private void AddRules(IEnumerable<CallFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature.Call == null)
                {
                    if (feature.SatisifiesStaticConstraints(PositionState))
                    {
                        if (feature is PartnerCalls partnerCalls)
                        {
                            Debug.Assert(PartnerCalls == null);
                            PartnerCalls = partnerCalls;
                        }
                        else if (feature is CallAnnotation annotation)
                        {
                            Annotations.Add(annotation);
                        }
                        else
                        {
                            Debug.Fail("Unknown feature with null call.");
                        }
                    }
                }
                else
                {
                    LogBidRule(feature);
                    if (PositionState.IsValidNextCall(feature.Call) &&
                        !PositionCalls.ContainsKey(feature.Call))
                    {
                        if (feature.SatisifiesStaticConstraints(PositionState))
                        {
                            if (!this.ContainsKey(feature.Call))
                            {
                                this[feature.Call] = new CallDetails(this, feature.Call);
                            }
                            this[feature.Call].Add(feature);

                            if (BestCall == null &&
                                feature is BidRule rule && 
                                PositionState.PrivateHandConforms(rule))
                            {
                                BestCall = this[feature.Call];
                            }
                        }
                    }
                }
            }
            var calls = Keys.ToList();  // We are going to modify keys inside the loop so initialize enumerator ourside of loop
            foreach (var call in calls)
            {
                if (!this[call].HasRules)
                {
                    Debug.Assert(BestCall == null || !call.Equals(BestCall.Call));
                    this.Remove(call);
                }
                else
                {
                    // Add any group annotations to every call
                    this[call].Annotations.AddRange(this.Annotations);
                }
            }
        }


        // IF this feature is a BidRule then examine all of the things that the selection code will and
        // log the results.  In the future we may want to disalbe logging globally, so this function could
        // check a flag in BiddingState before doing all this workl
        private void LogBidRule(CallFeature feature)
        {
            BidRule rule = feature as BidRule;
            if (rule == null) return;

            var entry = new Entry { BidRule = rule };
            if (!PositionState.IsValidNextCall(rule.Call))
            {
                entry.Action = Action.Illegal;
            }
            else if (PositionCalls.ContainsKey(rule.Call))
            {
                entry.Action = Action.Duplicate;
            }
            else
            {
                foreach (var constraint in rule.Constraints)
                if (constraint is StaticConstraint staticConstraint)
                {
                    if (!staticConstraint.Conforms(feature.Call, PositionState))
                    {
                        entry.FailingConstraints.Add(staticConstraint);
                    }
                }
                if (entry.FailingConstraints.Count > 0)
                {
                    entry.Action = Action.Rejected;
                }
                else
                {
                    entry.Action = Action.Accepted;
                }
            }
            if (entry.Action == Action.Accepted && PositionState.HasHand)
            {
                entry.Action = Action.Chosen;
                foreach (var constraint in rule.Constraints)
                {
                    if (constraint is DynamicConstraint dynamicConstraint)
                    {
                        if (!dynamicConstraint.Conforms(feature.Call, PositionState, PositionState._privateHandSummary))
                        {
                            entry.Action = Action.NotChosen;
                            entry.FailingConstraints.Add(constraint);
                        }
                    }
                }
            }
            PositionCalls.Log.Add(entry);
        }
    }
}