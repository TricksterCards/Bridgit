﻿namespace BridgeBidding
{
    public class IsFlat : DynamicConstraint
	{
		protected bool _desiredValue;
		public IsFlat(bool desiredValue = true)
		{
			this._desiredValue = desiredValue;
		}

		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			return hs.IsFlat == null || hs.IsFlat == _desiredValue;
		}
	}

	public class ShowsFlat: IsFlat, IShowsState, IDescribeConstraint
	{
		public ShowsFlat(bool desiredValue = true) : base(desiredValue) { }
		void IShowsState.ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements)
		{
			showHand.ShowIsBalanced(_desiredValue);
			/*
            if (_desiredValue == true)
            {
                foreach (var suit in BasicBidding.BasicSuits)
                {
                    showHand.Suits[suit].ShowShape(3, 4);
                }
            }
			*/
        }

		string IDescribeConstraint.Describe(Call call, PositionState ps)
		{
			return _desiredValue ? "flat" : "not flat";
		}
	}
}
