using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;


namespace BridgeBidding.PBN
{
    public static class FromString
    {
        public static List<Game> ParseGames(string text)
        {
            var gameTextList = new List<String>();
            // First we break the list of games into indivigual games.  Games start with the first [] tag and end with the
            // first blank line (or end of file) that is not in a comment block.  So this logic simply looks for 
            var lines = text.Split(new[] { "\n", "\n\r" }, StringSplitOptions.None)
                .Select(line => line.Trim());
            StringBuilder gameText = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (gameText.Length > 0)
                    {
                        gameTextList.Add(gameText.ToString());
                        gameText.Clear();
                    }
                }
                else
                {
                    // TODO: Need to detect {} comments and copy them.  But for now, we will just copy everything 
                    gameText.AppendLine(line);
                }
            }
            if (gameText.Length > 0)
            {
                gameTextList.Add(gameText.ToString());
            }
            var games = new List<Game>();
            foreach (var gt in gameTextList)
            {
                games.Add(Game.Parse(gt));
            }
            return games;
        }

/*
       	public static List<Game> Games(string text)
        {
            var games = new List<Game>();
			Game game = null; 
            var tags = TokenizeTags(text);

			// This code is not completely correct in that it triggers off of the [Event] tag to indicate the
			// start of a game section of a afile.


            foreach (var tag in tags)
            {
				if (tag.Name == "Event")
				{		
					if (game != null)
					{
						games.Add(game);
					}
					game = new Game();
				}
				Debug.Assert(game != null);
				// For now we will not import Note tags.  Ignored.  They are the only tags allowed to be duplicates.
				// TODO: Deal with note in auction and play sections.
				if (tag.Name != "Note")
				{
                    if (tag.Name == "Dealer")
                    {
                        Enum.TryParse<Direction>(tag.Value, out game.Dealer);
                    }
					if (tag.Name == "Board") 
					{
						int.TryParse(tag.Value, out game.Board);
					}
					if (tag.Name == "Deal")
					{
                        // TODO: THIS IS WRONG.  WE SHOULD NOT OVERRIDE DEALER.
						game.ParseDeal(tag.Value);
					}
					// TODO: Should tags we parse be added to the dictionary or not?  
					// For the time being i will say yes.
					game.Tags[tag.Name] = tag.Value;
					if (tag.Data.Count > 0)
					{
						game.TagData[tag.Name] = tag.Data;
					}
				}
            }

			if (game != null)
			{
				games.Add(game);
			}

            return games;
        }

*/



        internal static List<PBNTag> TokenizeTags(string text)
        {
            var tag = new PBNTag { Data = new List<string>() };
            var tags = new List<PBNTag>();
            var lines = text.Split(new[] { "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("%") && !line.StartsWith(";"));
            foreach (var line in lines)
                if (line.StartsWith("["))
                {
                    tag = new PBNTag { Data = new List<string>() };
                    tags.Add(tag);
                    tag.Name = line.Substring(1, line.IndexOf(' ') - 1);
                    var start = line.IndexOf('"') + 1;
                    var end = line.LastIndexOf('"') - start;
                    tag.Value = line.Substring(start, end);
                }
                else
                {
                    tag.Data.Add(line);
                }

            return tags;
        }

        internal class PBNTag
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public List<string> Data { get; set; }
        }
	}
}