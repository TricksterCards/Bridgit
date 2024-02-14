using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BridgeBidding
{

	public class Deal: Dictionary<Direction, Hand>
	{
		public Game Game { get; }
		internal Deal(Game game)
		{
			this.Game = game;
			foreach (Direction direction in Enum.GetValues(typeof(Direction)))
			{
				this[direction] = null;
			}
		}

		public override string ToString()
		{
			var dealer = Game.Dealer;
			StringBuilder sb = new StringBuilder($"{dealer}:");
			var direction = dealer;
			while (true)
			{
				sb.Append(this[direction] == null ? "-" : this[direction].ToString());
				direction = BridgeBidder.LeftHandOpponent(direction);
				if (direction == dealer)
				{
					return sb.ToString();
				}
				sb.Append(" ");
			}
		}
	}
}