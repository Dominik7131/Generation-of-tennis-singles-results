using System;
using System.IO;


namespace StatsAnalyzer
{
    class Program
    {
        private static string inputFilePath;
        private static string resultsFilePath;
        private static string statsFilePath;

        private static string defaultResultsFilePath = $"data{Path.DirectorySeparatorChar}match_scores_1991-2016_unindexed_csv.csv";
        private static string defaultStatsFilePath = $"data{Path.DirectorySeparatorChar}match_stats_1991-2016_unindexed_csv.csv";

        private static bool isScoreSelected;
        private static bool isDurationSelected;
        private static bool isAcesSelected;

        private const string SCORE_FLAG = "-score";
        private const string DURATION_FLAG = "-duration";
        private const string ACES_FLAG = "-ace";

        static void Main(string[] args)
        {
            bool areArgumentsValid = ProcessArguments(args);
            if (!areArgumentsValid)
            {
                return;
            }

            if (resultsFilePath == null || statsFilePath == null)
            {
                LoadMatchFiles();
            }

            Analyzer analyzer = new Analyzer(inputFilePath, resultsFilePath, statsFilePath);
            try
            {
                if (isScoreSelected)
                {
                    analyzer.AnalyzeScore();
                }
                else if (isDurationSelected)
                {
                    analyzer.CountAverageMixedDuration();
                }
                else if (isAcesSelected)
                {
                    analyzer.CountAces();
                }
            }
            catch (IOException)
            {
                Console.WriteLine("File error");
            }
        }

        private static bool ProcessArguments(string[] arguments)
        {
            if (arguments.Length < 1 || arguments.Length > 3)
            {
                Console.WriteLine("Invalid arguments length");
                PrintUsage();
                return false;
            }

            int maxArgsLength = 0;
            if (arguments[0].StartsWith('-'))
            {
                if (arguments[0] == SCORE_FLAG)
                {
                    isScoreSelected = true;
                    maxArgsLength = 2; // Flag, input file path
                }
                else if (arguments[0] == DURATION_FLAG)
                {
                    isDurationSelected = true;
                    maxArgsLength = 3; // Flag, results and stats file paths
                }
                else if (arguments[0] == ACES_FLAG)
                {
                    isAcesSelected = true;
                    maxArgsLength = 2; // Flag, stats file path
                }
            }
            
            if (maxArgsLength == 0)
            {
                Console.WriteLine("Invalid arguments");
                PrintUsage();
                return false;
            }

            int inputFilePathIndex = 1;
            if (isScoreSelected)
            {
                inputFilePath = arguments[inputFilePathIndex];

                if (!File.Exists(inputFilePath))
                {
                    Console.WriteLine($"Error: File not found: {inputFilePath}");
                    return false;
                }
            }

            if (isAcesSelected)
            {
                if (arguments.Length > 2)
                {
                    Console.WriteLine("Too many arguments");
                    PrintUsage();
                    return false;
                }

                if (arguments.Length == maxArgsLength)
                {
                    statsFilePath = arguments[inputFilePathIndex];

                    if (!File.Exists(statsFilePath))
                    {
                        Console.WriteLine($"Error: File not found: {statsFilePath}");
                        return false;
                    }
                }

            }
            else if (isDurationSelected && arguments.Length == maxArgsLength)
            {
                resultsFilePath = arguments[inputFilePathIndex];
                statsFilePath = arguments[inputFilePathIndex + 1];

                if (!File.Exists(resultsFilePath))
                {
                    Console.WriteLine($"Error: File not found: {resultsFilePath}");
                    return false;
                }
                if (!File.Exists(statsFilePath))
                {
                    Console.WriteLine($"Error: File not found: {statsFilePath}");
                    return false;
                }
            }
            return true;
        }

        private static bool LoadMatchFiles()
        {
            if (resultsFilePath == null)
            {
                if (!File.Exists(defaultResultsFilePath))
                {
                    Console.WriteLine($"Error: File not found: {defaultResultsFilePath}");
                    return false;
                }
                resultsFilePath = defaultResultsFilePath;
            }

            if (statsFilePath == null)
            {
                if (!File.Exists(defaultStatsFilePath))
                {
                    Console.WriteLine($"Error: File not found: {defaultStatsFilePath}");
                    return false;
                }
                statsFilePath = defaultStatsFilePath;
            }
            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Available arguments:");
            Console.WriteLine($"{SCORE_FLAG} [FILE] \t Analyzes score from the given input file");
            Console.WriteLine($"{DURATION_FLAG} (SCORES) (STATS) \t Calculates average match length");
            Console.WriteLine($"{ACES_FLAG} (STATS) \t Analyzes probability of hitting an ace from first serve");
        }
    }
}