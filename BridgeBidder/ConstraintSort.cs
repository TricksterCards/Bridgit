using System;
using System.Collections.Generic;

namespace BridgeBidding
{
	public static class ConstraintSort {
        private static List<Type> DescriptionOrder = new List<Type>() {
            typeof(ShowsPoints),
            typeof(PairShowsPoints),
            typeof(ShowsShape),
            typeof(PairShowsMinShape)
        };

        public static int ForDescription(Constraint constraint) {
            var index = DescriptionOrder.IndexOf(constraint.GetType());
            return index == -1 ? DescriptionOrder.Count : index;
        }
    }
}
