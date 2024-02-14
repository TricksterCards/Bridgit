using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BridgeBidding
{
 
	public class Auction: List<Auction.AnnotatedCall>
    {
        public Game Game { get; } 

        public Call[] Calls => this.Select(annotatedCall => annotatedCall.Call).ToArray();

        internal Auction(Game game)
        {
            this.Game = game;
        }

        public class AnnotatedCall
        {
            public Call Call;
            public string Note;
        }

        public void Add(CallDetails callDetails)
        {
            string note = null;
            foreach (var annotation in callDetails.Annotations)
            {
                if (note == null)
                {
                    note = "";
                }
                else
                {
                    note += ";";
                }
                note += $"{annotation.Type}: {annotation.Text}";
            }
            Add(callDetails.Call, note);
        }

        public void Add(Call call, string note = null)
        {
            Add(new AnnotatedCall { Call = call, Note = note });
        }
/*
        public void Update(BiddingState bs)
        {
            this.Clear();
            if (bs.Dealer.Direction != Game.Dealer)
            {
                throw new Exception($"Game dealer is {Game.Dealer} and bidding state is {bs.Dealer.Direction}.  These must be equal to call Update");
            }
            var calls = bs.GetAuction();
            foreach (var callDetails in calls)
            {
                Add(callDetails);
            }
        }
*/
/*
        public static Auction FromGame(Game game)
        {
            Direction firstToAct;
            if (!Enum.TryParse<Direction>(game.Tags["Auction"], out firstToAct))
            {
                throw new FormatException("Auction tag does not specify first to act.");
            }
			var auctionText = string.Join(" ", game.TagData["Auction"]);
            return FromString(firstToAct, auctionText, game.AuctionNotes);
        }
*/
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
/*
        public static Auction Parse(string auctionText)
        {
            var auction = new Auction { FirstToAct = firstToAct };
		    var tokens = auctionText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var token in tokens)
			{
                if (token.StartsWith("="))
                {   
                    // If we were called from a basic string then note references make no sense and are ignored.
                    if (notes != null)
                    {
                        auction.Last().Note = notes.GetValue(token);
                    }
                }
                // TODO: We should really remember $annotations too, but for now only notes are supported.
				else if (!token.StartsWith("$") && !token.Equals("+"))
				{
                    auction.Add(new CallWithNote { Call = Call.Parse(token) });
				}
			}
			return auction;
		}

*/
        public override string ToString()
        {
            if (this.Count == 0) return "";
            // TODO: Validate auction.  Use Contract class to make sure bids are correct.
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
                if (this[i].Note != null)
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


