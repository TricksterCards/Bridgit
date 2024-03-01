namespace BridgeBidding
{
    public enum Direction { N = 0, E = 1, S = 2, W = 3 }

    public static class ExtendDirection
    {
        public static Direction Partner(this Direction d)
		{
			return (Direction)(((int)d + 2) % 4);
		}

		public static Direction RightHandOpponent(this Direction d)
		{
			return (Direction)(((int)d + 3) % 4);
		}

		public static Direction LeftHandOpponent(this Direction d)
		{
			return (Direction)(((int)d + 1) % 4);
		}

		public static Pair Pair(this Direction d)
		{
			return (Pair)((int)d % 2);
		}
    }
}
