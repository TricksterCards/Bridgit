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
            var clone = game.Clone();
            Assert.AreEqual(clone.ToString(), game.ToString());
            clone.Board = 45;
            Assert.AreNotEqual(clone.ToString(), game.ToString());
        }

    }
}
  