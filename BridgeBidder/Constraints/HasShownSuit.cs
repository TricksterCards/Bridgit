﻿using System.Diagnostics;

namespace BridgeBidding
{
    public class HasShownSuit : StaticConstraint
    {
        Suit? _suit;
        bool _eitherPartner;
        public HasShownSuit(Suit? suit, bool eitherPartner)
        {
            this._suit = suit;
            this._eitherPartner = eitherPartner;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                var firstToShow = ps.PairState.FirstToShow(suit);
                if (firstToShow == null) return false;
                return _eitherPartner || firstToShow == ps;
            }
            Debug.Fail("No suit for call in HasShownSuit constraint.");
            return false;
        }

        public override string GetLogDescription(Call call, PositionState ps)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                var strain = suit.ToStrain();
                if (_eitherPartner) 
                {
                    return $"{ps.Direction.Pair()} has shown {suit.ToSymbol()}";
                }
                return $"{ps.Direction} has shown {suit.ToSymbol()}";
            }
            return null;    // This is really an invalid state...
        }
    }
}
