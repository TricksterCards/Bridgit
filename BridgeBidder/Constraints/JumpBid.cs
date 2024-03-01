using System.Linq;
using System.Diagnostics;

namespace BridgeBidding
{
    public class JumpBid : StaticConstraint
	{
		private int[] _jumpLevels;
		public JumpBid(params int[] jumpLevels)
		{
			this._jumpLevels = jumpLevels;
			Debug.Assert(jumpLevels.Length > 0, "JumpBid must have at least one level");
			Debug.Assert(jumpLevels.All(l => l >= 0 && l <= 5), "JumpBid levels must be between 0 and 5");
		}

		public override bool Conforms(Call call, PositionState ps)
		{
			if (call is Bid bid)
			{
				return this._jumpLevels.Contains(ps.BiddingState.Contract.IsJump(bid));
			}
			return false;
		}

        public override string GetLogDescription(Call call, PositionState ps)
        {
			if (_jumpLevels.First() == 0)
			{
				return "not a jump bid";
			}
			// return all of the levels that are jumped to
            return $"jump {string.Join(", ", _jumpLevels)}";
        }
    }
}
