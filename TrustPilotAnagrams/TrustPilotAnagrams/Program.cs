using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

        public static void FourWordsAnagrams(List<string> lineWorker, string anagramWithoutSpaces, int anagramlength)
        {
            var words = WordWithCleanLetters(lineWorker);
            var possbileWrapCombinations = PrintCombinations(anagramlength, 3);
            possbileWrapCombinations.AddRange(PrintCombinations(anagramlength, 4));

            Parallel.ForEach(possbileWrapCombinations, combination =>
            {
                var combinationVal = combination.Split(',').Select(int.Parse).ToArray();
                RecursiveMethod(anagramWithoutSpaces, words, combinationVal, 0, "");
                 
            });
        }

        private static void RecursiveMethod(string anagramWithoutSpaces, Dictionary<int, HashSet<string>> words,
            IReadOnlyList<int> combinationVal, int i, string anagram)
        {
            if (combinationVal.Count == i || CorrectDictionary.Count == CorrectHashDict.Count)
            {
                return;
            }

            var wrapper = words.Where(x => x.Key == combinationVal[i]).Select(x => x.Value)
                .ToList();

            if (!wrapper.Any())
                return;

            foreach (var word in wrapper.First())
            {
                var str = CharsToRemove(word, anagramWithoutSpaces);

                if (str.Length != anagramWithoutSpaces.Length - combinationVal[i])
                {
                    continue;
                }

                if (str.Length == 0)
                {
                    CheckHash(new List<string>()
                    {
                        anagram + " " + word
                    });
                }

                var anagrame = anagram + " " + word;
                var j = i + 1;
                RecursiveMethod(str, words, combinationVal, j, anagrame);
            }
        }

        private static string CharsToRemove(string second, string str)
        {
            foreach (var index in second.Select(s => str.IndexOf(s.ToString(), StringComparison.Ordinal)))
            {
                str = (index < 0)
                    ? str
                    : str.Remove(index, 1);
            }

            return str;
        }

        private static Dictionary<int, HashSet<string>> WordWithCleanLetters(IEnumerable<string> lineWorker)
        {
            var words = lineWorker.GroupBy(s => s.Length, s => s)
                .ToDictionary(k => k.Key, k => new HashSet<string>(k));

            var test = "";
            foreach (var w in words)
            {
                foreach (var letters in w.Value)
                {
                    var cc = letters.ToCharArray();
                    foreach (var c in cc)
                    {
                        if (!test.Contains(c.ToString()))
                        {
                            test += c;
                        } 
                    }
                }
            }

            return words;
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
                    if (CorrectHashDict.TryGetValue(CreateMd5(anag.TrimStart()), out var hashName) && hashName != null &&
                        !CorrectDictionary.ContainsKey(hashName))
                    {
                        CorrectDictionary.Add(hashName, anag.TrimStart());
                    }
                }
            }
        }

        private static bool CompareStringsNew(string letters, string word)
        {
            // return letters.OrderBy(x => x).SequenceEqual(word.OrderBy(x => x));

            foreach (var ix in letters.Select(c => word.IndexOf(c)))
            {
                if (ix < 0)
                    return false;
                word = word.Remove(ix, 1);
            }

            return true;
        }

        private static bool CompareStrings(string letters, string word)
        {
            foreach (var index in word.Select(t => letters.IndexOf(t)))
            {
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
                list.Add(res.Trim());
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