using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace BridgeBidding
{

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

		public Rank Rank { get; }
		public Suit Suit { get; }
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

		public static List<Card> NewDeck(bool shuffle)
		{
			var deck = new List<Card>();
			foreach (var suit in Suits)
			{
				foreach (Rank rank in Enum.GetValues(typeof(Rank)))
				{
					deck.Add(new Card(rank, suit));
				}
			}
			if (shuffle)
			{
				var r = new Random();
				deck = deck.OrderBy(x => r.Next()).ToList();
			}
			return deck;
		}

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

        public override string ToString()
        {
            return $"{Rank.ToLetter()}{Suit.ToLetter()}";
		}
	}
}
