using System;
using System.Collections.Generic;
using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class TestHand
    {

        [TestMethod]
        public void Basic()
        {
            var hand = Hand.Parse("AKQ.654.543.AKQJ");
            Assert.AreEqual(19, hand.HighCardPoints());
            Assert.IsTrue(hand.IsBalanced);
            Assert.IsTrue(hand.Is4333);
            Assert.AreEqual(4, hand.CountsBySuit()[Suit.Clubs]);
            Assert.AreEqual(6, hand.Losers());
            Assert.AreEqual(0, hand.Losers(Suit.Spades));
        }

    }
}