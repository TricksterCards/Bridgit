using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class TestDirection
    {
        [TestMethod]
        public void TestLeftHandOpponent()
        {
            Assert.AreEqual(Direction.E, Direction.N.LeftHandOpponent());
            Assert.AreEqual(Direction.S, Direction.E.LeftHandOpponent());
            Assert.AreEqual(Direction.W, Direction.S.LeftHandOpponent());
            Assert.AreEqual(Direction.N, Direction.W.LeftHandOpponent());
        }

        [TestMethod]
        public void TestPartner()
        {
            Assert.AreEqual(Direction.N, Direction.S.Partner());
            Assert.AreEqual(Direction.E, Direction.W.Partner());
            Assert.AreEqual(Direction.S, Direction.N.Partner());
            Assert.AreEqual(Direction.W, Direction.E.Partner());
        }

        [TestMethod]
        public void TestRightHandOpponent()
        {
            Assert.AreEqual(Direction.W, Direction.N.RightHandOpponent());
            Assert.AreEqual(Direction.N, Direction.E.RightHandOpponent());
            Assert.AreEqual(Direction.E, Direction.S.RightHandOpponent());
            Assert.AreEqual(Direction.S, Direction.W.RightHandOpponent());
        }

        [TestMethod]
        public void TestPair()
        {
            Assert.AreEqual(Pair.NS, Direction.N.Pair());
            Assert.AreEqual(Pair.EW, Direction.E.Pair());
            Assert.AreEqual(Pair.NS, Direction.S.Pair());
            Assert.AreEqual(Pair.EW, Direction.W.Pair());
        }
    }
}