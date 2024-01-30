using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


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
            this.PositionCalls = positionCalls;
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
            }
        }

    }
}