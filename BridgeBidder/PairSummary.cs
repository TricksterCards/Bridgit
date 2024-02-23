﻿using System;
using System.Collections.Generic;



namespace BridgeBidding
{
    public class PairSummary
    {


        public class SuitSummary
        {
            internal (int Min, int Max) _quality;

            public (int Min, int Max) Shape { get; set; }
            public (int Min, int Max) Points { get; set; }

            public (SuitQuality Min, SuitQuality Max) Quality
            {
                get { return ((SuitQuality)_quality.Min, (SuitQuality)_quality.Max); }
                set { _quality.Min = (int)value.Min; _quality.Max = (int)value.Max; }
            }

     //       public (int A, int B)? Keycards
      //      {
       //         get; set;
       //     }

            public bool? HaveQueen { get; set; }

            public bool? Stopped { get; set; }



            public SuitSummary(HandSummary.SuitSummary ss1, HandSummary.SuitSummary ss2)
            {
                this.Shape = AddRange(ss1.GetShape(), ss2.GetShape(), 13);
                // TODO: Think about this - intersect range?  Not sure...
              // TODO: FIX THIS.  TOTALLY BUSTED  this._quality = IntersectRange(ss1._quality, ss2._quality);
                this.Stopped = null;
                if (ss1.Stopped == true || ss2.Stopped == true)
                {
                    this.Stopped = true;
                }
                else
                {
                    if (ss1.Stopped == false || ss2.Stopped == false)
                    {
                        this.Stopped = false;
                    }
                }
              //  this.Quality = (SuitQuality.Poor, SuitQuality.Solid);
              //  this.Keycards = null;
            }
            // TODO: There are other properties like "Stopped", "Has Ace", that can go here...
        }

        

        public static (int Min, int Max) AddRange((int Min, int Max)? r1, (int Min, int Max)? r2, int max)
        {
            if (r1 == null)
            {
                if (r2 == null) return (0, max);
                return ((int, int))r2;
            }
            if (r2 == null)
            {
                return ((int, int))r1;
            }
            (int Min, int Max) range1 = ((int Min, int Max))r1;
            (int Min, int Max) range2 = ((int Min, int Max))r2;
            return (Math.Min(max, range1.Min + range2.Min), Math.Min(max, range1.Max + range2.Max));
        }

        private static (int Min, int Max) IntersectRange((int Min, int Max) r1, (int Min, int Max) r2)
        {
            return (Math.Max(r1.Min, r2.Min), Math.Min(r1.Max, r2.Max));
        }

        public (int Min, int Max) Points;
        // TODO: What about these points?  Are they really necessay
       // public (int Min, int Max) HighCardPoints;
        //public (int Min, int Max) StartingPoints;
        public Dictionary<Suit, SuitSummary> Suits;
        public HashSet<Suit> ShownSuits = new HashSet<Suit>();
        public HashSet<Strain> ShownStrains = new HashSet<Strain>();

        public PairSummary(HandSummary hs1, HandSummary hs2, PairAgreements pa)
        {
            this.Points = AddRange(hs1.Points, hs2.Points, 100);
          //  this.HighCardPoints = AddRange(hs1.GetHighCardPoints(), hs2.GetHighCardPoints(), 40);
           // this.StartingPoints = AddRange(hs1.GetStartingPoints(), hs2.GetStartingPoints(), 100);
     //       this.CountAces = other.CountAces;
      //      this.CountKings = other.CountKings;
            this.Suits = new Dictionary<Suit, SuitSummary>();
            foreach (Suit suit in Card.Suits)
            {
                Suits[suit] = new SuitSummary(hs1.Suits[suit], hs2.Suits[suit]);
                if (pa.Strains[suit.ToStrain()].LongHand != null)
                { 
                    ShownSuits.Add(suit);
                    ShownStrains.Add(suit.ToStrain());
                }
            }
            // TODO: Need to show NT if that has been bid...  ShownStrains.Add(...)
        }

        public PairSummary(PositionState ps) : this(ps.PublicHandSummary, ps.Partner.PublicHandSummary, ps.PairState.Agreements) { }

        public static PairSummary Opponents(PositionState ps)
        {
            return new PairSummary(ps.LeftHandOpponent);
        }
    }
}
