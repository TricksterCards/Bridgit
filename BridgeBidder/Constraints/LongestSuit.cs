

using System;
using System.Diagnostics;

namespace BridgeBidding
{
    public class LongestSuit : DynamicConstraint, IDescribeConstraint
    {
        protected Suit? _suit = null;



        public LongestSuit(Suit? suit) 
        {
            this._suit = suit;
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                int longestOther = 0;
                foreach (var other in Card.Suits)
                {
                    if (other != suit)
                    {
                        var otherShape = hs.Suits[other].GetShape();
                        longestOther = Math.Max(longestOther, otherShape.Max);
                    }
                }
                var shape = hs.Suits[suit].GetShape();
                return shape.Min > longestOther;
            }
            Debug.Fail("No suit specified in call or constraint declaration");
            return false;
		}

        public string Describe(Call call, PositionState ps)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                return $"longest suit is {suit.ToSymbol()}";
            }
            return null;
        }
    }

	public class ShowsLongestSuit : LongestSuit, IShowsState
	{
        public ShowsLongestSuit(Suit? suit) : base(suit) { }

	    void IShowsState.ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showArgeements)
		{
            if (GetSuit(_suit, call) is Suit suit)
            {
                int minOther = 3;   // The longest suit must be longer than 3.
                foreach (var other in Card.Suits)
                {
                    if (other != suit)
                    {
                        var otherShape = ps.PublicHandSummary.Suits[other].GetShape();
                        minOther = Math.Max(minOther, otherShape.Min);
                    }
                }
                var shape = ps.PublicHandSummary.Suits[suit].GetShape();
                showHand.Suits[suit].ShowShape(minOther + 1, shape.Max);
            }
        }

    }


}
