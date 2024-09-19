using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace NewsGenerator
{
    enum MessageType { TITLE, RESULT, MATCHTITLE }
    enum Category { NONE, INTRO, SEMIFINAL, FINAL, ONESIDED, CLOSE, TURN, UNEXPECTED, RETIRED }

    static class Templates
    {
        public static string[] titles { get; private set; }
        public static string[] results { get; private set; }
        public static string[] sets { get; private set; }

        private const string TITLES_FILE_NAME = "titles.json";
        private const string RESULTS_FILE_NAME = "results.json";
        private const string SETS_FILE_NAME = "sets.json";
        private const string DIRECTORY_NAME = "templates";

        // Info for choosing templates
        public static HashSet<int>[] usedIndexes { get; set; } = new HashSet<int>[Enum.GetNames(typeof(MessageType)).Length];
        public static int[] tries { get; set; } = new int[Enum.GetNames(typeof(MessageType)).Length];
        public static int triesLimit { get; } = 500;
        public static Random random { get; } = new Random();


        public static bool Init()
        {
            bool areTemplatesLoaded = LoadFiles($"data{Path.DirectorySeparatorChar}{DIRECTORY_NAME}{Path.DirectorySeparatorChar}", TITLES_FILE_NAME, RESULTS_FILE_NAME, SETS_FILE_NAME);

            for (int i = 0; i < usedIndexes.Length; i++)
            {
                usedIndexes[i] = new HashSet<int>();
            }
            return areTemplatesLoaded;
        }

        private static bool LoadFiles(string path, string titlesFileName, string resultsFileName, string matchTitlesFileName)
        {
            string titlesPath = $"{path}{titlesFileName}";
            string resultsPath = $"{path}{resultsFileName}";
            string setsPath = $"{path}{matchTitlesFileName}";

            string inputError = "Chyba šablon";
            if (!File.Exists(titlesPath))
            {
                MessageBox.Show("Chyba: soubor pro šablony titulků nenalezen", inputError);
                return false;
            }
            if (!File.Exists(resultsPath))
            {
                MessageBox.Show("Chyba: soubor pro šablony výsledků nenalezen", inputError);
                return false;
            }
            if (!File.Exists(setsPath))
            {
                MessageBox.Show("Chyba: soubor pro šablony setů nenalezen", inputError);
                return false;
            }

            try
            {
                titles = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(titlesPath));
                results = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(resultsPath));
                sets = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(setsPath));
            }
            catch (IOException)
            {
                MessageBox.Show("Chyba při načítání souboru šablon");
                return false;
            }
            return true;
        }

        public static void ResetTries(MessageType messageType)
        {
            // For sets we have enough categories
            // No need to disable them
            if (messageType == MessageType.TITLE || messageType == MessageType.RESULT)
            {
                Categories.Change(messageType);
            }
            usedIndexes[(int)messageType].Clear();
            tries[(int)messageType] = 0;
        }

        public static void ResetAllTries()
        {
            for (int i = 0; i < usedIndexes.Length; i++)
            {
                usedIndexes[i].Clear();
            }
            for (int i = 0; i < tries.Length; i++)
            {
                tries[i] = 0;
            }
        }

        public static int GetTemplateCount(MessageType type)
        {
            if (type == MessageType.TITLE)
            {
                return titles.Length;
            }
            else if (type == MessageType.RESULT)
            {
                return results.Length;
            }
            else if (type == MessageType.MATCHTITLE)
            {
                return sets.Length;
            }
            else
            {
                MessageBox.Show("Nepodporovaná část článku");
                return 0;
            }
        }

        public static bool GetTemplate(MessageType type, int index, out string template, out bool[] templateCategories)
        {
            if (type == MessageType.TITLE)
            {
                string[] tokens = titles[index].Split('\n');
                template = tokens[0];
                templateCategories = Categories.Parse(tokens[1]);
                return true;
            }
            else if (type == MessageType.RESULT)
            {
                string[] tokens = results[index].Split('\n');
                template = tokens[0];
                templateCategories = Categories.Parse(tokens[1]);
                return true;
            }
            else if (type == MessageType.MATCHTITLE)
            {
                string[] tokens = sets[index].Split('\n');
                template = tokens[0];
                templateCategories = Categories.Parse(tokens[1]);
                return true;
            }
            else
            {
                template = "";
                templateCategories = null;
                Console.WriteLine("Unsupported message type");
                return false;
            }
        }
    }
}