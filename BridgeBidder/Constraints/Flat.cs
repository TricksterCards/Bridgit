namespace BridgeBidding
{
    public class IsFlat : HandConstraint
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

	// TODO: Perhaps just make this part of Balanced...
	public class ShowsFlat: IsFlat, IShowsHand, IDescribeConstraint
	{
		public ShowsFlat(bool desiredValue = true) : base(desiredValue) { }
		void IShowsHand.ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
		{
			showHand.ShowIsBalanced(_desiredValue);
            if (_desiredValue == true)
            {
                foreach (var suit in Card.Suits)
                {
                    showHand.Suits[suit].ShowShape(3, 4);
                }
            }
        }

		string IDescribeConstraint.Describe(Call call, PositionState ps)
		{
			return _desiredValue ? "flat" : "not flat";
		}
	}
}
