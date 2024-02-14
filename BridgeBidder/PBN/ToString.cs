using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BridgeBidding.PBN
{
/*
 //   public static class ToString//
	{  
        public static string Vulnerable(BiddingState bs)
        {
			// TODO: Really?  Is this necessary?
			return bs.Game.Vulnerable.ToString();
        }

	//	public static string Direction(Direction direction)
	//	{
	//		return direction.ToString();
	//	}

/*
        public static string Deal(Direction dealer, Dictionary<Direction, Hand> hands)
        {
            string deal = $"{dealer}";
            var direction = dealer;
			for (int i = 0; i < 4; i++)
			{
				var hand = hands[direction];
				if (hand == null)
				{
					deal += "-";
				}
				else 
				{
					deal += hand.ToString();
				}
				if (i < 3) deal += " ";
				direction = BridgeBidder.LeftHandOpponent(direction);
			}
            return deal;
        }
		*/
/*
		public static string Hand(Hand hand)
		{
			var s = "";
			var suitCards = hand.GroupBy(c => c.Suit).ToDictionary(g => g.Key, g => g.OrderBy(sc => sc.Rank).Reverse());
			foreach (var suit in FromString.HandSuitOrder)
			{
				if (suitCards.ContainsKey(suit))
				{
					foreach (var card in suitCards[suit])
					{
						s += Card.RankToSymbol[card.Rank];
					}
				}
                if (suit != Suit.Clubs) s += ".";
			}
			return s;
		}

		public static string Tag(string key, string value)
		{
			return $"[{key} \"{value}\"]\n";
		}

		public static string Notes(Game.Notes notes)
		{
			var s = "";
			foreach (var noteValue in notes.GetValues())
			{
				s += Tag("Note", noteValue);
			}
			return s;
		}
	}

	public class Game
	{
		public class Notes
		{
			private List<string> _notes = new List<string>();
			public int Add(string note)
			{
				var i = _notes.IndexOf(note);
				if (i == -1)
				{
					i = _notes.Count;
					_notes.Add(note);
				}
				return i + 1;
			}
			public List<string> GetValues()
			{
				var values = new List<String>(_notes.Count);
				for (int i = 0; i < _notes.Count; i++)
				{
					values.Add($"{i+1}:{_notes[i]}");
				}
				return values;
			}
			
		}

		public Dictionary<string, string> Tags = new Dictionary<string, string>();
		public Dictionary<string, List<String>> TagData = new Dictionary<string, List<string>>();

		public Notes AuctionNotes = new Notes();
		public Notes PlayNotes = new Notes();

		public static string[] MTS = new string[]
		{
			"Event",
			"Site",
			"Date", 
			"West",       
			"North",      
			"East",       
			"South",      
			"Dealer",     
			"Vulnerable", 
			"Deal",       
			"Scoring",    
			"Declarer",   
			"Contract",   
			"Result"    
		};

		private string GetSectionWithNotes(string tagName, Notes notes)
		{
			var s = "";
			if (Tags.ContainsKey(tagName))
			{
				s += Serialize.TagString(tagName, Tags[tagName]);
				if (TagData.ContainsKey(tagName))
				{
					foreach (var line in TagData[tagName])
					{
						s += $"{line}\n";
					}
				}
				foreach (var note in notes.GetValues())
				{
					s += Serialize.TagString("Note", note);
				}
			}
			return s;
		}

		public string GetGameText()
		{
			var sb = new StringBuilder();
			foreach (var tagName in MTS)
			{
				if (Tags.ContainsKey(tagName))
				{
					sb.Append(Serialize.TagString(tagName, Tags[tagName]));
				}
			}
			sb.Append(GetSectionWithNotes("Auction", AuctionNotes));
			// TODO: Any tags not in MTS need to be alphabatized and rendered...
			return sb.ToString();
		}
    }
    */
}