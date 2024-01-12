using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestBridgeBidder
{
    public class PBNTest
    {
        public string Bid { get; set; }
        public string Deal { get; set; }
        public string Vulnerable { get; set; }
        public string Auction { get; set; }
        public string Name { get; set; }


        public static IEnumerable<object[]> LoadTests(string subdirectory)
        {
            var result = new List<PBNTest>();
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var files = Directory.GetFiles(Path.Combine(dir, subdirectory), "*.pbn");
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var text = File.ReadAllText(file);
                try
                {
                    var tests = PBN.ImportTests(text);
                    var usedNames = new HashSet<string>();
                    foreach (var test in tests)
                    {
                        test.Name = $"{filename}: {test.Name}";
                        while (usedNames.Contains(test.Name))
                        {
                            test.Name += "*DUP*";
                        }
                        usedNames.Add(test.Name);
                        result.Add(test);
                    }
                } catch (Exception e)
                {
                    var test = new PBNTest { Name = $"File {filename} failed to load:  Excpetion {e}" };
                    result.Add(test);
                } 
            }
            return result.Select(r => new object[] { r }).ToArray();
        }

        public static string GetDataDisplayName(MethodInfo _, object[] data)
        {
            return (data[0] as PBNTest).Name;
        }

    }
}
