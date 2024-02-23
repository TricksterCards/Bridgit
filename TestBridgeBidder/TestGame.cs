using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class TestGame
    {

        [TestMethod]
        public void Basic()
        {
            var game = new Game();
            Assert.IsNull(game.Event);
            Assert.AreEqual(game.Vulnerable, Vulnerable.None);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Scoring, Scoring.MP);
            Assert.IsNull(game.Deal[Direction.N]);
            game.DealRandomHands();
            Assert.IsNotNull(game.Deal[Direction.N]);
        }

        [TestMethod]
        public void TestClone()
        {
            var game = new Game();
            game.DealRandomHands();
            game.Dealer = Direction.S;
            game.Vulnerable = Vulnerable.All;
            game.Event = "Test clone";
            game.Board = 42;
            game.DealRandomHands();
            game.Auction.Add(Bid._1H);
            var clone = game.Clone();
            Assert.AreEqual(clone.ToString(), game.ToString());
            clone.Board = 45;
            Assert.AreNotEqual(clone.ToString(), game.ToString());
        }

        [TestMethod]
        public void TestStandardBoard()
        {
            var game = new Game();
            game.SetStandardBoard(1);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.None);
            
            game.SetStandardBoard(2);
            Assert.AreEqual(game.Dealer, Direction.E);
            Assert.AreEqual(game.Vulnerable, Vulnerable.NS);

            game.SetStandardBoard(3);
            Assert.AreEqual(game.Dealer, Direction.S);
            Assert.AreEqual(game.Vulnerable, Vulnerable.EW);

            game.SetStandardBoard(4);
            Assert.AreEqual(game.Dealer, Direction.W);
            Assert.AreEqual(game.Vulnerable, Vulnerable.All);

            // Vulnerabiity shifts -> 1 for every set of 4 boards.
            game.SetStandardBoard(5);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.NS);

            game.SetStandardBoard(6);
            Assert.AreEqual(game.Dealer, Direction.E);
            Assert.AreEqual(game.Vulnerable, Vulnerable.EW);

            game.SetStandardBoard(7);
            Assert.AreEqual(game.Dealer, Direction.S);
            Assert.AreEqual(game.Vulnerable, Vulnerable.All);

            game.SetStandardBoard(8);
            Assert.AreEqual(game.Dealer, Direction.W);
            Assert.AreEqual(game.Vulnerable, Vulnerable.None);

            game.SetStandardBoard(9);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.EW);

            game.SetStandardBoard(13);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.All);

            game.SetStandardBoard(14);
            Assert.AreEqual(game.Dealer, Direction.E);
            Assert.AreEqual(game.Vulnerable, Vulnerable.None);

            // Boards repeat every 16, so back to beginning.
            game.SetStandardBoard(17);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.None);
            
            game.SetStandardBoard(18);
            Assert.AreEqual(game.Dealer, Direction.E);
            Assert.AreEqual(game.Vulnerable, Vulnerable.NS);

            game.SetStandardBoard(19);
            Assert.AreEqual(game.Dealer, Direction.S);
            Assert.AreEqual(game.Vulnerable, Vulnerable.EW);

            game.SetStandardBoard(20);
            Assert.AreEqual(game.Dealer, Direction.W);
            Assert.AreEqual(game.Vulnerable, Vulnerable.All);   

            game.SetStandardBoard(21);
            Assert.AreEqual(game.Dealer, Direction.N);
            Assert.AreEqual(game.Vulnerable, Vulnerable.NS);   
        }

        // Test the game auction
        [TestMethod]
        public void TestAuction()
        {
            var game = new Game();
            game.SetStandardBoard(1);
            Assert.AreEqual(game.Dealer, Direction.N);
            game.Auction.Add(Bid._1H);
            game.Auction.Add(Call.Pass);
            game.Auction.Add(Call.Pass);
            game.Auction.Add(Call.Pass);
            Assert.AreEqual(game.Auction.Count, 4);
            Assert.AreEqual(game.Auction[0].Call, Bid._1H);
            Assert.AreEqual(game.Auction[1].Call, Call.Pass);
            Assert.AreEqual(game.Auction[2].Call, Call.Pass);
            Assert.AreEqual(game.Auction[3].Call, Bid.Pass);
            Assert.AreEqual(game.Auction[0].HasNote, false);
            Assert.AreEqual(game.Auction[1].HasNote, false);
            Assert.AreEqual(game.Auction[2].HasNote, false);
            Assert.AreEqual(game.Auction[3].HasNote, false);
            game.Auction[0].Note = "Note 1";
            game.Auction[2].Note = "Note 2";
            Assert.AreEqual(game.Auction[0].HasNote, true);
            Assert.AreEqual(game.Auction[1].HasNote, false);
            Assert.AreEqual(game.Auction[2].HasNote, true);
            Assert.AreEqual(game.Auction[3].HasNote, false);
            Assert.AreEqual(game.Auction[0].Note, "Note 1");
            Assert.AreEqual(game.Auction[2].Note, "Note 2");
            
            Assert.IsTrue(game.Auction.IsValid());
            game.Auction[2].Call = Call.Double;
            Assert.IsFalse(game.Auction.IsValid());
            game.Auction[2].Call = Bid._2H;
            Assert.IsTrue(game.Auction.IsValid());
            game.Auction[1].Call = Bid._3NT;
            Assert.IsFalse(game.Auction.IsValid());
            game.Auction[2].Call = Call.Pass;
            Assert.IsTrue(game.Auction.IsValid());
        }

    }
}
  