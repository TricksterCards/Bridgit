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
    public class TestBridgeBidder
    {
        public static IEnumerable<object[]> SAYCTestData
        {
            get
            {
                var result = new List<PBNTest>();
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var files = Directory.GetFiles(Path.Combine(dir, "SAYC"), "*.pbn");
                foreach (var file in files)
                {
                    try
                    {
                        var filename = Path.GetFileName(file);
                        var text = File.ReadAllText(file);
                        var tests = PBN.ImportTests(text);

                        foreach (var test in tests)
                        {
                            test.Name = $"{filename}: {test.Name}";
                            result.Add(test);
                        }
                    } catch (Exception e)
                    {
                        var test = new PBNTest { LoadError = e, Name = Path.GetFileName(file) };
                        result.Add(test);
                    } 
                }
                return result.Select(r => new object[] { r }).ToArray();
            }
        }

        public static string GetSAYCTestDataDisplayName(MethodInfo _, object[] data)
        {
            return (data[0] as PBNTest).Name;
        }

        [TestMethod]
        [DynamicData(nameof(SAYCTestData), DynamicDataDisplayName=nameof(GetSAYCTestDataDisplayName))]
        public void RunSAYCTests(PBNTest test)
        {
            if (test.LoadError != null)
            {
                Assert.Fail($"File {test.Name} failed due to excpetion {test.LoadError.Message}");
            } 
            else 
            {
                var suggestion = BridgeBidder.SuggestBid(test.Deal, test.Vulnerable, test.Auction);

                Assert.AreEqual(test.Bid, suggestion);
            }
        }

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
    }
}
  