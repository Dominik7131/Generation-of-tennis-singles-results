using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using static Utility.MorphoditaGenerator;
using static Utility.MorphoditaTagger;
using static NewsGenerator.TextUtility;


namespace NewsGenerator
{
    class TemplateFiller
    {
        MessageType messageType;
        string template;
        bool[] templateCategories;
        Player[] players;
        MatchParameters match;
        bool isDebug;

        int winnerIndex;
        int loserIndex;
        int currentSet;

        Random random = new Random();
        StringBuilder resultBuilder;
        string[] words;
        int wordsIndex;
        bool isSentenceBeginning;
        bool isLastCharInterpunction;
        char lastChar;
        bool skipSentenceBeginningCheck;
        bool isPrepositionProcessed;
        bool isSwapWinnerAndLoser;

        bool isWinnerBetterRanked;
        bool[] isRankUsed;
        bool[] isNationNounUsed;
        bool[] isNationAdjectiveUsed;
        bool[,] isInSetPlayerNameUsed;
        bool areMale;
        bool[] isReferenceUsable; // False if player should be referenced by his last name only
        bool isDetourPhraseUsed; // [4_VÍTĚZ] -> hráče jménem [1_VÍTĚZ]

        static List<string> notInflectedTournamentParts = new List<string>
        {
            "Paris", "Indian", "Wells", "London"
        };

        static class SetInfoHolder
        {
            public static bool[] isSetPlayerNameUsed { get; set; }
        }

        public TemplateFiller(MessageType messageType, string template, bool[] templateCategories, MatchParameters match, bool swapWinnerAndLoser, bool isDebug)
        {
            this.messageType = messageType;
            this.template = template;
            this.templateCategories = templateCategories;
            this.players = match.players;
            this.match = match;
            this.isSwapWinnerAndLoser = swapWinnerAndLoser;
            this.isDebug = isDebug;

            // For shorter notation
            winnerIndex = Player.winnerIndex;
            loserIndex = Player.loserIndex;
            currentSet = match.currentSet;
            areMale = players[winnerIndex].isMale;

            isInSetPlayerNameUsed = new bool[match.score.Count, Player.playerCount];
            resultBuilder = new StringBuilder();

            isWinnerBetterRanked = players[winnerIndex].rank < players[loserIndex].rank;
            isRankUsed = new bool[Player.playerCount];
            isNationNounUsed = new bool[Player.playerCount];
            isNationAdjectiveUsed = new bool[Player.playerCount];
            isReferenceUsable = new bool[Player.playerCount];
            isDetourPhraseUsed = false;

            for (int i = 0; i < isReferenceUsable.Length; i++)
            {
                isReferenceUsable[i] = true;
            }

            if (currentSet == 1)
            {
                SetInfoHolder.isSetPlayerNameUsed = new bool[Player.playerCount];
            }

            // Do not use nation info if both players have the same one
            if (players[winnerIndex].nation == players[loserIndex].nation)
            {
                players[winnerIndex].nation = null;
                players[loserIndex].nation = null;
                players[winnerIndex].country = null;
                players[loserIndex].country = null;
            }
        }

        public bool Fill(out string result)
        {
            result = "";
            if (template == "")
            {
                return false;
            }

            if (isDebug)
            {
                string rawCategories = Categories.ConvertToNames(templateCategories);
                resultBuilder.Append($"{template}\n{rawCategories}\n\n");
            }

            isSentenceBeginning = true;
            skipSentenceBeginningCheck = false;
            words = template.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (wordsIndex = 0; wordsIndex < words.Length; wordsIndex++)
            {
                // Append space between words
                if (wordsIndex != 0)
                {
                    resultBuilder.Append(' ');
                }

                lastChar = words[wordsIndex][^1];
                isLastCharInterpunction = lastChar == '.' || lastChar == ',';

                if (words[wordsIndex].StartsWith('{'))
                {
                    ProcessVerb();
                }
                else if (!words[wordsIndex].StartsWith('['))
                {
                    resultBuilder.Append(words[wordsIndex]);
                    isSentenceBeginning = lastChar == '.';
                }
                else
                {
                    bool isPlaceHolderValid = ProcessPlaceholder();
                    if (!isPlaceHolderValid)
                    {
                        return false;
                    }
                }
            }
            result = resultBuilder.ToString();
            return true;
        }

