using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace BridgeBidding
{


    public class HasLosers : DynamicConstraint
    {
        protected int _min;
        protected int _max;
        protected Suit? _suit;
        
        protected bool _handLosers;

        public HasLosers(bool handLosers, Suit? suit, int min, int max)
        {
            // TODO:  Completely broken. But OK for now.  Need to rethink initiaializer
            this._handLosers = handLosers;
            this._suit = suit;
            this._min = min;
            this._max = max;
        }

        private (int Min, int Max) GetLosers(Call call, HandSummary hs)
        {
            (int, int)? losers;
            if (_handLosers) 
            {
                losers = hs.Losers;
                if (losers == null)
                {
                    losers = (0, 12);
                }
            }
            else 
            {
                if (GetSuit(_suit, call) is Suit suit)
                {
                    losers = hs.Suits[suit].Losers;
                }
                else
                {
                    Debug.Fail("Need to specify a suit for LTC");
                    losers = null;
                }
                if (losers == null)
                {
                    losers = (0, 3);
                }
            }
            return ((int, int))losers;
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            (int Min, int Max) losers = GetLosers(call, hs);
            return (_min <= losers.Max && _max >= losers.Min);
        }
    }

    public class ShowsLosers : HasLosers, IShowsState, IDescribeConstraint
    {
        public ShowsLosers(bool handLosers, Suit? suit, int min, int max) : base(handLosers, suit, min, max)
        {
        }

        public void ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements)
        {
            if (_handLosers) 
            {
                showHand.ShowLosers(_min, _max);
            }
            else
            {
                if (GetSuit(_suit, call) is Suit suit)
                {
                    showHand.Suits[suit].ShowLosers(_min, _max);
                }
            }
        }
        
        string IDescribeConstraint.Describe(Call call, PositionState ps)
        {
            var range = Range.GetString(_min, _max, 13);
            var s = _min == 1 && _min == _max ? "" : "s";
            if (_handLosers) {
                return $"{range} loser{s} in hand";
            }
            else
            {
                if (GetSuit(_suit, call) is Suit suit) {
                    return $"{range} loser{s} in {suit}";
                }
            }
            return null;
        }
    }
}
