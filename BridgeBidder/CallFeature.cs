using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	public abstract class CallFeature
	{
		public Call Call { get; }
		protected List<Constraint> _constraints = new List<Constraint>();


		protected CallFeature(Call call, params Constraint[] constraints)
		{
			this.Call = call;
            // TODO: Need to get rid of goofy side-effect
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
				this._constraints.Add(constraint);
			}
		}

		public bool SatisifiesStaticConstraints(PositionState ps)
		{
			foreach (var constraint in _constraints)
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
