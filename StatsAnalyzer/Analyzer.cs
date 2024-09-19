using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;


namespace StatsAnalyzer
{
    class Analyzer
    {
        // Column names for csv files are from https://datahub.io/sports-data/atp-world-tour-tennis-data
        enum ResultsColumnName
        {
            TOURNEY_YEAR_ID, TOURNEY_ORDER, TOURNEY_SLUG, TOURNEY_URL_SUFFIX, TOURNEY_ROUND_NAME, ROUND_ORDER, MATCH_ORDER, WINNER_NAME, WINNER_PLAYER_ID,
            WINNER_SLUG, LOSER_NAME, LOSER_PLAYER_ID, LOSER_SLUG, WINNER_SEED, LOSER_SEED, MATCH_SCORE_TIEBREAKS, WINNER_SETS_WON, LOSER_SET_WON,
            WINNER_GAMES_WON, LOSER_GAMES_WON, WINNER_TIEBREAKS_WON, LOSER_TIEBREAKS_WON, MATCH_ID, MATCH_STATS_URL_SUFFIX
        };
        enum StatsColumnName
        {
            TOURNEY_ORDER, MATCH_ID, MATCH_STATS_URL_SUFFIX, MATCH_TIME, MATCH_DURATION, WINNER_ACES, WINNER_DOUBLE_FAULTS,
            WINNER_FIRST_SERVERS_IN, WINNER_FIRST_SERVERS_TOTAL, WINNER_FIRST_SERVE_POINTS_WON, WINNER_FIRST_SERVE_POINTS_TOTAL,
            WINNER_SECOND_SERVER_POINTS_WON, WINNER_SECOND_SERVER_POINTS_TOTAL, WINNER_BREAK_POINTS_SAVED, WINNER_BREAK_POINTS_SERVER_TOTAL,
            WINNER_SERVICE_POINTS_WON, WINNER_SERVICE_POINTS_TOTAL, WINNER_FIRST_SERVER_RETURN_WON, WINNER_FIRST_SERVE_RETURN_TOTAL,
            WINNER_SECOND_SERVE_RETURN_WON, WINNER_SECOND_SERVE_RETURN_TOTAL, WINNER_BREAK_POINTS_CONVERTED, WINNER_BREAK_POINTS_RETURN_TOTAL,
            WINNER_SERVICE_GAMES_PLAYED, WINNER_RETURN_POINTS_WON, WINNER_RETURN_POINTS_TOTAL, WINNER_TOTAL_POINTS_WON, WINNER_TOTAL_POINTS_TOTAL,
            LOSER_ACES, LOSER_DOUBLE_FAULTS, LOSER_FIRST_SERVERS_IN, LOSER_FIRST_SERVERS_TOTAL, LOSER_FIRST_SERVE_POINTS_WON,
            LOSER_FIRST_SERVE_POINTS_TOTAL, LOSER_SECOND_SERVER_POINTS_WON, LOSER_SECOND_SERVER_POINTS_TOTAL, LOSER_BREAK_POINTS_SAVED,
            LOSER_BREAK_POINTS_SERVER_TOTAL, LOSER_SERVICE_POINTS_WON, LOSER_SERVICE_POINTS_TOTAL, LOSER_FIRST_SERVER_RETURN_WON,
            LOSER_FIRST_SERVE_RETURN_TOTAL, LOSER_SECOND_SERVE_RETURN_WON, LOSER_SECOND_SERVE_RETURN_TOTAL, LOSER_BREAK_POINTS_CONVERTED,
            LOSER_BREAK_POINTS_RETURN_TOTAL, LOSER_SERVICE_GAMES_PLAYED, LOSER_RETURN_POINTS_WON, LOSER_RETURN_POINTS_TOTAL, LOSER_TOTAL_POINTS_WON,
            LOSER_TOTAL_POINTS_TOTAL
        };

        private string inputFilePath { get; }
        private string resultsFilePath { get; }
        private string statsFilePath { get; }

        private static List<List<(int, int)>> scores = new List<List<(int, int)>>();

        private const int MAX_WINNER_LOST_SETS_TO_TWO = 2;
        private const int MAX_WINNER_LOST_SETS_TO_THREE = 3;
        private const int MAX_SETS_TO_TWO = 4;
        private const int MAX_SETS_TO_THREE = 6;

