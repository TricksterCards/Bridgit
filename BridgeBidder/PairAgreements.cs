using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace BridgeBidding
{

    public class PairAgreements: State, IEquatable<PairAgreements>
    {

        public class ShowState
        {
            public PairAgreements PairAgreements { get; protected set; }
       //     public Dictionary<Strain, SuitAgreements.ShowState> Strains { get; protected set; }

            public ShowState(PairAgreements startState = null)
            {
                PairAgreements = new PairAgreements(startState);
           //     this.Strains = new Dictionary<Strain, SuitAgreements.ShowState>();
           //     foreach (Strain strain in Enum.GetValues(typeof(Strain)))
           //     {
           //         this.Strains[strain] = new SuitAgreements.ShowState(PairAgreements.Strains[strain]);
           //     }
            }

            // TODO: This name not the best...
            public void ShowAgreedStrain(Strain trumpStrain)
            {
                PairAgreements.AgreedStrain = trumpStrain;  
            }

            public void ShowForcing1Round(PositionState ps)
            {
                PairAgreements._forcedPosition = ps.Partner;
                PairAgreements._forcedThroughRound = ps.Partner.BidRound + 1;
            }

            public void ShowForcingToGame()
            {
                PairAgreements.ForcingToGame = true;
            }

       //     public void Combine(PairAgreements other, CombineRule combineRule)
       //     {
        //        PairAgreements.Combine(other, combineRule);
        //    }
            // TODO: Need to actually do something here.....
        }
/*
        // TODO: Add conventions here...
        // Anything else about global agreements that are not specific to the hand.
        public class SuitAgreements: IEquatable<SuitAgreements>
        {
            private PairAgreements _pairAgreements;
            private Strain _strain;
            public class ShowState
            {
                public SuitAgreements SuitAgreements { get; protected set; }

                public ShowState(SuitAgreements suitAgreements)
                {
                    this.SuitAgreements = suitAgreements;          
                }

                // TODO.  What's up here?  Does there need to be a way to force a switch of long hand?
                public void ShowLongHand(PositionState longHand)
                {
                    if (SuitAgreements.LongHand == null)
                    {
                        SuitAgreements.LongHand = longHand;
                    }
                    // TODO: This is ugly too.   Review Two suited bids will screw up with this logic.
                    SuitAgreements._pairAgreements.LastShownStrain = SuitAgreements._strain;
                }


            }
            public PositionState LongHand { get; protected set; }
            public PositionState Dummy => LongHand?.Partner;
            public bool Shown => LongHand != null; 

            public SuitAgreements(PairAgreements pairAgreements, Strain strain, SuitAgreements other)
            {
                this._pairAgreements = pairAgreements;
                this._strain = strain;
                this.LongHand = other == null ? null : other.LongHand;
            }

            public bool Equals(SuitAgreements other)
            {
                return (this.LongHand == other.LongHand);
            }
*/
/*
            public void Combine(SuitAgreements other, CombineRule combineRule)
            {
                if (combineRule == CombineRule.CommonOnly)
                {
                    if (this.LongHand == null || other.LongHand == null)
                    {
                        this.LongHand = null;
                    }
                   
                }
                else if (this.LongHand == null)
                {
                    // Is this right?  If other.LongHand exists it will over
                    this.LongHand = other.LongHand;
                }
            }
*/
        
        public Strain? AgreedStrain { get; private set; } = null;

        // TODO: This is ugly an not combined properly.  Review all uses of this
      //  public Strain? LastShownStrain { get; private set; } = null;

        public bool ForcingToGame { get; private set; } = false;

        private PositionState _forcedPosition = null;
        private int _forcedThroughRound = 0;

        public bool IsForcedToBid(PositionState ps) => (ps == _forcedPosition && ps.BidRound <= _forcedThroughRound);

        public Suit? TrumpSuit => AgreedStrain?.ToSuit();
 

       // public Dictionary<Strain, SuitAgreements> Strains { get; }


        public PairAgreements(PairAgreements other = null)
        {
            if (other != null)
            {
                this.AgreedStrain = other.AgreedStrain;
         //       this.LastShownStrain = other.LastShownStrain;
                this.ForcingToGame = other.ForcingToGame;
                this._forcedPosition = other._forcedPosition;
                this._forcedThroughRound = other._forcedThroughRound;
            }
        }
         //   this.AgreedStrain = other == null ? null : other.AgreedStrain;
        //    this.Strains = new Dictionary<Strain, SuitAgreements>();
        //    foreach (Strain strain in Enum.GetValues(typeof(Strain)))
        //    {
        //        this.Strains[strain] = new SuitAgreements(this, strain, other == null ? null : other.Strains[strain]);
//            }
  //      }
/*
        protected void Combine(PairAgreements other, CombineRule cr)
        {
            // TODO: THIS IS BROKEN.  FOR NOW JUST USE THE LAST ONE SHOWN-
            if (cr == CombineRule.Show || cr == CombineRule.Merge)
            {
                this.LastShownStrain = other.LastShownStrain;
                if (other.ForcingToGame != null)
                {
                    Debug.Assert(this.ForcingToGame == null || this.ForcingToGame == other.ForcingToGame, "ForcingToGame should not be different");
                    this.ForcingToGame = other.ForcingToGame;
                }
                // TODO: Now this is more interesting.  It needs to be reset each bid round...
                this.Forcing1Round = other.Forcing1Round;
            }

            // TODO: Need to actually do something here. 
            // For now this works...
            if (this.AgreedStrain == null && cr != CombineRule.CommonOnly)
            {
                this.AgreedStrain = other.AgreedStrain;
            }
            foreach (Strain strain in Enum.GetValues(typeof(Strain)))
            {
                Strains[strain].Combine(other.Strains[strain], cr);
            }
            // TODO: What to do if trump overridden?  Seems possible, but we really need the idea of "LAST ONE DECIDED"

        }
        */
   
        public bool Equals(PairAgreements other)
        {
            if (this.AgreedStrain != other.AgreedStrain ||
                this._forcedPosition != other._forcedPosition ||
                this._forcedThroughRound != other._forcedThroughRound ||
                this.ForcingToGame != other.ForcingToGame)
             //   this.LastShownStrain != other.LastShownStrain)
            {
                return false;
            }
         //   foreach (Strain strain in Enum.GetValues(typeof(Strain)))
          //  {
          //      if (!this.Strains[strain].Equals(other.Strains[strain])) return false;
          //  }
            return true;
        }
    }



 



}
