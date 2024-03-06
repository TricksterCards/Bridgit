using System.Diagnostics;

namespace BridgeBidding
{
    public class IsCueBid : StaticConstraint
    {
        private Suit? _suit;

        public IsCueBid(Suit? suit)
        {
            this._suit = suit;
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
            return pairSummary.ShownSuits.Contains((Suit)suit);
        }
    }
}
