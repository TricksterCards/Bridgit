using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;

using BridgeBidding;
using BridgeBidding.PBN;
using System.Text;


namespace bridgit;

public class TestFile: List<Game>
{
    public string FilePath;
    public string FileName;

    public List<Game> FailingTests = new List<Game>();


    public bool Loaded = false;


    public static TestFile CreateNew(string path, string fileName)
    {
        var filePath = Path.Combine(path, $"{fileName}.pbn");
        return new TestFile(filePath);
    }

    public static bool FileExists(string path, string fileName)
    {
        var filePath = Path.Combine(path, $"{fileName}.pbn");
        return File.Exists(filePath);
    }

    private TestFile(string filePath)
    {
        this.FilePath = filePath;
        this.FileName = Path.GetFileNameWithoutExtension(filePath);
    }

    // TODO: For now we will just load the file.  
    public void Load()
    {
        FailingTests.Clear();
        var text = File.ReadAllText(FilePath); 
        this.Clear();
        this.AddRange(FromString.ParseGames(text)); 
        foreach (var game in this)
        {
            UpdateStatus(game);
        }      
    }

    public void UpdateStatus(Game game)
    {
        if (AuctionPasses(game))
        {
            if (FailingTests.Contains(game))
            {
                FailingTests.Remove(game);
            }
        }
        else
        {
            if (!FailingTests.Contains(game))
            {
                FailingTests.Add(game);
            }
        }
    }

    public static bool AuctionPasses(Game game)
    {
        var testGame = game.Clone();
        testGame.Auction.Clear();
        var bs = new BiddingState(testGame);
        foreach (var call in game.Auction.Calls)
        {
            var choices = bs.GetCallChoices();
            if (bs.NextToAct.HasHand)
            {
                if (choices.BestCall == null || !choices.BestCall.Call.Equals(call))
                {
                    return false;
                }
            }
            bs.MakeCall(call);
        }
        return true;
    }

    public void Save()
    {
        var sb = new StringBuilder();
        foreach (var game in this)
        {
            sb.Append(game.ToString());
            sb.AppendLine();
        }
        File.WriteAllText(FilePath, sb.ToString());
    }

    public void RenumberBoards()
    {
        for (int i = 0; i < this.Count; i++)
        {
            this[i].Board = i+1;
        }
    }

    public static TestFile[] EnumDirectory(string dir)
    {
        var testFiles = new List<TestFile>();
        var files = Directory.GetFiles(dir, "*.pbn");
        foreach (var file in files)
        {
            testFiles.Add(new TestFile(file));
        }
        return testFiles.OrderBy(g => g.FileName).ToArray();
    }

}