using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BridgeBidding
{
    public class BetterMinor : DynamicConstraint
	{
		Suit? _suit;
		public BetterMinor(Suit? suit)
		{
			_suit = suit;
		}
        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
		{
			if (GetSuit(_suit, call) is Suit better && (better == Suit.Clubs || better == Suit.Diamonds))
			{
                var shapeClubs = hs.Suits[Suit.Clubs].GetShape();
                var shapeDiamonds = hs.Suits[Suit.Diamonds].GetShape();
                // TODO: Perhaps in the future we can constrain on public hands but for now
                // this constraint will only return false when a private hand summary is given.
                // This means that the min and max of the shapes are equal.
                if (shapeClubs.Min != shapeClubs.Max || shapeDiamonds.Min != shapeDiamonds.Max) return true;
                if (shapeClubs.Min < shapeDiamonds.Min) return (better == Suit.Diamonds);
                if (shapeClubs.Min > shapeDiamonds.Min) return (better == Suit.Clubs);
                // They are equal.  If 4 or longer then select diamonds, otherwise clubs are the "best"
                if (shapeClubs.Min < 4) return (better == Suit.Clubs);
                return (better == Suit.Diamonds);

            }
            return false;
        }

		

    }
}

