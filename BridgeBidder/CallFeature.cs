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
				if (constraint is StaticConstraint staticConstraint)
				{
					if (!staticConstraint.Conforms(Call, ps)) return false;
				}
			}
			return true;
		}
    }
}
