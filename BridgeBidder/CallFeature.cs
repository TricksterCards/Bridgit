using System;
using System.Collections.Generic;
using System.Diagnostics;

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
			Debug.Assert(constraint != null);
			if (constraint is ConstraintGroup group)
			{
				foreach (Constraint child in group.ChildConstraints)
				{
					AddConstraint(child);
				}
			}
			else
			{
				this.Constraints.Add(constraint);
			}
		}

		public bool SatisifiesStaticConstraints(PositionState ps)
		{
			foreach (var constraint in Constraints)
			{
				if (constraint is StaticConstraint sc && !sc.Conforms(Call, ps)) return false;
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
}
