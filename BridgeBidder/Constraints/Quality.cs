﻿using System.Diagnostics;

namespace BridgeBidding
{

    public enum SuitQuality { Poor = 0, Decent = 1, Good = 2, Excellent = 3, Solid = 4 }


	public class HasQuality : HandConstraint
	{
		protected Suit? _suit;
		protected SuitQuality _min;
		protected SuitQuality _max;

		public HasQuality(Suit? suit, SuitQuality min, SuitQuality max)
		{
			this._suit = suit;
			this._min = min;
			this._max = max;
		}


		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			if (GetSuit(_suit, call) is Suit suit)
			{
				var quality = hs.Suits[suit].GetQuality();
				return ((int)_min <= (int)quality.Max && (int)_max >= (int)quality.Min);
			}
			Debug.Fail("No suit for HasQuality constraint");
			return false;
		}
	}

	public class ShowsQuality : HasQuality, IShowsHand, IDescribeConstraint
	{
		public ShowsQuality(Suit? suit, SuitQuality min, SuitQuality max) : base(suit, min, max)
		{
		}

		public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)	
		{
			if (GetSuit(_suit, call) is Suit suit)
			{
				showHand.Suits[suit].ShowQuality(_min, _max);
			}
		}
		
		string IDescribeConstraint.Describe(Call call, PositionState ps)
		{
			if (GetSuit(_suit, call) is Suit suit)
			{
				var suitSymbol = suit.ToSymbol();
				var minStr = _min.ToString().ToLowerInvariant();
				var maxStr = _max.ToString().ToLowerInvariant();

				if (_max == SuitQuality.Solid)
					return $"{suitSymbol} {minStr}+";
					
				if (_min == _max)
					return $"{suitSymbol} {minStr}";

				return $"{suitSymbol} {minStr}–{maxStr}";
			}
			return null;
		}
	}


}
