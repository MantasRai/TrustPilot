using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TrustPilotAnagrams
{
    public static class Program
    {
        private static readonly Dictionary<string, string> CorrectDictionary = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> CorrectHashDict = new Dictionary<string, string>
        {
            {"e4820b45d2277f3844eac66c903e84be", "Easy"},
            {"23170acc097c24edb98fc5488ab033fe", "Medium"},
            {"665e5bcb0c20062fe8abaaf4628bb154", "Hard"}
        };

        static void Main(string[] args)
        {
            const string anagram = "poultry outwits ants";
            var anagramWithoutSpaces = anagram.Replace(" ", "");

            var wordlistFile = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "wordlist.txt"));

            var anagramlength = anagram.Count(c => !char.IsWhiteSpace(c));
            var listOfChars = anagram.Select(x => new string(new[] { x })).Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct().ToArray();

            var wordlistToWorkWith = wordlistFile.Where(x => x.Length <= anagramlength)
                .Where(x => x.All(c => listOfChars.Contains(c.ToString())));

            var wordlist = wordlistToWorkWith.Where(line => CompareStrings(anagramWithoutSpaces, line))
                .Distinct()
                .ToList();

            ThreeWordsAnagrams(wordlist, anagramWithoutSpaces, anagramlength, listOfChars);
            AlertHashStatus();
            FourWordsAnagrams(wordlist, anagramWithoutSpaces, anagramlength);
            AlertHashStatus(1);

            if (CorrectDictionary.Count == 0)
            {
                Console.WriteLine("Sorry, algorithm could not found any of the correct phrases");
            }

            Console.WriteLine("Application end");
            Console.ReadKey();
        }

        public static void ThreeWordsAnagrams(List<string> lineWorker, string anagramWithoutSpaces, int anagramlength, string[] listOfChars)
        {

            var words = lineWorker.GroupBy(s => s.Length, s => s).ToDictionary(k => k.Key, k => new HashSet<string>(k));
            var possbileWrapCombinations = PrintCombinations(anagramlength, 3);

            foreach (var combination in possbileWrapCombinations)
            {
                var combinationVal = combination.Split(',');

                var firsWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[0])).Select(x => x.Value).ToList();
                var secondWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[1])).Select(x => x.Value).ToList();
                var thirdWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[2])).Select(x => x.Value).ToList();

                var firstWrapperWordDictionary = firsWrapperList[0].Select(s => s)
                    .ToDictionary(k => k,
                        k => new HashSet<string>(secondWrapperList[0]
                                .Where(x => x != k && CompareStrings(anagramWithoutSpaces, x + k)))
                            .ToDictionary(kk => kk,
                                kk => new HashSet<string>(thirdWrapperList[0]
                                    .Where(xx => xx != kk && xx != k && k != kk && CompareStrings(anagramWithoutSpaces, xx + k + kk)))));

                var anagramss = new List<string>();
                foreach (var fff in firstWrapperWordDictionary)
                {
                    if (fff.Value.Count <= 0) continue;
                    foreach (var ff in fff.Value)
                    {
                        if (ff.Value.Count <= 0) continue;
                        anagramss.AddRange(ff.Value.Select(f => fff.Key + " " + ff.Key + " " + f));
                    }
                }
                CheckHash(anagramss);
            }
        }

        public static void FourWordsAnagrams(List<string> lineWorker, string anagramWithoutSpaces, int anagramlength)
        {
            var words = lineWorker.GroupBy(s => s.Length, s => s).ToDictionary(k => k.Key, k => new HashSet<string>(k));
            var possbileWrapCombinations = PrintCombinations(anagramlength, 4);

            foreach (var combination in possbileWrapCombinations)
            {
                var combinationVal = combination.Split(',');

                var firsWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[0])).Select(x => x.Value).ToList();
                var secondWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[1])).Select(x => x.Value).ToList();
                var thirdWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[2])).Select(x => x.Value).ToList();
                var fourthWrapperList = words.Where(x => x.Key == int.Parse(combinationVal[3])).Select(x => x.Value).ToList();

                var firstWrapperWordDictionary = firsWrapperList[0].Select(s => s)
                    .ToDictionary(k => k,
                        k => new HashSet<string>(secondWrapperList[0]
                            .Where(x => x != k && CompareStrings(anagramWithoutSpaces, x + k))));

                Parallel.ForEach(firstWrapperWordDictionary, firstWrapp =>
                {
                    var anagrams = new List<string>();
                    foreach (var secondWrapp in firstWrapp.Value)
                    {
                        foreach (var thirdWrapp in thirdWrapperList[0])
                        {
                            if (!CompareStrings(anagramWithoutSpaces,
                                firstWrapp.Key + secondWrapp + thirdWrapp)) continue;

                            var wrapper2 = fourthWrapperList[0].ToList();
                            wrapper2.RemoveAll(x => !CompareStrings(anagramWithoutSpaces, firstWrapp.Key + secondWrapp + thirdWrapp + x));

                            anagrams.AddRange(wrapper2.Select(fourWrapp => firstWrapp.Key + " " + secondWrapp + " " + thirdWrapp + " " + fourWrapp));
                        }
                    }
                    CheckHash(anagrams);
                });
            }
        }

        public static void AlertHashStatus(int finalResult = 0)
        {
            if (finalResult == 0)
            {
                foreach (var hash in CorrectHashDict)
                {
                    Console.WriteLine(
                        CorrectDictionary.ContainsKey(hash.Value)
                            ? $"Correct anagram phrase for {hash.Value} hash is: {CorrectDictionary[hash.Value]}"
                            : "Still searching for other phrases");
                }
            }
            else
            {
                Console.WriteLine("------------Final alert------------");
                foreach (var hash in CorrectHashDict)
                {
                    if (CorrectDictionary.ContainsKey(hash.Value))
                        Console.WriteLine($"Correct anagram phrase for {hash.Value} hash is: {CorrectDictionary[hash.Value]}");
                }
                Console.WriteLine("-----------------------------------");
            }
        }

        public static void CheckHash(List<string> anagrams)
        {
            foreach (var anagr in anagrams)
            {
                foreach (var anag in PermuteWords(anagr))
                {
                    string hashName;
                    if (CorrectHashDict.TryGetValue(CreateMd5(anag.TrimStart()), out hashName) && hashName != null &&
                        !CorrectDictionary.ContainsKey(hashName))
                    {
                        CorrectDictionary.Add(hashName, anag.TrimStart());
                    }
                }
            }
        }

        private static bool CompareStrings(string letters, string word)
        {
            foreach (var t in word)
            {
                var index = letters.IndexOf(t);
                if (index == -1)
                {
                    return false;
                }
                letters = letters.Substring(0, index) + letters.Substring(index + 1);
            }
            return true;
        }

        public static List<string> PermuteWords(string word)
        {
            var wordArray = word.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var usedWord = new bool[wordArray.Length];
            const string res = "";
            var list = new List<string>();
            Permute(wordArray, usedWord, res, 0, list);
            return list;
        }

        private static void Permute(IReadOnlyList<string> ss, IList<bool> used, string res, int level, ICollection<string> list)
        {
            if (level == ss.Count && res != "")
            {
                list.Add(res);
                return;
            }
            for (var i = 0; i < ss.Count; i++)
            {
                if (used[i]) continue;
                used[i] = true;
                Permute(ss, used, res + " " + ss[i], level + 1, list);
                used[i] = false;
            }
        }

        private static string CreateMd5(string s)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(s));

                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static List<string> PrintCombinations(int num, int lenth)
        {
            var allCombinations = BuildCombinations(GetCombinationTrees(num, num));
            var b = allCombinations.Select(c => string.Join(",", c)).ToList();

            var possbileCombinations = b.Where(s => s.TrimEnd(',').Split(',').Length == lenth && s.Split(',').All(x => int.Parse(x) < 12))
                .Select(s => s.TrimEnd(',')).ToList();

            return possbileCombinations;
        }

        internal class Combination
        {
            internal int Num;
            internal IEnumerable<Combination> Combinations;
        }

        internal static IEnumerable<Combination> GetCombinationTrees(int num, int max)
        {
            return Enumerable.Range(1, num)
                .Where(n => n <= max)
                .Select(n =>
                    new Combination
                    {
                        Num = n,
                        Combinations = GetCombinationTrees(num - n, n)
                    });
        }

        internal static IEnumerable<IEnumerable<int>> BuildCombinations(IEnumerable<Combination> combinations)
        {
            if (!combinations.Any())
            {
                return new[] { new int[0] };
            }
            return combinations.SelectMany(c =>
                BuildCombinations(c.Combinations)
                    .Select(l => new[] { c.Num }.Concat(l))
            );
        }
    }
}