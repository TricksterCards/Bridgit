﻿using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    public class KeyCards : HandConstraint, IShowsHand, IDescribeConstraint
	{
		HashSet<int> _count;
		Suit? _trumpSuit;
		bool? _haveQueen;
		public KeyCards(Suit? trumpSuit, bool? haveQueen, params int[] count)
		{
			_trumpSuit = trumpSuit;
			_haveQueen = haveQueen;
			_count = new HashSet<int>(count);
		}



		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			var keyCards = hs.CountAces;
			if (_trumpSuit != null)
			{
				// First check the status of the queen, exiting early if possible.
				if (_haveQueen != null)
				{
					var q = hs.Suits[(Suit)_trumpSuit].HaveQueen;
					if (q == null) return true; // If we don't know, then it could be true
					if (q != _haveQueen) return false;
				}
				keyCards = hs.Suits[(Suit)_trumpSuit].KeyCards;
			}
			if (keyCards == null) return true;	// If we don't know, we don't know
			return _count.Intersect(keyCards).Count() > 0;
		}

		public string Describe(Call call, PositionState ps)
		{
			var s = _count.Count == 1 && _count.Contains(1) ? "" : "s";
			if (_trumpSuit is Suit)
			{
				var str = $"{string.Join(" or ", _count)} key card{s}";
				if (_haveQueen is bool haveQueen)
				{
					str += haveQueen ? " and queen" : " no queen";
				}
				return str;
			}
			else
			{
				return $"{string.Join(" or ", _count)} Ace{s}";
			}
		}

		public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
		{
			if (_trumpSuit is Suit suit)
			{
				showHand.Suits[suit].ShowKeyCards(_count);
				if (_haveQueen is bool haveQueen)
				{
					showHand.Suits[suit].ShowHaveQueen(haveQueen);
				}
			}
			else
			{
				showHand.ShowCountAces(_count);
			}	
		}
	}


	public class Kings : HandConstraint, IShowsHand, IDescribeConstraint
	{
		HashSet<int> _count;

		public Kings(params int[] count)
		{
			_count = new HashSet<int>(count);
		}



		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			if (hs.CountKings is HashSet<int> countKings) {
				return _count.Intersect(countKings).Count() > 0;
			}
			return true;	// If we don't know then we don't know...
		}

		public string Describe(Call call, PositionState ps)
		{
			var s = _count.Count == 1 && _count.Contains(1) ? "" : "s";
			return $"{string.Join(" or ", _count)} King{s}";
		}

        public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
		{
			showHand.ShowCountKings(_count);
		}
	}

}
