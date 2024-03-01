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