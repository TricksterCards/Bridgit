using System;
using System.Diagnostics;


namespace BridgeBidding
{
    // This class is used for PairHasShownPoints and PairShowsPoints.  HasShown is a static constraint, while
    // the Shows class is a dynamic constaint that also shows state.  This class implements all of the logic, while
    // the two constraint class simply implement the approriate Constraint methods and delegate to this class.
    public class PairPoints
    {
        protected bool _useStartingPoints;
        protected bool _useAgreedStrain;
        protected Suit? _suit;
        protected int _min;
        protected int _max;
        public PairPoints(Suit? suit, int min, int max)
        {
            this._useStartingPoints = false;
            this._useAgreedStrain = false;
            this._suit = suit;
            this._min = min;
            this._max = max;
            Debug.Assert(max >= min);
        }

        public PairPoints(int min, int max)
        {
            this._useStartingPoints = false;
            this._useAgreedStrain = true;
            this._suit = null;
            this._min = min;
            this._max = max;
        }

        public Suit? GetSuit(PositionState ps, Suit? suit, Call call)
        {
            if (_useAgreedStrain)
            {
                return ps.PairState.Agreements.TrumpSuit;
            }
            return Constraint.GetSuit(suit, call);
        }

        public (int Min, int Max) GetPoints(Call call, PositionState ps, HandSummary hs)
        {
            var points = hs.StartingPoints;
            if (!_useStartingPoints && GetSuit(ps, _suit, call) is Suit suit)
            {
                // We want to find the first of a pair who has shown a suit.
                // Note that this is different from the first BID of a suit since
                // a transfer shows a suit, but not the strain bid.  Therefore
                // we must search first position of a pair to show at least 4 cards
                // in a praritular suit.
                var firstToShow = ps.PairState.FirstToShow(suit);
                if (firstToShow == ps)
                {
                    points = hs.Suits[suit].LongHandPoints;
                }
                else if (firstToShow != null)
                {
                    Debug.Assert(firstToShow == ps.Partner);
                    points = hs.Suits[suit].DummyPoints;
                }
            }
            if (points == null)
            {
                points = hs.Points;
                // TODO: Carefully consider what this means!  We have to assume that the value of suit points
                // can be significantly higher than starting points.
                if (!_useStartingPoints && points != null)
                {
                    var newPoints = ((int Min, int Max))points;
                    newPoints.Max += 8;
                    points = newPoints;
                }
            }
            return (points == null) ? (0, 100) : ((int, int))points;
        }

        public string Describe(Call call, PositionState ps)
        {
            var range = Range.GetString(_min, _max, 40);
            return $"{range} pair points";
        }

        public bool DynamicallyConforms(Call call, PositionState ps, HandSummary hs)
        {
            var positionPoints = GetPoints(call, ps, hs);
            var pointsPartner = GetPoints(call, ps.Partner, ps.Partner.PublicHandSummary);
            return (positionPoints.Max + pointsPartner.Min >= _min && positionPoints.Min + pointsPartner.Min <= _max);
        }

        public bool StaticallyConforms(Call call, PositionState ps)
        {
            var positionPoints = GetPoints(call, ps, ps.PublicHandSummary);
            var pointsPartner = GetPoints(call, ps.Partner, ps.Partner.PublicHandSummary);
            var minPoints = positionPoints.Min + pointsPartner.Min;
            return (minPoints >= _min && minPoints <= _max);
        }

        private Strain ToStrain(Suit? suit)
        {
            if (suit is Suit s) return s.ToStrain();
            return Strain.NoTrump;
        }

        public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
        {
            var pointsThis = GetPoints(call, ps, ps.PublicHandSummary);
            var pointsPartner = GetPoints(call, ps.Partner, ps.Partner.PublicHandSummary);
            var suit = Constraint.GetSuit(_suit, call);
            int showMin = Math.Max(_min - pointsPartner.Min, 0);
            int showMax = Math.Max(_max - pointsPartner.Min, 0);
            PositionState firstToShow = suit == null ? null : ps.PairState.FirstToShow((Suit)suit);
            if (this._useStartingPoints || firstToShow == null)
            {
                showHand.ShowStartingPoints(showMin, showMax);
            }
            else if (firstToShow == ps)
            {
                showHand.Suits[(Suit)suit].ShowLongHandPoints(showMin, showMax);
            }
            else
            {
                Debug.Assert(firstToShow == ps.Partner);
                showHand.Suits[(Suit)suit].ShowDummyPoints(showMin, showMax);
            }
        }

        /*
        public (int Min, int Max) GetPoints(Call call, PositionState ps, HandSummary hs)
        {
			var positionPoints = GetPoints(call, ps, hs);
			var pointsPartner = GetPoints(call, ps.Partner, ps.Partner.PublicHandSummary);
            return (positionPoints.Min + pointsPartner.Min,  positionPoints.Max + pointsPartner.Max);
		}
        
        protected (int MinThis, int MaxThis, int MinPartner, int MaxPartner) GetPoints(Bid bid, PositionState ps, HandSummary hs)
        {
            // Assume we will have to use starting points.  Override if appropriate
            var thisPoints = hs.GetStartingPoints();
            var partnerPoints = ps.Partner.PublicHandSummary.GetStartingPoints();
            if (!this._useStartingPoints)
            {
                var suit = bid.SuitIfNot(_suit);
                if (ps.PairAgreements.Suits[suit].LongHand != null)
                {
                    (int Min, int Max)? suitThisPoints = null;
                    (int Min, int Max)? suitPartnerPoints = null;
                    if (ps.PairAgreements.Suits[suit].LongHand == ps)
                    {
                        suitThisPoints = hs.Suits[suit].LongHandPoints;
                        suitPartnerPoints = ps.Partner.PublicHandSummary.Suits[suit].DummyPoints;
                    }
                    else
                    {
                        suitThisPoints = hs.Suits[suit].DummyPoints;
                        suitPartnerPoints = ps.PublicHandSummary.Suits[suit].LongHandPoints;
                    }
                    if (suitThisPoints != null) { thisPoints = ((int, int))suitThisPoints; }
                    if (suitPartnerPoints != null) { partnerPoints = ((int, int))suitPartnerPoints; }
                }
            }
            return (thisPoints.Min, thisPoints.Max, partnerPoints.Min, partnerPoints.Max);
        }
        */
    }

	public class PairHasShownPoints : StaticConstraint
    {
		private PairPoints _pairPoints;
		public PairHasShownPoints(Suit? suit, int min, int max)
		{
			this._pairPoints = new PairPoints(suit, min, max);
		}

		public PairHasShownPoints(int min, int max)
		{
			this._pairPoints = new PairPoints(min, max);
		}

		public override bool Conforms(Call call, PositionState ps)
		{
			return _pairPoints.StaticallyConforms(call, ps);
		}
	}

    public class PairShowsPoints : HandConstraint, IShowsHand, IDescribeConstraint
    {
        private PairPoints _pairPoints;
        public PairShowsPoints(Suit? suit, int min, int max)
        {
            this._pairPoints = new PairPoints(suit, min, max);
        }

        public PairShowsPoints(int min, int max)
        {
            this._pairPoints = new PairPoints(min, max);
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            return _pairPoints.DynamicallyConforms(call, ps, hs);
        }
    
        string IDescribeConstraint.Describe(Call call, PositionState ps)
        {
            return _pairPoints.Describe(call, ps);
        }

        public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
        {
            _pairPoints.ShowHand(call, ps, showHand);
        }
    }
}