        private void ProcessVerb()
        {
            if (isLastCharInterpunction)
            {
                words[wordsIndex] = words[wordsIndex][..^1];
            }
            string verb = words[wordsIndex][1..^1];

            bool isLemmaValid = GetLemma(verb, out string lemma);

            bool isWomenGender = !areMale;
            if (isWomenGender)
            {
                if (lemma.Contains("jít"))
                {
                    verb = $"{verb[..^2]}la";
                }
                else
                {
                    verb += 'a';
                }
            }

            AppendWord(verb);

            if (isLastCharInterpunction)
            {
                resultBuilder.Append(lastChar);
            }
            if (lastChar == '.')
            {
                isSentenceBeginning = true;
            }
        }

        private bool ProcessPlaceholder()
        {
            if (isLastCharInterpunction)
            {
                words[wordsIndex] = words[wordsIndex][..^1];
            }

            string content = words[wordsIndex][1..^1]; // Remove square brackets
            string[] parts = content.Split('_');

            string preposition = "";
            if (parts.Length > 2)
            {
                preposition = parts[1].ToLower();
            }

            int wordCase = parts[0][0] - '0';
            string keyWord = parts[^1];
            string secondMentionKeyWord = "ODKAZ";
            bool isSecondMention = keyWord.Contains(secondMentionKeyWord);

            if (keyWord.StartsWith("VÍTĚZ") || keyWord.StartsWith("PORAŽENÝ"))
            {
                bool isWinner = keyWord.StartsWith("VÍTĚZ");
                if (isSwapWinnerAndLoser)
                {
                    isWinner = !isWinner;
                }
                int playerIndex = isWinner ? 0 : 1;

                if (keyWord.Contains("SLOVO"))
                {
                    string word = "";
                    if (isWinner)
                    {
                        word = players[playerIndex].isMale ? "vítěz" : "vítězka";
                    }
                    else
                    {
                        word = players[playerIndex].isMale ? "poražený" : "poražená";
                    }
                    CreateWordForm(word, wordCase, out string newForm);
                    AppendWord(newForm);
                }
                else
                {
                    bool isNameValid = false;

                    if (messageType == MessageType.TITLE)
                    {
                        isNameValid = isSecondMention ? FillPlayerReference(isWinner, preposition, wordCase) : FillPlayerName(isWinner, preposition, wordCase);
                    }
                    else if (messageType == MessageType.RESULT)
                    {
                        if (!isSecondMention)
                        {
                            isNameValid = FillPlayerReference(isWinner, preposition, wordCase, isAppendIfFail: false);
                            bool isDetourPhraseShorterVersion = false; // "S hráčem z Ekvádoru HRÁČEM jménem Gonzalo Escobar" -> "S hráčem Ekvádoru jménem Gonzalo Escobar"
                            if (isReferenceUsable[playerIndex])
                            {
                                resultBuilder.Append(' ');
                                isDetourPhraseShorterVersion = true;
                            }
                            isNameValid = FillPlayerName(isWinner, preposition, wordCase, isDetourPhraseShorterVersion: isDetourPhraseShorterVersion);
                        }
                        else
                        {
                            isNameValid = isReferenceUsable[playerIndex] ? FillPlayerReference(isWinner, preposition, wordCase) : FillPlayerName(isWinner, preposition, wordCase, isLastNameOnly: true);
                        }
                    }
                    else if (messageType == MessageType.MATCHTITLE)
                    {
                        bool isNameAppended = messageType == MessageType.MATCHTITLE && SetInfoHolder.isSetPlayerNameUsed[playerIndex];
                        isNameValid = isNameAppended ? FillPlayerReference(isWinner, preposition, wordCase) : FillPlayerName(isWinner, preposition, wordCase);
                    }

                    if (!isNameValid)
                    {
                        return false;
                    }
                }
            }
            else if (keyWord.Contains("SKÓRESET"))
            {
                bool isScoreWinner = keyWord.Contains("VÍTĚZ");

                for (int i = 0; i < match.setsAggregation[match.setsAggregationIndex]; i++)
                {
                    int winnerSetScore = match.score[currentSet - 1 + i].scoreWinner;
                    int loserSetScore = match.score[currentSet - 1 + i].scoreLoser;

                    // If loser won this set, swap their score
                    if (winnerSetScore < loserSetScore)
                    {
                        int previousWinnerScore = winnerSetScore;
                        winnerSetScore = loserSetScore;
                        loserSetScore = previousWinnerScore;
                    }

                    if (i != 0)
                    {
                        if (i + 1 != match.setsAggregation[match.setsAggregationIndex])
                        {
                            resultBuilder.Append($", ");
                        }
                        else
                        {
                            resultBuilder.Append($" a ");
                        }
                    }
                    resultBuilder.Append(isScoreWinner ? $"{winnerSetScore}:{loserSetScore}" : $"{loserSetScore}:{winnerSetScore}");
                }
            }
            else if (keyWord == "SKÓREVÍTĚZ" || keyWord == "SKÓREPORAŽENÝ")
            {
                bool isScoreWinner = keyWord.Contains("VÍTĚZ");

                for (int x = 0; x < match.score.Count; x++)
                {
                    int higherScore = match.score[x].Item1;
                    int lowerScore = match.score[x].Item2;
                    if (match.score[x].Item2 > match.score[x].Item2)
                    {
                        int previousHigherScore = higherScore;
                        higherScore = lowerScore;
                        lowerScore = previousHigherScore;
                    }

                    resultBuilder.Append(isScoreWinner ? $"{higherScore}:{lowerScore}" : $"{lowerScore}:{higherScore}");

                    if (x + 1 < match.score.Count)
                    {
                        resultBuilder.Append(", ");
                    }
                }
            }
            else if (keyWord == "KOLO")
            {
                ProcessPreposition(preposition, match.round[0]);
                FillRound(match.round, isNextRound: false, wordCase);
            }
            else if (keyWord == "DALŠÍKOLO")
            {
                if (match.round[0] == "finále")
                {
                    return false;
                }

                string[] nextRound = GetNextRound(match.round);

                ProcessPreposition(preposition, nextRound[0]);
                FillRound(nextRound, isNextRound: true, wordCase);
            }
            else if (keyWord == "TURNAJTYP")
            {
                string tournamentWord = "okruh";
                string[] generalCategory = areMale ? new string[] { tournamentWord, "ATP" } : new string[] { tournamentWord, "WTA" };

                ProcessPreposition(preposition, tournamentWord);

                foreach (string word in generalCategory)
                {
                    CreateWordForm(word, wordCase, out string newForm);
                    AppendWord($"{newForm} ");
                }
            }
            else if (keyWord == "TURNAJ")
            {
                string[] words;
                if (match.tournament.category == null || match.tournament.category.Length == 0)
                {
                    words = new string[] { "turnaj" };
                }
                else
                {
                    words = match.tournament.category;
                }

                ProcessPreposition(preposition, words[0]);
                foreach (string word in words)
                {
                    CreateWordForm(word, wordCase, out string newForm);
                    AppendWord(newForm);
                }
            }
            else if (keyWord == "TURNAJNÁZEV")
            {
                if (preposition != "")
                {
                    if (preposition == "na")
                    {
                        preposition = "v";
                    }

                    if ((match.tournament.name[0] == "Roland" || match.tournament.name[0] == "Turnaj") && (preposition == "v"))
                    {
                        preposition = "na";
                    }

                    ProcessPreposition(preposition, match.tournament.name[0]);
                }

                if (match.tournament.name.Length == 2 && match.tournament.name[0] == "Roland" && match.tournament.name[1] == "Garros")
                {
                    wordCase = 1; // No flexion
                }

                bool isInflection = !match.isRealMatchInput;

                for (int x = 0; x < match.tournament.name.Length; x++)
                {
                    string tourNameFormed = match.tournament.name[x];

                    if (isInflection)
                    {
                        CreateWordForm(match.tournament.name[x], wordCase, out tourNameFormed);
                    }

                    AppendWord($"{tourNameFormed} ");
                }

                if (match.tournament.category != new string[] { "grandslam" })
                {
                    string tourType = areMale ? "ATP" : "WTA";
                    AppendWord($"{tourType}");
                }
            }
            else if (keyWord == "TURNAJMÍSTO")
            {
                if (match.tournament.city == null || match.tournament.city.Length == 0)
                {
                    return false; // Choose different template
                }

                string[] places = match.tournament.city;
                preposition = ProcessPlacePreposition(preposition, places[0]);
                ProcessPreposition(preposition, places[0]);
                for (int i = 0; i < places.Length; i++)
                {
                    string placeFormed = places[i];
                    if (!notInflectedTournamentParts.Contains(places[i]))
                    {
                        CreateWordForm(places[i], wordCase, out placeFormed);
                    }

                    AppendWord($"{placeFormed} ");
                }
            }
            else if (keyWord == "TURNAJPOVRCH")
            {
                if (match.tournament.surface == null || match.tournament.surface.Length == 0)
                {
                    return false; // Choose different template
                }

                string[] words = match.tournament.surface;

                ProcessPreposition(preposition, words[0]);

                for (int i = 0; i < words.Length; i++)
                {
                    string newForm = "";
                    if (words[i].EndsWith("ý"))
                    {
                        CreateWordForm(words[i], wordCase, out newForm, isAdjective: true);
                    }
                    else
                    {
                        CreateWordForm(words[i], wordCase, out newForm);
                    }
                    AppendWord($"{newForm} ");
                }
            }
            else if (keyWord == "DOBA")
            {
                if (preposition == "v")
                {
                    preposition = "za";
                }
                ProcessPreposition(preposition, match.length);
                AppendMatchLength(wordCase);
            }
            else if (keyWord == "DOBOVÝ")
            {
                ProcessPreposition(preposition, match.length);
                AppendMatchLength(wordCase, isAdjective: true);
            }
            else if (keyWord == "TITUL" || keyWord == "TITULVÍTĚZ" || keyWord == "TITULPORAŽENÝ")
            {
                string word = "titul";
                ProcessPreposition(preposition, word);
                CreateWordForm(word, wordCase, out string newForm);
                AppendWord(newForm);
            }
            else if (keyWord == "MEČBOLVÍTĚZ" || keyWord == "MEČBOLPORAŽENÝ" || keyWord == "BREJKBOL")
            {
                // No data from the input
                return false;
            }
            else if (keyWord == "SET")
            {
                string set = keyWord.ToLower();

                bool isSingular = true;
                bool allSetsSameCategory = false;
                if (messageType == MessageType.TITLE)
                {
                    ProcessPreposition(preposition, set);
                    resultBuilder.Append($"{match.score.Count} ");
                    isSingular = match.score.Count < 2;
                }
                else if (messageType == MessageType.MATCHTITLE && match.setsAggregationIndex < match.setsAggregation.Count)
                {
                    allSetsSameCategory = match.setsAggregation[match.setsAggregationIndex] == match.score.Count;
                    string word = "";

                    if (allSetsSameCategory)
                    {
                        if (match.score.Count == 2)
                        {
                            word = "obou";
                        }
                        else if (match.score.Count == 3)
                        {
                            word = "všech třech";
                        }
                        else
                        {
                            // Unfinished match
                            // E.g.: 5:2 -> "V prvním setu..."
                            allSetsSameCategory = false;
                        }
                    }

                    if (allSetsSameCategory)
                    {
                        ProcessPreposition(preposition, word);
                    }
                    else
                    {
                        ProcessPreposition(preposition, set);
                    }


                    if (allSetsSameCategory)
                    {
                        AppendWord($"{word} setech");
                    }
                    else if (match.setsAggregation[match.setsAggregationIndex] == 1)
                    {
                        resultBuilder.Append($"{currentSet}. ");
                    }
                    else if (match.setsAggregation[match.setsAggregationIndex] == 2)
                    {
                        resultBuilder.Append($"{currentSet}. a {(currentSet + 1)}. ");
                    }
                    else if (match.setsAggregation[match.setsAggregationIndex] == 3)
                    {
                        resultBuilder.Append($"{currentSet}., {currentSet + 1}. a {currentSet + 2}. ");
                    }
                }
                else
                {
                    ProcessPreposition(preposition, set);
                    resultBuilder.Append($"{currentSet}. ");
                }

                if ((messageType == MessageType.MATCHTITLE) && !allSetsSameCategory)
                {
                    CreateWordForm(set, wordCase, out string newForm, isSingular);
                    AppendWord(newForm);
                }
            }
            else if (keyWord == "SETOVÝPOČET")
            {
                string setCountAdjective = GetSetCountAdjective(match.score.Count);
                string setWordAdjective = "setový";

                ProcessPreposition(preposition, setCountAdjective);
                string nextWord = GetNextWord();
                bool isFemininum = IsWordFemininum(nextWord);
                CreateWordForm(setWordAdjective, wordCase, out string setWordAdjectiveFormed, isAdjective: true, isMale: !isFemininum);
                AppendWord(setCountAdjective);
                AppendWord(setWordAdjectiveFormed);
            }
            else if (keyWord == "KTERÝ" || keyWord == "JENŽ" || keyWord == "ON" || keyWord == "JEHO")
            {
                string pronoun = keyWord.ToLower();

                ProcessPreposition(preposition, pronoun);
                CreateWordForm(pronoun, wordCase, out string pronounFormed, isPronoun: true, isMale: areMale);
                AppendWord(pronounFormed);
            }

            resultBuilder.RemoveLast(' ');

            if (isSentenceBeginning && !skipSentenceBeginningCheck)
            {
                isSentenceBeginning = false;
            }
            if (!skipSentenceBeginningCheck)
            {
                skipSentenceBeginningCheck = false;
            }
            if (isLastCharInterpunction)
            {
                resultBuilder.Append(lastChar);
                if (lastChar == '.')
                {
                    isSentenceBeginning = true;
                }
            }
            isPrepositionProcessed = false;

            return true;
        }

