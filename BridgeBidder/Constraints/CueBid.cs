﻿using System.Diagnostics;

namespace BridgeBidding
{
    public class CueBid : StaticConstraint
    {
        private Suit? _suit;
        private bool _desiredValue;
        public CueBid(Suit? suit, bool desiredValue)
        {
            this._suit = suit;
            this._desiredValue = desiredValue;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            var suit = _suit;
            if (suit == null && call is Bid bid)
            {
                suit = bid.Suit;
            }
            if (suit == null)
            {
                Debug.Fail("No suit specified for cuebid");
                return false;
            }

            var pairSummary = PairSummary.Opponents(ps);
            if (pairSummary.ShownSuits.Contains((Suit)suit))
            {
                // It is a CueBid.  Return TRUE IFF we want a cuebid else false (we don't conform).
                return _desiredValue;
            }
            return !_desiredValue;
        }
    }
}