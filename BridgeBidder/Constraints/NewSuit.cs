using System.Diagnostics;

namespace BridgeBidding
{
    public class NewSuit : StaticConstraint, IDescribeConstraint
    {
        Suit? _suit;

        public NewSuit(Suit? suit)
        {
            this._suit = suit;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                return ps.PairState.FirstToShow(suit) == null;
            }
            Debug.Fail("No suit for call in NewSuit constraint.");
            return false;
        }

        public string Describe(Call call, PositionState ps)
        {
            return "new suit";
        }
    }
}
