using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BridgeBidding
{


    public abstract class Constraint
    {
      //  public bool StaticConstraint = false;
      //  public abstract bool Conforms(Call call, PositionState ps, HandSummary hs);


        public static Suit? GetSuit(Suit? s, Call call)
        {
            if (s != null) { return s; }
            if (call is Bid bid)
            {
                return bid.Suit;
            }
            return null;
        }

        public static Strain? GetStrain(Strain? strain, Call call)
        {
            if (strain != null) { return strain; }
            if (call is Bid bid) { return bid.Strain; }
            return null;
        }

        public string GetDescription(PositionState ps)
        {
            if (this is IDescribeConstraint describeConstraint)
            {
                return describeConstraint.Describe(null, ps);
            }
            return "*";
        }

    }

    public abstract class StaticConstraint: Constraint
    {
        public abstract bool Conforms(Call call, PositionState ps);
    }

    public class SimpleStaticConstraint: StaticConstraint, IDescribeConstraint
    {
        Func<Call, PositionState, bool> _eval;
        Func<Call, PositionState, string> _getDescription;

        public SimpleStaticConstraint(Func<Call, PositionState, bool> eval = null,
                                      Func<Call, PositionState, string> getDescription = null,
                                      string description = null)
        {
            Debug.Assert(getDescription == null || description == null, "Cannot specify both getDescription and description");
            _eval = eval != null ? eval : (call, ps) => true;
            _getDescription = getDescription != null ? getDescription : (call, ps) => description;
        }
       
        public override bool Conforms(Call call, PositionState ps)
        {
            return _eval(call, ps);
        }

        public virtual string Describe(Call call, PositionState ps)
        {
            return _getDescription(call, ps);
        }
    }

    public abstract class DynamicConstraint: Constraint
    {
        public abstract bool Conforms(Call call, PositionState ps, HandSummary hs);
    }


    public interface IShowsState 
    {
        void ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements);
    }

    
    public interface IDescribeConstraint
    {
        string Describe(Call call, PositionState ps);
    }

    public interface IDescribeMultipleConstraints
    {
        List<string> Describe(Call call, PositionState ps, List<Constraint> constraints);
    }

}