        public Analyzer(string inputFilePath, string resultsFilePath, string statsFilePath)
        {
            this.inputFilePath = inputFilePath;
            this.resultsFilePath = resultsFilePath;
            this.statsFilePath = statsFilePath;
        }

        public void ParseResults()
        {
            string[] texts;

            try
            {
                texts = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(inputFilePath));
            }
            catch (JsonReaderException exception)
            {
                Console.WriteLine($"File error: {exception.Message}");
                return;
            }


            foreach (string text in texts)
            {
                List<(int, int)> sets = ParseScore(text);
                if (sets.Count > 1)
                {
                    scores.Add(sets);
                }
            }
        }

        public List<(int, int)> ParseScore(string line)
        {
            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<(int, int)> sets = new List<(int, int)>();
            int multiplier = 1;

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Contains(':'))
                {
                    string[] setTokens = words[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (setTokens.Length != 2)
                    {
                        continue;
                    }

                    // Remove trailing non digit characters
                    if (!char.IsDigit(setTokens[1][^1]))
                    {
                        setTokens[1] = setTokens[1][..^1];
                    }

                    bool isValid = int.TryParse(setTokens[0], out int firstNumber);
                    isValid = int.TryParse(setTokens[1], out int secondNumber) && isValid;

                    if (isValid)
                    {
                        for (int x = 0; x < multiplier; x++)
                        {
                            sets.Add((firstNumber, secondNumber));
                        }
                    }
                    multiplier = 1;
                }
                else if (words[i] == "dvakrát")
                {
                    multiplier = 2;
                }
                else if (words[i] == "třikrát")
                {
                    multiplier = 3;
                }
            }
            return sets;
        }

        /// <summary>
        /// Prints detailed information about sets and average percentage of games won by winner
        /// </summary>
        public void AnalyzeScore()
        {
            ParseResults();

            int[] winnerLostSetsToTwo = new int[MAX_WINNER_LOST_SETS_TO_TWO];
            int[] winnerLostSetsToThree = new int[MAX_WINNER_LOST_SETS_TO_THREE];
            int[] winnerCloseSetsToTwo = new int[MAX_SETS_TO_TWO];
            int[] winnerCloseSetsToThree = new int[MAX_SETS_TO_THREE];

            int allGamesWinner = 0;
            int allGamesLoser = 0;
            int invalidEntries = 0;

            foreach (List<(int, int)> score in scores)
            {
                int gamesWinner = 0;
                int gamesLoser = 0;
                int winnerWonSets = 0;
                int winnerLostSets = 0;
                int closeSets = 0;
                bool firstPlayerWonGame = true;

                for (int i = 0; i < score.Count; i++)
                {
                    gamesWinner += score[i].Item1;
                    gamesLoser += score[i].Item2;

                    if (score[i].Item1 < score[i].Item2)
                    {
                        winnerLostSets++;
                    }
                    else
                    {
                        winnerWonSets++;
                    }

                    if (Math.Abs(score[i].Item1 - score[i].Item2) <= 2)
                    {
                        closeSets++;
                    }

                    if (i + 1 == score.Count)
                    {
                        firstPlayerWonGame = score[i].Item1 > score[i].Item2;
                    }
                }

                if (!IsLastSetFinished(score))
                {
                    invalidEntries++;
                    continue;
                }

                if (!firstPlayerWonGame)
                {
                    // Swap values for winner and loser
                    int games = gamesLoser;
                    gamesLoser = gamesWinner;
                    gamesWinner = games;

                    int sets = winnerWonSets;
                    winnerWonSets = winnerLostSets;
                    winnerLostSets = sets;
                }
                allGamesWinner += gamesWinner;
                allGamesLoser += gamesLoser;

                if (winnerWonSets == 2)
                {
                    winnerLostSetsToTwo[winnerLostSets]++;
                    winnerCloseSetsToTwo[closeSets]++;
                }
                else
                {
                    winnerLostSetsToThree[winnerLostSets]++;
                    winnerCloseSetsToThree[closeSets]++;
                }
            }

            PrintScoreResults(allGamesWinner, allGamesLoser, invalidEntries, winnerLostSetsToTwo, winnerLostSetsToThree, 
                winnerCloseSetsToTwo, winnerCloseSetsToThree);
        }

        private bool IsLastSetFinished(List<(int, int)> score)
        {
            int lastSetScoreWinner = score[^1].Item1;
            int lastSetScoreLoser = score[^1].Item2;
            if (lastSetScoreWinner < lastSetScoreLoser)
            {
                int previousLastSetScoreWinner = lastSetScoreWinner;
                lastSetScoreWinner = lastSetScoreLoser;
                lastSetScoreLoser = previousLastSetScoreWinner;
            }

            return !Tennis.IsLastSetNotFinished(lastSetScoreWinner, lastSetScoreLoser);
        }

        private void PrintScoreResults(int allGamesWinner, int allGamesLoser, int invalidEntries, int[] winnerLostSetsToTwo, int[] winnerLostSetsToThree, 
            int[] winnerCloseSetsToTwo, int[] winnerCloseSetsToThree)
        {
            Console.WriteLine($"Entries: {scores.Count - invalidEntries}");

            Console.WriteLine($"Winner lost sets to two winners: ");
            for (int x = 0; x < winnerLostSetsToTwo.Length; x++)
            {
                Console.WriteLine($"{x} lost set(s): {winnerLostSetsToTwo[x]}");
            }
            Console.WriteLine($"Winner lost sets to three winners: ");
            for (int x = 0; x < winnerLostSetsToThree.Length; x++)
            {
                Console.WriteLine($"{x} lost set(s): {winnerLostSetsToThree[x]}");
            }
            Console.WriteLine();

            Console.WriteLine($"Winner close sets to two winners: ");
            for (int x = 0; x < winnerCloseSetsToTwo.Length; x++)
            {
                Console.WriteLine($"{x} close set(s): {winnerCloseSetsToTwo[x]}");
            }
            Console.WriteLine($"Winner close sets to three winners: ");
            for (int x = 0; x < winnerCloseSetsToThree.Length; x++)
            {
                Console.WriteLine($"{x} close set(s): {winnerCloseSetsToThree[x]}");
            }

            double allGamesWonByWinnerPercentage = ((double)allGamesWinner / (allGamesWinner + allGamesLoser)) * 100;
            Console.WriteLine($"Average percentage of winner won games from all games: {allGamesWonByWinnerPercentage}");
        }

        /// <summary>
        /// Counts total number of aces and probability of hitting an ace from first serve
        /// </summary>
        public void CountAces()
        {
            StreamReader statsReader = new StreamReader(statsFilePath);

            int acesTotal = 0;
            int firstServesTotal = 0;
            int lines = 0;

            // Skip header
            if (!statsReader.EndOfStream)
            {
                statsReader.ReadLine();
            }

            while (!statsReader.EndOfStream)
            {
                string[] tokens = statsReader.ReadLine().Split(',');

                if (tokens.Length == 0) 
                { 
                    return; 
                }

                bool isValid = int.TryParse(tokens[(int)StatsColumnName.WINNER_ACES], out int aces);

                if (!isValid)
                {
                    //Console.WriteLine("Invalid row on line: {0}", lines);
                    continue;
                }

                int.TryParse(tokens[(int)StatsColumnName.WINNER_FIRST_SERVERS_TOTAL], out int firstServes);

                acesTotal += aces;
                firstServesTotal += firstServes;
                lines++;
            }
            Console.WriteLine("Aces stats: ");
            Console.WriteLine($"Valid file lines: {lines}, Total aces: {acesTotal}, Probability of hitting ace from first serve: {Math.Round(acesTotal / (double)firstServesTotal, 2, MidpointRounding.AwayFromZero)}");
        }

        /// <summary>
        /// Counts average length of matches to 2 and 3 winning sets
        /// </summary>
        public void CountAverageMixedDuration()
        {
            List<string> statsContent = LoadFileContent(statsFilePath);
            List<string> resultContent = LoadFileContent(resultsFilePath);

            int matchTimeSum2W = 0;
            int matchTimeSum3W = 0;
            int lines2W = 0;
            int lines3W = 0;
            int setsTotal2W = 0;
            int setsTotal3W = 0;

            for (int i = 0; i < statsContent.Count; i++)
            {
                string[] tokens = statsContent[i].Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length <= (int)StatsColumnName.MATCH_DURATION)
                {
                    continue;
                }

                bool isDurationValid = int.TryParse(tokens[(int)StatsColumnName.MATCH_DURATION], out int matchDuration);
                if (!isDurationValid)
                {
                    continue;
                }

                string matchID = tokens[(int)StatsColumnName.MATCH_ID];

                string[] resultTokens = FindResultEntryByMatchID(matchID, resultContent);
                if (resultTokens == null)
                {
                    continue;
                }

                // Do not count duration of unfinished matches
                string matchScore = resultTokens[(int)ResultsColumnName.MATCH_SCORE_TIEBREAKS];
                if (matchScore.Contains("(RET)"))
                {
                    continue;
                }

                string[] matchScoreTokens = matchScore.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                bool isTwoSetsToWin = IsTwoSetsToWin(matchScoreTokens);
                int playedSets = matchScoreTokens.Length;

                if (isTwoSetsToWin)
                {
                    matchTimeSum2W += matchDuration;
                    setsTotal2W += playedSets;
                    lines2W++;
                }
                else
                {
                    matchTimeSum3W += matchDuration;
                    setsTotal3W += playedSets;
                    lines3W++;
                }
            }

            PrintDurationResult(lines2W, lines3W, setsTotal2W, setsTotal3W, matchTimeSum2W, matchTimeSum3W);
        }

        private void PrintDurationResult(int lines2W, int lines3W, int setsTotal2W, int setsTotal3W, int matchTimeSum2W, int matchTimeSum3W)
        {
            int decimalPlacesToRound = 2;
            Console.WriteLine($"Total sets played for 2W: {setsTotal2W} in total length of: {matchTimeSum2W} minutes");

            double averageLengthOfSetOf2WMatch = Math.Round(matchTimeSum2W / (double)setsTotal2W, decimalPlacesToRound, MidpointRounding.AwayFromZero);
            Console.WriteLine($"From that average length of 1 set: {averageLengthOfSetOf2WMatch} minutes");

            double averageLengthOf2WMatch = Math.Round(matchTimeSum2W / (double)lines2W, decimalPlacesToRound, MidpointRounding.AwayFromZero);
            Console.WriteLine("Two winner sets duration: ");
            Console.WriteLine($"Valid file lines: {lines2W}, Sum: {matchTimeSum2W} minutes, Average length of one match: {averageLengthOf2WMatch} minutes");

            Console.WriteLine();

            Console.WriteLine($"Total sets played for 3W: {setsTotal3W} in total length of: {matchTimeSum3W} minutes");

            double averageLengthOfSetOf3WMatch = Math.Round(matchTimeSum3W / (double)setsTotal3W, decimalPlacesToRound, MidpointRounding.AwayFromZero);
            Console.WriteLine($"From that average length of 1 set: {averageLengthOfSetOf3WMatch} minutes");

            double averageLengthOf3WMatch = Math.Round(matchTimeSum3W / (double)lines3W, decimalPlacesToRound, MidpointRounding.AwayFromZero);
            Console.WriteLine("Three winner sets duration: ");
            Console.WriteLine($"Valid file lines: {lines3W}, Sum: {matchTimeSum3W}, Average length of one match: {averageLengthOf3WMatch} minutes");
        }

        private List<string> LoadFileContent(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            List<string> content = new List<string>();

            // Skip header
            if (!reader.EndOfStream)
            {
                reader.ReadLine();
            }

            while (!reader.EndOfStream)
            {
                content.Add(reader.ReadLine());
            }

            return content;
        }

        private string[] FindResultEntryByMatchID(string matchID, List<string> resultContent)
        {
            for (int i = 0; i < resultContent.Count; i++)
            {
                string[] tokens = resultContent[i].Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length <= (int)ResultsColumnName.MATCH_ID)
                {
                    continue;
                }

                if (tokens[(int)ResultsColumnName.MATCH_ID] == matchID)
                {
                    return tokens;
                }
            }
            return null;
        }

        private bool IsTwoSetsToWin(string[] tokens)
        {
            int firstPlayerSetsCount = 0;
            int secondPlayerSetsCount = 0;

            foreach (string token in tokens)
            {
                int scoreFirstPlayer = token[0] - '0';
                int scoreSecondPlayer = token[1] - '0';

                if (scoreFirstPlayer > scoreSecondPlayer)
                {
                    firstPlayerSetsCount++;
                }
                else
                {
                    secondPlayerSetsCount++;
                }
            }

            // Take the player who won more sets
            if (firstPlayerSetsCount < secondPlayerSetsCount)
            {
                firstPlayerSetsCount = secondPlayerSetsCount;
            }
            return firstPlayerSetsCount == 2;
        }
    }
}
