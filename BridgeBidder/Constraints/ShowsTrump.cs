using System.Diagnostics;

namespace BridgeBidding
{
    public class ShowsTrump : StaticConstraint, IShowsAgreement, IDescribeConstraint
    {
        private Strain? _trumpStrain;
        public ShowsTrump(Strain? trumpStrain)
        {
            this._trumpStrain = trumpStrain;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            if (GetStrain(_trumpStrain, call) == null)
            {
                Debug.Fail("Strain must be specified or call must be a bid for TrumpSuit constraint");
                return false;
            }
            return true;
        }

        public void ShowAgreement(Call call, PositionState ps, PairAgreements.ShowState showAgreements)
        {
            if (GetStrain(_trumpStrain, call) is Strain strain)
            {
                showAgreements.ShowAgreedStrain(strain);
            }
        }

        string IDescribeConstraint.Describe(Call call, PositionState ps)
        {
            if (GetStrain(_trumpStrain, call) is Strain strain)
            {
                return $"agree on {strain.ToSymbol()}";
            }
            return null;
        }
    }
}
