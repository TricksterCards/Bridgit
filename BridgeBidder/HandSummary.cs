using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace BridgeBidding
{
    public class HandSummary: State, IEquatable<HandSummary>
	{
		public class ShowState
		{
			public HandSummary HandSummary { get; private set; }
			public Dictionary<Suit, SuitSummary.ShowState> Suits { get; protected set; }

			public ShowState(HandSummary startState = null)
			{
			
				this.HandSummary = (startState == null) ? new HandSummary() : new HandSummary(startState);
				this.Suits = new Dictionary<Suit, SuitSummary.ShowState>();
				foreach (Suit suit in Enum.GetValues(typeof(Suit)))
				{
					this.Suits[suit] = new SuitSummary.ShowState(HandSummary, HandSummary.Suits[suit]);
				}
			}

	
			public void ShowStartingPoints(int min, int max)
			{
				HandSummary.StartingPoints = CombineRange(HandSummary.StartingPoints, (min, max), CombineRule.Show);
				HandSummary.ShowPoints(min, max);
			}
			public void ShowHighCardPoints(int min, int max)
			{
				HandSummary.HighCardPoints = CombineRange(HandSummary.HighCardPoints, (min, max), CombineRule.Show);
				HandSummary.ShowPoints(min, max);
			}
			public void ShowNoTrumpLongHandPoints(int min, int max)
			{
				HandSummary.NoTrumpLongHandPoints = CombineRange(HandSummary.NoTrumpLongHandPoints, (min, max), CombineRule.Show);
			}

            public void ShowNoTrumpDummyPoints(int min, int max)
            {
                HandSummary.NoTrumpDummyPoints = CombineRange(HandSummary.NoTrumpDummyPoints, (min, max), CombineRule.Show);
            }

			public void ShowLosers(int min, int max)
			{
				HandSummary.Losers = CombineRange(HandSummary.Losers, (min, max), CombineRule.Show);
			}

            public void ShowIsBalanced(bool isBalanced)
			{
				// TODO: This needs to union?  What if one is true and one is false???
				HandSummary.IsBalanced = CombineBool(HandSummary.IsBalanced, isBalanced, CombineRule.Show);
			}

			public void ShowIsFlat(bool isFlat)
			{
				HandSummary.IsFlat = CombineBool(HandSummary.IsFlat, isFlat, CombineRule.Show);
			}



			public void ShowCountAces(HashSet<int> countAces)
			{
				HandSummary.CountAces = CombineIntSet(HandSummary.CountAces, countAces, CombineRule.Show);
			}


			public void ShowCountKings(HashSet<int> countKings)
			{
				HandSummary.CountKings = CombineIntSet(HandSummary.CountKings, countKings, CombineRule.Show);
			}

			public void Combine(HandSummary other, CombineRule combineRule)
			{
                HandSummary.Combine(other, combineRule);
            }

        }
		

        public class SuitSummary: IEquatable<SuitSummary>
        {
            public class ShowState
			{ 

				private HandSummary _handSummary;
				private SuitSummary _suitSummary;

				internal ShowState(HandSummary handSummary, SuitSummary suitSummary)
                {
					_handSummary = handSummary;
                    _suitSummary = suitSummary;
                }

                public void ShowShape(int min, int max)
                {
                    _suitSummary.Shape = CombineRange(_suitSummary.Shape, (min, max), CombineRule.Show);
                }

                public void ShowDummyPoints(int min, int max)
                {
					_suitSummary.DummyPoints = CombineRange(_suitSummary.DummyPoints, (min, max), CombineRule.Show);
					_handSummary.ShowPoints(min, max);
                }

                public void ShowLongHandPoints(int min, int max)
                {
                    _suitSummary.LongHandPoints = CombineRange(_suitSummary.LongHandPoints, (min, max), CombineRule.Show);
					_handSummary.ShowPoints(min, max);
                }

                public void ShowQuality(SuitQuality min, SuitQuality max)
                {
                    _suitSummary._quality = CombineRange(_suitSummary._quality, ((int)min, (int)max), CombineRule.Show);
                }

				public void ShowLosers(int min, int max)
				{
					_suitSummary.Losers = CombineRange(_suitSummary.Losers, (min, max), CombineRule.Show);
				}
			
				public void ShowKeyCards(HashSet<int> keyCards)
				{
					_suitSummary.KeyCards = CombineIntSet(_suitSummary.KeyCards, keyCards, CombineRule.Show);
				}
				
				public void ShowHaveQueen(bool haveQueen)
				{
					_suitSummary.HaveQueen = CombineBool(_suitSummary.HaveQueen, haveQueen, CombineRule.Show);
				}
				public void ShowStopped(bool stopped)
				{
					_suitSummary.Stopped = CombineBool(_suitSummary.Stopped, stopped, CombineRule.Show);
				}

				public void ShowRuleOf9Points(int points)
				{
					_suitSummary.RuleOf9Points = CombineInt(_suitSummary.RuleOf9Points, points, CombineRule.Show);
				}
            }

            internal (int Min, int Max)? _quality;

            public (int Min, int Max)? Shape { get; protected set; }
            public (int Min, int Max)? DummyPoints { get; protected set; }

            public (int Min, int Max)? LongHandPoints { get; protected set; }

			public int? RuleOf9Points { get; protected set; }

			public (int Min, int Max)? Losers { get; protected set; }

			public (int Min, int Max) GetShape()
			{
				if (Shape == null) return (0, 13);
				return ((int Min, int Max))Shape;
			}

			internal void TrimShape(int claimed)
			{
				var shape = GetShape();
				if (shape.Max + claimed - shape.Min > 13)
				{
					var newMax = 13 - claimed + shape.Min;
					Shape = (shape.Min, newMax);
				}
			}
/*
			public (int Min, int Max) GetDummyPoints()
			{
				if (DummyPoints == null) return (0, 100);
				return ((int Min, int Max))DummyPoints;
			}


			public (int Min, int Max) GetLongHandPoints()
			{
				if (LongHandPoints == null) return (0, 100);
				return ((int, int))LongHandPoints;
			}
*/
			public (SuitQuality Min, SuitQuality Max) GetQuality()
			{
				if (_quality == null) { return (SuitQuality.Poor, SuitQuality.Solid); }
				return ((SuitQuality, SuitQuality))Quality;
			}


            public (SuitQuality Min, SuitQuality Max)? Quality
            {
                get {
					if (_quality == null) { return null; }
					var q = ((int Min, int Max))_quality; 
					return ((SuitQuality)q.Min, (SuitQuality)q.Max);
				}
            }

            public HashSet<int> KeyCards
            {
                get; protected set;
            }

            public bool? HaveQueen { get; protected set; }

			public bool? Stopped { get; protected set; }

            public SuitSummary()
            {
				this.Shape = null;
				this.DummyPoints = null;
                this.LongHandPoints = null;
                this._quality = null;
				this.Losers = null;
                this.KeyCards = null;
				this.HaveQueen = null;
				this.Stopped = null;
				this.RuleOf9Points = null;
            }
            // TODO: There are other properties like "Stopped", "Has Ace", that can go here...

            public SuitSummary(SuitSummary other)
            {
                this.Shape = other.Shape;
                this.DummyPoints = other.DummyPoints;
                this.LongHandPoints = other.LongHandPoints;
                this._quality = other._quality;
				this.Losers = other.Losers;
				this.KeyCards = other.KeyCards;
				this.HaveQueen= other.HaveQueen;
				this.Stopped = other.Stopped;
				this.RuleOf9Points= other.RuleOf9Points;
            }
		
            internal void Combine(SuitSummary other, CombineRule cr)
            {
                this.Shape = CombineRange(this.Shape, other.Shape, cr);
                this.DummyPoints = CombineRange(this.DummyPoints, other.DummyPoints, cr);
                this.LongHandPoints = CombineRange(this.LongHandPoints, other.LongHandPoints, cr);
                this._quality = CombineRange(this._quality, other._quality, cr);
				this.Losers = CombineRange(this.Losers, other.Losers, cr);
                this.HaveQueen = CombineBool(this.HaveQueen, other.HaveQueen, cr);
				this.Stopped = CombineBool(this.Stopped, other.Stopped, cr);
				this.KeyCards = CombineIntSet(this.KeyCards, other.KeyCards, cr);
				this.RuleOf9Points = CombineInt(this.RuleOf9Points, other.RuleOf9Points, cr);
            }


            public bool Equals(SuitSummary other)
            {
                return (this.Shape == other.Shape &&
					    this.DummyPoints == other.DummyPoints &&
						this.LongHandPoints == other.LongHandPoints &&
						this._quality == other._quality &&
						this.Losers == other.Losers &&
						this.HaveQueen == other.HaveQueen &&
						this.Stopped == other.Stopped &&
						this.KeyCards == other.KeyCards &&
						this.RuleOf9Points == other.RuleOf9Points);
            }
        }

		public (int Min, int Max)? HighCardPoints { get; protected set; }
		public (int Min, int Max)? StartingPoints { get; protected set; }

		public (int Min, int Max)? Points { get; protected set; }

		public (int Min, int Max)? NoTrumpLongHandPoints { get; protected set; }
		public (int Min, int Max)? NoTrumpDummyPoints { get; protected set; }

		public (int Min, int Max)? Losers { get; protected set; }

	//	public (int Min, int Max) GetPoints()
	//	{
//			if (Points == null) { return (0, 100); }
	//		return ((int, int))Points;
	//	}

//		public (int Min, int Max) GetStartingPoints()
//        {
//            if (StartingPoints == null) { return (0, 100); }
//            return ((int, int))StartingPoints;
 //       }

        public bool? IsBalanced { get; protected set; }

		public bool? IsFlat { get; protected set; }

		// TODO: Perhaps things like this:
		public HashSet<int> CountAces { get; protected set; }
			
		public HashSet<int> CountKings { get; protected set; }

		public Dictionary<Suit, SuitSummary> Suits { get; protected set; }

		public HandSummary()
		{
			this.Points = null;
			this.HighCardPoints = null; 
			this.StartingPoints = null;
			this.NoTrumpLongHandPoints = null;
			this.NoTrumpDummyPoints = null;
			this.Losers = null;
			this.IsBalanced = null;
			this.IsFlat = null;
			this.CountAces = null;
			this.CountKings = null;
			this.Suits = new Dictionary<Suit, SuitSummary>();
			foreach (Suit suit in Enum.GetValues(typeof(Suit)))
			{
				Suits[suit] = new SuitSummary();
			}
		}

		public HandSummary(HandSummary other)
		{
			this.Points = other.Points;
			this.HighCardPoints = other.HighCardPoints;
			this.StartingPoints = other.StartingPoints;
			this.NoTrumpLongHandPoints = other.NoTrumpLongHandPoints;
			this.NoTrumpDummyPoints = other.NoTrumpDummyPoints;
			this.Losers = other.Losers;
			this.IsBalanced = other.IsBalanced;
			this.IsFlat = other.IsFlat;
			this.CountAces = other.CountAces;
			this.CountKings = other.CountKings;
			this.Suits = new Dictionary<Suit, SuitSummary>();
			foreach (Suit suit in Enum.GetValues(typeof(Suit)))
			{
				Suits[suit] = new SuitSummary(other.Suits[suit]);
			}
		}



			/*
		protected static (int Min, int Max) ShowRange((int Min, int Max)? r1, (int Min, int Max) r2)
		{
            if (r1 == null) { return r2; }
			(int Min, int Max) range1 = ((int Min, int Max))r1;
            return (Math.Min(range1.Min, r2.Min), Math.Max(range1.Max, r2.Max));
        }
			*/


		// This is called by the ShowState methods whenever any "Points" is modified.  
		protected void ShowPoints(int min, int max)
		{
			this.Points = CombineRange(this.Points, (min, max), CombineRule.Show);
		}


		protected void Combine(HandSummary other, CombineRule cr)
		{
			this.Points = CombineRange(this.Points, other.Points, cr);
			this.HighCardPoints = CombineRange(this.HighCardPoints, other.HighCardPoints, cr);
			this.StartingPoints = CombineRange(this.StartingPoints, other.StartingPoints, cr);
			this.NoTrumpLongHandPoints = CombineRange(this.NoTrumpLongHandPoints, other.NoTrumpLongHandPoints, cr);
			this.NoTrumpDummyPoints = CombineRange(this.NoTrumpDummyPoints, other.NoTrumpDummyPoints, cr);
			this.Losers = CombineRange(this.Losers, other.Losers, cr);
			this.IsBalanced = CombineBool(this.IsBalanced, other.IsBalanced, cr);
			this.IsFlat = CombineBool(this.IsFlat, other.IsFlat, cr);
			this.CountAces = CombineIntSet(this.CountAces, other.CountAces, cr);
			this.CountKings = CombineIntSet(this.CountKings, other.CountKings, cr);
			foreach (Suit suit in Card.Suits)
			{
				this.Suits[suit].Combine(other.Suits[suit], cr);
			}
			TrimShape();
		}


		
		public void TrimShape()
		{
			
			int claimed = 0;
			foreach (var suit in Card.Suits)
			{
				claimed += Suits[suit].GetShape().Min;
			}
			foreach (var suit in Card.Suits)
			{
				Suits[suit].TrimShape(claimed);
			}
		}

		private static bool EqualIntSet(HashSet<int> s1, HashSet<int> s2)
		{
			if (s1 == null)
			{
				return (s2 == null);
			}
			if (s2 == null) return false;
			return s1.SetEquals(s2);
		}

        public bool Equals(HandSummary other)
        {
			if (this.Points != other.Points ||
				this.HighCardPoints != other.HighCardPoints ||
				this.StartingPoints != other.StartingPoints ||
				this.NoTrumpLongHandPoints != other.NoTrumpLongHandPoints ||
				this.NoTrumpDummyPoints != other.NoTrumpDummyPoints ||
				this.Losers != other.Losers ||
				this.IsBalanced != other.IsBalanced ||
				this.IsFlat != other.IsFlat ||
				!EqualIntSet(this.CountAces, other.CountAces) ||
				!EqualIntSet(this.CountKings, other.CountKings)) { return false; }
			foreach (Suit suit in Enum.GetValues(typeof(Suit)))
			{
				if (!this.Suits[suit].Equals(other.Suits[suit])) return false;
			}
			return true;
        }

		public static string RangeToString(string name, (int min, int max)? r)
		{
			if (r != null)
			{
                (int Min, int Max) range = ((int Min, int Max))r;
				if (range.Min == range.Max)
					return $"{name}: {range.Min}\n";
				return $"{name}: {range.Min}-{range.Max}\n";
			}
			return "";
		}
        public override string ToString()
        {
            string s = "";
			s += RangeToString("Points", Points);
			s += RangeToString("HCP", HighCardPoints);
			foreach (var suit in Card.Suits)
			{
				s += RangeToString(suit.ToString(), Suits[suit].Shape);
			}
			return s;
        }
    }

	
}
