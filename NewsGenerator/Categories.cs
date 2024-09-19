using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NewsGenerator
{
    static class Categories
    {
        public static string[] categoryNames { get; } = { "", "začátek", "semifinále", "finále", "jednoznačný", "těsný", "otočení", "nečekaný", "nedokončený" };
        public static int categoryLength { get; } = Enum.GetNames(typeof(Category)).Length;
        public static bool[] originalCategories = new bool[categoryLength];
        public static bool[][] originalCategoriesSet; // Categories from parameters analysis

        static bool[][] currentCategories; // Currently set categories for titles and results
        static bool[] isFromTheRight;

        private const int ARTICLE_PARTS_LENGTH = 2; // Title and result
        private const int ONE_SIDED_GAME_PERCENTAGE_THRESHOLD = 75;
        private const int CLOSE_GAME_PERCENTAGE_THRESHOLD = 55;
        private const int UNEXPECTED_RANKING_DISTANCE = 57;

        public static void Init()
        {
            isFromTheRight = new bool[ARTICLE_PARTS_LENGTH];
            currentCategories = new bool[ARTICLE_PARTS_LENGTH][];
        }

        public static void Reset()
        {
            RestoreOriginalCategories(currentCategories[(int)MessageType.TITLE]);
            RestoreOriginalCategories(currentCategories[(int)MessageType.RESULT]);
            Templates.ResetAllTries();
        }

        public static void Analyze(List<(int scoreWinner, int scoreLoser)> score, Player[] players, string[] round)
        {
            originalCategoriesSet = new bool[score.Count][];

            // Analyze total score
            int gamesWinner = 0;
            int gamesLoser = 0;
            int setsWinner = 0;
            int set = 0;
            int winnerLostSets = 0;
            int closeSets = 0; // Sets where set was won by at most 2 points
            int winnerSetsWinStreak = 0;

            foreach ((int scoreWinner, int scoreLoser) in score)
            {
                originalCategoriesSet[set] = new bool[categoryLength];
                if (scoreWinner > scoreLoser)
                {
                    setsWinner++;
                    winnerSetsWinStreak++;
                }
                else
                {
                    winnerLostSets++;
                    winnerSetsWinStreak = 0;
                }

                if (Math.Abs(scoreWinner - scoreLoser) <= 2)
                {
                    closeSets++;
                }
                gamesWinner += scoreWinner;
                gamesLoser += scoreLoser;

                AnalyzeSet(set, scoreWinner, scoreLoser);
                set++;
            }

            SetCategories(score, players, round, gamesWinner, gamesLoser, winnerLostSets, closeSets, winnerSetsWinStreak);
            TurnOffUnwantedCombinations();
            currentCategories[(int)MessageType.TITLE] = new bool[categoryLength];
            currentCategories[(int)MessageType.RESULT] = new bool[categoryLength];
            RestoreOriginalCategories(currentCategories[(int)MessageType.TITLE]);
            RestoreOriginalCategories(currentCategories[(int)MessageType.RESULT]);
        }

        private static void TurnOffUnwantedCombinations()
        {
            int activeCategoriesCount = ActiveCategoriesCount(originalCategories);

            if (activeCategoriesCount > 2 && originalCategories[(int)Category.UNEXPECTED] && originalCategories[(int)Category.TURN])
            {
                originalCategories[(int)Category.TURN] = false;
                activeCategoriesCount--;
            }
            if (activeCategoriesCount > 2 && originalCategories[(int)Category.UNEXPECTED] && originalCategories[(int)Category.CLOSE])
            {
                originalCategories[(int)Category.CLOSE] = false;
                activeCategoriesCount--;
            }
            if (activeCategoriesCount > 2 && originalCategories[(int)Category.UNEXPECTED] && originalCategories[(int)Category.ONESIDED])
            {
                originalCategories[(int)Category.ONESIDED] = false;
                activeCategoriesCount--;
            }

            if (activeCategoriesCount > 2 && originalCategories[(int)Category.CLOSE] && originalCategories[(int)Category.TURN])
            {
                originalCategories[(int)Category.CLOSE] = false;
                activeCategoriesCount--;
            }

            if (activeCategoriesCount > 2 && originalCategories[(int)Category.RETIRED] && originalCategories[(int)Category.UNEXPECTED])
            {
                originalCategories[(int)Category.UNEXPECTED] = false;
                activeCategoriesCount--;
            }

            // Check categories compatibility
            if ((originalCategories[(int)Category.ONESIDED] && originalCategories[(int)Category.CLOSE])
                || (originalCategories[(int)Category.ONESIDED] && originalCategories[(int)Category.TURN])
                || (originalCategories[(int)Category.INTRO] && originalCategories[(int)Category.FINAL])
                || (originalCategories[(int)Category.INTRO] && originalCategories[(int)Category.SEMIFINAL])
                || (originalCategories[(int)Category.SEMIFINAL] && originalCategories[(int)Category.FINAL]))
            {
                MessageBox.Show("Error: Incompatible categories");
            }

            bool isDebug = false;
            if (isDebug)
            {
                string rawCategories = ConvertToNames(originalCategories);
                MessageBox.Show($"Kategorie zápasu: {rawCategories}\n");
            }
        }

        private static int ActiveCategoriesCount(bool[] categories)
        {
            int activeCategoriesCount = 0;
            if (categories[(int)Category.INTRO])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.SEMIFINAL])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.FINAL])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.ONESIDED])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.CLOSE])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.TURN])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.UNEXPECTED])
            {
                activeCategoriesCount++;
            }
            if (categories[(int)Category.RETIRED])
            {
                activeCategoriesCount++;
            }
            return activeCategoriesCount;
        }

        private static void AnalyzeSet(int set, int gamesWinner, int gamesLoser)
        {
            int higherNumber = gamesWinner;
            int lowerNumber = gamesLoser;
            if (gamesLoser > gamesWinner)
            {
                higherNumber = gamesLoser;
                lowerNumber = gamesWinner;
            }

            // ToDo: Use Utility function
            bool isScoreLessThanSix = gamesWinner < 6 && gamesLoser < 6;
            bool isScoreSixAndFive = gamesWinner == 6 && gamesLoser == 5;
            bool isLastSetNotFinished = isScoreLessThanSix || isScoreSixAndFive;
            if (isLastSetNotFinished)
            {
                originalCategoriesSet[set][(int)Category.RETIRED] = true;
                return;
            }

            if (higherNumber >= 7)
            {
                originalCategoriesSet[set][(int)Category.CLOSE] = true;
            }
            else if (lowerNumber <= 2)
            {
                originalCategoriesSet[set][(int)Category.ONESIDED] = true;
            }
        }

        private static void SetCategories(List<(int scoreWinner, int scoreLoser)> score, Player[] players, string[] round, int gamesWinner, int gamesLoser, 
            int winnerLostSets, int closeSets, int winnerSetsWinStreak)
        {
            double allWonGamesWinnerPercentage = (double)gamesWinner / (gamesWinner + gamesLoser) * 100;
            bool isOneSided = winnerLostSets == 0 && allWonGamesWinnerPercentage >= ONE_SIDED_GAME_PERCENTAGE_THRESHOLD;
            originalCategories[(int)Category.ONESIDED] = isOneSided;

            bool isAllSetsPlayed = (score.Count == 3 && winnerLostSets == 1) || (score.Count == 5 && winnerLostSets == 2);
            bool isClose = isAllSetsPlayed && closeSets >= 2 && allWonGamesWinnerPercentage <= CLOSE_GAME_PERCENTAGE_THRESHOLD;
            originalCategories[(int)Category.CLOSE] = isClose;

            bool isTurn = (score.Count == 3 && winnerSetsWinStreak == 2) || (score.Count >= 4 && winnerSetsWinStreak == 3);
            originalCategories[(int)Category.TURN] = isTurn;

            bool areRanksValid = players[Player.winnerIndex].rank != 0 && players[Player.loserIndex].rank != 0;
            originalCategories[(int)Category.UNEXPECTED] = areRanksValid && ((players[Player.winnerIndex].rank - players[Player.loserIndex].rank) >= UNEXPECTED_RANKING_DISTANCE);

            originalCategories[(int)Category.INTRO] = round.Length == 2 && round[0] == "1." && round[1] == "kolo";
            originalCategories[(int)Category.SEMIFINAL] = round.Length == 1 && round[0] == "semifinále";
            originalCategories[(int)Category.FINAL] = round.Length == 1 && round[0] == "finále";

            bool winnerWonLessThanOneTwoSets = score.Count - winnerLostSets <= 1;
            bool isLastSetNotFinished = (score[^1].scoreWinner < 6 && score[^1].scoreLoser < 6) || (score[^1].scoreWinner == 6 && score[^1].scoreLoser == 5);
            originalCategories[(int)Category.RETIRED] = isLastSetNotFinished || winnerWonLessThanOneTwoSets;
        }

        public static bool AreCategoriesSame(bool[] categories1, bool[] categories2)
        {
            for (int i = 0; i < categoryLength; i++)
            {
                if (categories1[i] != categories2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static void Change(MessageType messageType)
        {
            // Change current category combination
            // If categories were not turned off, start turning them off from the left
            // If categories are turned off, restore them and start turning them off from the right
            // Then start again from the first step

            int index = (int)messageType;

            int currentCategoriesCount = ActiveCategoriesCount(currentCategories[index]);
            int originalCategoriesCount = ActiveCategoriesCount(originalCategories);

            if (originalCategoriesCount == 2 && currentCategoriesCount == 1)
            {
                if (isFromTheRight[index])
                {
                    RestoreOriginalCategories(currentCategories[index]);
                }
            }

            if (currentCategoriesCount == 0)
            {
                RestoreOriginalCategories(currentCategories[index]);
                isFromTheRight[index] = false;
                // All combinations aready tried
                // Start again from the original category combination
                return;
            }

            // Turn off first category from the left because for two turned on categories it will be most probably one of the rounds categories
            // When there are not enough usable templates of this combination do the same from the right
            for (int i = 0; i < currentCategories[index].Length; i++)
            {
                int categoryIndex = isFromTheRight[index] ? (currentCategories[index].Length - i - 1) : i;
                if (currentCategories[index][categoryIndex])
                {
                    currentCategories[index][categoryIndex] = false;
                    break;
                }
            }
            isFromTheRight[index] = !isFromTheRight[index];
        }

        private static void RestoreOriginalCategories(bool[] categories)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                categories[i] = originalCategories[i];
            }
        }

        public static bool AreCategoriesSuitable(bool[] templateCategories, MessageType messageType, int currentSet = 0)
        {
            if (messageType == MessageType.TITLE || messageType == MessageType.RESULT)
            {
                bool[] current = currentCategories[(int)messageType];

                //  Categoris is not suitable if template does not contain chosen category
                if ((current[(int)Category.INTRO] && !templateCategories[(int)Category.INTRO])
                    || (current[(int)Category.SEMIFINAL] && !templateCategories[(int)Category.SEMIFINAL])
                    || (current[(int)Category.FINAL] && !templateCategories[(int)Category.FINAL])
                    || (current[(int)Category.ONESIDED] && !templateCategories[(int)Category.ONESIDED])
                    || (current[(int)Category.CLOSE] && !templateCategories[(int)Category.CLOSE])
                    || (current[(int)Category.TURN] && !templateCategories[(int)Category.TURN])
                    || (current[(int)Category.UNEXPECTED] && !templateCategories[(int)Category.UNEXPECTED])
                    || (current[(int)Category.RETIRED] && !templateCategories[(int)Category.RETIRED]))
                {
                    return false;
                }

                // Also we do not want from template to contain any extra category
                if ((!current[(int)Category.INTRO] && templateCategories[(int)Category.INTRO])
                    || (!current[(int)Category.SEMIFINAL] && templateCategories[(int)Category.SEMIFINAL])
                    || (!current[(int)Category.FINAL] && templateCategories[(int)Category.FINAL])
                    || (!current[(int)Category.ONESIDED] && templateCategories[(int)Category.ONESIDED])
                    || (!current[(int)Category.CLOSE] && templateCategories[(int)Category.CLOSE])
                    || (!current[(int)Category.TURN] && templateCategories[(int)Category.TURN])
                    || (!current[(int)Category.UNEXPECTED] && templateCategories[(int)Category.UNEXPECTED])
                    || (!current[(int)Category.RETIRED] && templateCategories[(int)Category.RETIRED]))
                {
                    return false;
                }

                return true;
            }
            else
            {
                // If we will have enough category combinations with intro category then we can express first set as the beginning
                // bool isFirstSetOnly = currentSet == 1 && setsAggregation[currentSet - 1] == 1;
                // if (isFirstSetOnly) { if (!templateCategories[(int)Category.INTRO]) { return false; } }

                // And for the last set setting final category
                // bool isLastSetOnly = currentSet == categoriesSet.Rank && setsAggregation[currentSet - 1] == 1;
                if ((originalCategoriesSet[currentSet - 1][(int)Category.ONESIDED] && !templateCategories[(int)Category.ONESIDED])
                    || (originalCategoriesSet[currentSet - 1][(int)Category.CLOSE] && !templateCategories[(int)Category.CLOSE])
                    || (originalCategoriesSet[currentSet - 1][(int)Category.RETIRED] && !templateCategories[(int)Category.RETIRED]))
                {
                    return false;
                }

                // Also we do not want from template to contain any extra category
                if ((!originalCategoriesSet[currentSet - 1][(int)Category.INTRO] && templateCategories[(int)Category.INTRO])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.SEMIFINAL] && templateCategories[(int)Category.SEMIFINAL])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.FINAL] && templateCategories[(int)Category.FINAL])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.ONESIDED] && templateCategories[(int)Category.ONESIDED])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.CLOSE] && templateCategories[(int)Category.CLOSE])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.TURN] && templateCategories[(int)Category.TURN])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.UNEXPECTED] && templateCategories[(int)Category.UNEXPECTED])
                    || (!originalCategoriesSet[currentSet - 1][(int)Category.RETIRED] && templateCategories[(int)Category.RETIRED]))
                {
                    return false;
                }

                return true;
            }
        }

        public static bool[] Parse(string rawCategories)
        {
            bool[] categories = new bool[Enum.GetNames(typeof(Category)).Length];

            string[] categoryTokens = rawCategories.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (string category in categoryTokens)
            {
                if (category == categoryNames[(int)Category.INTRO])
                {
                    categories[(int)Category.INTRO] = true;
                }
                else if (category == categoryNames[(int)Category.SEMIFINAL])
                {
                    categories[(int)Category.SEMIFINAL] = true;
                }
                else if (category == categoryNames[(int)Category.FINAL])
                {
                    categories[(int)Category.FINAL] = true;
                }
                else if (category == categoryNames[(int)Category.ONESIDED])
                {
                    categories[(int)Category.ONESIDED] = true;
                }
                else if (category == categoryNames[(int)Category.CLOSE])
                {
                    categories[(int)Category.CLOSE] = true;
                }
                else if (category == categoryNames[(int)Category.TURN])
                {
                    categories[(int)Category.TURN] = true;
                }
                else if (category == categoryNames[(int)Category.UNEXPECTED])
                {
                    categories[(int)Category.UNEXPECTED] = true;
                }
                else if (category == categoryNames[(int)Category.RETIRED])
                {
                    categories[(int)Category.RETIRED] = true;
                }
            }

            return categories;
        }

        public static string ConvertToNames(bool[] categories)
        {
            string rawCategories = "";

            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i])
                {
                    if (rawCategories == "")
                    {
                        rawCategories += categoryNames[i];
                    }
                    else
                    {
                        rawCategories += $" {categoryNames[i]}";
                    }
                }
            }

            if (rawCategories == "")
            {
                rawCategories = "NONE";
            }
            return rawCategories;
        }
    }
}
