namespace BridgeBidding
{

	public static class Range
	{

		public static string GetString(int min, int max, int limit) {
			if (min == max)
				return $"{max}";

			if (max >= limit)
				return $"{min}+";

			return $"{min}–{max}";
		}
	}
}
