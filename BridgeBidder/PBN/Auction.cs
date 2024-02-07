using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BridgeBidding.PBN
{
 
	public class Auction: List<Auction.PbnCall>
    {
        public Direction FirstToAct;

        public Call[] Calls => this.Select(pbnCall => pbnCall.Call).ToArray();

        public class PbnCall
        {
            public Call Call;
            public string Note;

        }

        public static Auction FromBiddingState(BiddingState bs)
        {
            var auction = new Auction() { FirstToAct = bs.Dealer.Direction };
            var calls = bs.GetAuction();
            foreach (var callDetails in calls)
            {
                string note = null;
				foreach (var annotation in callDetails.Annotations)
				{
					if (annotation.Type == CallAnnotation.AnnotationType.Alert ||
						annotation.Type == CallAnnotation.AnnotationType.Announce)
					{
						if (note == null)
                        {
                            note = "";
                        }
                        else
						{
							note += ";";
						}
						note += $"{annotation.Type} {annotation.Text}";
					}
				}
                auction.Add(new PbnCall { Call = callDetails.Call, Note = note });
            }
            return auction;
        }


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

        public static Auction FromString(Direction firstToAct, string auctionText, PBN.Game.Notes notes = null)
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
                    auction.Add(new PbnCall { Call = Call.Parse(token) });
				}
			}
			return auction;
		}


        public void UpdateGame(Game game)
        {
            // TODO: Validate auction.  Use Contract class to make sure bids are correct.
			game.Tags["Auction"] = FirstToAct.ToString();
            game.AuctionNotes.Clear();
			List<string> lines = new List<string>();
			var curLine = "";
            int numPasses = 0;
            int numPassesEndsAuction = 4;
			for (int i = 0; i < this.Count; i++)
			{
                var call = this[i].Call;
				curLine += call.ToString();
                if (this[i].Note != null)
                {
					string noteReference = game.AuctionNotes.Add(this[i].Note);
					curLine += $" {noteReference}";
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
					curLine += " +";
				}

				if (i % 4 == 3)
				{
					lines.Add(curLine);
					curLine = "";
				}
				else if (i + 1 < this.Count)
				{
					curLine += " ";
				}
			}
			if (curLine.Length > 0)
			{
				lines.Add(curLine);
			}
			game.TagData["Auction"] = lines;
		}
    }
}


