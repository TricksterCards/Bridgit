using System.Diagnostics;

namespace BridgeBidding
{
    class PositionProxy : StaticConstraint, IDescribeConstraint
	{
		private RelativePosition _relativePosition;
		private StaticConstraint _constraint;

		public enum RelativePosition { Partner, LHO, RHO }
		public PositionProxy(RelativePosition relativePosition, Constraint constraint)
		{
			_relativePosition = relativePosition;
			_constraint = constraint as StaticConstraint;
			Debug.Assert(_constraint != null);
		}


		private PositionState GetPosition(PositionState positionState)
		{
			if (_relativePosition == RelativePosition.Partner) { return positionState.Partner; }
			if (_relativePosition == RelativePosition.LHO) { return positionState.LeftHandOpponent; }
			Debug.Assert(_relativePosition == RelativePosition.RHO);
			return positionState.RightHandOpponent;
		}


		public override bool Conforms(Call call, PositionState ps)
		{
			return _constraint.Conforms(call, GetPosition(ps));
		}

		public string Describe(Call call, PositionState ps)
		{
			if (_constraint is IDescribeConstraint describeConstraint)
			{
				return $"{_relativePosition} {describeConstraint.Describe(call, GetPosition(ps))}";
			}
			return null;
		}

		public override string GetLogDescription(Call call, PositionState ps)
		{
			string desc = Describe(call, ps);
			return desc == null ? $"{_relativePosition} {_constraint.GetLogDescription(call, GetPosition(ps))}" : desc;
		}	
	}
}
