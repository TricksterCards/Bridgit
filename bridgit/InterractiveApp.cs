using System;
using System.Diagnostics;
using BridgeBidding;
using BridgeBidding.PBN;
using System.Text;
using System.Data;
using System.Reflection;

namespace bridgit;

public class InterractiveApp
{



    private static string TestDirPath(params string[] dirs)
    {
        var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (execDir == null) throw new Exception("Could not find executing directory");
        var dir = Path.Combine(execDir, "..", "..", "..", "..", "TestBridgeBidder");
        foreach (var d in dirs)
        {
            dir = Path.Combine(dir, d);
        }
        return Path.GetFullPath(dir);
    }
    
    public static string PassingTestsDirectory = TestDirPath("TwoOverOneGameForce");
    public static string NewTestsDirectory = TestDirPath("LCStandard", "New");


    private TestFile? _testFile = null;

    private int _selectedGame = 0;

    public static void Show()
    {
        var app = new InterractiveApp();
        Console.Title = "Bridgit Command Line Test Tool";
        app.RunCommandLoop();
    }

    private string PromptForInput(string prompt, params string[] valid)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input != null)
            {
                input = input.ToUpper();
                if (valid.Contains(input)) return input;
            }
        }
    }

    private bool Confirm(string prompt)
    {
        return (PromptForInput(prompt, "Y", "N") == "Y");
    }

    private void RunCommandLoop()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Bridgit Test Editor");
            Console.WriteLine();
            Console.WriteLine("L: Load a test file");
            Console.WriteLine("C: Create a new test file");
            Console.WriteLine("Q: Quit this program");
            var input = PromptForInput("Command: ", "L", "C", "Q");
            switch (input)
            {
                case "L":
                    LoadTests();
                    break;
                case "C":
                    CreateTests();
                    break;
                case "Q": return;
            }
        }
    }


    private int ReadPostiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            int i;
            if (int.TryParse(input, out i) && i > 0) return i;
        }
    }

    private bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (Y/N)? ");
            var input = Console.ReadLine();
            if (input != null)
            {
                input = input.ToUpper();
                if (input == "Y") return true;
                if (input == "N") return false;
            }
        }
    }

    private List<Call> ReadAuction(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            var game = new Game();
            game.ParseAuction(input);
            return game.Auction.Calls;
        }
    }

    private List<Call> ReadCalls(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            var error = false;
            var calls = new List<Call>();
            if (!string.IsNullOrEmpty(input))
            {
                var callStrings = input.Split(' ');
                foreach (var c in callStrings)
                {
                    Call call;
                    if (Call.TryParse(c, out call))
                    {
                        calls.Add(call);
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            if (!error) return calls;
        }
    }

    private void CreateTests()
    {
        Console.Clear();
        Console.WriteLine("Create a new test file");
        Console.WriteLine();
        
        var initialAuction = ReadAuction("Initial auction: ");
        var desiredCalls = ReadCalls("Desired calls (leave blank for any): ");
        string error;
        if (!CreateTest.IsValidAuction(initialAuction, desiredCalls, out error))
        {
            Console.WriteLine($"Invalid auction: {error}");
            Console.ReadLine();
            return;
        }
        bool singleOnly = ReadYesNo("Single hand only");
        bool completeAuction = false;
        if (!singleOnly)
        {
            completeAuction = ReadYesNo("Complete auction");
        }

        var numTests = ReadPostiveInt(desiredCalls.Count == 0 ? "Number of tests: " : "Number of tests (per call): ");

        var fileName = string.Join(' ', initialAuction.Select(c => c.ToString()));
        if (fileName == "") fileName = "Open";

        Console.Write($"Filename (blank for default \"{fileName}\"): ");
        var input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input)) fileName = input;

        if (TestFile.FileExists(NewTestsDirectory, fileName))
        {
            if (!Confirm("File already exists.  Overwrite (Y/N)? "))
            {
                return;
            }
        }
        var testFile = TestFile.CreateNew(NewTestsDirectory, fileName);

        if (desiredCalls.Count == 0)
        {
            for (int i = 0; i < numTests; i++)
            {
                testFile.Add(CreateTest.NewTest(i+1, singleOnly, initialAuction, null, completeAuction));
            }
        }
        else
        {
            int boardNumber = 1;
            foreach (var call in desiredCalls)
            {
                for (int i = 0; i < numTests; i++)
                {                
                    testFile.Add(CreateTest.NewTest(boardNumber, singleOnly, initialAuction, call, completeAuction));
                    boardNumber++;
                }
            }
        }

        this._testFile = testFile;
        this._selectedGame = 0;
        EditGameFile();
    }

    private void LoadTests()
    {
        Console.Clear();
        string type = PromptForInput("N for new tests or P for passing tests: ", "N", "P");
        string dir = type == "N" ? NewTestsDirectory : PassingTestsDirectory;   
        var testFiles = TestFile.EnumDirectory(dir);
        for (int i = 0; i < testFiles.Length; i++)
        {
            Console.WriteLine($"{i + 1, 3}: {testFiles[i].FileName}");
        }
        Console.WriteLine();
        Console.Write("Number of file to load or 0 to exit: ");
        while (true)
        {
            var input = Console.ReadLine();
            if (input == null) input = "";
            int selected;
            if (int.TryParse(input, out selected) && selected >= 0 && selected <= testFiles.Length + 1)
            {
                if (selected == 0) return;
                this._testFile = testFiles[selected - 1];
                this._testFile.Load();
                this._selectedGame = 0;
                EditGameFile();
                return;
            }
        }
    }

    private void EditGameFile()
    {
        if (_testFile == null) throw new InvalidOperationException("No game file loaded");
        while (true)
        {
            Console.Clear();
            if (_testFile.FailingTests.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{_testFile.FailingTests.Count} auction are failing.");
                Console.ResetColor();
            }
            Console.WriteLine($"Editing {_testFile.FileName}, Board {_testFile[_selectedGame].Board}, {_selectedGame + 1} of {_testFile.Count}");
            var testEditor = new TestEditor(_testFile[_selectedGame]);
            testEditor.RunAuctionTest(false);
            Console.WriteLine();

            var input = PromptForInput("E: Edit, D: Delete, A: Add, R: Re-run auction, ?: Details, #: Renumber S: Save, N: Next, Q: Quit ",
                                        "E", "D", "A", "R", "S", "?", "??", "N", "", "Q", "P", "#", "PBN");
            // TOOD: All of the commands...
            switch (input)
            {
                case "Q":
                    return;

                case "PBN":
                    Console.WriteLine();
                    Console.WriteLine(_testFile[_selectedGame].ToString());
                    Console.ReadLine();
                    break;

                case "E":
                    var editor = new TestEditor(_testFile[_selectedGame]);
                    editor.EditAuction();
                    break;

                case "#":
                    _testFile.RenumberBoards();
                    break;

                case "S":
                    if (Confirm("Are you sure you want to save (Y/N)? "))
                    {
                        _testFile.Save();
                        Console.WriteLine("File saved");
                    }
                    break;
                
                case "":
                case "N":
                    _selectedGame += _selectedGame < _testFile.Count - 1 ? 1 : 0;
                    break;

                case "P":
                    _selectedGame -= _selectedGame > 0 ? 1 : 0;
                    break;

                case "D":
                    if (Confirm("Delete this test (Y/N)? "))
                    {
                        // TODO: Need to delete the test here..
                        Console.WriteLine("Should have deleted a test.  Write code Ralph!");
                    }
                    break;

                case "A":
                    Console.WriteLine("Should add test here!!!");
                    break;

                case "?":
                    testEditor.RunAuctionTest(true);
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    break;

                case "??":
                    testEditor.RunAuctionTest(true, true);
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    break;    
            }

          
        }
    }



    private InterractiveApp()
    {

    }

    static void ProcessPbnText(string firstLine)
    {
        var gameText = new StringBuilder();
        gameText.Append(firstLine);
        gameText.Append('\n');
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                var game = Game.Parse(gameText.ToString());
                Console.WriteLine("PARSED IT !!!");
                var editor = new TestEditor(game);
                editor.RunAuctionTest(false);
                return;
            }
            gameText.Append(line);
            gameText.Append('\n');
        }
    }

    public static void BidDeal(Game game)
    {
        Console.WriteLine($"bid --deal {game.Deal}");
        Display.Game(game);
        var bs = new BiddingState(game);
        while (!bs.Contract.AuctionComplete)
        {
            var choices = bs.GetCallChoices();
            var callDetails = choices.BestCall;
            if (callDetails == null)
            {
                if (!bs.GetCallChoices().ContainsKey(Call.Pass))
                {
                    Console.WriteLine("*** OUCH!  NO PASS CHOICE AND NO BEST CALL!");
                }
                Console.WriteLine("ERROR:  No BestCall for call choices.");
                Console.WriteLine("Auction so far...");
                DisplayBiddingState(bs);
                choices = bs.DEBUG_ReEvaluateCallChoices();
                callDetails = choices.BestCall;
                if (callDetails == null)
                {
                    if (choices.ContainsKey(Call.Pass))
                    {
                        callDetails = choices[Call.Pass];
                    }
                    else 
                    {
                        choices.AddPassRule();
                        callDetails = choices[Call.Pass];
                    }
                }
            }
            Console.WriteLine($"{bs.NextToAct.Direction}: {callDetails.Call}");
            foreach (var a in callDetails.Annotations)
            {
                Console.WriteLine($"   {a.Type}: {a.Text}");
            }
            var desc = callDetails.GetCallDescriptions();
            foreach (var descList in desc)
            {
                var description = string.Join(", ", descList);
                Console.WriteLine($"      {description}");
            }
            bs.MakeCall(callDetails);
        }
        DisplayBiddingState(bs);
        Console.WriteLine();
        Console.WriteLine();
        /*


        Console.WriteLine(game.GetGameText());	
        Console.WriteLine("Declarer's know hand summary:");
        Console.WriteLine(bs.Contract.Declarer.PublicHandSummary.ToString());
        Console.WriteLine();
        Console.WriteLine("Dummy's known hand summary:");
        Console.WriteLine(bs.Contract.Declarer.Partner.PublicHandSummary.ToString());
        */                
    }

    private static void DisplayBiddingState(BiddingState bs)
    {
        Display.Game(bs.Game);
        Display.Auction(bs.Game);
    }



}