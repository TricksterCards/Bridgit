using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace BridgeBidding
{
    public enum Vulnerable { None, NS, EW, All }

	public class Deal: Dictionary<Direction, Hand>
	{
		public Board Board { get; }
		internal Deal(Board board)
		{
			this.Board = board;
			foreach (Direction direction in Enum.GetValues(typeof(Direction)))
			{
				this[direction] = null;
			}
		}

		public override string ToString()
		{
			var dealer = Board.Dealer;
			StringBuilder sb = new StringBuilder($"{dealer}:");
			var direction = dealer;
			while (true)
			{
				sb.Append(this[direction] == null ? "-" : this[direction].ToString());
				direction = BridgeBidder.LeftHandOpponent(direction);
				if (direction == dealer)
				{
					return sb.ToString();
				}
				sb.Append(" ");
			}
		}
	}

    public class Board
    {
        public Vulnerable Vulnerable = Vulnerable.None;
        public Direction Dealer = Direction.N;
        public Deal Deal { get; }

		public Board()
		{
			Deal = new Deal(this);
		}

		public static Board Parse(string deal, string vulnerable)
		{
			var board = new Board();
			board._Parse(deal, vulnerable);
			return board;
		}

		protected void _Parse(string deal, string vulnerable)
		{	
			ParseDeal(deal);
			if (vulnerable == null)
			{
				throw new ArgumentNullException("vulnerable");
			}
			else
			{
				if (!Enum.TryParse<Vulnerable>(vulnerable, out this.Vulnerable))
				{
					throw new FormatException($"Vulnerable value {vulnerable} is invalid.");
				}
			}
		}

		public void ParseDeal(string deal)
		{
			if (deal == null)
			{
				throw new ArgumentNullException("deal");
			}
			if (deal.Length < 9)
			{
				throw new FormatException("deal paramerter is too short to be valid PBN deal format");
			}
			if (deal.Substring(1, 1) != ":" || !Enum.TryParse<Direction>(deal.Substring(0,1), out Dealer))
			{
				throw new FormatException($"Dealer prefix {deal.Substring(0, 2)} is invalid");
			};
            var handStrings = deal.Substring(2).Split(' ');
			if (handStrings.Length != 4) 
			{
				throw new ArgumentException("deal must contain 4 hands");
			}
			var direction = Dealer;
            foreach (var handString in handStrings)
            {
				Deal[direction] = Hand.Parse(handString);
				direction = BridgeBidder.LeftHandOpponent(direction);
            }

			int totalExpected = 0;
			var allCards = new HashSet<Card>();
			foreach (var hand in Deal)
			{
				if (hand.Value != null) 
				{
					allCards.UnionWith(hand.Value);
					totalExpected += 13;
					if (allCards.Count < totalExpected)
					{
						foreach (var otherHand in Deal)
						{
							if (otherHand.Key != hand.Key && otherHand.Value != null)
							{
								var dup = otherHand.Value.Intersect(hand.Value);
								if (dup.Count() > 0)
								{
									throw new FormatException($"{dup.First()} duplicated in {hand.Key} and {otherHand.Key}");
								}
							}
						}
						throw new FormatException($"One or more duplicated cards in {deal}");
					}
				}
			}
        }


		public void DealCards(List<Card> deck)
		{
			if (deck.Count != 52)
			{
				throw new ArgumentException($"Deck must contain 52 cards.  deck parameter count = {deck.Count}");
			}
			var direction = Direction.N;
			for (int h = 0; h < 4; h++)
			{
				Deal[direction] =  new Hand(deck.GetRange(h * 13, 13));
				direction = BridgeBidder.LeftHandOpponent(direction);
			}
		}

		public void DealRandomHands()
		{
			DealCards(Card.NewDeck(true));
		}
    }
}

