using System.Diagnostics;

namespace BridgeBidding
{

	public enum Strain { Clubs = 0, Diamonds = 1, Hearts = 2, Spades = 3, NoTrump = 4 }

	public static class ExtendStrain
	{
		public static string ToLetter(this Strain s)
		{
			
			switch (s)
			{
				case Strain.Clubs:     return "C";
				case Strain.Diamonds:  return "D";
				case Strain.Hearts:    return "H";
				case Strain.Spades:    return "S";
				case Strain.NoTrump:   return "NT";
			}
			return "";
		}

		public static string ToSymbol(this Strain s)
		{
			switch (s)
			{
				case Strain.Clubs:    return "♣";
				case Strain.Diamonds: return "♦";
				case Strain.Hearts:   return "♥";
				case Strain.Spades:   return "♠";
				case Strain.NoTrump:  return "NT";
			}
			return "";
		}

		public static Suit? ToSuit(this Strain s)
		{
			switch (s)
			{
				case Strain.Clubs:    return Suit.Clubs;
				case Strain.Diamonds: return Suit.Diamonds;
				case Strain.Hearts:   return Suit.Hearts;
				case Strain.Spades:   return Suit.Spades;
			}
			Debug.Assert(s == Strain.NoTrump);
			return null;
		}
	}
}