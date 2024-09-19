using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static NewsGenerator.TextUtility;

namespace NewsGenerator
{

    class MatchInput
    {
        // Mandatory parameters
        public string[] nameWinner { get; set; }
        public string[] nameLoser { get; set; }
        public string areMen { get; set; }
        public string[] tournamentName { get; set; }
        public string round { get; set; }
        public string score { get; set; }
        public string length { get; set; }

        // Optional parameters
        public string[] tournamentPlace { get; set; }
        public string[] tournamentSurface { get; set; }
        public string[] tournamentType { get; set; }
    }

    class PlayerInput
    {
        public string[] name { get; set; }
        public int rank { get; set; }
        public string country { get; set; }
        public int age { get; set; }
    }

    class Player
    {
        public static int playerCount = 2;
        public static int winnerIndex = 0;
        public static int loserIndex = 1;

        public string[] name { get; set; }
        public bool isMale { get; set; }
        public int rank { get; set; }
        public int age { get; set; }
        public string country { get; set; }
        public string nation { get; set; }
        public string nationAdjective { get; set; }
    }

    class Tournament
    {
        public string[] name { get; set; }
        public string[] continent { get; set; }
        public string[] country { get; set; }
        public string[] city { get; set; }
        public string[] surface { get; set; }
        public string[] category { get; set; }
    }

    static class InputList
    {
        public static List<string> rounds { get; } = new List<string>
        {
            { "1. kolo" }, { "2. kolo" }, { "3. kolo" }, { "4. kolo" },
            { "osmifinále" }, { "čtvrtfinále" }, { "semifinále" }, { "finále" }
        };

        public static List<string[]> surfaces { get; set; } = new List<string[]>
        {
            new string[] { "tráva" }, new string[] { "antuka" }, new string[] { "tvrdý", "povrch" }, new string[] { "koberec" }
        };
    }

