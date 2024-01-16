using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
    public enum BidForce { Nonforcing, Invitational, Forcing1Round, ForcingToGame, Signoff }


    public class BidRule
	{

        public Call Call { get; }

		public BidForce Force { get; }


		private List<Constraint> _constraints = new List<Constraint>();
		public BidRule(Call call, BidForce force, params Constraint[] constraints)
		{
			this.Call = call;
			this.Force = force;
			foreach (Constraint constraint in constraints)
			{
				AddConstraint(constraint);
			}
		}


		public void AddConstraint(Constraint constraint)
		{
			if (constraint is ConstraintGroup group)
			{
				foreach (Constraint child in group.ChildConstraints)
				{
					AddConstraint(child);
				}
			}
			else
			{
				this._constraints.Add(constraint);
			}
		}

		public bool SatisifiesStaticConstraints(PositionState ps)
		{
			foreach (Constraint constraint in _constraints)
			{
				if (constraint is StaticConstraint staticConstraint &&
					!staticConstraint.Conforms(Call, ps))
				{
					return false;
				}
			}
			return true;


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

		/*
        public bool Conforms(bool onceAndDoneOnly, PositionState ps, HandSummary hs)
		{
			foreach (Constraint constraint in _constraints)
			{
				if (onceAndDoneOnly)
				{
					Debug.Assert(hs == null);	// Once and done rules can not rely on hand summary
					if (constraint.OnceAndDone && !constraint.Conforms(Bid, ps, hs)) { return false; }
				}
				else
				{ 
					if (!constraint.OnceAndDone && !constraint.Conforms(Bid, ps, hs)) { return false; }
				}
			}
			return true;
		}
		*/

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
	}


	public class PartnerBidRule : BidRule
	{
		public BidChoicesFactory PartnerBids{ get; private set; }

        public PartnerBidRule(Call call, BidChoicesFactory partnerBids, params Constraint[] constraints) :
			base(call, BidForce.Nonforcing, constraints)
        {
			Debug.Assert(partnerBids != null);
            this.PartnerBids = partnerBids;
        }
    }

}
