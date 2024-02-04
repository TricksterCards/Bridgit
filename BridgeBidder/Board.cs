using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace BridgeBidding
{
    public enum Vulnerable { None, NS, EW, All }

    public class Board
    {
        public int? Number = null;
        public Vulnerable Vulnerable = Vulnerable.None;
        public Direction Dealer = Direction.N;
        public Dictionary<Direction, Hand> Hands = new Dictionary<Direction, Hand>();


		public Board()
		{

		}

		public Board(string dealString)
		{
			var deal = PBN.Deal.Parse(dealString);
			this.Hands = deal.Hands;
			this.Dealer = deal.Dealer;
			this.Number = null;
		}

		public void Deal(List<Card> deck)
		{
			if (deck.Count != 52)
			{
				throw new ArgumentException($"Deck must contain 52 cards.  deck parameter count = {deck.Count}");
			}
			var direction = Direction.N;
			for (int h = 0; h < 4; h++)
			{
				var hand = new Hand();
				hand.UnionWith(deck.GetRange(h * 13, 13));
				Hands[direction] = hand;
				direction = BridgeBidder.LeftHandOpponent(direction);
			}
		}

		public void DealRandomHands()
		{
			Deal(Card.NewDeck(true));
		}
    }
}

