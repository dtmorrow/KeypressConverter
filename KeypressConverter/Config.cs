using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace KeypressConverter
{
    class Config
    {
        // KeyConfig.txt should remain in same folder as executable
        internal static string KeyConfigPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\" + "KeyConfig.txt";
        internal static ConvertedKey[] ConvertedKeys;

        /// <summary>
        /// Creates KeyConfig.txt with all keys commented out 
        /// </summary>
        internal static void CreateKeyConfig()
        {
            List<string> config = new List<string>();
            config.Add("# List of all key names from Keys enum. Uncomment to convert key. Not all keys may work.");
            config.Add("# Append a space followed by a positive integer to the key to delay the KeyUp event for that many milliseconds.");
            string[] allKeys = Enum.GetNames(typeof(Keys));
            Array.Sort(allKeys);
            foreach (var key in allKeys)
            {
                config.Add("#" + key);
            }
            File.WriteAllLines(KeyConfigPath, config);
        }

        /// <summary>
        /// Reads uncommented keys from KeyConfig.txt 
        /// </summary>
        /// <returns>Keys that need to be converted</returns>
        internal static ConvertedKey[] ReadKeyConfig()
        {
            List<ConvertedKey> keys = new List<ConvertedKey>();
            string[] lines = File.ReadAllLines(KeyConfigPath);

            foreach (var line in lines)
            {
                if (!line.StartsWith("#")) // Only parse uncommented lines
                {
                    string[] split = line.Trim().Split(' ');

                    if (split.Length == 0) // If no tokens on line, skip
                    {
                        continue;
                    }

                    ConvertedKey newKey = new ConvertedKey();

                    Keys key;
                    int delay;

                    if (Enum.TryParse(split[0], out key)) // If key can't be parsed, just ignore it.
                    {
                        newKey.Key = key;
                        newKey.Delay = 0;

                        if (split.Length > 1 && int.TryParse(split[1], out delay) && delay > 0) // If delay is invalid or less than 0, don't set it
                        {
                            newKey.Delay = delay;
                        }

                        keys.Add(newKey);
                    }
                }
            }

            return keys.ToArray();
        }
    }
}