        private bool FillPlayerName(bool isWinner, string preposition, int wordCase, bool isLastNameOnly = false, bool isDetourPhraseShorterVersion = false)
        {
            bool isValid = true;
            int playerIndex = isWinner ? 0 : 1;
            int startIndex = 0;

            if (messageType == MessageType.TITLE || (messageType == MessageType.MATCHTITLE && isReferenceUsable[playerIndex]) || isLastNameOnly)
            {
                // Append only surname
                if (messageType == MessageType.MATCHTITLE)
                {
                    SetInfoHolder.isSetPlayerNameUsed[playerIndex] = true;
                }
                startIndex = players[playerIndex].name.Length - 1;
                isValid = AppendName(players[playerIndex].name, startIndex, wordCase, preposition, ref isPrepositionProcessed) && isValid;
            }
            else
            {
                // Append whole name
                isValid = AppendName(players[playerIndex].name, startIndex, wordCase, preposition, ref isPrepositionProcessed) && isValid;
            }

            if (!isValid && !isDetourPhraseUsed)
            {
                // "Hráče jménem [1_...]"
                if (!isDetourPhraseShorterVersion)
                {
                    string playerWord = "hráč";
                    ProcessPreposition(preposition, playerWord);
                    CreateWordForm(playerWord, wordCase, out string playerWordFormated);
                    AppendWord(playerWordFormated);
                    AppendWord(" ");
                }
                string nextWord = "jménem";
                AppendWord(nextWord);
                AppendWord(" ");
                wordCase = 1;
                isValid = AppendName(players[playerIndex].name, startIndex, wordCase, preposition, ref isPrepositionProcessed);
                isDetourPhraseUsed = true;
            }

            return isValid;
        }

