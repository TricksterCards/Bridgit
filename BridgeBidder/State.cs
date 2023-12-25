using System;
using System.Collections.Generic;
using System.Linq;


namespace BridgeBidding
{
    public abstract class State
	{
        public enum CombineRule
        {
            Show,       // If left or right is null the use other.  If both non-null then use smallest min, largest max
            CommonOnly, // If either left or right is null, result is null.  Otherwise smallest min, largest max
            Merge,      // If either left or right is null then use other.  If both non-null then use largest min, smallest max
        }

        protected static (int Min, int Max)? CombineRange((int Min, int Max)? a, (int Min, int Max)? b, CombineRule cr)
        {
            if (a != null && b != null)
            {
                (int Min, int Max) rangeA = ((int Min, int Max))a;
                (int Min, int Max) rangeB = ((int Min, int Max))b;
                if (cr == CombineRule.Merge)
                {
                    return (Math.Max(rangeA.Min, rangeB.Min), Math.Min(rangeA.Max, rangeB.Max));
                }
                return (Math.Min(rangeA.Min, rangeB.Min), Math.Max(rangeA.Max, rangeB.Max));
            }
            // If we are only merging common properties then if either is null, the result is null
            if (cr == CombineRule.CommonOnly)
            {
                return null;
            }
            return (a == null) ? b : a;
        }



        protected static bool? CombineBool(bool? b1, bool? b2, CombineRule cr)
        {
            if (b1 == null)
            {
                return (cr == CombineRule.CommonOnly) ? null : b2;
            }
            if (b2 == null)
            {
                return (cr == CombineRule.CommonOnly) ? null : b1;
            }
            // TODO: Is this right?  We know nothing if conflicting information?  Seems reasonable...
            return b1;
        }

        protected static int? CombineInt(int? i1, int? i2, CombineRule cr)
        {
            if (i1 == null)
            {
                return (cr == CombineRule.CommonOnly) ? null : i2;
            }
            if (i2 == null)
            {
                return (cr == CombineRule.CommonOnly) ? null : i1;
            }
            // TODO: Is this right?  We know nothing if conflicting information?  Seems reasonable...
            return i1;
        }

		protected static HashSet<int> CombineIntSet(HashSet<int> s1, HashSet<int> s2, CombineRule cr)
		{
			if (s1 == null)
			{
				return (cr == CombineRule.CommonOnly) ? null : s2;
			}
			if (s2 == null)
			{
				return (cr == CombineRule.CommonOnly) ? null: s1;
			}
			if (cr == CombineRule.Merge)
			{
				return new HashSet<int>(s1.Intersect(s2));
			}
			return new HashSet<int>(s1.Union(s2));
		}

    }
}