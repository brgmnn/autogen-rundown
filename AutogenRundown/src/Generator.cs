using BepInEx;

namespace AutogenRundown
{
    static internal class Text
    {
        /// <summary>
        /// Converts an integer to its roman numerals
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UnreachableException"></exception>
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException(nameof(number), "insert value between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            return "";
        }
    }

    static internal class Generator
    {
        private static UInt32 pid = 100000;

        public static string DisplaySeed { get; set; } = "";

        public static string Seed { get; set; } = "";

        public static Random Random { get; private set; } = new Random();

        /// <summary>
        /// Coin flip with a true/false outcome. Optional chance param
        /// </summary>
        /// <param name="chance"></param>
        /// <returns></returns>
        public static bool Flip(double chance = 0.5)
            => Random.NextDouble() < chance;

        /// <summary>
        /// Get's the nth element from an enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns>Returns null if no element can be picked (empty collection)</returns>
        public static T? Pick<T>(IEnumerable<T> collection) =>
            collection.Count() > 0 ? collection.ElementAt(Random.Next(collection.Count())) : default(T);

        /// <summary>
        /// Pick()'s an item from a collection and then removes it so it may not be subsequently drawn
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T Draw<T>(ICollection<T> collection)
        {
            var item = Pick<T>(collection);
            collection.Remove(item);
            return item;
        }

        public interface ISelectable
        {
            public double Weight { get; set; }
        }

        /// <summary>
        /// Randomly selects from a collection of items weighted by their individual properties Weight.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T Select<T>(ICollection<T> collection) where T : ISelectable
        {
            var totalWeight = collection.Sum(x => x.Weight);
            var rand = Random.NextDouble();
            var randomWeight = rand * totalWeight;

            double weightSum = 0;

            foreach (var item in collection)
            {
                weightSum += item.Weight;

                if (randomWeight <= weightSum)
                    return item;
            }

            return collection.Last();
        }

        /// <summary>
        /// Gets a new persistent Id
        /// </summary>
        /// <returns></returns>
        public static UInt32 GetPersistentId() => pid++;

        /// <summary>
        /// Sets the seed (without saving) to the 
        /// </summary>
        public static void GenerateTimeSeed()
        {
            DateTime utcNow = DateTime.UtcNow;

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pst = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tzi);

            var now = pst.ToString("yyyy_MM_dd");
            var display = pst.ToString("MM/dd");

            Seed = now;
            DisplaySeed = $"<color=orange>{display}</color>";
        }

        /// <summary>
        /// Regenerates the seed value and then reload the generators
        /// </summary>
        public static void RegenerateSeed()
        {
            string GetWord()
            {
                var word = Pick(GeneratorData.Words.SeedWords);
                return word.Substring(0, 1).ToUpper() + word.Substring(1);
            }

            Seed = $"{GetWord()}_{GetWord()}_{GetWord()}";
            DisplaySeed = Seed;

            WriteSeed();
            Reload();
        }

        /// <summary>
        /// Reload the generators
        /// </summary>
        public static void Reload()
        {
            Random = new Random(GetHashCode(Seed));
        }

        public static void ReadOrSetSeed(string seed = "")
        {
            if (seed != "")
            {
                Seed = seed;
                return;
            }

            GenerateTimeSeed();
        }

        public static void WriteSeed()
        {
            var dir = Path.Combine(Paths.PluginPath, "MyFirstPlugin");
            var path = Path.Combine(dir, "seed.txt");

            // Ensure the directory exists
            Directory.CreateDirectory(dir);

            using (FileStream fs = File.Create(path))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(Seed);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static int GetHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
