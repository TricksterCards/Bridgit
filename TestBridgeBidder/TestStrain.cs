using System.Runtime.CompilerServices;
using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class TestStrain
    {
        [TestMethod]
        public void ToSuit()
        {
            Assert.AreEqual(Suit.Clubs, Strain.Clubs.ToSuit());
            Assert.AreEqual(Suit.Diamonds, Strain.Diamonds.ToSuit());
            Assert.AreEqual(Suit.Hearts, Strain.Hearts.ToSuit());
            Assert.AreEqual(Suit.Spades, Strain.Spades.ToSuit());
            Assert.IsNull(Strain.NoTrump.ToSuit());
        }
    }
}