namespace BridgeBidding
{
	public enum Suit { Clubs = 0, Diamonds = 1, Hearts = 2, Spades = 3 }

	public static class ExtendSuit
	{
		public static string ToLetter(this Suit s)
		{
			
			switch (s)
			{
				case Suit.Clubs:    return "C";
				case Suit.Diamonds: return "D";
				case Suit.Hearts:   return "H";
				case Suit.Spades:   return "S";
			}
			return "";
		}

		public static string ToSymbol(this Suit s)
		{
			switch (s)
			{
				case Suit.Clubs:    return "♣";
				case Suit.Diamonds: return "♦";
				case Suit.Hearts:   return "♥";
				case Suit.Spades:   return "♠";
			}
			return "";
		}
	}
}