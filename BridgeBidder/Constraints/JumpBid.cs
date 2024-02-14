using System.Linq;

namespace BridgeBidding
{
    public class JumpBid : StaticConstraint
	{
		private int[] _jumpLevels;
		public JumpBid(params int[] jumpLevels)
		{
			this._jumpLevels = jumpLevels;
		}

		public override bool Conforms(Call call, PositionState ps)
		{
			if (call is Bid bid)
			{
				return this._jumpLevels.Contains(ps.BiddingState.Contract.IsJump(bid));
			}
			return false;
		}
	}
}
