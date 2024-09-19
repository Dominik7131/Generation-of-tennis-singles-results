using System;
using System.Collections.Generic;
using static TemplateExtractor.RecognizedKeyWords;
using static TemplateExtractor.Flags;
using static TemplateExtractor.TextProcessorInfo;
using static TemplateExtractor.TextProcessorUtility;
using static TemplateExtractor.Lists;
using static Utility.UDPipe;
using Utility;


namespace TemplateExtractor
{
    class TemplateCreator
    {
        private List<List<string[]>> sentences { get; set; }

        public TemplateCreator(List<List<string[]>> sentences)
        {
            this.sentences = sentences;
        }

        /// <summary>
        /// Creates template from the "sentences" with key words from RecognizedKeyWords class
        /// </summary>
        /// <returns></returns>
        public bool Create()
        {
            // Returns true if every placeholder was successfully created
            bool isTemplateValid = true;
            bool isLastWordPreposition = false;
            bool isPreviousWordPreposition = false;
            int prepositionLength = 0;

            bool isPreviousWordConjuction = false;
            int conjunctionLength = 0;

            string previousWord = "";
            string previousLemma = "";

            bool isSpaceBefore = false;
            int nameIndex = 0;
            int verbIndex = 0;
            int thisRoundIndex = 0;
            int nextRoundIndex = 1;
            int tourPlacesIndex = 0;
            int tourNamesIndex = 0;
            int scoreIndex = 0;
            bool isNameAppended = false;

            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    string[] tags = sentences[sentenceID][wordID];
                    string word = tags[WORD_INDEX];
                    string lemmaLower = tags[LEMMA_INDEX].ToLower();
                    string uPosTag = tags[U_POS_TAG_INDEX];
                    string feats = tags[FEATURES_INDEX];
                    string depRel = tags[DEP_REL_INDEX];
                    string misc = tags[MISC_INDEX];
                    int wordCase = GetWordCase(sentences, sentenceID, wordID);

                    // Remove marked words
                    if (wordsToDelete.Contains((sentenceID, wordID)))
                    {
                        // ToDo: Remove item from tournaments when adding it into deleted words
                        if (tourPlacesIndex < tournamentPlaces.Count && tournamentPlaces[tourPlacesIndex].Item1 == sentenceID && tournamentPlaces[tourPlacesIndex].Item2 == wordID)
                        {
                            tourPlacesIndex++;
                        }
                        if (tourNamesIndex < tournamentNames.Count && tournamentNames[tourNamesIndex].Item1 == sentenceID && tournamentNames[tourNamesIndex].Item2 == wordID)
                        {
                            tourNamesIndex++;
                        }
                        continue;
                    }

                    isPreviousWordPreposition = isLastWordPreposition;
                    isLastWordPreposition = false;

                    if (messageType == MessageType.MATCHTITLE)
                    {
                        if (matchInvalidNouns.Contains(lemmaLower))
                        {
                            return false;
                        }
                    }

                    // Remove any preposition in front of verb, preposition, conjunction, period or comma
                    if ((uPosTag == "VERB" || uPosTag == "ADP" || uPosTag == "CCONJ" || word == "." || word == ",") && isPreviousWordPreposition)
                    {
                        RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        if (templateBuilder.EndsWith(" "))
                        {
                            templateBuilder.Length--;
                        }
                    }

                    bool isPreviousWordComma = previousWord == ",";
                    // Remove conjunction or comma in front of period
                    if (word == "." && (isPreviousWordConjuction || isPreviousWordComma))
                    {
                        int spaceLength = 1;
                        if (templateBuilder.Length == conjunctionLength)
                        {
                            spaceLength = 0;
                        }
                        templateBuilder.Length -= conjunctionLength + spaceLength;
                    }

                    // Remove comma in front of period
                    if (word == "." && templateBuilder.Length > 0 && templateBuilder[^1] == ',')
                    {
                        templateBuilder.Length--;
                    }

                    // isSpaceBefore is set from the previous word
                    if (isSpaceBefore && word != "," && word != "." && templateBuilder.Length > 0)
                    {
                        templateBuilder.Append(' ');
                    }
                    isSpaceBefore = !misc.StartsWith("SpaceAfter=No");

                    if (nameIndex < playerNames.Count && playerNames[nameIndex].sentenceID == sentenceID && playerNames[nameIndex].wordID == wordID)
                    {
                        isTemplateValid = CreateNameTemplate(nameIndex, tags, wordCase, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                        wordID += playerNames[nameIndex].length - 1;
                        isSpaceBefore = true;
                        nameIndex++;
                        isNameAppended = true;
                    }
                    else if (verbIndex < verbsSingleGender.Count && verbsSingleGender[verbIndex].sentenceID == sentenceID && verbsSingleGender[verbIndex].wordID == wordID)
                    {
                        CreateVerbTemplate(verbIndex, sentenceID, word, lemmaLower, feats);
                        verbIndex++;
                    }
                    else if (rounds[thisRoundIndex] != null && rounds[thisRoundIndex].Item1 == sentenceID && rounds[thisRoundIndex].Item2 == wordID)
                    {
                        isTemplateValid = CreateRoundTemplate(isNextRound: false, sentenceID, wordID, word, wordCase, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        isSpaceBefore = true;
                    }
                    else if (rounds[nextRoundIndex] != null && rounds[nextRoundIndex].Item1 == sentenceID && rounds[nextRoundIndex].Item2 == wordID)
                    {
                        isTemplateValid = CreateRoundTemplate(isNextRound: true, sentenceID, wordID, word, wordCase, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        isSpaceBefore = true;
                    }
                    else if (tourPlacesIndex < tournamentPlaces.Count && tournamentPlaces[tourPlacesIndex].sentenceID == sentenceID && tournamentPlaces[tourPlacesIndex].wordID == wordID)
                    {
                        isTemplateValid = CreateTournamentTemplate(tourPlacesIndex, sentenceID, wordID, lemmaLower, wordCase, isName: false, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                        isSpaceBefore = true;
                        wordID += tournamentPlaces[tourPlacesIndex].length - 1;
                        tourPlacesIndex++;

                        if ((messageType == MessageType.MATCHTITLE) && isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (tourNamesIndex < tournamentNames.Count && tournamentNames[tourNamesIndex].sentenceID == sentenceID && tournamentNames[tourNamesIndex].wordID == wordID)
                    {
                        isTemplateValid = CreateTournamentTemplate(tourNamesIndex, sentenceID, wordID, lemmaLower, wordCase, isName: true, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                        isSpaceBefore = true;
                        wordID += tournamentNames[tourNamesIndex].length - 1;
                        tourNamesIndex++;

                        if ((messageType == MessageType.MATCHTITLE) && isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (scoreIndex < scores.Count && scores[scoreIndex].sentenceID == sentenceID && scores[scoreIndex].wordID == wordID)
                    {
                        string placeholder = scores[scoreIndex].isScoreWinner ? $"[1_SKÓREVÍTĚZ]" : $"[1_SKÓREPORAŽENÝ]";
                        templateBuilder.Append(placeholder);

                        wordID += "0:0".Length - 1;
                        isSpaceBefore = true;
                        scoreIndex++;
                    }
                    else if (tournamentLemmas.Contains(lemmaLower))
                    {
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            string placeholderName = "TURNAJ";
                            isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        }
                        else if (isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (lemmaLower == "okruh" || lemmaLower == "kategorie")
                    {
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            string placeholderName = "TURNAJTYP";
                            isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        }
                        else if (isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (playerReferences.Contains((sentenceID, wordID)))
                    {
                        CreatePlayerReferenceTemplate(sentenceID, wordID, wordCase, isPreviousWordPreposition, previousWord, previousLemma);
                    }
                    else if (surfacesLemmas.Contains(lemmaLower))
                    {
                        string placeholderName = "TURNAJPOVRCH";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                    }
                    else if (lemmaLower == "vítěz" || lemmaLower == "vítězka" || lemmaLower == "šampion" || lemmaLower == "šampionka" || lemmaLower == "mistr" || lemmaLower == "mistrině")
                    {
                        string placeholderName = "VÍTĚZSLOVO";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                    }
                    else if (lemmaLower == "poražený")
                    {
                        string placeholderName = "PORAŽENÝSLOVO";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                    }
                    else if (lemmaLower == "hodina" || lemmaLower == "minuta")
                    {
                        // Vyhrál za hodinu a dvě minuty
                        // Hodina = 4th case, Minuta = 2nd case
                        if (wordCase == 2)
                        {
                            wordCase = 4;
                        }
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            string placeholderName = "DOBA";
                            isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        }
                        else if (isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (lemmaLower.EndsWith("hodinový"))
                    {
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            string placeholderName = "DOBOVÝ";
                            isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        }
                        else if (isPreviousWordPreposition)
                        {
                            RemovePreposition(prepositionLength, ref isPreviousWordConjuction);
                        }
                    }
                    else if (titlesLemmas.Contains(lemmaLower))
                    {
                        string placeholderName = isWinnerAndLoserSwapped ? "TITULPORAŽENÝ" : "TITULVÍTĚZ";
                        isTemplateValid = CreateTitleTemplate(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                    }
                    else if (lemmaLower == "mečbol")
                    {
                        categories[(int)Category.TURN] = true;
                        string placeholderName = isMatchBallWinner ? "MEČBOLVÍTĚZ" : "MEČBOLPORAŽENÝ";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                        if (placeholderName == "MEČBOLPORAŽENÝ")
                        {
                            categories[(int)Category.TURN] = true;
                        }
                    }
                    else if (lemmaLower == "set" || lemmaLower == "sada")
                    {
                        string placeholderName = "SET";
                        AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                        isTemplateValid = false;
                    }
                    else if (lemmaLower.EndsWith("setový"))
                    {
                        // ToDo: Check matchTitle in AppendPlaceholder function
                        // ToDo: Make list for unwanted placeholder names in matchTitles
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            string placeholderName = "SETOVÝPOČET";
                            isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                        }
                    }
                    else if (lemmaLower == "který" && feats.Contains("PronType=Int"))
                    {
                        string placeholderName = "KTERÝ";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                    }
                    else if (lemmaLower == "jenž" && (feats.Contains("PronType=Int") || depRel == "nsubj" || word == "níž"))
                    {
                        // Issue: Kvitová nestačila na Vinciovou, s níž za necelých padesát minut prohrála 2:6 a 0:6.
                        // -> pronoun "níž"
                        string placeholderName = "JENŽ";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                    }
                    else if (lemmaLower == "on" && (!feats.Contains("Gender=Masc,Neut") || feats.Contains("Variant=Short") || word == "ním"))
                    {
                        string placeholderName = "ON";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;

                    }
                    else if (lemmaLower == "jeho")
                    {
                        string placeholderName = "JEHO";
                        isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
                    }
                    else // Append word unchanged
                    {
                        if (uPosTag == "ADP")
                        {
                            isLastWordPreposition = true;
                            prepositionLength = word.Length;
                        }
                        else if (lemmaLower == "." && isPreviousWordPreposition)
                        {
                            isLastWordPreposition = false;
                        }

                        // ToDo: Check if previous is preposition and if so check its form
                        // E.g. Ve duelu -> V duelu
                        templateBuilder.Append(word);
                    }

                    if ((messageType == MessageType.MATCHTITLE) && templateBuilder != null)
                    {
                        if (templateBuilder.EndsWith("DALŠÍKOLO]") || templateBuilder.EndsWith("TITULVÍTĚZ]") || templateBuilder.EndsWith("TITULPORAŽENÝ]"))
                        {
                            return false;
                        }
                    }

                    if (templateBuilder.EndsWith(" "))
                    {
                        templateBuilder.Length--;
                    }

                    previousWord = word;
                    previousLemma = lemmaLower;

                    isPreviousWordConjuction = uPosTag == "CCONJ";
                    if (isPreviousWordConjuction)
                    {
                        conjunctionLength = word.Length;
                    }
                }
            }

            if (!isNameAppended || scores.Count > 1)
            {
                isTemplateValid = false;
            }
            return isTemplateValid;
        }

        private bool AppendPlaceholder(int wordCase, string placeholderName, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            if (wordCase < 1 || wordCase > 7)
            {
                templateBuilder.Append($"[INVALID_{placeholderName}]");
                return false;
            }
            if (isPreviousWordPreposition && prepositionLemmasToInsertInPlaceholders.Contains(previousLemma.ToLower()))
            {
                templateBuilder.Length -= (previousWord.Length + 1);
                templateBuilder.Append($"[{wordCase}_{previousLemma.ToUpper()}_{placeholderName}]");
            }
            else
            {
                templateBuilder.Append($"[{wordCase}_{placeholderName}]");
            }
            return true;
        }

        private void RemovePreposition(int prepositionLength, ref bool isPreviousWordConjuction)
        {
            if (templateBuilder.EndsWith(" "))
            {
                templateBuilder.Length--;
            }

            if (templateBuilder.Length < prepositionLength)
            {
                templateBuilder.Length = 0;
            }
            else
            {
                templateBuilder.Length -= prepositionLength;
            }

            int lastSpaceIndex = -1;
            if (templateBuilder.Length > 0)
            {
                lastSpaceIndex = templateBuilder.LastIndexOf(' ');
            }

            string lastWord;
            if (lastSpaceIndex == -1)
            {
                lastWord = templateBuilder.ToString();
            }
            else
            {
                lastWord = templateBuilder.ToString(lastSpaceIndex, templateBuilder.Length - lastSpaceIndex);
            }

            if (lastWord == " a")
            {
                isPreviousWordConjuction = true;
            }
        }

        private bool CreateNameTemplate(int nameIndex, string[] playerNameTags, int wordCase, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            bool isTemplateValid = true;

            // Check prepositions
            if (isPreviousWordPreposition)
            {
                string previousLemmaLower = previousLemma.ToLower();
                // "od" not added because: Prohrát po porážce OD někoho
                if (previousLemmaLower == "po")
                {
                    isTemplateValid = false;
                }
            }

            if (wordCase == 6)
            {
                // Typically tournament recognized as player name
                // E.g.: Tenistka Martincová vydřela v Bogotě postup do druhého kola -> [1_VÍTĚZ] {vydřel} [6_PORAŽENÝ] postup do [2_KOLO]
                return false;
            }

            if (nameIndex <= 1)
            {
                bool isWinner = isWinnerAndLoserSwapped;
                if (nameIndex == 1)
                {
                    isWinner = !isWinner;
                }
                string placeholderName = isWinner ? "PORAŽENÝ" : "VÍTĚZ";

                isTemplateValid = AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma) && isTemplateValid;
            }
            else
            {
                // Substitute existing placeholder, only change word case
                // E.g. Humbert odvrátil mečbol proti Danielu Evansovi. ... Humbert zvítězil.

                bool isNameFound = IsExistingPlayer(playerIndex: 0, playerNameTags, wordCase, isPreviousWordPreposition, previousWord, previousLemma, ref isTemplateValid);

                if (!isNameFound)
                {
                    isNameFound = IsExistingPlayer(playerIndex: 1, playerNameTags, wordCase, isPreviousWordPreposition, previousWord, previousLemma, ref isTemplateValid);

                    if (!isNameFound)
                    {
                        // No existing player found
                        templateBuilder.Append("[INVALID_THIRD_NAME]");
                        isTemplateValid = false;
                    }
                }
            }
            return isTemplateValid;
        }

        public bool IsExistingPlayer(int playerIndex, string[] playerNameTags, int wordCase, bool isPreviousWordPreposition, string previousWord, string previousLemma, ref bool isTemplateValid)
        {
            // ToDo: return true or false and rest should do the calling function
            isTemplateValid = true;

            string originalNameLemma = playerNameTags[LEMMA_INDEX];

            for (int x = 0; x < playerNames[playerIndex].Item3; x++)
            {
                string[] tags = sentences[playerNames[playerIndex].Item1][playerNames[playerIndex].Item2 + x];
                string lemma = tags[LEMMA_INDEX];

                if (originalNameLemma == lemma) //(playerNameParts[2] == lemma)
                {
                    if (wordCase < 1 || wordCase > 7)
                    {
                        templateBuilder.Append(isWinnerAndLoserSwapped ? $"[INVALID_PORAŽENÝ]" : $"[INVALID_VÍTĚZ]");
                        isTemplateValid = false;
                    }
                    else
                    {
                        string placeholderName = isWinnerAndLoserSwapped ? "PORAŽENÝ" : "VÍTĚZ";
                        AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                        //templateBuilder.Append($"[{wordCase}{placeholderName}");
                    }
                    return true;
                }
            }
            return false;
        }

        private bool CreateVerbTemplate(int verbIndex, int sentenceID, string word, string lemma, string feats)
        {
            // Issue: Cesta Štěpánka skončila -- do not convert into male gender
            // Solution: Check if subject is player
            // However: Belgické tenistce Kim Clijstersové se nevydařil první letošní turnaj -- turnaj is subject

            // We need to check if player or player mention in the same sentence is subject
            // However: Tenista Štěpánek vyhrál. -- "tenista" is subject but gets deleted because name is depending on this word
            // Quick solution: Check if player is in nominative
            // ToDo: Check sentence ID of the player and the verb
            bool isPlayerInNominative = playerTemplate[0].wordCase == 1 || (playerTemplate.Length > 1 && playerTemplate[1].wordCase == 1);

            // Dativ E.g. Dalším velkým překvapením se stalo vyřazení pátého nasazeného Thomase Mustera z Rakouska, který podlehl Angličanovi Timu Henmanovi.
            bool isPlayerInDativ = playerTemplate[0].wordCase == 3 || (playerTemplate.Length > 1 && playerTemplate[1].wordCase == 3);

            bool isPlayerMentionInNominative = false;
            foreach (var playerReference in playerReferences)
            {
                int wordCase = GetWordCase(sentences, playerReferences[0].sentenceID, playerReferences[0].wordID);
                if (playerReference.sentenceID == sentenceID && wordCase == 1)
                {
                    isPlayerMentionInNominative = true;
                    break;
                }
            }

            List<string> playerVerbs = new List<string> // Verbs that can only describe players
            {
                "vyhrát", "prohrát", "zvítězit", "podlehnout"
            };
            bool isPlayerVerb = playerVerbs.Contains(lemma);

            bool isPlayerSubject = isPlayerInNominative || isPlayerInDativ || isPlayerMentionInNominative;

            if (isPlayerSubject)
            {
                if (verbsSingleGender[verbIndex].Item3 == true) // Verb in male gender
                {
                    templateBuilder.Append($"{{{word}}}"); // Vyhrál -> {Vyhrál}
                }
                else // Individual exceptions
                {
                    bool isPolarityNegative = feats.Contains("Polarity=Neg");
                    if (lemma.EndsWith("jít")) // prošel, přišel, vyšel, šel...
                    {
                        string prefix = lemma[..^3];
                        if (isPolarityNegative)
                        {
                            templateBuilder.Append($"{{ne{prefix}šel}}");
                        }
                        else
                        {
                            templateBuilder.Append($"{{{prefix}šel}}");
                        }
                    }
                    else
                    {
                        templateBuilder.Append($"{{{word[0..^1]}}}"); // E.g.: vyhrála -> {vyhrál}
                    }
                }
            }
            else
            {
                templateBuilder.Append(word);
            }
            return true;
        }

        private bool CreateTournamentTemplate(int tourIndex, int sentenceID, int wordID, string lemma, int wordCase, bool isName, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            string lemmaLower = lemma.ToLower();
            string placeholderName = isName ? "TURNAJNÁZEV" : "TURNAJMÍSTO";
            // získal první titul z turnaje Masters -> {získal} [TITUL] [2_Z_TURNAJ]
            // ale asi lepší bude {získal} [TITUL] [2_Z_TURNAJ] [2_TURNAJTYP]
            if ((lemmaLower == "masters" || lemmaLower == "challenger"))
            {
                // Can be improved by trying to recover word case probably from previous word if it is "turnaj"
                if (wordCase >= 1 && wordCase <= 7)
                {
                    placeholderName = "TURNAJTYP";
                    AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                }
                return true;
            }

            List<(int, int, int)> tournaments = isName ? tournamentNames : tournamentPlaces;

            // Defined word case from other parts of tournaments is usually wrong
            // Instead, try looking at previous word if it is a preposition
            // E.g. NA Roland Garros
            // Note: Cannot check previous word in general because this can happen:
            // Španělský tenista Rafael Nadal se stal počtvrté v kariéře šampionem US Open.
            // -> šampionem = 7th word case, US Open = 2nd word case
            if ((wordCase < 1 || wordCase > 7) && wordID > 0 && isPreviousWordPreposition)
            {
                wordCase = GetWordCase(sentences, tournaments[tourIndex].Item1, wordID - 1);
            }


            bool isTemplateValid = true;

            if (wordCase >= 1 && wordCase <= 7) // Case defined
            {
                if (messageType != MessageType.MATCHTITLE)
                {
                    AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                }
                else if (templateBuilder.Length > 0 && templateBuilder.EndsWith(" "))
                {
                    templateBuilder.Length--;
                }
            }
            else // Invalid case -- template cannot be used
            {
                templateBuilder.Append($"[INVALID_{placeholderName}]");
                isTemplateValid = false;
            }
            return isTemplateValid;
        }

        private bool CreateRoundTemplate(bool isNextRound, int sentenceID, int wordID, string word, int wordCase, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            isNextRound = isNextRound && !isNoNextRound;
            int roundIndex = isNextRound ? 1 : 0;
            int roundLength = 1;
            string placeholderName = isNextRound ? "DALŠÍKOLO" : "KOLO";

            // First word does not have defined case -- Try to find it in other parts
            if (wordCase < 1 || wordCase > 7)
            {
                for (int x = 1; x < roundLength; x++)
                {
                    wordCase = GetWordCase(sentences, sentenceID, wordID + x);

                    // Found defined case
                    if (wordCase >= 1 && wordCase <= 7)
                    {
                        break;
                    }
                }
            }

            if (wordCase < 1 || wordCase > 7)
            {
                // Invalid word case -- template cannot be used
                placeholderName = roundIndex == 0 ? "KOLO" : "DALŠÍKOLO";
                templateBuilder.Append($"[INVALID_{placeholderName}]");
                return false;
            }
            else
            {
                if (roundIndex == 0)
                {
                    if (isNextRoundOnly)
                    {
                        placeholderName = "DALŠÍKOLO";
                        AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                    }
                    else
                    {
                        if (messageType != MessageType.MATCHTITLE)
                        {
                            placeholderName = "KOLO";
                            AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                        }
                        else
                        {
                            placeholderName = "SET";
                            AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);

                            if (isPreviousWordPreposition && previousLemma != "v")
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    placeholderName = "DALŠÍKOLO";
                    AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
                }
            }
            return true;
        }

        private void CreatePlayerReferenceTemplate(int sentenceID, int wordID, int wordCase, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            bool isWinnerReference = wordCase == 1;

            string head = sentences[sentenceID][wordID][HEAD_INDEX];
            int.TryParse(head, out int headID);
            string headUPosTag = sentences[sentenceID][headID][U_POS_TAG_INDEX];
            bool isHeadVerb = headUPosTag == "VERB";

            if (!isHeadVerb)
            {
                Console.WriteLine("Warning: Parent of player reference is not verb");
            }

            string headLemma = sentences[sentenceID][headID][LEMMA_INDEX];
            string headFeats = sentences[sentenceID][headID][FEATURES_INDEX];
            bool isPolarityNegative = headFeats.Contains("Polarity=Neg");
            bool isSubjectLoser = verbsSubjectLoser.Contains(headLemma) || (isPolarityNegative && verbsSubjectLoserNegativePolarity.Contains(headLemma));


            if (isSubjectLoser)
            {
                isWinnerReference = !isWinnerReference;
            }


            // bool isSentenceStart = template[template.Length - 2] == '.';
            // Usually in the beginning of the sentence is mentioned first mentioned player again
            // Issue: Veselý vyřadil na turnaji Tennyse Sandgrena. Američana porazil 6:4 a 7:6.

            string placeholderName = isWinnerReference ? "VÍTĚZODKAZ" : "PORAŽENÝODKAZ";
            AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
        }

        private bool CreateTitleTemplate(int wordCase, string placeholderName, bool isPreviousWordPreposition, string previousWord, string previousLemma)
        {
            if (isPreviousWordPreposition)
            {
                string previousLemmaLower = previousLemma.ToLower();
                if (previousLemmaLower == "o")
                {
                    categories[(int)Category.SEMIFINAL] = true;
                }
                else if (previousLemmaLower == "po")
                {
                    // E.g.: Unwanted: Po triumfu v Nur-Sultanu skončil v Paříži v 1. kole
                    return false;
                }
            }
            AppendPlaceholder(wordCase, placeholderName, isPreviousWordPreposition, previousWord, previousLemma);
            return true;
        }

    }
}
