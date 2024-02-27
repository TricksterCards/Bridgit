using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{


    public class HasPoints : DynamicConstraint
    {
        protected int _min;
        protected int _max;
        protected Suit? _trumpSuit;
        protected PointType _pointType;

        // TODO: Add concept of "Suit" points, and perhaps idea of "best points" if the pair 
        // have a known fit.  Not exactly sure.  Maybe just a pair points problem.
        public enum PointType { HighCard, Starting, Dummy }


        public HasPoints(Suit? trumpSuit, int min, int max, PointType pointType)
        {
            // TODO:  Completely broken. But OK for now.  Need to rethink initiaializer
            this._pointType = pointType;
            this._trumpSuit = trumpSuit;
            this._min = min;
            this._max = max;
        }

        // Returns the points for the hand, adjusted for dummy points if appropriate.
        protected (int, int) GetPoints(Call call, PositionState ps, HandSummary hs)
        {
            (int, int)? points = null;
            switch (_pointType)
            {
                case PointType.HighCard:
                    points = hs.HighCardPoints;
                    break;
                case PointType.Starting:
                    points = hs.StartingPoints;
                    break;
                case PointType.Dummy:
                    
                    if (GetSuit(_trumpSuit, call) is Suit suit)
                    {
                        points = hs.Suits[suit].DummyPoints;

                    }
                    break;
            }
            if (points == null)
            {
                points = hs.Points;
            }
            return points == null ? (0, 100) : ((int, int))points;
        }


        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            (int Min, int Max) points = GetPoints(call, ps, hs);
            return (_min <= points.Max && _max >= points.Min);
        }


    }

    class ShowsPoints : HasPoints, IShowsState, IDescribeConstraint, IDescribeMultipleConstraints
    {
        public ShowsPoints(Suit? trumpSuit, int min, int max, PointType pointType) : base(trumpSuit, min, max, pointType) { }


        void IShowsState.ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements)
        {
            switch (_pointType)
            {
                case PointType.HighCard:
                    showHand.ShowHighCardPoints(_min, _max);
                    break;
                case PointType.Starting:
                    showHand.ShowStartingPoints(_min, _max);
                    break;
                case PointType.Dummy:
                    if (GetSuit(_trumpSuit, call) is Suit suit)
                    {
            
                        showHand.Suits[suit].ShowDummyPoints(_min, _max);
                    }
                    else
                    {
                        Debug.Fail("Need to specify a suit.");
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }
    
        // IDescribeMultipleConstraints
        public List<string> Describe(Call call, PositionState ps, List<Constraint> constraints)
        {
            Debug.Assert(constraints.Contains(this));

            // This code favors Dummy points first, then HCP, then starting points.  It only ever returns
            // a single string.
            var descriptions = new List<string>();
            ShowsPoints best = this;
            if (constraints.Count > 1)
            {
                foreach (var constraint in constraints)
                {
                    if (constraint is ShowsPoints sp)
                    {
                        if (sp._pointType == PointType.Dummy ||
                            (sp._pointType == PointType.HighCard && best._pointType != PointType.Starting))
                        {
                            best = sp;
                        }
                    }
                    else
                    {
                        Debug.Fail("Internal error.  Should only be called with constraints of type ShowsPoints.");
                    }
                }
            }
            descriptions.Add(best.Describe(call, ps));     
            return descriptions;
        }

        public string Describe(Call call, PositionState ps)
        {
            var range = Range.GetString(_min, _max, 40);

            switch (_pointType)
            {
                case PointType.HighCard:
                    return $"{range} HCP";

                case PointType.Starting:
                    return $"{range} points";

                case PointType.Dummy:
                    if (GetSuit(_trumpSuit, call) is Suit suit)
                    {
                        return $"{range} dummy points";
                    }
                    Debug.Fail("Need to specify a suit.");
                    return null;
                
                default:
                    Debug.Fail("Internal error.  ");
                    return null;
            }

        }
    }

/*  --- THIS IS THE CODE SNIPPET FOR "SUIT" POINTS
                        if (ps.PairState.Agreements.Strains[Call.SuitToStrain(suit)].LongHand == ps)
                        {
                            points = hs.Suits[suit].LongHandPoints;
                        }
                        else if (ps.PairState.Agreements.Strains[Call.SuitToStrain(suit)].Dummy == ps)
                        {
                            points = hs.Suits[suit].DummyPoints;
                        }

*/


}
