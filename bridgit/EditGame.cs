using System;
using System.Reflection;
using System.CommandLine;
using System.Diagnostics;

using BridgeBidding;
using BridgeBidding.PBN;

namespace bridgit;

public class EditGame
{
    public static void TryItOut()
    {
        try
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.UseShellExecute = true;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "Open.pbn";
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.StartInfo.WorkingDirectory = "/Users/ralphlipe/Trickster/BridgeBidding/TestBridgeBidder/TwoOverOneGameForce";
                myProcess.StartInfo.Arguments = "";
                myProcess.Start();
                // This code assumes the process you are starting will terminate itself.
                // Given that it is started without a window so you cannot terminate it
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.
                myProcess.WaitForExit(10000000);
                Console.WriteLine("PROCESS HAS TERMINATED??");
                Console.WriteLine(myProcess.HasExited);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}