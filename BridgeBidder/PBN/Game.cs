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
            public void Clear()
            {
                _notes.Clear();
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
				int.TryParse(Tags["Board"], out boardNumber); 
			}
			var deal = Tags["Deal"];
			string vulnerable = "None";
			Tags.TryGetValue("Vulnerable", out vulnerable);
			if (boardNumber != int.MinValue)
			{
				return FromString.Board(deal, vulnerable, boardNumber);
			}
			return FromString.Board(deal, vulnerable);
		}

		public Call[] GetAuction()
		{
			return FromString.Auction(TagData["Auction"]);
		}

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

		public string GetGameText()
		{
			var sb = new StringBuilder();
			foreach (var tagName in MTS)
			{
				if (Tags.ContainsKey(tagName))
				{
					sb.Append(PBN.ToString.Tag(tagName, Tags[tagName]));
					sb.Append(GetTagData(tagName));
				}
			}
			sb.Append(GetSectionWithNotes("Auction", AuctionNotes));
			// TODO: Any tags not in MTS need to be alphabatized and rendered...
			return sb.ToString();
		}

        public void Update(BiddingState bs)
        {
            Tags["Dealer"] = PBN.ToString.Direction(bs.Dealer.Direction);
            Tags["Vulnerable"] = PBN.ToString.Vulnerable(bs);
            Tags["Deal"] = PBN.ToString.Deal(bs.Dealer.Direction, bs.Board.Hands);
			if (bs.Board.Number != null)
			{
				Tags["Board"] = bs.Board.Number.ToString();
			}
			UpdateAuction(bs);
			if (bs.Contract.AuctionComplete)
			{
				Tags["Contract"] = bs.Contract.ToString();
				if (bs.Contract.Declarer != null)
				{
					Tags["Declarer"] = bs.Contract.Declarer.Direction.ToString();
				}
			}
        }


		private void UpdateAuction(BiddingState bs)
		{
			Tags["Auction"] = PBN.ToString.Direction(bs.Dealer.Direction);
            AuctionNotes.Clear();
			var auction = bs.GetAuction();
			List<string> lines = new List<string>();
			var curLine = "";
			for (int i = 0; i < auction.Count; i++)
			{
				curLine += auction[i].Call.ToString();
				foreach (var annotation in auction[i].Annotations)
				{
					if (annotation.Type == CallAnnotation.AnnotationType.Alert ||
						annotation.Type == CallAnnotation.AnnotationType.Announce)
					{
						var noteId = AuctionNotes.Add(annotation.Text);
						curLine += $" ={noteId}=";
					}
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

    }
}