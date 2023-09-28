using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using RandomDataGenerator;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace MyFirstPlugin
{
    static internal class Generator
    {
        private static UInt32 pid = 100000;

        public static string Seed { get; set; } = "";

        public static Random Random { get; private set; } = new Random();

        /// <summary>
        /// Get's the nth element from an enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T Pick<T>(IEnumerable<T> collection) =>
            collection.ElementAt(Random.Next(collection.Count()));

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
            DateTime.Now.ToString("M_dd");
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

            WriteSeed();
            Reload();
        }

        /// <summary>
        /// Reload the generators
        /// </summary>
        public static void Reload()
        {
            Random = new Random(GetHashCode(Seed));

            RandomizerFactory.GetRandomizer(new FieldOptionsFirstName
            {
                Seed = GetHashCode(Seed)
            });
        }

        public static void ReadSeed()
        {
            var dir = Path.Combine(Paths.PluginPath, "MyFirstPlugin");
            var path = Path.Combine(dir, "seed.txt");

            // Ensure the directory exists
            Directory.CreateDirectory(dir);
            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    Seed = (sr.ReadLine() ?? "").Trim();
                }
            }
            catch (FileNotFoundException)
            {
                Plugin.Logger.LogInfo("Seed config file not found.");
            }
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
