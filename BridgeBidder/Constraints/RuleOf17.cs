﻿using System.Diagnostics;

namespace BridgeBidding
{
    public class RuleOf17 : DynamicConstraint, IDescribeConstraint
    {
        private Suit? _suit;

        public RuleOf17(Suit? suit)
        {
            _suit = suit;
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            // Note that we use the Max points for this rule.  This means that for unknown hands we
            // will always conform (it's possible that rule of 17 will work) but for specific hands
            // we will eleminate the bid
            if (GetSuit(_suit, call) is Suit suit)
            {
                if (hs.HighCardPoints == null) { return true; }
                (int Min, int Max) hcp = ((int, int))hs.HighCardPoints;
                var pts = hcp.Max;
                pts += hs.Suits[suit].GetShape().Max;
                return pts >= 17;
            }
            Debug.Fail("Need to specify suit for rule of 17");
            return false;
        }
		
		string IDescribeConstraint.Describe(Call call, PositionState ps)
		{
			return "rule of 17";
		}
    }
}
