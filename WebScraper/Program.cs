using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;


namespace WebScraper
{
    class Program
    {
        static void Main()
        {
            Scraper scraper = new Scraper();
            scraper.CreateMenRankingDatabase();

            bool createRandomNames = false;

            if (createRandomNames)
            {
                string playerMenPath = $"data{Path.DirectorySeparatorChar}rawMenNames100.txt";
                string playerWomenPath = $"data{Path.DirectorySeparatorChar}rawWomenNames100.txt";
                CreatePlayerNames(isMen: true, playerMenPath);
                CreatePlayerNames(isMen: false, playerWomenPath);
            }
        }

        /// <summary>
        /// Creates list of player names and serializes them into json file
        /// </summary>
        private static void CreatePlayerNames(bool isMen, string filePath)
        {
            // Files for men are from: https://www.livesport.cz/tenis/zebricky
            // For women (for "ová" endings): https://www.tenisovysvet.cz/ranking/wta
            List<string[]> playerNames = new List<string[]>();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            try
            {
                using StreamReader playerReader = new StreamReader(filePath);

                if (isMen)
                {
                    while (!playerReader.EndOfStream)
                    {
                        playerReader.ReadLine(); // rank
                        string name = playerReader.ReadLine();
                        string country = playerReader.ReadLine();
                        if (country.StartsWith('+') || country.StartsWith('-'))
                        {
                            // Extra info about rank increase or decrease
                            playerReader.ReadLine();
                        }
                        playerReader.ReadLine(); // points
                        playerReader.ReadLine(); // tournaments

                        string[] nameTokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        Array.Reverse(nameTokens); // start with firstname
                        playerNames.Add(nameTokens);
                    }
                }
                else
                {
                    while (!playerReader.EndOfStream)
                    {
                        string playerName = playerReader.ReadLine();
                        string[] nameParts = playerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        playerNames.Add(nameParts);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("File Error");
            }

            string json = $"{JsonConvert.SerializeObject(playerNames)}";
            string fileName = isMen ? "menNames.json" : "womenNames.json";
            File.WriteAllText(fileName, json);
        }
    }
}
