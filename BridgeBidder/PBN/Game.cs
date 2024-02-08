using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;

namespace BridgeBidding.PBN
{
 
	public class Game
	{
		public class Notes
		{
			private List<string> _notes = new List<string>();

			public string GetValue(string noteReference)
			{
				int i;
				if (noteReference.Length < 3 || !noteReference.StartsWith("=") || !noteReference.EndsWith("=") ||
				   (!int.TryParse(noteReference.Substring(1, noteReference.Length - 2), out i)))
				{
					throw new FormatException($"Note reference {noteReference} is not properly formed.");
				}
				if (i < 1 || i > _notes.Count)
				{
					throw new FormatException($"Note reference {noteReference} refers to a non-existant note.");
				}
				return _notes[i - 1];
			}

			// Returns a note ID reference in the form "=x=" where x is an integer starting with 1.
			public string Add(string note)
			{
				var i = _notes.IndexOf(note);
				if (i == -1)
				{
					i = _notes.Count;
					_notes.Add(note);
				}
				return $"={i+1}=";
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
            public void Clear()
            {
                _notes.Clear();
			}
		}



		public Dictionary<string, string> Tags = new Dictionary<string, string>();
		public Dictionary<string, List<String>> TagData = new Dictionary<string, List<string>>();
		public Dictionary<string, string> TagCommentary = new Dictionary<string, string>();

		public Notes AuctionNotes = new Notes();
		public Notes PlayNotes = new Notes();

		public static string[] MTS = new string[]
		{
			"Event",
			"Site",
			"Date", 
			"Board",
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

		public Board GetBoard()
		{
			int boardNumber = int.MinValue;
			if (Tags.ContainsKey("Board"))
			{ 
				if (!int.TryParse(Tags["Board"], out boardNumber))
				{
					boardNumber = int.MinValue;
				} 
			}
			var deal = Tags["Deal"];
			string vulnerable;
			if (!Tags.TryGetValue("Vulnerable", out vulnerable)) {
				vulnerable = "None";
			}
			if (boardNumber != int.MinValue)
			{
				return FromString.Board(deal, vulnerable, boardNumber);
			}
			return FromString.Board(deal, vulnerable);
		}

		// TODO: Decide on the model for this.  Should clients call GetXXX like GetBoard() and GetAuction()
		// and hide the Auction.FromGame() internally.
		public Auction GetAuction()
		{
			return Auction.FromGame(this);
		}

		// More food for thought.  Should there be a SetAuction() which takes an auction object and updates
		// the current game?  Seems reasonable.  So instead of auction.Update(game) would be  Game.UpdateFrom(auction)...
		// This model is all over the place right now.

		// Another idea is that Board, Dealer, etc are not handled as tags and are objects.  So Deal() is the deal.
		// If you change it and then call game.ToString() the game is changed.  So we parse known things and keep them
		// as objects that can be directly manipulated.

		private string GetSectionWithNotes(string tagName, Notes notes)
		{
			var s = "";
			if (Tags.ContainsKey(tagName))
			{
				s += PBN.ToString.Tag(tagName, Tags[tagName]);
				s += GetTagData(tagName);
				foreach (var note in notes.GetValues())
				{
					s += PBN.ToString.Tag("Note", note);
				}
			}
			return s;
		}

		private string GetTagData(string tagName)
		{
			var s = "";
			if (TagData.ContainsKey(tagName))
			{
				foreach (var line in TagData[tagName])
				{
					s += $"{line}\n";
				}
			}
			return s;
		}

		private string GetCommentary(string tagName)
		{
			var s = "";
			if (TagCommentary.ContainsKey(tagName))
			{
				s = $"{{\n{TagCommentary[tagName]}\n}}\n";
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
					sb.Append(PBN.ToString.Tag(tagName, Tags[tagName]));
					sb.Append(GetTagData(tagName));
					sb.Append(GetCommentary(tagName));
				}
			}
			sb.Append(GetSectionWithNotes("Auction", AuctionNotes));
			// TODO: Any tags not in MTS need to be alphabatized and rendered...
			return sb.ToString();
		}

        public void Update(BiddingState bs)
        {
            Tags["Dealer"] = bs.Dealer.Direction.ToString();
            Tags["Vulnerable"] = PBN.ToString.Vulnerable(bs);
            Tags["Deal"] = new Deal(bs.Board).ToString();
			if (bs.Board.Number != null)
			{
				Tags["Board"] = bs.Board.Number.ToString();
			}
			Auction.FromBiddingState(bs).UpdateGame(this);
		//	UpdateAuction(bs);
			if (bs.Contract.AuctionComplete)
			{
				Tags["Contract"] = bs.Contract.ToString();
				if (bs.Contract.Declarer != null)
				{
					Tags["Declarer"] = bs.Contract.Declarer.Direction.ToString();
				}
			}
        }

/*
		private void UpdateAuction(BiddingState bs)
		{
			Tags["Auction"] = bs.Dealer.Direction.ToString();
            AuctionNotes.Clear();
			var auction = bs.GetAuction();
			List<string> lines = new List<string>();
			var curLine = "";
			for (int i = 0; i < auction.Count; i++)
			{
				curLine += auction[i].Call.ToString();
				string note = "";
				foreach (var annotation in auction[i].Annotations)
				{
					if (annotation.Type == CallAnnotation.AnnotationType.Alert ||
						annotation.Type == CallAnnotation.AnnotationType.Announce)
					{
						if (note.Length > 0)
						{
							note += ";";
						}
						note += $"{annotation.Type} {annotation.Text}";
					}
				}
				if (note.Length > 0)
				{
					string noteReference = AuctionNotes.Add(note);
					curLine += $" {noteReference}";
				}

				if (i + 1 == auction.Count && !bs.Contract.AuctionComplete)
				{
					curLine += " +";
				}

				if (i % 4 == 3)
				{
					lines.Add(curLine);
					curLine = "";
				}
				else if (i + 1 < auction.Count)
				{
					curLine += " ";
				}
			}
			if (curLine.Length > 0)
			{
				lines.Add(curLine);
			}
			TagData["Auction"] = lines;
		}
*/
    }
}