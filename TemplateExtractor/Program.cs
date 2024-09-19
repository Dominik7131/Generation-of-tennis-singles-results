using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace TemplateExtractor
{
    public enum MessageType { TITLE, RESULT, MATCHTITLE }

    class Program
    {
        private static string inputFilePath { get; set; }
        private static MessageType messageType { get; set; }
        private static bool isDebug { get; set; }

        private const string TITLE_FLAG = "-title";
        private const string RESULT_FLAG = "-result";
        private const string SET_FLAG = "-set";
        private const string DEBUG_FLAG = "-debug";


        static void Main(string[] args)
        {
            bool areArgumentsValid = ProcessArguments(args);
            if (!areArgumentsValid)
            {
                return;
            }

            Console.WriteLine("Creating templates...");
            HashSet<string> templates = LoadInput();
            WriteOutput(templates);
        }

        private static HashSet<string> LoadInput()
        {
            HashSet<string> templates = new HashSet<string>();
            string[] texts = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(inputFilePath));

            foreach (string text in texts)
            {
                RecognizedKeyWords.Init();
                Flags.Init(isDebug, isDeletedWordsDebugFlag: false);

                // NameTag -> UDPipe -> Template
                NamedEntitiesProcessor namedEntitiesProcessor = new NamedEntitiesProcessor(messageType);
                string template = namedEntitiesProcessor.ProcessEntities(text);
                if (template != "")
                {
                    templates.Add(template);
                }
            }
            return templates;
        }

        private static void WriteOutput(HashSet<string> templates, bool isJsonOutput = true)
        {
            // file.json -> fileOut.json
            string fileName = Path.GetFileName(inputFilePath);
            int jsonExtensionLength = ".json".Length;
            string newExtension = isJsonOutput ? ".json" : ".txt";
            string outputFileName = $"{fileName[..^jsonExtensionLength]}Out{newExtension}";

            if (isJsonOutput)
            {
                List<string> templatesList = templates.ToList();
                string templatesJson = JsonConvert.SerializeObject(templatesList);
                File.WriteAllText(outputFileName, templatesJson);
            }
            else
            {
                using StreamWriter outputWriter = new StreamWriter(outputFileName);
                foreach (string t in templates)
                {
                    outputWriter.WriteLine(t);
                }
            }
        }

        private static bool ProcessArguments(string[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3)
            {
                Console.WriteLine("Invalid arguments length");
                PrintUsage();
                return false;
            }

            if (arguments[0] == TITLE_FLAG)
            {
                messageType = MessageType.TITLE;
            }
            else if (arguments[0] == RESULT_FLAG)
            {
                messageType = MessageType.RESULT;
            }
            else if (arguments[0] == SET_FLAG)
            {
                messageType = MessageType.MATCHTITLE;
            }
            else
            {
                Console.WriteLine("Invalid first argument");
                PrintUsage();
                return false;
            }

            if (arguments.Length == 3)
            {
                if (arguments[1] == DEBUG_FLAG)
                {
                    isDebug = true;
                }
                else
                {
                    Console.WriteLine("Invalid second argument");
                    PrintUsage();
                    return false;
                }
            }

            inputFilePath = arguments[^1];
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Error: File not found: {inputFilePath}");
                return false;
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Available arguments:");
            Console.WriteLine($"{TITLE_FLAG} (-debug) [FILE] \t Creates templates for titles");
            Console.WriteLine($"{RESULT_FLAG} (-debug) [FILE] \t Creates templates for results");
            Console.WriteLine($"{SET_FLAG} (-debug) [FILE] \t Creates templates for sets");
        }
    }
}