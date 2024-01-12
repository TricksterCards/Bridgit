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
        public static IEnumerable<object[]> SAYCTestData => PBNTest.LoadTests("SAYC");
        public static IEnumerable<object[]> TwoOverOneGameForceData => PBNTest.LoadTests("TwoOverOneGameForce");

        public static string GetDataDisplayName(MethodInfo _, object[] data)
        {
            return (data[0] as PBNTest).Name;
        }

        [TestMethod]
        [DynamicData(nameof(SAYCTestData), DynamicDataDisplayName=nameof(GetDataDisplayName))]      
        public void SAYCTests(PBNTest test)
        {
                var suggestion = BridgeBidder.SuggestBid(test.Deal, test.Vulnerable, test.Auction);

                Assert.AreEqual(test.Bid, suggestion);
        }

        [TestMethod]
        [DynamicData(nameof(TwoOverOneGameForceData), DynamicDataDisplayName=nameof(GetDataDisplayName))]        
        public void TwoOverOneTests(PBNTest test)
        {
                var suggestion = BridgeBidder.SuggestBid(test.Deal, test.Vulnerable, test.Auction);

                Assert.AreEqual(test.Bid, suggestion);
        }

    }
}
  