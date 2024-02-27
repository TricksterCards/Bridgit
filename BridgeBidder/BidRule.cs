using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	// TODO: Think through "force" vs intent.  
    public enum BidForce { Unknown, Nonforcing, Invitational, Forcing1Round, ForcingToGame, Signoff }


    public class BidRule : CallFeature
	{

		public BidForce Force { get; }


		public BidRule(Call call, BidForce force, params Constraint[] constraints) :
		 	base(call, constraints)
		{
			this.Force = force;
		}

        public bool SatisifiesDynamicConstraints(PositionState ps, HandSummary hs)
        {
            foreach (Constraint constraint in Constraints)
            {
                if (constraint is DynamicConstraint dynamicConstraint &&
					!dynamicConstraint.Conforms(Call, ps, hs)) 
				{
					return false;
				}
            }
            return true;
        }

		public (HandSummary, PairAgreements) ShowState(PositionState ps)
		{
			bool showedSuit = false;
			var showHand = new HandSummary.ShowState();
			var showAgreements = new PairAgreements.ShowState();
			foreach (Constraint constraint in Constraints)
			{
				if (constraint is IShowsState showsState)
				{
					showsState.ShowState(Call, ps, showHand, showAgreements);
				}
				if (constraint is ShowsSuit) { showedSuit = true; }
			}
			if (!showedSuit && Call is Bid)		// TODO: Should this be the case for Suit.Unknown too?  Think this through.  Right now I think yes.
			{
				var showSuit = new ShowsSuit(true) as IShowsState;
				showSuit.ShowState(Call, ps, showHand, showAgreements);
			}
			return (showHand.HandSummary, showAgreements.PairAgreements);
		}

		internal List<string> ConstraintDescriptions(PositionState ps)
		{
			HashSet<Type> didMultiDescribe = new HashSet<Type>();
			var descriptions = new List<string>();
			foreach (var constraint in Constraints)
			{
				// If a constraint implements IDescribeMultipleConstraints it should also implement IDescribeConstraint
				// so that a test tool can get the description of every constraint.
			  	if (constraint is IDescribeMultipleConstraints describeMultiple)
				{
					if (!didMultiDescribe.Contains(constraint.GetType()))
					{
						didMultiDescribe.Add(constraint.GetType());
						List<Constraint> sameConstraint = Constraints.FindAll(c => c.GetType() == constraint.GetType());
						descriptions.AddRange(describeMultiple.Describe(Call, ps, sameConstraint));
					}
				}
				else if (constraint is IDescribeConstraint describe)
				{
					var d = describe.Describe(Call, ps);
					if (d != null)
						descriptions.Add(d);
				}
				
			}
			return (descriptions.Count == 0) ? null : descriptions;
		}

		public string GetDescription(PositionState ps)
		{
			var desc = ConstraintDescriptions(ps);
			if (desc == null)
				return "";
			return string.Join(", ", desc);
		}
    }
}
