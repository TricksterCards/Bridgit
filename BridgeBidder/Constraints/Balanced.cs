namespace BridgeBidding
{
	public class ShowsBalanced : HandConstraint, IShowsHand, IDescribeConstraint
	{
		protected bool _desiredValue;
		public ShowsBalanced(bool desiredValue)
		{
			this._desiredValue = desiredValue;
		}

		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			return hs.IsBalanced == null || hs.IsBalanced == _desiredValue;
		}

	    public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
		{
			showHand.ShowIsBalanced(_desiredValue);
			if (_desiredValue == true)
			{
				foreach (var suit in Card.Suits)
				{
					showHand.Suits[suit].ShowShape(2, 5);
				}
			}
			
		}

		string IDescribeConstraint.Describe(Call call, PositionState ps)
		{
			return _desiredValue ? "balanced" : "not balanced";
		}
	}

}
