using Newtonsoft.Json;
using System;
using System.IO;


namespace CTKNewsParser
{
    static class BlackLists
    {
        private static string blackListsPath = $"data{Path.DirectorySeparatorChar}blackLists{Path.DirectorySeparatorChar}";

        private const string KEY_WORDS_FILE_NAME = "KWBlackList.json";
        private const string GLOBAL_BLACK_LIST_FILE_NAME = "globalBlackList.json";
        private const string TITLE_BLACK_LIST_FILE_NAME = "titlesBlackList.json";
        private const string TITLE_BUT_CONTINUE_BLACK_LIST_FILE_NAME = "titlesButContinueBlackList.json";
        private const string RESULT_BLACK_LIST_FILE_NAME = "resultsBlackList.json";
        private const string MATCH_BLACK_LIST_FILE_NAME = "matchesBlackList.json";
        private const string TWO_WORDS_PHRASES_BLACK_LIST_FILE_NAME = "TwoWordsPhrasesBlackList.json";

        private static string keyWordsFilePath { get; } = $"{blackListsPath}{KEY_WORDS_FILE_NAME}";
        private static string globalFilePath { get; } = $"{blackListsPath}{GLOBAL_BLACK_LIST_FILE_NAME}";
        private static string titlesFilePath { get; } = $"{blackListsPath}{TITLE_BLACK_LIST_FILE_NAME}";
        private static string titlesButContinueFilePath { get; } = $"{blackListsPath}{TITLE_BUT_CONTINUE_BLACK_LIST_FILE_NAME}";
        private static string resultsFilePath { get; } = $"{blackListsPath}{RESULT_BLACK_LIST_FILE_NAME}";
        private static string matchesFilePath { get; } = $"{blackListsPath}{MATCH_BLACK_LIST_FILE_NAME}";
        private static string twoWordsPhrasesFilePath { get; } = $"{blackListsPath}{TWO_WORDS_PHRASES_BLACK_LIST_FILE_NAME}";

        public static string[] keyWords { get; private set; }
        public static string[] global { get; private set; }
        public static string[] titles { get; private set; }
        public static string[] titlesButContinue { get; private set; } // Do not use title but still check other parts
        public static string[] results { get; private set; }
        public static string[] matches { get; private set; }
        public static Tuple<string, string>[] blackListTwoWordsPhrases { get; private set; }


        public static bool Initialize()
        {
            bool areFilesPresent = CheckFileExistence();
            if (!areFilesPresent)
            {
                return false;
            }

            bool areListsLoaded = LoadFromFiles();
            return areListsLoaded;
        }

        private static bool CheckFileExistence()
        {
            string errorFileNotFound = "Error: File not found: ";
            if (!File.Exists(keyWordsFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{keyWordsFilePath}");
                return false;
            }
            if (!File.Exists(globalFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{globalFilePath}");
                return false;
            }
            if (!File.Exists(titlesFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{titlesFilePath}");
                return false;
            }
            if (!File.Exists(titlesButContinueFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{titlesButContinueFilePath}");
                return false;
            }
            if (!File.Exists(resultsFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{resultsFilePath}");
                return false;
            }
            if (!File.Exists(matchesFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{matchesFilePath}");
                return false;
            }
            if (!File.Exists(twoWordsPhrasesFilePath))
            {
                Console.WriteLine($"{errorFileNotFound}{twoWordsPhrasesFilePath}");
                return false;
            }
            return true;
        }

        private static bool LoadFromFiles()
        {
            try
            {
                using StreamReader keyWordsReader = new StreamReader(keyWordsFilePath);
                {
                    keyWords = JsonConvert.DeserializeObject<string[]>(keyWordsReader.ReadLine());
                }
                using StreamReader globalReader = new StreamReader(globalFilePath);
                {
                    global = JsonConvert.DeserializeObject<string[]>(globalReader.ReadLine());
                }
                using StreamReader titleReader = new StreamReader(titlesFilePath);
                {
                    titles = JsonConvert.DeserializeObject<string[]>(titleReader.ReadLine());
                }
                using StreamReader titleButContinueReader = new StreamReader(titlesButContinueFilePath);
                {
                    titlesButContinue = JsonConvert.DeserializeObject<string[]>(titleButContinueReader.ReadLine());
                }
                using StreamReader resultsReader = new StreamReader(resultsFilePath);
                {
                    results = JsonConvert.DeserializeObject<string[]>(resultsReader.ReadLine());
                }
                using StreamReader matchesReader = new StreamReader(matchesFilePath);
                {
                    matches = JsonConvert.DeserializeObject<string[]>(matchesReader.ReadLine());
                }

                blackListTwoWordsPhrases = JsonConvert.DeserializeObject<Tuple<string, string>[]>(File.ReadAllText(twoWordsPhrasesFilePath));
            }
            catch (Exception exception)
            {
                if (exception is JsonSerializationException || exception is JsonReaderException)
                {
                    Console.WriteLine($"Error while loading black lists: {exception.Message}");
                    return false;
                }

                throw;
            }
            return true;
        }
    }
}
