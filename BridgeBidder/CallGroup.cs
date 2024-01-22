using System.Diagnostics;
using System.Collections.Generic;


namespace BridgeBidding
{
    public class CallGroup : Dictionary<Call, CallDetails>
    {
        public PartnerCalls PartnerCalls  { get; private set; }
        public List<CallAnnotation> Annotations { get; }
        public Call BestCall = null;



        public static CallGroup Create(PositionState ps, HashSet<Call> existingCalls, IEnumerable<CallFeature> rules)
        {
            var group = new CallGroup();
            group.AddRules(ps, existingCalls, rules);
            return group;
        }

        private CallGroup()
        {
            Annotations = new List<CallAnnotation>();
        }


        public void AddRules(PositionState ps, 
                            HashSet<Call> existingCalls,
                            IEnumerable<CallFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature.Call == null)
                {
                    if (feature.SatisifiesStaticConstraints(ps))
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
                    // TODO: Right now it is fine to all
                    if (ps.IsValidNextCall(feature.Call) && !existingCalls.Contains(feature.Call))
                    {
                        if (feature.SatisifiesStaticConstraints(ps))
                        {
                            if (!this.ContainsKey(feature.Call))
                            {
                                this[feature.Call] = new CallDetails(this, feature.Call);
                            }
                            this[feature.Call].Add(feature);

                            if (BestCall == null &&
                                feature is BidRule rule && 
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
}