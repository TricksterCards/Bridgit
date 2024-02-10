using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;


namespace BridgeBidding.PBN
{
    public static class FromString
    {
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
					if (tag.Name == "Board") 
					{
						int.TryParse(tag.Value, out game.BoardNumber);
					}
					if (tag.Name == "Deal")
					{
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





        private static List<PBNTag> TokenizeTags(string text)
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

        private class PBNTag
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public List<string> Data { get; set; }
        }
	}
}