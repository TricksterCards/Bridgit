﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BridgeBidding
{


    public abstract class Constraint
    {

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

        // This method is intended to be used by logging and debugging software.
        // There may be some implementations that only want descriptions for logging and debugging but don't want to 
        // show up in the UI.  These constraints should override this method instead of implementing IDescribeConstraint.
        public virtual string GetLogDescription(Call call, PositionState ps)
        {
            if (this is IDescribeConstraint describeConstraint)
            {
                var desc = describeConstraint.Describe(call, ps);
                if (!string.IsNullOrEmpty(desc)) { return desc; }
            }
            return this.GetType().Name;
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

        string LogDescription { get; }

        public SimpleStaticConstraint(Func<Call, PositionState, bool> eval = null,
                                      Func<Call, PositionState, string> getDescription = null,
                                      string description = null,
                                      string logDescription = null)
        {
            Debug.Assert(getDescription == null || description == null, "Cannot specify both getDescription and description");
            _eval = eval != null ? eval : (call, ps) => true;
            _getDescription = getDescription != null ? getDescription : (call, ps) => description;
            LogDescription = logDescription;
        }
       
        public override bool Conforms(Call call, PositionState ps)
        {
            return _eval(call, ps);
        }

        public virtual string Describe(Call call, PositionState ps)
        {
            return _getDescription(call, ps);
        }
        public override string GetLogDescription(Call call, PositionState ps)
        {
            if (LogDescription != null) { return LogDescription; }
            return base.GetLogDescription(call, ps);
        }
    }

    public abstract class HandConstraint: Constraint
    {
        public abstract bool Conforms(Call call, PositionState ps, HandSummary hs);
    }


    public interface IShowsHand 
    {
        void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand);
    }



    public interface IDescribeConstraint
    {
        string Describe(Call call, PositionState ps);
    }

    public interface IDescribeMultipleConstraints
    {
        string Describe(Call call, PositionState ps, List<Constraint> constraints);
    }

}


