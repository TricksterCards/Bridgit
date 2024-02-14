﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	// TODO: Think through "force" vs intent.  
    public enum BidForce { Unknown, Nonforcing, Invitational, Forcing1Round, ForcingToGame, Signoff }


    public class BidRule : CallFeature
	{

		public BidForce Force { get; }


		public BidRule(Call call, BidForce force, params Constraint[] constraints) :
		 	base(call, constraints)
		{
			this.Force = force;
		}

        public bool SatisifiesDynamicConstraints(PositionState ps, HandSummary hs)
        {
            foreach (Constraint constraint in _constraints)
            {
                if (constraint is DynamicConstraint dynamicConstraint &&
					!dynamicConstraint.Conforms(Call, ps, hs)) 
				{
					return false;
				}
            }
            return true;
        }

		public (HandSummary, PairAgreements) ShowState(PositionState ps)
		{
			bool showedSuit = false;
			var showHand = new HandSummary.ShowState();
			var showAgreements = new PairAgreements.ShowState();
			foreach (Constraint constraint in _constraints)
			{
				if (constraint is IShowsState showsState)
				{
					showsState.ShowState(Call, ps, showHand, showAgreements);
				}
				if (constraint is ShowsSuit) { showedSuit = true; }
			}
			if (!showedSuit && Call is Bid)		// TODO: Should this be the case for Suit.Unknown too?  Think this through.  Right now I think yes.
			{
				var showSuit = new ShowsSuit(true) as IShowsState;
				showSuit.ShowState(Call, ps, showHand, showAgreements);
			}
			return (showHand.HandSummary, showAgreements.PairAgreements);
		}

		internal List<string> ConstraintDescriptions(PositionState ps)
		{
			var descriptions = new List<string>();
			foreach (var constraint in _constraints)
			{
				if (constraint is IDescribeConstraint describe)
				{
					var d = describe.Describe(Call, ps);
					if (d != null)
						descriptions.Add(d);
				}
			}
			return (descriptions.Count == 0) ? null : descriptions;
		}
	}
}
