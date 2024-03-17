using System;
using System.Diagnostics;

namespace BridgeBidding
{
    public class TakeoutSuit : HandConstraint, IShowsHand
    {
        private Suit? _suit;
  
        public TakeoutSuit(Suit? suit)
        {
            this._suit = suit;
        }



        // TODO: Move this to BasicBidding after massive merge
        public static Suit HigherRanking(Suit s1, Suit s2)
        {
            Debug.Assert(s1 != s2);
            Debug.Assert(s1 == Suit.Clubs || s1 == Suit.Diamonds || s1 == Suit.Hearts || s1 == Suit.Spades);
            Debug.Assert(s2 == Suit.Clubs || s2 == Suit.Diamonds || s2 == Suit.Hearts || s2 == Suit.Spades);
            switch (s1)
            {
                case Suit.Clubs:
                    return s2;
                case Suit.Diamonds:
                    return (s2 == Suit.Clubs) ? s1 : s2;
                case Suit.Hearts:
                    return (s2 == Suit.Spades) ? s2 : s1;
                case Suit.Spades:
                    return s1;
            }
            throw new ArgumentException();  // TODO: Is this OK?  Is it right?
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                if (ps.OppsPairState.HaveShownSuit(suit)) { return false; }
                foreach (Suit other in Enum.GetValues(typeof(Suit)))
                {
                    if (other != suit && !ps.OppsPairState.HaveShownSuit(other))
                    {
                        // TODO: This may not be ideal but we always will prefer the higher ranking
                        // suit if all other things are equal.  Perhaps if low point range we would
                        // want to prefer lower suit.  
                        var betterSuit = new IsBetterSuit(suit, other, HigherRanking(suit, other), false);
                        if (!betterSuit.Conforms(call, ps, hs)) { return false; }
                    }
                }
                return true;
            }
            Debug.Fail("No suit specified for TakeoutSuit constraint");
            return false;
        }

        // TODO: This is not exactly right.  We PROBABLY have at least 4 in the suit...

        public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
        {
            if (GetSuit(_suit, call) is Suit suit)
            {
                showHand.Suits[suit].ShowShape(4, 11);
            }
        }
    }

}