        private bool FillPlayerReference(bool isWinner, string preposition, int wordCase, bool isAppendIfFail = true)
        {
            int playerIndex = isWinner ? 0 : 1;
            bool isResultValid = true;

            if (!isReferenceUsable[playerIndex])
            {
                if (isAppendIfFail)
                {
                    // Append only last name
                    int startIndex = players[playerIndex].name.Length - 1;
                    bool isPrepositionChecked = false;
                    isResultValid = AppendName(players[playerIndex].name, startIndex, wordCase, preposition, ref isPrepositionChecked) && isResultValid;
                }
                return isResultValid;
            }

            Random random = new Random();
            int rank = players[playerIndex].rank;
            bool isNation = players[playerIndex].country != null;
            bool isPlayerBetterRanked = isWinner ? isWinnerBetterRanked : !isWinnerBetterRanked;


            if (isPlayerBetterRanked && !isRankUsed[playerIndex] && rank <= 10 && rank != 0)
            {
                string numberNoun = areMale ? GetMasculinumNounLemmaFromNumber(rank) : GetFemininumNounLemmaFromNumber(rank);
                string[] words = areMale ? new string[] { numberNoun, "hráč", "světový", "žebříček" } : new string[] { "světový", numberNoun };

                List<string> newPhrase = new List<string>();

                if (rank == 8)
                {
                    // For some reason Morphodita creates weird word form
                    isResultValid = false;
                }

                int previousWordCase = wordCase;

                for (int i = 0; i < words.Length; i++)
                {
                    if (i > 1)
                    {
                        wordCase = 2; // světového žebříčku
                    }

                    string newForm;
                    if (words[i].EndsWith('í') || words[i].EndsWith('ý')) // "První, druhý,..."
                    {
                        isResultValid = CreateWordForm(words[i], wordCase, out newForm, isAdjective: true, isMale: areMale) && isResultValid;
                    }
                    else
                    {
                        isResultValid = CreateWordForm(words[i], wordCase, out newForm) && isResultValid;
                    }
                    newPhrase.Add(newForm);
                }

                wordCase = previousWordCase;

                if (isResultValid)
                {
                    isRankUsed[playerIndex] = true;
                    ProcessPreposition(preposition, words[0]);

                    for (int i = 0; i < newPhrase.Count; i++)
                    {
                        AppendWord(newPhrase[i]);

                        if (i + 1 != newPhrase.Count)
                        {
                            resultBuilder.Append(' ');
                        }
                    }
                    return isResultValid;
                }

                // Otherwise something went wrong -> append different player reference
                isResultValid = true;
            }

            // Player noun references
            string playerNounReference = "";
            int randomNumber = random.Next(0, 3);

            if (randomNumber == 0)
            {
                if (players[playerIndex].age > 35)
                {
                    playerNounReference = areMale ? "veterán" : "veteránka";
                }
                else
                {
                    randomNumber++;
                }
            }

            if (randomNumber == 1)
            {
                playerNounReference = areMale ? "tenista" : "tenistka";
            }
            else if (randomNumber == 2)
            {
                playerNounReference = areMale ? "hráč" : "hráčka";
            }

            // Nation noun/adjective
            if (isNationAdjectiveUsed[playerIndex] && isNationNounUsed[playerIndex])
            {
                isNationAdjectiveUsed[playerIndex] = false;
                isNationNounUsed[playerIndex] = false;
                isRankUsed[playerIndex] = false;
                isNation = true;
            }

            bool isAdjective = random.Next(0, 2) == 0;
            isAdjective = isAdjective && !isNationAdjectiveUsed[playerIndex] && players[playerIndex].country != null;

            if (isNation)
            {
                if (isAdjective)
                {
                    // Českým hráčem
                    string nationAdjective = CountryToAdjective(players[playerIndex].country, areMale);
                    isResultValid = CreateWordForm(nationAdjective, wordCase, out string nationAdjectiveFormated, isAdjective: true, isMale: areMale);
                    isResultValid = CreateWordForm(playerNounReference, wordCase, out string playerNounReferenceFormated) && isResultValid;

                    if (isResultValid)
                    {
                        ProcessPreposition(preposition, nationAdjectiveFormated);
                        isNationAdjectiveUsed[playerIndex] = true;
                        AppendWord(nationAdjectiveFormated);
                        resultBuilder.Append(' ');
                        AppendWord(playerNounReferenceFormated);
                        return isResultValid;
                    }
                }

                if (!isAdjective || !isResultValid)
                {
                    // "Hráč z Česka"
                    isResultValid = CreateWordForm(playerNounReference, wordCase, out string playerNounReferenceFormated);
                    string newPreposition = "z";
                    int previousWordCase = wordCase;
                    wordCase = 2;
                    isResultValid = CreateWordForm(players[playerIndex].country, wordCase, out string nationFormated) && isResultValid;
                    wordCase = previousWordCase;

                    if (isResultValid)
                    {
                        ProcessPreposition(preposition, playerNounReferenceFormated);
                        isNationNounUsed[playerIndex] = true;
                        AppendWord(playerNounReferenceFormated);
                        resultBuilder.Append(' ');
                        ProcessPreposition(newPreposition, nationFormated, isForcePreposition: true);
                        AppendWord(nationFormated);
                        return isResultValid;
                    }
                }
            }

            // Everything failed append players last name as reference
            isReferenceUsable[playerIndex] = false;
            return FillPlayerReference(isWinner, preposition, wordCase, isAppendIfFail);
        }

