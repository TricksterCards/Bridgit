using System.Diagnostics;

namespace BridgeBidding
{
    internal class AgreedStrain : StaticConstraint
    {
        private Strain[] _strains;
        public AgreedStrain(params Strain[] strains)
        {
            this._strains = strains;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            var strains = _strains;
            if (strains.Length == 0)
            {
                if (GetStrain(null, call) is Strain strain)
                {
                    strains = new Strain[] { strain };
                }
                else
                {
                    Debug.Fail("Call has no strain.");
                    return false;
                }
            }
            // TODO: This does not seem to be right for "NT".  Perhaps it is ok...
            foreach (var s in Card.Suits)
            {
                if (ps.PairState.LastShownSuit == s) return true;
            }
            return false;
        }
    }
}
