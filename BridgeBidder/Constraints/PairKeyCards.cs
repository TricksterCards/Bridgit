using System;
using System.Linq;

namespace BridgeBidding
{
    internal class PairKeyCards : DynamicConstraint
	{
		int[] _count;
		Suit? _trumpSuit;
		bool? _hasQueen;
		
		public PairKeyCards(Suit? trumpSuit, bool? hasQueen, params int[] count)
		{
			_trumpSuit = trumpSuit;
			_hasQueen = hasQueen;
			_count = count;
		}
		public int TotalKeyCards
		{
			get { return _trumpSuit == null ? 4 : 5; }
		}

		// TODO: Implement ShowState??? Is that necessary?  
		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			var ourKeyCards = hs.CountAces;
			var partnerKeyCards = ps.Partner.PublicHandSummary.CountAces;
			bool? partnerHasQueen = null;
			bool? weHaveQueen = null;
			if (_trumpSuit is Suit suit)
			{
				ourKeyCards = hs.Suits[suit].KeyCards;
				weHaveQueen = hs.Suits[suit].HaveQueen;
				partnerKeyCards = ps.Partner.PublicHandSummary.Suits[suit].KeyCards;
				partnerHasQueen = ps.Partner.PublicHandSummary.Suits[suit].HaveQueen;
			}
			if (ourKeyCards == null)
			{
				if (partnerKeyCards == null) return true;   // We know nothing..
				return _count.Max() >= partnerKeyCards.Min();
			}
			if (partnerKeyCards == null)
			{
				return _count.Max() >= ourKeyCards.Min();
			}
			// If someone DOES have a queen and we explictitly want none, then don't conform.
			if (_hasQueen == false && (weHaveQueen == true || partnerHasQueen == true)) return false;
			if (_hasQueen == true && (weHaveQueen == false && partnerHasQueen == false)) return false;
			foreach (var ourCount in ourKeyCards)
			{
				foreach (var partnerCount in partnerKeyCards)
				{
					if (_count.Contains(ourCount + partnerCount)) return true;
				}
			}
			return false;
		}
	}

	public class PairKings : DynamicConstraint
	{
		private int[] _count;
		public PairKings(params int[] count)
		{
			_count = count;
		}

		public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			var ourKings = hs.CountKings;
			var partnerKings = ps.Partner.PublicHandSummary.CountKings;
			if (ourKings == null)
			{
				if (partnerKings == null) return true;   // We know nothing..
				return _count.Max() >= partnerKings.Min();
			}
			if (partnerKings == null)
			{
				return _count.Max() >= ourKings.Min();
			}
			foreach (var ourCount in ourKings)
			{
				foreach (var partnerCount in partnerKings)
				{
					if (_count.Contains(ourCount + partnerCount)) return true;
				}
			}
			return false;
		}
	}
}
