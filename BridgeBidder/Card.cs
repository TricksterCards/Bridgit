﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace BridgeBidding
{

    public enum Suit { Clubs = 0, Diamonds = 1, Hearts = 2, Spades = 3 }

	public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }


	public class Card
	{
		public static Suit[] Suits = { Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades };


		public static Rank ParseRank(char rankString)
		{
			Rank rank;
			if (StringToRank.TryGetValue(rankString, out rank))
			{
				return rank;
			}
			throw new ArgumentException($"rank {rankString} is invalid character");
		}

		public Rank Rank { get; private set; }
		public Suit Suit { get; private set; }
		public Card(Rank rank, Suit suit)
		{
			this.Rank = rank;
			this.Suit = suit;
		}
		public override int GetHashCode()
		{
			return (int)Rank * 32 + (int)Suit;
		}
		public override bool Equals(object obj) 
		{
			return (obj is Card card && card.Rank == Rank && card.Suit == Suit);
		}


		public static Dictionary<string, Suit> StringToSuit = new Dictionary<string, Suit>
		{
			{ "♣", Suit.Clubs    },
			{ "C", Suit.Clubs    },
			{ "♦", Suit.Diamonds },
			{ "D", Suit.Diamonds },
			{ "♥", Suit.Hearts   },
			{ "H", Suit.Hearts   },
			{ "♠", Suit.Spades   },
			{ "S", Suit.Spades   }
		};

		public static Dictionary<char, Rank> StringToRank = new Dictionary<char, Rank>
		{
			{ '2', Rank.Two },
			{ '3', Rank.Three },
			{ '4', Rank.Four },
			{ '5', Rank.Five },
			{ '6', Rank.Six },
			{ '7', Rank.Seven },
			{ '8', Rank.Eight },
			{ '9', Rank.Nine },
			{ 'T', Rank.Ten },
			{ 'J', Rank.Jack },
			{ 'Q', Rank.Queen },
			{ 'K', Rank.King },
			{ 'A', Rank.Ace }
		};
		public static Dictionary<string, Suit> SymbolToSuit = new Dictionary<string, Suit>
		{
			{  "♣",  Suit.Clubs    },
			{  "♦",  Suit.Diamonds },
			{  "♥",  Suit.Hearts   },
			{  "♠",  Suit.Spades   }
		};



		public static Dictionary<Suit, string> SuitToSymbol = new Dictionary<Suit, string>
		{
			{ Suit.Clubs,    "♣" },
			{ Suit.Diamonds, "♦" },
			{ Suit.Hearts,   "♥" },
			{ Suit.Spades,   "♠" }
		};

	}
}
