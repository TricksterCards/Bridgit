using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    var filename = Path.GetFileName(file);
                    var text = File.ReadAllText(file);
                    var tests = PBN.ImportTests(text);

                    foreach (var test in tests)
                    {
                        test.Name = $"{filename}: {test.Name}";
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
            //var vul = test.Vulnerable;

            // TODO: ** HUGE MYSTERY *** HOW DO WE EVER GET HERE WITH Vulnerable == NULL?
            // I can not figure out the code path that does this.
            var vul = test.Vulnerable == null ? "None" : test.Vulnerable;
            var suggestion = BridgeBidder.SuggestBid(test.Deal, vul, test.Auction);

            Assert.AreEqual(test.Bid, suggestion);
        }
    }
}