        private void FillRound(string[] round, bool isNextRound, int wordCase)
        {
            string newForm = "";
            // MorphoDiTa cannot generate forms of "finále" but can generate "semifinále"
            if (round[0] == "finále")
            {
                // Generate forms of "semifinále" instead
                CreateWordForm("semifinále", wordCase, out newForm);
                // Remove "semi" from the beginning of the new form
                newForm = newForm[4..];
            }
            else if (char.IsDigit(round[0][0]) && round[1] == "kolo")
            {
                CreateWordForm(round[1], wordCase, out newForm);
                newForm = $"{round[0]} {newForm}";
            }
            else
            {
                CreateWordForm(round[0], wordCase, out newForm);
            }

            if (isSentenceBeginning)
            {
                isSentenceBeginning = false;
                newForm = char.ToUpper(newForm[0]) + newForm[1..];
            }
            resultBuilder.Append(newForm);
        }

        private void ProcessPreposition(string preposition, string wordAfterPreposition, bool isForcePreposition = false)
        {
            bool processPreposition = (preposition != "" && !isPrepositionProcessed) || isForcePreposition;
            if (processPreposition)
            {
                string newPreposition = FormatPreposition(preposition, wordAfterPreposition);

                if (isSentenceBeginning)
                {
                    newPreposition = char.ToUpper(newPreposition[0]) + newPreposition[1..];
                    isSentenceBeginning = false;
                }
                resultBuilder.Append(newPreposition + ' ');
            }
            isPrepositionProcessed = true;
        }

