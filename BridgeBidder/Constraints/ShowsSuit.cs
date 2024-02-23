using System.Diagnostics;
using System.Linq;

namespace BridgeBidding
{
    public class ShowsSuit : DynamicConstraint, IShowsState
    {
        private Suit[] _suits;
        private bool _showBidSuit;

        // NOTE: It is OK for both showBidSuit to be false and suits to be null.  This indicates
        // that the rule does not show any suit.
        public ShowsSuit(bool showBidSuit, params Suit[] suits)
        {
            this._showBidSuit = showBidSuit;
            this._suits = suits;
        }
        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            return true;
        }

        void IShowsState.ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements)
        {
            if (_showBidSuit &&
                GetSuit(null, call) is Suit suit)
            {
                showAgreements.Strains[suit.ToStrain()].ShowLongHand(ps);
            }
            if (_suits != null)
            {
                foreach (var s in _suits)
                {
                    showAgreements.Strains[s.ToStrain()].ShowLongHand(ps);
                }
            }
        }
    }
}
