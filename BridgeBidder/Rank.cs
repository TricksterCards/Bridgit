namespace BridgeBidding
{
	public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

	public static class ExtendRank
	{
		public static string ToLetter(this Rank r)
		{
			switch (r)
			{
				case Rank.Two:   return "2";
				case Rank.Three: return "3";
				case Rank.Four:  return "4";
				case Rank.Five:  return "5";
				case Rank.Six:   return "6";
				case Rank.Seven: return "7";
				case Rank.Eight: return "8";
				case Rank.Nine:  return "9";
				case Rank.Ten:   return "T";
				case Rank.Jack:  return "J";
				case Rank.Queen: return "Q";
				case Rank.King:  return "K";
				case Rank.Ace:   return "A";
			}
			return "";
		}
	}
}