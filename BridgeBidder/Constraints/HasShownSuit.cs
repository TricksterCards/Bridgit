using System.Diagnostics;

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
                var strain = suit.ToStrain();
                if (_eitherPartner) {
                    return ps.PairState.Agreements.Strains[strain].Shown;
                }
                return ps.PairState.Agreements.Strains[strain].LongHand == ps;
            }
            Debug.Fail("No suit for call in HasShownSuit constraint.");
            return false;
        }
    }
}
