using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BridgeBidding
{
 
	public class Auction: List<Auction.AnnotatedCall>
    {
        public Game Game { get; } 

        public List<Call> Calls => this.Select(annotatedCall => annotatedCall.Call).ToList();

        internal Auction(Game game)
        {
            this.Game = game;
        }

        public class AnnotatedCall
        {
            public Call Call;
            public string Note;

            public bool HasNote => !string.IsNullOrEmpty(Note);
        }

        public void Add(CallDetails callDetails)
        {
            // Select the text from every annothation and join them into a single note.
            string note = string.Join(";", callDetails.Annotations.Select(a => $"{a.Type}: {a.Text}"));
            Add(callDetails.Call, note);
        }

        public void Add(Call call, string note = null)
        {
            Add(new AnnotatedCall { Call = call, Note = note });
        }

        public bool IsValid()
        {
            string error;
            return IsValid(out error);
        }

        public bool IsValid(out string error)
        {
            return ContractState.IsValidAuction(Game.Dealer, Calls, out error);
        }

        // TODO: This really needs to work properly if the full PBN text is passed in.  But for now, we just expect a single
        // stirng of calls that start with the dealer.
        internal void Parse(string auctionText)
        {
            this.Clear();
            var tokens = auctionText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var token in tokens)
			{
			    if (token.StartsWith("="))
                {
                    // TODO: Need to keep track of notes.
                }
                else if (!token.StartsWith("$") && !token.Equals("+"))
				{
                    Add(new AnnotatedCall { Call = Call.Parse(token) });
				}
			}
        }

        // TODO: Should we actually confirm that the auction is valid here?
        // Perhaps it is right to seraialize the auction even if it is invalid.
        public override string ToString()
        {
            if (this.Count == 0) return "";

            var sb = new StringBuilder();
            var notes = new List<string>();
            sb.AppendLine($"[Auction \"{Game.Dealer}\"]");

            int numPasses = 0;
            int numPassesEndsAuction = 4;
            int numCallsThisLine = 0;
			for (int i = 0; i < this.Count; i++)
			{
                var call = this[i].Call;
                sb.Append(call.ToString());
                numCallsThisLine++;
                if (this[i].HasNote)
                {
                    var noteIndex = notes.IndexOf(this[i].Note);
                    if (noteIndex < 0)
                    {
                        noteIndex = notes.Count;
                        notes.Add(this[i].Note);
                    }
					sb.Append($" ={noteIndex+1}=");
				}
                if (call.Equals(Call.Pass))
                {
                    numPasses += 1;
                }
                else
                {
                    numPasses = 0;
                    numPassesEndsAuction = 3;
                }
				if (i + 1 == this.Count && numPasses < numPassesEndsAuction)
				{
					sb.Append(" +");
				}

				if (numCallsThisLine == 4)
				{
                    sb.AppendLine();
                    numCallsThisLine = 0;
				}
				else if (i + 1 < this.Count)
				{
					sb.Append(' ');
				}
			}
			if (numCallsThisLine > 0)
			{
				sb.AppendLine();
			}
            for (int noteIndex = 0; noteIndex < notes.Count; noteIndex++)
            {
                sb.AppendLine($"[Note \"{noteIndex+1}:{notes[noteIndex]}\"]");
            }
            return sb.ToString();
		}
    }
}


