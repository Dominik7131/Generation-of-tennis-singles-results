using System;
using System.IO;


namespace CTKNewsParser
{
    class Program
    {
        private static string inputFilePath { get; set; }

        private static bool isSeparating { get; set; }
        private static bool isFiltering { get; set; }
        private static bool isCiting { get; set; }

        private const string FILTERING_FLAG = "-filter";
        private const string CITING_FLAG = "-cite";
        private const string SEPARATING_FLAG = "-separate";

        private const string TXT_EXTENSION = ".txt";
        private const string JSON_EXTENSION = ".json";


        static void Main(string[] args)
        {
            bool areArgumentsValid = ProcessArguments(args);
            if (!areArgumentsValid)
            {
                return;
            }

            bool isInitializationValid = BlackLists.Initialize();
            if (!isInitializationValid)
            {
                return;
            }

            try
            {
                if (isFiltering)
                {
                    Console.WriteLine("Filtering news...");
                    string outputFilePath = $"filtered{TXT_EXTENSION}";
                    Filterer newsFilterer = new Filterer(inputFilePath, outputFilePath);
                    newsFilterer.Filter();
                }
                else if (isCiting)
                {
                    Console.WriteLine("Citing news...");
                    string outputFilePath = $"citations{TXT_EXTENSION}";
                    Citator citator = new Citator(inputFilePath, outputFilePath);
                    citator.Cite();
                }
                else if (isSeparating)
                {
                    Console.WriteLine("Processing news...");
                    string outputTitlesPath = $"titles{JSON_EXTENSION}";
                    string outputResultsPath = $"results{JSON_EXTENSION}";
                    string outputMatchPath = $"matches{JSON_EXTENSION}";
                    Utility.MorphoditaTagger.Init();
                    Separator divider = new Separator(inputFilePath, outputTitlesPath, outputResultsPath, outputMatchPath, isDebug: true);
                    divider.DivideArticles(); // Execution time approximately 40 minutes for all filtered news
                }
            }
            catch (IOException)
            {
                Console.WriteLine("File Error");
            }
        }

        private static bool ProcessArguments(string[] arguments)
        {
            if (arguments.Length != 2)
            {
                Console.WriteLine("Invalid arguments length");
                PrintUsage();
                return false;
            }

            inputFilePath = arguments.Length == 1 ? arguments[0] : arguments[1];
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Error: File not found: {inputFilePath}");
                return false;
            }

            if (arguments.Length == 2)
            {
                if (arguments[0] == FILTERING_FLAG)
                {
                    isFiltering = true;
                }
                else if (arguments[0] == CITING_FLAG)
                {
                    isCiting = true;
                }
                else if (arguments[0] == SEPARATING_FLAG)
                {
                    isSeparating = true;
                }
                else
                {
                    Console.WriteLine("Invalid arguments");
                    return false;
                }
            }
            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Available arguments:");
            Console.WriteLine($"{FILTERING_FLAG} [FILE] \t Filters news by keywords");
            Console.WriteLine($"{SEPARATING_FLAG} [FILE] \t Divides news into smaller parts");
            Console.WriteLine($"{CITING_FLAG} [FILE] \t Cites news");
        }
    }
}