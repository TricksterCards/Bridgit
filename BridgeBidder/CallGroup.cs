using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using static BridgeBidding.PositionCalls;

namespace BridgeBidding
{


    public class CallGroup : Dictionary<Call, CallDetails>
    {
        public PositionCalls PositionCalls { get; }

        public PositionState PositionState => PositionCalls.PositionState;

        public CallProperties PartnerCalls  { get; private set; }
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
       
        private void AddRules(IEnumerable<CallFeature> features)
        {
            RecurseAddRules(features);
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
                    this[call].Annotations.AddRange(this.Annotations);
                }
            }
        }

        // Add only calls that are not already defined in the PositionCalls
        private void RecurseAddRules(IEnumerable<CallFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature is CallFeatureGroup group)
                {
                    if (group.SatisifiesStaticConstraints(PositionState))
                    {
                        RecurseAddRules(group.Features);
                    }
                }
                else if (feature.Call == null)
                {
                    if (feature.SatisifiesStaticConstraints(PositionState))
                    {
                        if (feature is CallProperties partnerCalls)
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
                    if (feature is BidRule bidRule)
                    {
                        PositionCalls.LogBidRule(bidRule);
                    }
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

        }
    }
}