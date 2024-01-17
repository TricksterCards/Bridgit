using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BridgeBidding
{
	public class Hand : HashSet<Card>
	{
		public Hand() { }


		public static Hand ParsePbnFormat(string handString, bool requireFullHand = true)
		{
			if (handString == null)
			{
				throw new ArgumentNullException("handString");
			}
			if (handString == "-") 
			{
				return null;
			}

			Suit[] pbnSuits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };

			var suits = handString.Split('.');
			if (suits.Length != pbnSuits.Length)
			{
				throw new ArgumentException("handString does not contain four suits");
			}
			Hand hand = new Hand();			
			for (var i = 0; i < suits.Length; i++)
				foreach (var rankChar in suits[i])
				{
					var card = new Card(Card.ParseRank(rankChar), pbnSuits[i]);
					if (hand.Contains(card))
					{
						throw new ArgumentException($"Duplicate card {card} in {handString}");
					}
					hand.Add(card);
				}
		
			if (requireFullHand && hand.Count != 13)
			{
				throw new ArgumentException($"hand {handString} does not contains {hand.Count} cards.  Should have 13.");
			}

			return hand;
		}

		public int HighCardPoints(Suit? suit = null)
		{
			var highCardPoints = 0;

			if (suit == null)
			{
				//  basic points for high cards
				highCardPoints += this.Count(c => c.Rank == Rank.Ace) * 4;
				highCardPoints += this.Count(c => c.Rank == Rank.King) * 3;
				highCardPoints += this.Count(c => c.Rank == Rank.Queen) * 2;
				highCardPoints += this.Count(c => c.Rank == Rank.Jack);
			} 
			else
			{
				highCardPoints += this.Count(c => (c.Rank == Rank.Ace && c.Suit == suit)) * 4;
				highCardPoints += this.Count(c => (c.Rank == Rank.King && c.Suit == suit)) * 3;
				highCardPoints += this.Count(c => (c.Rank == Rank.Queen && c.Suit == suit)) * 2;
				highCardPoints += this.Count(c => (c.Rank == Rank.Jack && c.Suit == suit));
			}
			return highCardPoints;
		}

		public int LengthPoints()
		{
			//  add for long suit length (adding one for each card over 4 in each suit)
			return Card.Suits.Select(suit => this.Count(c => c.Suit == suit)).Where(count => count >= 5).Sum(count => count - 4);
		}

		public Dictionary<Suit, int> CountsBySuit()
		{
			var counts = this.GroupBy(c => c.Suit).ToDictionary(g => g.Key, g => g.Count());

			//  initialize the missing suits to zero
			foreach (var suit in Card.Suits.Where(suit => !counts.ContainsKey(suit)))
				counts[suit] = 0;

			return counts;
		}

		public bool IsBalanced
		{
			get
			{
				var suitCounts = this.GroupBy(c => c.Suit).Select(g => new { suit = g.Key, count = g.Count() }).OrderBy(sc => sc.count).ToList();
				return suitCounts.Count == 4 && suitCounts[0].count >= 2 && suitCounts[1].count >= 3;
			}
		}


		public bool Is4333
		{
			get
			{
				var counts = CountsBySuit();
				bool found4 = false;
				foreach (var suit in Card.Suits)
				{
					if (counts[suit] > 4) return false;
					if (counts[suit] == 4)
					{
						if (found4) return false;
						found4 = true;
					}
				}
				return true;
			}
		}

		public bool IsGoodSuit(Suit suit)
		{
			//  TODO: should we consider a hand "good" if we have more than the minimum count (requires extra argument)?
			//if (minimum > 0 && CountsBySuit(hand)[suit] > minimum)
			//    return true;

			//  otherwise if we have two of the top three Honors or three of the top five Honors in a suit, then it is considered "good"
			return this.Count(c => c.Suit == suit && c.Rank >= Rank.Queen) >= 2 || this.Count(c => c.Suit == suit && c.Rank >= Rank.Ten) >= 3;
		}

		public int Losers(Suit? s = null)
		{
			int losers = 0;
			if (s is Suit suit)
			{
				losers = Math.Min(this.Count(c => (c.Suit == suit)), 3);
				if (losers == 3 && this.Contains(new Card(Rank.Queen, suit))) losers--;
				if (losers >=2 && this.Contains(new Card(Rank.King, suit))) losers--;
				if (losers >= 1 && this.Contains(new Card(Rank.Ace, suit))) losers--;
			}
			else
			{
				foreach (Suit x in Card.Suits)
				{
					losers += Losers(x);
				}
			}
			return losers;
		}

	}
}