        public bool AppendNationNoun(string preposition, string nationNoun, int wordCase)
        {
            if (nationNoun == null)
            {
                return false;
            }

            ProcessPreposition(preposition, nationNoun);
            bool isValid = CreateWordForm(nationNoun, wordCase, out string newForm);

            newForm = CheckSentenceBeginning(newForm);
            resultBuilder.Append($"{newForm} ");
            return isValid;
        }

        public bool AppendNationAdjective(string preposition, string nationAdjective, int wordCase)
        {
            if (nationAdjective == null)
            {
                return false;
            }

            bool isAdjective = true;

            ProcessPreposition(preposition, nationAdjective);
            bool isCreated = CreateWordForm(nationAdjective, wordCase, out string newForm, isAdjective: isAdjective);

            if (!isCreated)
            {
                return false;
            }

            newForm = CheckSentenceBeginning(newForm);
            resultBuilder.Append($"{newForm} ");
            return true;
        }

        public void AppendRankAdjectives(string preposition, int rank, int wordCase)
        {
            ProcessPreposition(preposition, "světová");

            CreateWordForm("světová", wordCase, out string newForm, isAdjective: true, isMale: false);

            if (isSentenceBeginning)
            {
                newForm = char.ToUpper(newForm[0]) + newForm[1..];
            }
            resultBuilder.Append($"{newForm} ");

            bool isTennisAdjective = random.Next(0, 2) == 0;
            if (isTennisAdjective)
            {
                string tennisAdjectiv = "tenisový";
                CreateWordForm(tennisAdjectiv, wordCase, out newForm, isAdjective: true, isMale: false);
                resultBuilder.Append($"{newForm} ");
            }

            string numberLemma = GetFemininumNounLemmaFromNumber(rank);
            CreateWordForm(numberLemma, wordCase, out newForm);
            resultBuilder.Append($"{newForm} ");

            isSentenceBeginning = false;
        }

