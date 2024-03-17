using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace BridgeBidding
{
	public abstract class CallFeature
	{
		public Call Call { get; }
		public List<Constraint> Constraints = new List<Constraint>();


		protected CallFeature(Call call, params Constraint[] constraints)
		{
			this.Call = call;
			foreach (Constraint constraint in constraints)
			{
				AddConstraint(constraint);
			}
		}


		public void AddConstraint(Constraint constraint)
		{
			// IT SEEMS THAT NULL IS FINE ... FIGUIRE IT OUT... Debug.Assert(constraint != null);
			if (constraint is ConstraintGroup group)
			{
				foreach (Constraint child in group.ChildConstraints)
				{
					AddConstraint(child);
				}
			}
			else if (constraint != null)
			{
				this.Constraints.Add(constraint);
			}
		}

		public bool SatisifiesStaticConstraints(PositionState ps)
		{
			foreach (var constraint in Constraints)
			{
				if (constraint is StaticConstraint sc && !sc.Conforms(this.Call, ps)) return false;
			}
			return true;
		}

		public List<Constraint> FailingStaticConstraints(PositionState ps)
		{
			var failingConstraints = new List<Constraint>();
			foreach (var constraint in Constraints)
			{
				if (constraint is StaticConstraint sc && !sc.Conforms(Call, ps)) 
				{
					failingConstraints.Add(sc);
				}
			}
			return failingConstraints;
		}
    }

	// TODO: Move this somewhere else.  But for now, it's here.
	public class CallFeatureGroup : CallFeature
	{
		public List<CallFeature> Features = new List<CallFeature>();

		public CallFeatureGroup() : base(null, new Constraint[0])
		{
		}
	}


}
/*
	public class ShowAgreedStrain : CallAgreement
	{
		public Strain? Strain;
		public ShowAgreedStrain(Call call, Strain? strain, params StaticConstraint[] constraints) : base(call, constraints)
		{
			this.Strain = strain;
		}

		public override void ShowAgreement(Call call, PositionState ps, PairAgreements.ShowState showAgreements)
		{
			if (Strain.HasValue)
			{
				showAgreements.ShowAgreedStrain(Strain.Value);
			}
			else if (call is Bid bid)
			{
				showAgreements.ShowAgreedStrain(bid.Strain);
			}
		}
	}
	public class ShowForcing1Round : CallAgreement
	{
		public ShowForcing1Round(Call call, params StaticConstraint[] constraints) : base(call, constraints)
		{
		}

		public override void ShowAgreement(Call call, PositionState ps, PairAgreements.ShowState showAgreements)
		{
			showAgreements.ShowForcing1Round(ps);
		}
	}

	public class ShowForcingToGame : CallAgreement
	{
		public ShowForcingToGame(Call call, params StaticConstraint[] constraints) : base(call, constraints)
		{
		}

		public override void ShowAgreement(Call call, PositionState ps, PairAgreements.ShowState showAgreements)
		{
			showAgreements.ShowForcingToGame();
		}
	}
	*/
