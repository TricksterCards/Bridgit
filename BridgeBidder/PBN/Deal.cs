using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BridgeBidding.PBN
{
 
	public class Deal
    {
        public Direction Dealer;
        public Dictionary<Direction, Hand> Hands;

        public Deal()
        {
            this.Hands = new Dictionary<Direction, Hand>();
        }

        public Deal(Board board)
        {
            this.Dealer = board.Dealer;
            this.Hands = board.Hands;
        }

        // TODO: Think through what is best here.  Should this have a static method that returns
        // a deal object?  Should this be a struct?  Should PBN.FromString.Deal() logic be in here?
		public static Deal Parse(string s)
		{
            var deal = new Deal();
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (s.Length < 9)
			{
				throw new FormatException("deal paramerter is too short to be valid PBN deal format");
			}
			if (s.Substring(1, 1) != ":" || !Enum.TryParse<Direction>(s.Substring(0,1), out deal.Dealer))
			{
				throw new FormatException($"Dealer prefix {s.Substring(0, 2)} is invalid");
			};
            var handStrings = s.Substring(2).Split(' ');
			if (handStrings.Length != 4) 
			{
				throw new ArgumentException("deal must contain 4 hands");
			}
			var direction = deal.Dealer;
            foreach (var handString in handStrings)
            {
				deal.Hands[direction] = Hand.Parse(handString);
				direction = BridgeBidder.LeftHandOpponent(direction);
            }

			int totalExpected = 0;
			var allCards = new HashSet<Card>();
			foreach (var hand in deal.Hands)
			{
				if (hand.Value != null) 
				{
					allCards.UnionWith(hand.Value);
					totalExpected += 13;
					if (allCards.Count < totalExpected)
					{
						foreach (var otherHand in deal.Hands)
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
						// Throw a useful excpetion to help debug problems with 
						throw new FormatException($"One or more duplicated cards in {deal}");
					}
				}
			}
            return deal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Dealer}:");
            var direction = Dealer;
			while (true)
			{
                sb.Append(Hands[direction] == null ? "-" : Hands[direction].ToString());
				direction = BridgeBidder.LeftHandOpponent(direction);
                if (direction == Dealer)
                {
                    return sb.ToString();
                }
                sb.Append(" ");
			}
        }
    }
}