        private bool AppendName(string[] name, int startIndex, int wordCase, string preposition, ref bool isPrepositionChecked)
        {
            StringBuilder nameBuilder = new StringBuilder();

            for (int x = startIndex; x < name.Length; x++)
            {
                if (CreateWordForm(name[x], wordCase, out string nameFormed, isMale: areMale))
                {
                    ProcessPreposition(preposition, name[x]);
                    nameBuilder.Append($"{nameFormed} ");
                }
                else
                {
                    return false;
                }
            }

            AppendWord(nameBuilder.ToString());
            return true;
        }

        private void AppendMatchLength(int wordCase, bool isAdjective = false)
        {
            if (!match.length.Contains(':'))
            {
                resultBuilder.Append("[INVALID_TIME_FORMAT]");
                return;
            }

            string[] tokens = match.length.Split(':');
            bool isHourValid = int.TryParse(tokens[0], out int hours);
            bool isMinuteValid = int.TryParse(tokens[1], out int minutes);

            if ((!isHourValid || !isMinuteValid) || (hours < 0 || minutes < 0) || (hours == 0 && minutes == 0))
            {
                resultBuilder.Append("[INVALID_TIME]");
                return;
            }

            // Known issue: time is not always inflected properly
            // Other cases need to be added
            if (hours != 0 && wordCase == 4)
            {
                string hoursNewForm;
                if (hours == 1)
                {
                    hoursNewForm = "hodinu";
                }
                else if (hours == 2 || hours == 3 || hours == 4)
                {
                    hoursNewForm = "hodiny";
                }
                else
                {
                    hoursNewForm = "hodin";
                }
                resultBuilder.Append($"{hours} {hoursNewForm}");
            }
            else if (hours != 0 && wordCase == 6)
            {
                string hoursNewForm;
                if (hours == 1)
                {
                    hoursNewForm = "hodině";
                }
                else
                {
                    hoursNewForm = "hodinách";
                }
                resultBuilder.Append($"{hours} {hoursNewForm}");
            }

            if (hours != 0 && minutes != 0)
            {
                resultBuilder.Append(" a ");
            }

            if (minutes != 0)
            {
                string minutesNewForm;
                if (minutes == 1)
                {
                    minutesNewForm = "minutu";
                }
                else if (minutes == 2 || minutes == 3 || minutes == 4)
                {
                    minutesNewForm = "minuty";
                }
                else
                {
                    minutesNewForm = "minut";
                }
                resultBuilder.Append($"{minutes} {minutesNewForm}");
            }

            if (isAdjective)
            {
                // "Po dlouhém boji", "Po dlouhé bitvě"
                string word = "dlouhý";
                string nextWord = GetNextWord();
                bool isFemininum = IsWordFemininum(nextWord);
                CreateWordForm(word, wordCase, out string newForm, isAdjective: isAdjective, isMale: !isFemininum);
                AppendWord($" {newForm}");
            }
        }

        private void AppendWord(string word)
        {
            resultBuilder.Append(CheckSentenceBeginning(word));
        }

        private string CheckSentenceBeginning(string word)
        {
            if (isSentenceBeginning && word != "")
            {
                word = $"{char.ToUpper(word[0])}{word[1..]}";
                isSentenceBeginning = false;
            }
            return word;
        }

        private string GetNextWord()
        {
            if (wordsIndex + 1 == words.Length)
            {
                return "";
            }
            else
            {
                return words[wordsIndex + 1];
            }
        }
    }
}