using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using BridgeBidding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBridgeBidder
{
    [TestClass]
    public class InvalidArguments
    {

        // All subsequent tests are variations of this basic hand.  Various changes are made
        // to throw exceptions.  First make sure the basic sequence of 1H/2H all pass works.
        // Test with various known and unknown hands.
        [TestMethod]
        [DataRow("1H", "N:872.KQJ95.AK.952 - - -", "NS", "")]
        [DataRow("1H", "N:872.KQJ95.AK.952 - - -", "NS", null)]
        [DataRow("2H", "N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H Pass")]
        [DataRow("2H", "N:- - 953.T42.QJ4.AQ73 -", "NS", "1H Pass")]
        [DataRow("Pass", "N:872.KQJ95.AK.952 - - -", "NS", "1H Pass 2H Pass")]
        public void TestSuggestBid(string expected, string deal, string vulnerable, string auction)
        {
            var suggestion = BridgeBidder.SuggestBid(deal, vulnerable, auction);  
            Assert.AreEqual(expected, suggestion);
        }

        // Now similar tests with invalid null arguments 
        // Invalid parameter tests
        [TestMethod]
        [DataRow(null, "NS", "")]
        [DataRow("N:872.KQJ95.AK.952 - - -", null, "")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullParams(string deal, string vulnerable, string auction)
        {
            var suggestion = BridgeBidder.SuggestBid(deal, vulnerable, auction);  
        }


        [TestMethod]
        [DataRow("X:872.KQJ95.AK.952 - - -", "NS", "", DisplayName = "Invalid dealer X")]
        [DataRow("N 872.KQJ95.AK.952 - - -", "NS", "", DisplayName = "Invalid dealer tag, no ':'")]
        [DataRow("N:72.KQJ95.AK.952 - - -", "NS", "", DisplayName = "Too few cards in hand")]
        [DataRow("N:872.KQJ95.AK952 - - -", "NS", "", DisplayName = "Only three suits in hand")]
        [DataRow("N:872.KQJ95.AK.952 - 853.T42.QJ4.AQ73 -", "NS", "1H Pass", DisplayName = "Duplicate 8S in north and south hands")]
        [DataRow("N:872.KQJ95.AK.952 - - -", "Somebody", "", DisplayName = "Invalid vulnerable")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "0H Pass", DisplayName = "Invalid bid of zero hearts.")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H 8D", DisplayName = "Invalid bid of eight diamonds.")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H Joker", DisplayName = "Invalid call type.")]

        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidParams(string deal, string vulnerable, string auction)
        {
            var suggestion = BridgeBidder.SuggestBid(deal, vulnerable, auction);  
        }

        [TestMethod]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H 1D", DisplayName = "Can't bid lower than current contract")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H Pass X", DisplayName = "Can't double your partner")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H Pass 2H Pass Pass Pass Pass", DisplayName = "Too may passes")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H Pass XX", DisplayName = "Can't redouble if not doubled")]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H 7NT X XX Pass", DisplayName = "7NT XX ends auction")]
        [ExpectedException(typeof(AuctionException))]
        public void TestInvalidAuction(string deal, string vulnerable, string auction)
        {
            var suggestion = BridgeBidder.SuggestBid(deal, vulnerable, auction);  
        }

        [TestMethod]
        [DataRow("N:872.KQJ95.AK.952 - 953.T42.QJ4.AQ73 -", "NS", "1H", DisplayName = "Can't suggest bid if hand not specified.")]
        [ExpectedException(typeof(AuctionException))]
        public void TestNoHandNoSuggestion(string deal, string vulnerable, string auction)
        {
            var suggestion = BridgeBidder.SuggestBid(deal, vulnerable, auction);  
        }

    }
}
  