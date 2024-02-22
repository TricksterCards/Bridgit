using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;

using BridgeBidding;
using BridgeBidding.PBN;
using System.Text;

namespace bridgit;

public class GameFile: List<Game>
{
    public string FilePath;
    public string FileName;

    public bool Loaded = false;

    public GameFile(string filePath)
    {
        this.FilePath = filePath;
        this.FileName = Path.GetFileNameWithoutExtension(filePath);
    }

    // TODO: For now we will just load the file.  
    public void Load()
    {
        var text = File.ReadAllText(FilePath); 
        this.Clear();
        this.AddRange(FromString.ParseGames(text));       
    }

    public void Save()
    {
        var sb = new StringBuilder();
        foreach (var game in this)
        {
            sb.Append(game.ToString());
            sb.AppendLine();
        }
        Console.WriteLine("*** FOR SAFETY JUST PRINTING OUT GAME FILE CONTENTS FIRST! ***");
        Console.WriteLine(sb.ToString());
        Console.ReadLine();
       // File.WriteAllText(FilePath, sb.ToString());
    }

    public void RenumberBoards()
    {
        for (int i = 0; i < this.Count; i++)
        {
            this[i].Board = i+1;
        }
    }

    public static GameFile[] EnumDirectory(string subdirectory)
    {
        var gameFiles = new List<GameFile>();
        var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var dir = Path.Combine(execDir, "..", "..", "..", "..", "TestBridgeBidder");
        dir = Path.GetFullPath(dir);
        var files = Directory.GetFiles(Path.Combine(dir, subdirectory), "*.pbn");
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            gameFiles.Add(new GameFile(file));
        }
        return gameFiles.OrderBy(g => g.FileName).ToArray();
    }

}