    class MatchParameters
    {
        //static string matchInputPath = $"data{Path.DirectorySeparatorChar}inputs{Path.DirectorySeparatorChar}DJO-MED.txt";
        static string playerInfoPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}playerInfo.json";
        static string tournamentInfoPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}tournamentsInfo.json";
        static string menNamesPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}menNames.json";
        static string womenNamesPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}womenNames.json";
        static string tournamentNamesPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}tournamentNames.json";
        static string roundNamesPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}roundNames.json";

        public MatchInput matchInput { get; set; }
        public Player[] players { get; set; }
        public Tournament tournament { get; set; }
        public string[] round { get; set; }
        public List<(int scoreWinner, int scoreLoser)> score { get; set; }
        public List<int> setsAggregation { get; set; } // E.g. {1, 2} = 3 sets where first set is different and two last sets are the same
        public int setsAggregationIndex { get; set; }
        public string length { get; set; }

        public int currentSet { get; set; } = 1;

        // Control fields
        public bool isRealMatchInput { get; set; } = true;
        static bool isTournamentFromDatabase = false;


        public bool LoadMatchFromInput(string filePath = "", bool isRandomInput = false)
        {
            if (filePath != "" && !File.Exists(filePath))
            {
                Console.WriteLine("File does not exist");
                return false;
            }

            if (isRandomInput)
            {
                if (isRealMatchInput)
                {
                    matchInput = RealMatchInput.ParseRandomRealMatch();
                    matchInput = AddPlayerNames(matchInput);
                }
                else
                {
                    matchInput = CreateRandomInput();
                }
            }
            else
            {
                bool isFilePathInput = filePath != "";
                if (isFilePathInput)
                {
                    matchInput = LoadInputFromFile(filePath);
                }
                else
                {
                    matchInput = UIComponents.LoadInput();
                }
            }

            if (matchInput == null)
            {
                return false;
            }

            if (NewsGenerator.isEvaluation)
            {
                // Choose every second tournament from the database
                if (isTournamentFromDatabase)
                {
                    List<string[]> grandSlams = new List<string[]>()
                {
                    new string[] {"Australian", "Open" }, new string[] {"French", "Open" }, new string[] {"Wimbledon" }, new string[] { "US", "Open" }
                };
                    Random random = new Random();
                    int randomIndex = random.Next(0, grandSlams.Count);
                    matchInput.tournamentName = grandSlams[randomIndex];
                }
                isTournamentFromDatabase = !isTournamentFromDatabase;
            }


            if (isRandomInput)
            {
                PrintRandomlyGeneratedInfo(matchInput);
            }

            if (!CheckInput(matchInput))
            {
                return false;
            }

            CreatePlayers(matchInput);
            LoadExternalPlayerInfo();
            LoadExternalTournamentInfo(matchInput, filePath != "");

            round = matchInput.round.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            length = matchInput.length;

            Categories.Init();
            Categories.Analyze(score, players, round);
            AggregateSameSets();
            return true;
        }

        private void CreatePlayers(MatchInput matchInput)
        {
            players = new Player[Player.playerCount];
            players[Player.winnerIndex] = new Player();
            players[Player.winnerIndex].name = new string[matchInput.nameWinner.Length];
            players[Player.winnerIndex].name = matchInput.nameWinner;
            players[Player.winnerIndex].isMale = matchInput.areMen == "true";

            players[Player.loserIndex] = new Player();
            players[Player.loserIndex].name = new string[matchInput.nameLoser.Length];
            players[Player.loserIndex].name = matchInput.nameLoser;
            players[Player.loserIndex].isMale = players[Player.winnerIndex].isMale; // Both players have same gender
        }

        private MatchInput AddPlayerNames(MatchInput matchInput)
        {
            Random random = new Random();
            bool areMen = random.Next(0, 2) == 0;
            matchInput.areMen = areMen.ToString().ToLower();

            // Get random player names
            matchInput.nameWinner = GetRandomPlayerName(areMen);
            // Choose different second name
            while (matchInput.nameLoser == null || matchInput.nameLoser == matchInput.nameWinner)
            {
                matchInput.nameLoser = GetRandomPlayerName(areMen);
            }

            return matchInput;
        }

        private bool CheckInput(MatchInput matchInput)
        {
            string inputError = "Chyba vstupu";
            if (matchInput.nameWinner == null || matchInput.nameWinner.Length < 2)
            {
                MessageBox.Show("Neplatné jméno vítěze", inputError);
                return false;
            }
            if (matchInput.nameLoser == null || matchInput.nameLoser.Length < 2)
            {
                MessageBox.Show("Neplatné jméno poraženého hráče", inputError);
                return false;
            }
            if (matchInput.tournamentName == null || matchInput.tournamentName.Length == 0)
            {
                MessageBox.Show("Neplatné jméno turnaje", inputError);
                return false;
            }
            if (matchInput.length == null || !IsMatchLengthValid(matchInput.length))
            {
                MessageBox.Show($"Neplatná délka zápasu: {matchInput.length}", inputError);
                return false;
            }
            if (matchInput.score == null || !IsScoreValid(matchInput.score))
            {
                MessageBox.Show($"Neplatné skóre: {matchInput.score}", inputError);
                return false;
            }
            return true;
        }

        private bool IsMatchLengthValid(string length)
        {
            int colonIndex = length.IndexOf(':');
            if (colonIndex == -1)
            {
                return false;
            }

            string hours = length[0..colonIndex];
            string minutes = length[(colonIndex + 1)..];

            return int.TryParse(hours, out int _) && int.TryParse(minutes, out int _);
        }

        private bool IsScoreValid(string rawScore)
        {
            bool isScoreValid = ParseScore(rawScore, out List<(int scoreWinner, int scoreLoser)> score);
            if (!isScoreValid)
            {
                return false;
            }

            int setsWinner = 0;
            for (int i = 0; i < score.Count; i++)
            {
                bool isLastSet = i + 1 == score.Count;
                if (!isLastSet && !IsSetCompleted(score[i].scoreWinner, score[i].scoreLoser))
                {
                    return false;
                }
                else if (isLastSet)
                {
                    bool winnerWonLastSet = score[i].scoreWinner >= score[i].scoreLoser;
                    if (!winnerWonLastSet)
                    {
                        return false;
                    }
                }

                if (score[i].scoreWinner > score[i].scoreLoser)
                {
                    setsWinner++;
                }
            }

            if (setsWinner > 3)
            {
                return false;
            }

            this.score = score;
            return true;
        }

        private bool IsSetCompleted(int scoreWinner, int scoreLoser)
        {
            if (scoreWinner < scoreLoser)
            {
                int previousScoreWinner = scoreWinner;
                scoreWinner = scoreLoser;
                scoreLoser = previousScoreWinner;
            }

            if (scoreWinner < 6)
            {
                return false;
            }
            else if (scoreWinner == 6)
            {
                return scoreLoser < 5;
            }
            else if (scoreWinner == 7)
            {
                return scoreLoser == 5 || scoreLoser == 6;
            }
            else
            {
                return (scoreWinner - scoreLoser) == 2;
            }
        }

        private MatchInput LoadInputFromFile(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);
            try
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<MatchInput>(json);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private MatchInput CreateRandomInput()
        {
            MatchInput matchInput = new MatchInput();

            Random random = new Random();
            bool areMen = random.Next(0, 2) == 0;
            matchInput.areMen = areMen.ToString().ToLower();

            // Get random player names
            matchInput.nameWinner = GetRandomPlayerName(areMen);
            matchInput.nameLoser = GetRandomPlayerName(areMen);

            matchInput.tournamentName = GetRandomTournamentName();
            matchInput.round = GetRandomRound();
            matchInput.score = GetRandomScore(areMen, out List<(int, int)> setScores);
            matchInput.length = GetRandomLength(setScores);

            return matchInput;
        }

        private string[] GetRandomPlayerName(bool areMen)
        {
            string playersNameFilePath = areMen ? menNamesPath : womenNamesPath;

            if (!File.Exists(playersNameFilePath))
            {
                Console.WriteLine($"File not found: {playersNameFilePath}");
                return null;
            }

            using StreamReader playerReader = new StreamReader(playersNameFilePath);
            string playerNamesJson = playerReader.ReadToEnd();
            string[][] playerNames = JsonConvert.DeserializeObject<string[][]>(playerNamesJson);

            Random random = new Random();
            int randomPlayerIndex = random.Next(0, playerNames.Length);
            return playerNames[randomPlayerIndex];
        }

        private string[] GetRandomTournamentName()
        {
            Random random = new Random();
            using StreamReader tournamentReader = new StreamReader(tournamentNamesPath);
            string tournamentNamesJson = tournamentReader.ReadToEnd();
            string[][] tournamentNames = JsonConvert.DeserializeObject<string[][]>(tournamentNamesJson);

            int randomTournamentIndex = random.Next(0, tournamentNames.Length);
            return tournamentNames[randomTournamentIndex];
        }

        private string GetRandomRound()
        {
            Random random = new Random();
            using StreamReader roundReader = new StreamReader(roundNamesPath);
            string roundNamesJson = roundReader.ReadToEnd();
            string[] roundNames = JsonConvert.DeserializeObject<string[]>(roundNamesJson);

            int randomRoundIndex = random.Next(0, roundNames.Length);
            return roundNames[randomRoundIndex];
        }

        private string GetRandomScore(bool areMale, out List<(int scoreWinner, int scoreLoser)> setScores)
        {
            Random random = new Random();

            // How many sets player needs to win
            int setsToWin = random.Next(0, 2) == 0 ? 2 : 3;
            if (!areMale)
            {
                // Women usually play 2 sets to win
                setsToWin = 2;
            }

            int maxSetsCount = 5;
            if (setsToWin == 2)
            {
                maxSetsCount = 3;
            }

            int setsCount = random.Next(setsToWin, maxSetsCount + 1);
            setScores = new List<(int, int)>();

            int winnerSetsWon = 0;
            int loserSetsWon = 0;

            for (int i = 0; i < setsCount; i++)
            {
                bool isSetWinner = random.Next(0, 2) == 0;

                // Winner needs to win last set
                if (i + 1 == setsCount)
                {
                    isSetWinner = true;
                }

                // Do not let loser win the match
                if (loserSetsWon + 1 == setsToWin)
                {
                    isSetWinner = true;
                }

                // Winning player needs to lose winning set if it is not the last set
                if (winnerSetsWon + 1 == setsToWin && i + 1 != setsCount)
                {
                    isSetWinner = false;
                }

                if (isSetWinner)
                {
                    winnerSetsWon++;
                }
                else
                {
                    loserSetsWon++;
                }

                int minGemsLoserWon = 0;
                int maxGemsLoserWon = 6;
                int gamesLoser = random.Next(minGemsLoserWon, maxGemsLoserWon + 1);

                int gamesWinner = 6;
                // Let winner win the tie-break
                if (gamesLoser == 6)
                {
                    gamesWinner = 7;
                }

                // Winner needs to win one more set
                if (gamesLoser == 5)
                {
                    gamesWinner++;
                }

                setScores.Add(isSetWinner ? (gamesWinner, gamesLoser) : (gamesLoser, gamesWinner));
            }

            int randomNumber = random.Next(0, 101);
            if (randomNumber < 2)
            {
                // 2% chance that somebody did not finish the match
                // Change score in last set to make the match unfinished
                int scoreWinner = 5;
                int scoreLoser = 2;
                setScores[^1] = (scoreWinner, scoreLoser);
            }

            string score = "";
            for (int i = 0; i < setScores.Count; i++)
            {
                score += $"{setScores[i].scoreWinner}:{setScores[i].scoreLoser}";
                if (i + 1 != setScores.Count)
                {
                    score += ", ";
                }
            }
            return score;
        }

        private string GetRandomLength(List<(int scoreWinner, int scoreLoser)> setScores)
        {
            // For every played game add 2-5 minutes
            int games = 0;

            foreach ((int scoreWinner, int scoreLoser) setScore in setScores)
            {
                games += setScore.scoreWinner + setScore.scoreLoser;
            }

            int minGameLength = 2;
            int maxGameLength = 5;

            int randomGameLength = 0;

            for (int i = 0; i < games; i++)
            {
                Random random = new Random();
                randomGameLength += random.Next(minGameLength, maxGameLength + 1);
            }

            int hours = randomGameLength / 60;
            int minutes = randomGameLength % 60;
            return $"{hours}:{minutes}";
        }

        private void PrintRandomlyGeneratedInfo(MatchInput matchInput)
        {
            // Print randomly generated info:
            Console.Write($"Selected winner: ");
            foreach (string name in matchInput.nameWinner)
            {
                Console.Write($"{name} ");
            }
            Console.WriteLine();

            Console.Write($"Selected loser: ");
            foreach (string name in matchInput.nameLoser)
            {
                Console.Write($"{name} ");
            }
            Console.WriteLine();
            Console.WriteLine($"Are men: {matchInput.areMen}");
            Console.WriteLine($"Tournament: {matchInput.tournamentName}");
            Console.WriteLine($"Round: {matchInput.round}");
            Console.WriteLine($"Score: {matchInput.score}");
            Console.WriteLine($"Length: {matchInput.length}");
            Console.WriteLine();

        }
        private void LoadExternalPlayerInfo()
        {
            if (!File.Exists(playerInfoPath))
            {
                Console.WriteLine($"File not found: {playerInfoPath}");
                return;
            }

            PlayerInput[] playersInput;
            using (StreamReader playerInfoReader = new StreamReader(playerInfoPath))
            {
                string date = JsonConvert.DeserializeObject<string>(playerInfoReader.ReadLine());
                playersInput = JsonConvert.DeserializeObject<PlayerInput[]>(playerInfoReader.ReadToEnd());
            }

            // Remove diacritics
            string winnerFirstNameNoDiacritics = RemoveAccents(players[Player.winnerIndex].name[0]);
            string winnerSecondNameNoDiacritics = RemoveAccents(players[Player.winnerIndex].name[1]);

            string loserFirstNameNoDiacritics = RemoveAccents(players[Player.loserIndex].name[0]);
            string loserSecondNameNoDiacritics = RemoveAccents(players[Player.loserIndex].name[1]);

            bool isFilled1 = false;
            bool isFilled2 = false;
            for (int i = 0; i < playersInput.Length; i++)
            {
                if (playersInput[i].name[0] == winnerFirstNameNoDiacritics && playersInput[i].name[1] == winnerSecondNameNoDiacritics)
                {
                    FillPlayerInformation(Player.winnerIndex, playersInput, i);
                    isFilled1 = true;
                }
                else if (playersInput[i].name[0] == loserFirstNameNoDiacritics && playersInput[i].name[1] == loserSecondNameNoDiacritics)
                {
                    FillPlayerInformation(Player.loserIndex, playersInput, i);
                    isFilled2 = true;
                }
            }
            if (!isFilled1)
            {
                Console.Write("No additional info for: ");

                for (int i = 0; i < players[Player.winnerIndex].name.Length; i++)
                {
                    if (i + 1 == players[Player.winnerIndex].name.Length)
                    {
                        Console.Write(players[Player.winnerIndex].name[i]);
                    }
                    else
                    {
                        Console.Write($"{players[Player.winnerIndex].name[i]} ");
                    }
                }
                Console.WriteLine();
            }
            if (!isFilled2)
            {
                Console.Write("No additional info for: ");

                for (int i = 0; i < players[Player.loserIndex].name.Length; i++)
                {
                    if (i + 1 == players[Player.loserIndex].name.Length)
                    {
                        Console.Write(players[Player.loserIndex].name[i]);
                    }
                    else
                    {
                        Console.Write($"{players[Player.loserIndex].name[i]} ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private void LoadExternalTournamentInfo(MatchInput matchInput, bool isUsersFileInput)
        {
            string[] tournamentInputName = matchInput.tournamentName;
            string filePath = tournamentInfoPath;
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            Tournament[] tournamentsInput;
            using (StreamReader tournamentsInfoReader = new StreamReader(filePath))
            {
                tournamentsInput = JsonConvert.DeserializeObject<Tournament[]>(tournamentsInfoReader.ReadToEnd());
            }

            for (int i = 0; i < tournamentsInput.Length; i++)
            {
                if (tournamentInputName.Length == tournamentsInput[i].name.Length)
                {
                    bool isNameSame = true;
                    for (int x = 0; x < tournamentInputName.Length; x++)
                    {
                        if (tournamentInputName[x] != tournamentsInput[i].name[x])
                        {
                            isNameSame = false;
                            break;
                        }
                    }

                    if (isNameSame)
                    {
                        tournament = tournamentsInput[i];
                        return;
                    }
                }
            }

            // Tournament not found
            if (isUsersFileInput)
            {
                tournament = new Tournament()
                {
                    name = tournamentInputName,
                    city = matchInput.tournamentPlace,
                    category = matchInput.tournamentType,
                    surface = matchInput.tournamentSurface
                };
                UIComponents.SetOptionalInput();
            }
            else
            {
                tournament = UIComponents.LoadTournamentFromOptionalFields(tournamentInputName);
            }

        }
        private void FillPlayerInformation(int playerIndex, PlayerInput[] playersInput, int playerInputIndex)
        {
            players[playerIndex].rank = playersInput[playerInputIndex].rank;
            players[playerIndex].age = playersInput[playerInputIndex].age;
            players[playerIndex].country = playersInput[playerInputIndex].country;
            players[playerIndex].nation = CountryToNation(playersInput[playerInputIndex].country, players[playerIndex].isMale);
            players[playerIndex].nationAdjective = CountryToAdjective(playersInput[playerInputIndex].country, players[playerIndex].isMale);
        }

        private bool ParseScore(string rawScore, out List<(int scoreWinner, int scoreLoser)> score)
        {
            score = new List<(int scoreWinner, int scoreLoser)>();
            if (rawScore == "")
            {
                return false;
            }

            string[] tokens = rawScore.Split(',', StringSplitOptions.RemoveEmptyEntries);
            int firstDigitsSum = 0;
            int secondDigitsSum = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                string[] scoreParts = tokens[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                bool isFirstNumberValid = int.TryParse(scoreParts[0], out int firstNumber);
                if (!isFirstNumberValid)
                {
                    return false;
                }

                firstDigitsSum += firstNumber;
                bool isSecondNumberValid = int.TryParse(scoreParts[1], out int secondNumber);
                if (!isSecondNumberValid)
                {
                    return false;
                }
                secondDigitsSum += secondNumber;
                score.Add((firstNumber, secondNumber));
            }

            if (score[^1].scoreWinner < score[^1].scoreLoser)
            {
                // Second digits belong to winner
                // Swap positions in the score
                for (int i = 0; i < score.Count; i++)
                {
                    score[i] = (score[i].scoreLoser, score[i].scoreWinner);
                }
            }
            return true;
        }

        private void AggregateSameSets()
        {
            setsAggregation = new List<int>();
            int sameSets = 0;
            bool[] currentSetCategories = Categories.originalCategoriesSet[0];
            bool winnerWonLastSet = score[0].scoreWinner > score[0].scoreLoser;

            for (int i = 0; i < score.Count; i++)
            {
                bool winnerWonCurrentSet = score[i].scoreWinner > score[i].scoreLoser;
                if ((winnerWonCurrentSet == winnerWonLastSet) && Categories.AreCategoriesSame(currentSetCategories, Categories.originalCategoriesSet[i]))
                {
                    sameSets++;
                }
                else
                {
                    setsAggregation.Add(sameSets);
                    sameSets = 1;
                    currentSetCategories = Categories.originalCategoriesSet[i];
                }
                winnerWonLastSet = score[i].scoreWinner > score[i].scoreLoser;

                if (i + 1 == score.Count)
                {
                    setsAggregation.Add(sameSets);
                }
            }
        }

        private bool IsSameSetWinner(int scoreWinner, int scoreLoser, bool isSetWinner)
        {
            bool isCurrentSetWinner = scoreWinner > scoreLoser;
            return isCurrentSetWinner == isSetWinner;
        }

        public string Generate(MessageType messageType, bool isShowTemplates)
        {
            string result = "";
            int templateCount = Templates.GetTemplateCount(messageType);

            int messages = 1;
            if (messageType == MessageType.MATCHTITLE)
            {
                messages = setsAggregation.Count;
            }

            for (int i = 0; i < messages; i++)
            {
                bool isDone = false;
                while (!isDone)
                {
                    if (Templates.tries[(int)messageType] >= Templates.triesLimit)
                    {
                        Templates.ResetTries(messageType);
                    }

                    int randomIndex = Templates.random.Next(0, templateCount);

                    if (Templates.usedIndexes[(int)messageType].Contains(randomIndex))
                    {
                        Templates.tries[(int)messageType]++;
                        continue;
                    }
                    Templates.usedIndexes[(int)messageType].Add(randomIndex);

                    bool isTemplateValid = Templates.GetTemplate(messageType, randomIndex, out string template, out bool[] templateCategories);
                    if (isTemplateValid && Categories.AreCategoriesSuitable(templateCategories, messageType, currentSet))
                    {
                        // If loser won this set swap winner and loser
                        bool swapWinnerAndLoser = messageType == MessageType.MATCHTITLE && score[currentSet - 1].scoreWinner < score[currentSet - 1].scoreLoser;
                        TemplateFiller templateFiller = new TemplateFiller(messageType, template, templateCategories, this, swapWinnerAndLoser, isDebug: isShowTemplates);
                        isDone = templateFiller.Fill(out string text);
                        if (isDone)
                        {
                            result += i > 0 ? $"\n\n{text}" : text;
                        }
                    }
                }

                if (messageType == MessageType.MATCHTITLE)
                {
                    currentSet += setsAggregation[i];
                    setsAggregationIndex++;
                }
            }

            currentSet = 1;
            setsAggregationIndex = 0;
            return result;
        }
    }
}
