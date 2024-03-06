using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        public bool SatisifiesHandConstraints(PositionState ps, HandSummary hs)
        {
            foreach (Constraint constraint in Constraints)
            {
                if (constraint is HandConstraint HandConstraint &&
					!HandConstraint.Conforms(Call, ps, hs)) 
				{
					return false;
				}
            }
            return true;
        }

		public List<Constraint> FailingHandConstraints(PositionState ps, HandSummary hs)
		{
			var failingConstraints = new List<Constraint>();
			foreach (var constraint in Constraints)
			{
				if (constraint is HandConstraint HandConstraint && 
					!HandConstraint.Conforms(Call, ps, hs)) 
				{
					failingConstraints.Add(constraint);
				}
			}
			return failingConstraints;
		}
	
	// TODO:  THIS IS FUNDAMENTAL TO THIS CHANGE.  DO NOT CHECK IN WITHOUT FIXING THIS!!!!!

		public PairAgreements ShowAgreements(PositionState ps)
		{
		//	bool showedSuit = false;
			var showAgreements = new PairAgreements.ShowState(ps.PairState.Agreements);
			foreach (Constraint constraint in Constraints)
			{
			if (constraint is IShowsAgreement sa)
				{
					sa.ShowAgreement(Call, ps, showAgreements);
				}
		//		showedSuit |= (constraint is ShowsSuit);
			}
		//	if (!showedSuit && Call is Bid)		// TODO: Should this be the case for Suit.Unknown too?  Think this through.  Right now I think yes.
		//	{
		//		new ShowsSuit(true).ShowAgreement(Call, ps, showAgreements);
		//	}
			return showAgreements.PairAgreements;
		}

		public HandSummary ShowHand(PositionState ps)
		{
			var showHand = new HandSummary.ShowState();
			foreach (Constraint constraint in Constraints)
			{
				if (constraint is IShowsHand showsHand)
				{
					showsHand.ShowHand(Call, ps, showHand);
				}
			}
			return showHand.HandSummary;
		}

		internal List<string> ConstraintDescriptions(PositionState ps)
		{
			HashSet<Type> didMultiDescribe = new HashSet<Type>();
			var descriptions = new List<string>();

			foreach (var constraint in Constraints.OrderBy(ConstraintSort.ForDescription))
			{
				// If a constraint implements IDescribeMultipleConstraints it should also implement IDescribeConstraint
				// so that a test tool can get the description of every constraint.
			  	if (constraint is IDescribeMultipleConstraints describeMultiple)
				{
					if (!didMultiDescribe.Contains(constraint.GetType()))
					{
						didMultiDescribe.Add(constraint.GetType());
						List<Constraint> sameConstraint = Constraints.FindAll(c => c.GetType() == constraint.GetType());
						descriptions.Add(describeMultiple.Describe(Call, ps, sameConstraint));
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
