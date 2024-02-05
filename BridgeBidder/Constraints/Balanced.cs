namespace BridgeBidding
{
	public class IsBalanced : DynamicConstraint
	{
		protected bool _desiredValue;
		public IsBalanced(bool desiredValue)
		{
			this._desiredValue = desiredValue;
		}

		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			return hs.IsBalanced == null || hs.IsBalanced == _desiredValue;
		}
	}

	public class ShowsBalanced : IsBalanced, IShowsState
	{
		public ShowsBalanced(bool desiredValue) : base(desiredValue) { }
	    void IShowsState.ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements)
		{
			showHand.ShowIsBalanced(_desiredValue);
			// TODO: I am concerned that when a hand Shows a shape of 5X AND is balanced that the specifi
			// showing of 5X will be lost.  Think this through carefully.
			if (_desiredValue == true)
			{
				foreach (var suit in Card.Suits)
				{
					showHand.Suits[suit].ShowShape(2, 5);
				}
			}
			
		}
	}

}
