using System.Diagnostics;

namespace BridgeBidding
{
    // This is a special, magical class that never actually gets called.  When a rule is added the
    // ChildConstraints are added, and this constraint is essentially thrown away and never called.
    // This happens when the rule is being constructed, not when constraints are being evaluated
    // so this can not be used in conjunction with modifiers like PartnerProxy or any other contraint
    // that takes a child constraint.

    // IF and ONLY IF all child constraints are static then this constraint can
    public class ConstraintGroup: StaticConstraint
	{
		public Constraint[] ChildConstraints { get; }
		public ConstraintGroup(params Constraint[] childConstraints)
		{
			this.ChildConstraints = childConstraints;
		}

		// When this class is added directly to a rule then the child constraints are copied to the rule and
		// this method is never called.  However, PositionProxy will call this method if it is ever passed
		// a ConstraintGroup.  This will be fine since it only allows for static constraints and all of the 
		// constraints for a group must be static for the group to be considered static.
        public override bool Conforms(Call call, PositionState ps)
        {
			foreach (var constraint in ChildConstraints)
			{
				if (constraint is StaticConstraint staticConstraint)
				{
					if (!staticConstraint.Conforms(call, ps)) return false;
				}
				else
				{
					Debug.Fail("Dynamic constraints are not allowed for constraint groups that are not part of a rule");
					return false;
				}
			}
			return true;
        }
    }

}
