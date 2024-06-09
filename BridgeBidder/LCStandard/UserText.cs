using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
    public static class UserText
    {
        // Conventions
        public static string Blackwood = "Blackwood";
        public static string Gerber = "Gerber";
        public static string JacobyTransfer = "Jacoby Transfer";
        public static string Jacoby2NT = "Jacoby 2NT";
        public static string Jacoby2NTDescription = "4+ card support for opener's major with game-going values";
        public static string Michaels = "Michaels";
        public static string NegativeDouble = "Negative Double";
        public static string OneNoTrumpRange = "15 to 17";
        public static string TransferToHearts = $"Transfer to {Suit.Hearts.ToSymbol()}";
        public static string TransferToSpades = $"Transfer to {Suit.Spades.ToSymbol()}";
        public static string TransferToClubs = $"Transfer to {Suit.Clubs.ToSymbol()}";
        public static string ShowsVoidOrSingleton = "Shows void or singleton in bid suit";
        public static string Stayman = "Stayman";
        public static string Strong = "Strong";
        public static string TakeoutDouble = "Takeout Double";
        public static string SemiForcing = "semi-forcing";

        public static string InvertedMinorStrong = "strong, 10+ points";
        public static string InvertedMinorWeak = "weak, 0-7 points";

        public static string Drury = "Drury";

    }
}