using System;
using System.Collections.Generic;
using System.Text;
using static TemplateExtractor.Lists;
using static TemplateExtractor.RecognizedKeyWords;
using static TemplateExtractor.Flags;
using static TemplateExtractor.TextProcessorInfo;
using static TemplateExtractor.TextProcessorUtility;
using static Utility.UDPipe;
using Utility;


namespace TemplateExtractor
{
    public enum Category { NONE, INTRO, SEMIFINAL, FINAL, ONESIDED, CLOSE, UNEXPECTED, TURN, LENGTH, RETIRED }

    public class TextProcessor
    {
        private List<List<string[]>> sentences;
        private HashSet<string> templates;
        private string category = "";


        public TextProcessor()
        {
            playerReferences = new List<(int, int)>();
            verbsSingleGender = new List<(int, int, bool)>();
            templates = new HashSet<string>();
            isNoNextRound = rounds[0] != null && rounds[1] != null && rounds[0].Item3 == rounds[1].Item3; // Same round values
        }

        public string Process(string input)
        {
            // To simplify the code do mostly only one task per iteration through sentences
            // because readability is priority not performance and REST API requests take most of the time anyway

            bool isResponseValid = GetUDPipeResponse(input, out string debugMsg);
            if (!isResponseValid)
            {
                return isDebug ? debugMsg : "";
            }

            ProtectScore();
            ProcessNames(); // Create placeholder for winner and loser
            bool areVerbsValid = AnalyzeVerbs();
            if (!areVerbsValid)
            {
                return isDebug ? "Invalid verb\n" : "";
            }

            // First phase
            MarkPlayerNames();
            MarkWordsToDelete(); // Recursively delete all dependencies

            // Second phase
            MarkPlayerReferences();
            MarkWordsToDelete();

            // Last phase
            MarkRest();
            MarkWordsToDelete();

            TemplateCreator templateCreator = new TemplateCreator(sentences);
            bool isTemplateValid = templateCreator.Create();
            isTemplateValid = templateBuilder != null && isTemplateValid;

            CreateCategory();

            if (messageType == MessageType.MATCHTITLE)
            {
                bool areValid = CheckCategories();
                if (!areValid)
                {
                    return isDebug ? "Invalid category\n" : "";
                }
            }

            return PrepareTemplate(isTemplateValid);
        }

        private bool GetUDPipeResponse(string input, out string debugMsg)
        {
            bool isResponseValid = GetResponse(input, out Response response);
            if (!isResponseValid)
            {
                debugMsg = "Invalid UDPipe response";
                return false;
            }
            bool isParsedResponseValid = ParseResponse(response, messageType.ToString(), out sentences);
            if (!isParsedResponseValid)
            {
                debugMsg = "Invalid UDPipe parsed response";
                return false;
            }
            debugMsg = "";
            return true;
        }

        private void ProtectScore()
        {
            foreach (var (sentenceID, wordID, _) in scores)
            {
                protectedWords.Add((sentenceID, wordID));
            }
        }

        private bool IsAnimate(int sentenceID, int wordID)
        {
            string lemma = sentences[sentenceID][wordID][LEMMA_INDEX];
            string feats = sentences[sentenceID][wordID][FEATURES_INDEX];
            // "Favorit" is probably recognized as type of car and therefore set as inanimate
            bool isAnimate = feats.Contains("Animacy=Anim") || lemma == "favorit";
            return isAnimate;
        }

        private bool IsSingular(int sentenceID, int wordID)
        {
            string lemma = sentences[sentenceID][wordID][LEMMA_INDEX];
            string xPosTag = sentences[sentenceID][wordID][X_POS_TAG_INDEX];
            // UDPipe incorectly recognizes "jednoho obhájce" as plural noun
            bool isSingular = xPosTag[GRAMMATICAL_NUMBER_INDEX] == 'S' || lemma == "obhájce";
            return isSingular;
        }

        private void ProcessNames()
        {
            // Create placeholder for player
            for (int i = 0; i < playerNames.Count && i < playerTemplate.Length; i++)
            {
                int wordCase = GetWordCase(sentences, playerNames[i].sentenceID, playerNames[i].wordID);

                // Try to recover from unrecognized word case by finding word case at other name parts
                if (wordCase < 1 || wordCase > 7)
                {
                    for (int x = 1; x < playerNames[i].length && (wordCase < 1 || wordCase > 7); x++)
                    {
                        wordCase = GetWordCase(sentences, playerNames[i].sentenceID, playerNames[i].wordID + x);
                    }
                }

                // Try to recover from incorrectly recognized word case
                // Issue: E.g.: "Porazil Alberta Costu" -> "Porazil [2nd case] [4th case]" -> should be "Porazil [4th case] [4th case]"
                if (wordCase == 2)
                {
                    bool isOtherNamePartsWordCase4 = false;
                    for (int x = 1; x < playerNames[i].length; x++)
                    {
                        wordCase = GetWordCase(sentences, playerNames[i].sentenceID, playerNames[i].wordID + x);

                        isOtherNamePartsWordCase4 = wordCase == 4;
                        if (wordCase != 4)
                        {
                            break;
                        }
                    }
                    if (isOtherNamePartsWordCase4)
                    {
                        string xPosTag = sentences[playerNames[i].sentenceID][playerNames[i].wordID][X_POS_TAG_INDEX];
                        string newXPosTag = $"{xPosTag[0..WORD_CASE_INDEX]}{wordCase}{xPosTag[(WORD_CASE_INDEX + 1)..]}";
                        sentences[playerNames[i].sentenceID][playerNames[i].wordID][X_POS_TAG_INDEX] = newXPosTag;
                    }
                }

                if (wordCase < 1 || wordCase > 7) // Identification not successful
                {
                    playerTemplate[i].isValid = false;
                }
                else
                {
                    playerTemplate[i].isValid = true;
                    playerTemplate[i].wordCase = wordCase;

                    string xPosTag = sentences[playerNames[i].sentenceID][playerNames[i].wordID][X_POS_TAG_INDEX];
                    int originalWordCase = xPosTag[WORD_CASE_INDEX] - '0';
                    if (originalWordCase < 1 || originalWordCase > 7)
                    {
                        // Update newly found word case
                        string newXPosTag = $"{xPosTag[0..WORD_CASE_INDEX]}{wordCase}{xPosTag[(WORD_CASE_INDEX + 1)..]}";
                        sentences[playerNames[i].sentenceID][playerNames[i].wordID][X_POS_TAG_INDEX] = newXPosTag;
                    }
                }
            }
        }

        private bool AnalyzeVerbs()
        {
            // Recognizes winner and loser
            // Saves verbs that can be used for one gender only

            bool isWinnerRecognized = false;
            bool checkRounds = rounds[1] == null;

            // If first mentioned player is not in first case and second mentioned player is then swap winner and loser
            // E.g.: "Federera porazil Berdych" -> "[4_PORAŽENÝ] porazil [1_VÍTĚZ]"
            if (playerTemplate[0].wordCase != 1 && playerTemplate[1].wordCase == 1)
            {
                isWinnerAndLoserSwapped = true;
            }

            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    string[] tags = sentences[sentenceID][wordID];
                    string word = tags[WORD_INDEX];
                    string lemma = tags[LEMMA_INDEX];
                    string uPosTag = tags[U_POS_TAG_INDEX];
                    string feats = tags[FEATURES_INDEX];

                    if (messageType == MessageType.MATCHTITLE)
                    {
                        if (uPosTag == "AUX" && lemma == "být")
                        {
                            return false;
                        }
                    }

                    if (uPosTag != "VERB" && uPosTag != "AUX")
                    {
                        continue;
                    }

                    if (sentenceID == 0 && wordID == 0)
                    {
                        // Verb is in the beginning of the sentence -> Invalid template
                        return false;
                    }

                    protectedWords.Add((sentenceID, wordID));
                    if (uPosTag == "AUX")
                    {
                        // Deal with predicate nouns ("přísudek jmenný se sponou")
                        string head = sentences[sentenceID][wordID][HEAD_INDEX];
                        int.TryParse(head, out int headID);
                        if (headID != 0)
                        {
                            protectedWords.Add((sentenceID, headID));
                        }

                        string headLemma = sentences[sentenceID][headID][LEMMA_INDEX];
                        if (headLemma == "zástupce")
                        {
                            return false;
                        }
                    }

                    if (messageType == MessageType.MATCHTITLE)
                    {
                        if (setInvalidVerbs.Contains(lemma))
                        {
                            return false;
                        }
                    }

                    bool isPolarityNegative = feats.Contains("Polarity=Neg");
                    CheckVerbCategories(lemma, isPolarityNegative);


                    if (word.EndsWith("la") && feats.Contains("Gender=Fem"))
                    {
                        verbsSingleGender.Add((sentenceID, wordID, false));
                    }
                    else if (word.EndsWith("l") && feats.Contains("Gender=Mas"))
                    {
                        verbsSingleGender.Add((sentenceID, wordID, true));
                    }

                    if ((!isPolarityNegative && matchbalLoserVerbLemmas.Contains(lemma))
                        || (isPolarityNegative && matchbalLoserVerbLemmasNegative.Contains(lemma)))
                    {
                        isMatchBallWinner = false;
                    }

                    if (!isWinnerRecognized)
                    {
                        bool isNegativeVerb = (!isPolarityNegative && verbsSubjectLoser.Contains(lemma))
                            || (isPolarityNegative && verbsSubjectLoserNegativePolarity.Contains(lemma));

                        if (isNegativeVerb)
                        {
                            isWinnerAndLoserSwapped = !isWinnerAndLoserSwapped;
                        }
                        isWinnerRecognized = true;
                    }

                    if (checkRounds)
                    {
                        // Check current and next round to solve this problem:
                        // "Berdych porazil Djokoviče a postoupil do finále" -> "[1_VÍTĚZ] porazil [2_PORAŽENÝ] a postoupil do [2_KOLO]"
                        // "[2_KOLO]" should be "[2_DALŠÍKOLO]"
                        bool isNextRoundVerb = lemma == "postoupit" || lemma == "probojovat" || (lemma == "zahrát" && messageType == MessageType.TITLE);
                        if ((isNextRoundVerb || (messageType == MessageType.TITLE && lemma == "být")) && rounds[0] != null && rounds[1] == null)
                        {
                            isNextRoundOnly = true;
                        }
                    }
                }
            }
            return true;
        }

        private void CheckVerbCategories(string lemma, bool isNegative)
        {
            if (!isNegative)
            {
                foreach ((string, Category) verbCategory in verbCategories)
                {
                    if (lemma == verbCategory.Item1)
                    {
                        categories[(int)verbCategory.Item2] = true;
                    }
                }
            }
            else
            {
                foreach ((string, Category) verbCategory in verbCategoriesNegative)
                {
                    if (lemma == verbCategory.Item1)
                    {
                        categories[(int)verbCategory.Item2] = true;
                    }
                }
            }
        }

        private void MarkRest()
        {
            int tourPlacesIndex = 0;
            int tourNamesIndex = 0;

            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    string[] tags = sentences[sentenceID][wordID];
                    string word = tags[WORD_INDEX];
                    string lemmaLower = tags[LEMMA_INDEX].ToLower();
                    string uPosTag = tags[U_POS_TAG_INDEX];
                    string feats = tags[FEATURES_INDEX];
                    int headID = GetHeadID(sentences, sentenceID, wordID);

                    // Mark verbs
                    if (uPosTag == "VERB" || uPosTag == "AUX")
                    {
                        heads.Add((sentenceID, wordID, UPosTagsToAvoidADPCCONJ));
                        continue;
                    }

                    if (lemmaLower == "hodina" || lemmaLower == "minuta")
                    {
                        heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));
                        protectedWords.Add((sentenceID, wordID));
                        if (headID != 0)
                        {
                            if (!protectedWords.Contains((sentenceID, headID)))
                            {
                                wordsToDelete.Add((sentenceID, headID));
                                heads.Add((sentenceID, headID, uPosTagsToAvoidADP));

                                if (isDeletedWordsDebug)
                                {
                                    string headWord = sentences[sentenceID][headID][WORD_INDEX];
                                    debugWordsDeleted.Add((headWord, word));
                                }
                            }
                        }
                    }
                    else if (titlesLemmas.Contains(lemmaLower))
                    {
                        heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));
                        protectedWords.Add((sentenceID, wordID));

                        categories[(int)Category.FINAL] = true;
                    }
                    else if (tourPlacesIndex < tournamentPlaces.Count && tournamentPlaces[tourPlacesIndex].Item1 == sentenceID && tournamentPlaces[tourPlacesIndex].Item2 == wordID) // Mark tournament parts
                    {
                        for (int x = 0; x < tournamentPlaces[tourPlacesIndex].Item3; x++)
                        {
                            int newWordID = wordID + x;
                            heads.Add((sentenceID, newWordID, UPosTagsToAvoidADPCCONJ));
                            protectedWords.Add((sentenceID, newWordID));
                        }

                        wordID += tournamentPlaces[tourPlacesIndex].Item3 - 1;
                        tourPlacesIndex++;
                    }
                    else if (tourNamesIndex < tournamentNames.Count && tournamentNames[tourNamesIndex].Item1 == sentenceID && tournamentNames[tourNamesIndex].Item2 == wordID) // Mark tournament parts
                    {
                        for (int x = 0; x < tournamentNames[tourNamesIndex].Item3; x++)
                        {
                            int newWordID = wordID + x;
                            heads.Add((sentenceID, newWordID, UPosTagsToAvoidADPCCONJ));
                            protectedWords.Add((sentenceID, newWordID));
                        }

                        // Do not remove preposition from dependency of parrent
                        // E.g.: "Celkově třetí titul vybojoval v hale Bercy"
                        // Do not remove preposition "v" in "v hale Bercey"
                        ProtectPrepositionChild(sentenceID, headID);

                        wordID += tournamentNames[tourNamesIndex].Item3 - 1;
                        tourNamesIndex++;
                    }
                    else if (roundLemmas.Contains(lemmaLower))
                    {
                        protectedWords.Add((sentenceID, wordID));
                        heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));
                        ProtectPrepositionChild(sentenceID, headID);
                    }
                    else if (lemmaLower == "od")
                    {
                        if (!IsName(sentenceID, headID))
                        {
                            wordsToDelete.Add((sentenceID, headID));
                        }
                    }

                    bool isTournamentSurfaceMention = uPosTag == "NOUN" && surfacesLemmas.Contains(lemmaLower);

                    if (isTournamentSurfaceMention || tournamentLemmas.Contains(lemmaLower))
                    {
                        heads.Add((sentenceID, wordID, UPosTagsToAvoidADPCCONJ));
                        protectedWords.Add((sentenceID, wordID));

                        if (tournamentLemmas.Contains(lemmaLower))
                        {
                            // "Vyhrát titul" -> category final
                            bool isHeadUPosTagVerb = sentences[sentenceID][headID][U_POS_TAG_INDEX] == "VERB";
                            if (isHeadUPosTagVerb && sentences[sentenceID][headID][LEMMA_INDEX] == "vyhrát")
                            {
                                categories[(int)Category.FINAL] = true;
                            }
                        }
                    }

                    bool isPolarityNegative = feats.Contains("Polarity=Neg");
                    CheckModifiers(lemmaLower, isPolarityNegative, sentenceID, wordID);

                    if (uPosTag == "DET" && lemmaLower != "svůj" && lemmaLower != "tento")
                    {
                        protectedWords.Add((sentenceID, wordID));
                    }

                    // Delete all remaining nouns and their dependencies
                    if (uPosTag == "NOUN" && !protectedWords.Contains((sentenceID, wordID)))
                    {
                        heads.Add((sentenceID, wordID, new List<string>()));
                        wordsToDelete.Add((sentenceID, wordID));

                        if (isDeletedWordsDebug)
                        {
                            debugWordsDeleted.Add((word, "loseAndDelete"));
                        }
                    }
                }
            }
        }

        private void CheckModifiers(string lemma, bool isNegative, int sentenceID, int wordID)
        {
            foreach ((string, Category) modifier in modifiers)
            {
                if (lemma == modifier.Item1)
                {
                    ProcessModifiers(modifier.Item2, sentenceID, wordID);
                    return;
                }
            }

            foreach ((string, Category) modifier in modifiersNegative)
            {
                if (lemma == modifier.Item1 && isNegative)
                {
                    ProcessModifiers(modifier.Item2, sentenceID, wordID);
                    return;
                }
            }

            foreach ((string, Category) modifier in modifiersEndsWith)
            {
                if (lemma.EndsWith(modifier.Item1))
                {
                    ProcessModifiers(modifier.Item2, sentenceID, wordID);
                    return;
                }
            }
        }

        private bool IsName(int sentenceID, int wordID)
        {
            // Checks if "sentenceID" and "wordID" in sentences responds to player name
            for (int i = 0; i < playerNames.Count; i++)
            {
                if (playerNames[i].sentenceID != sentenceID)
                {
                    continue;
                }

                for (int x = 0; x < playerNames[i].length; x++)
                {
                    if (sentenceID == playerNames[i].sentenceID && wordID == playerNames[i].wordID + x)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ProcessModifiers(Category modifierCategory, int sentenceID, int wordID)
        {
            categories[(int)modifierCategory] = true;
            protectedWords.Add((sentenceID, wordID));

            if (modifierCategory == Category.LENGTH)
            {
                heads.Add((sentenceID, wordID, uPosTagsToAvoidLength));
            }
            else
            {
                heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));
            }
        }

        private void MarkPlayerNames()
        {
            for (int i = 0; i < playerNames.Count; i++)
            {
                string[] tags = sentences[playerNames[i].sentenceID][playerNames[i].wordID];
                string word = tags[WORD_INDEX];

                for (int x = 0; x < playerNames[i].length; x++)
                {
                    if (x > 0)
                    {
                        tags = sentences[playerNames[i].sentenceID][playerNames[i].wordID + x];
                    }
                    int sentenceID = playerNames[i].sentenceID;
                    int wordID = playerNames[i].wordID + x;
                    int.TryParse(tags[HEAD_INDEX], out int wordHeadID);

                    // Remove words depending on name
                    // E.g. "Připravená Petra Kvitová" -> "Petra Kvitová" ("Připravená" depends on "Petra")
                    heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));
                    protectedWords.Add((sentenceID, wordID));

                    // If this deleted word is parent of some preposition we need to protect the preposition
                    ProtectPrepositionChild(sentenceID, wordHeadID);

                    if (wordHeadID == 0)
                    {
                        continue;
                    }

                    // Remove words that name depends on
                    // E.g.: "Tenistka Kvitová vyhrála" -> "[1_VÍTĚZ] vyhrála". ("Kvitová" depends on "Tenistka")
                    // However e.g.: "V utkání o titul s Plíškovou" -> "V o titul s [1_VÍTĚZ]" ("Plíšková" depends on "utkání")
                    // Solution: Cases must match
                    // However e.g.: "Muguruzaová se rozloučila PROHROU nad Kuzněcovovou" -> "[1_PORAŽENÝ] se {rozloučil} nad [7_VÍTĚZ]".
                    // Solution: Add special case for lemma "prohra" and "výhra"
                    // Also: "Překvapil VÍTĚZSTVÍM nad Čiličem" -> "{Překvapil} nad [7_PORAŽENÝ]"
                    int wordCase = GetWordCase(sentences, sentenceID, wordID);
                    string headLemma = sentences[sentenceID][wordHeadID][LEMMA_INDEX];
                    string headUPostag = sentences[sentenceID][wordHeadID][U_POS_TAG_INDEX];
                    int headWordCase = GetWordCase(sentences, sentenceID, wordHeadID);

                    List<string> headLemmaExceptions = new List<string> { "výhra", "prohra", "vítězství" };

                    if (headLemmaExceptions.Contains(headLemma))
                    {
                        protectedWords.Add((sentenceID, wordHeadID));
                        continue;
                    }

                    bool isSameCase = wordCase == headWordCase;

                    if (headUPostag == "NOUN" && isSameCase)
                    {
                        heads.Add((sentenceID, wordHeadID, uPosTagsToAvoidADP));
                        wordsToDelete.Add((sentenceID, wordHeadID));

                        if (isDeletedWordsDebug)
                        {
                            string headWord = sentences[sentenceID][wordHeadID][WORD_INDEX];
                            debugWordsDeleted.Add((headWord, word));
                        }
                    }
                }
            }
        }

        private void ProtectPrepositionChild(int parrentSentenceID, int parrentWordHeadID)
        {
            for (int wordID = 0; wordID < sentences[parrentSentenceID].Count; wordID++)
            {
                string[] tags = sentences[parrentSentenceID][wordID];
                string uPosTag = tags[U_POS_TAG_INDEX];

                if (uPosTag != "ADP")
                {
                    continue;
                }

                string head = tags[HEAD_INDEX];
                int.TryParse(head, out int headID);

                if (headID == parrentWordHeadID)
                {
                    protectedWords.Add((parrentSentenceID, wordID));
                }
            }
        }

        private void MarkPlayerReferences()
        {
            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    // Skip words that will be deleted or protected words
                    if (wordsToDelete.Contains((sentenceID, wordID)) || protectedWords.Contains((sentenceID, wordID)))
                    {
                        continue;
                    }

                    string word = sentences[sentenceID][wordID][WORD_INDEX];
                    string lemma = sentences[sentenceID][wordID][LEMMA_INDEX];
                    string uPosTag = sentences[sentenceID][wordID][U_POS_TAG_INDEX];
                    string xPosTag = sentences[sentenceID][wordID][X_POS_TAG_INDEX];
                    string feats = sentences[sentenceID][wordID][FEATURES_INDEX];
                    string head = sentences[sentenceID][wordID][HEAD_INDEX];

                    bool isAnimate = IsAnimate(sentenceID, wordID);
                    bool isSingular = xPosTag[3] == 'S' || lemma == "obhájce"; // UDPipe incorectly recognizes "jednoho obhájce" as plural noun

                    if (lemma == "Rusko" && word == "Ruska")
                    {
                        lemma = "Ruska";
                    }

                    bool isMasculine = feats.Contains("Gender=Masc");
                    bool isFeminine = feats.Contains("Gender=Fem") || lemma == "Ruska"; // "Ruska": lemma "Rusko" recognized as neutrum


                    bool isPlayerReference = (uPosTag == "NOUN" || uPosTag == "PROPN") &&
                                            ((isFeminine && (lemma.EndsWith("ka") || lemma.EndsWith("kyně")))
                                            || (isMasculine && isAnimate));
                        
                    // For femininum check if it is "Animate"
                    if (feats.Contains("Gender=Fem"))
                    {
                        isPlayerReference = isPlayerReference && (feminineAnimacyLemmas.Contains(lemma) || tennisFeminineLemmas.Contains(lemma));
                    }

                    int.TryParse(head, out int headID);

                    string headUPosTag = sentences[sentenceID][headID][U_POS_TAG_INDEX];
                    bool isHeadVerb = headUPosTag == "VERB";
                    string headLemma = sentences[sentenceID][headID][LEMMA_INDEX];

                    if (isHeadVerb && headLemma == "stát")
                    {
                        // E.g.: "Brazilec Gustavo Kuerten se stal vítězem."
                        // -> "vítězem" is not reference
                        isPlayerReference = false;
                        // However this words should not be deleted
                        protectedWords.Add((sentenceID, wordID));
                    }

                    bool isAdjectiveException = lemma == "nasazený" && isHeadVerb;
                    // Cannot do for adjectives something like this:
                    // if (isHeadVerb && uPosTag == "ADJ" && lemma.EndsWith("ný")) { isPlayerReference = true; }
                    // Because it would mark incorrectly some other adjectives and it would need another exceptions ("se stal KONEČNOU")

                    if (isPlayerReference && !isSingular)
                    {
                        wordsToDelete.Add((sentenceID, wordID));
                        heads.Add((sentenceID, wordID, new List<string>()));
                        continue;
                    }

                    if (isAdjectiveException || (isPlayerReference && isHeadVerb))
                    {
                        playerReferences.Add((sentenceID, wordID));
                        protectedWords.Add((sentenceID, wordID));
                        heads.Add((sentenceID, wordID, uPosTagsToAvoidADP));

                        // Delete tournaments depending on player reference
                        // E.g.: "Vítězka Roland Garros" -> "[1_VÍTĚZODKAZ]"
                        // Dependencies: "Roland" -> "Vítězka", "Garros" -> "Roland"
                        List<int> removeTournamentIndexes = new List<int>();
                        for (int x = 0; x < tournamentPlaces.Count; x++)
                        {
                            int headSentenceID = tournamentPlaces[x].Item1;
                            int tournamentWordID = tournamentPlaces[x].Item2;
                            string tournamentHead = sentences[headSentenceID][tournamentWordID][HEAD_INDEX];
                            int.TryParse(tournamentHead, out int headWordID);

                            if (headSentenceID == sentenceID && headWordID == wordID)
                            {
                                for (int y = 0; y < tournamentPlaces[x].Item3; y++)
                                {
                                    protectedWords.Remove((headSentenceID, tournamentWordID + y));
                                }
                                removeTournamentIndexes.Add(x);
                            }
                        }
                        for (int x = 0; x < removeTournamentIndexes.Count; x++)
                        {
                            tournamentPlaces.RemoveAt(removeTournamentIndexes[x]);
                        }
                    }
                }
            }
        }

        private bool IsNameParent(int childSentenceID, int childWordID)
        {
            // Checks if parent of word defined by "childSentenceID" and "childWordID" in sentences responds to player name
            for (int i = 0; i < playerNames.Count; i++)
            {
                if (playerNames[i].sentenceID != childSentenceID)
                {
                    continue;
                }

                for (int x = 0; x < playerNames[i].length; x++)
                {
                    string head = sentences[playerNames[i].sentenceID][playerNames[i].wordID + x][HEAD_INDEX];
                    int.TryParse(head, out int headID);
                    if (headID == childWordID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void MarkWordsToDelete()
        {
            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    string[] tags = sentences[sentenceID][wordID];
                    string word = tags[WORD_INDEX];
                    string lemma = tags[LEMMA_INDEX];
                    string uPosTag = tags[U_POS_TAG_INDEX];
                    string xPosTag = tags[X_POS_TAG_INDEX];
                    string feats = tags[FEATURES_INDEX];
                    string head = tags[HEAD_INDEX];

                    int.TryParse(head, out int headID);

                    List<(int, int, List<string>)> newHeads = new List<(int, int, List<string>)>();

                    foreach (var h in heads)
                    {
                        // Do not delete word if:

                        // Word does not depend on head which should not have any dependencies
                        if (sentenceID != h.Item1 || headID != h.Item2)
                        {
                            continue;
                        }

                        // UPosTag of the word is forbidden
                        bool avoidUPosTag = h.Item3.Contains(uPosTag);
                        if (avoidUPosTag)
                        {
                            continue;
                        }

                        bool isComma = word == ",";
                        if (isComma)
                        {
                            continue;
                        }

                        // Word is some key part such as player name
                        bool isWordProtected = protectedWords.Contains((sentenceID, wordID));
                        if (isWordProtected)
                        {
                            continue;
                        }

                        // Issue: "Na první titul na okruhu si musí počkat" -> "Na titul si musí počkat"
                        // First preposition ("Na") needs to stay, second preposition ("na") needs to be deleted
                        // Solution: Do not delete any preposition depending on any key word
                        string headLemma = sentences[sentenceID][headID][LEMMA_INDEX];
                        if (titlesLemmas.Contains(headLemma) && uPosTag == "ADP")
                        {
                            continue;
                        }

                        // Word is not already prepared to be deleted
                        bool willBeDeleted = wordsToDelete.Contains((sentenceID, wordID));
                        if (willBeDeleted)
                        {
                            // Add this word into heads if it is not there (because heads were cleared after first call of this method)
                            if (!heads.ContainsID(sentenceID, wordID))
                            {
                                newHeads.Add((sentenceID, wordID, new List<string>()));
                            }
                            break;
                        }

                        // If head is verb, delete only adverbs with NumType=Ord feats
                        // E.g.: "Medveděv poprvé zvítězil v Paříži" -> "[1_VÍTĚZ] {zvítězil} [6_V_MÍSTO]"
                        // Degree=Cmp
                        // E.g.: "Porazil za VÍCE než hodinu"
                        // Degree=Pos
                        // E.g.: "Porazil JEDNOZNAČNĚ"
                        // XPosTag starts with Db
                        // E.g. "DNES prohrál", however "SICE prohrával, ale NAKONEC vyhrál"
                        // "Rozhodl tie-break" (Dependency: "tie" -> "rozhodl")
                        List<string> exceptions = new List<string>()
                        {
                            "nakonec", "sice", "tak", "dále", "výborně", "dobře", "špatně", "tie"
                        };

                        string hUPostag = sentences[h.Item1][h.Item2][U_POS_TAG_INDEX];
                        bool isHeadVerb = hUPostag == "VERB";
                        if (isHeadVerb && (!feats.Contains("NumType=Ord") && !feats.Contains("Degree=Cmp") && !feats.Contains("Degree=Pos") && !xPosTag.StartsWith("Db") || exceptions.Contains(lemma)))
                        {
                            continue;
                        }

                        // Otherwise delete word
                        wordsToDelete.Add((sentenceID, wordID));
                        // Try to delete any child of the deleted word
                        newHeads.Add((sentenceID, wordID, new List<string>()));

                        if (isDeletedWordsDebug)
                        {
                            string hWord = sentences[h.Item1][h.Item2][WORD_INDEX];
                            debugWordsDeleted.Add((word, hWord));
                        }

                        break;
                    }

                    // If any word was added check recursive dependencies by reseting for-loop
                    if (newHeads.Count > 0)
                    {
                        for (int x = 0; x < newHeads.Count; x++)
                        {
                            heads.Add(newHeads[x]);
                        }
                        newHeads.Clear();
                        sentenceID = 0;
                        wordID = -1; // -1 because "wordID" will be immediately incremented in for loop
                    }
                }
            }
            heads.Clear();
        }

        private void CreateCategory()
        {
            string[] categoryNames = new string[categories.Length];
            // Category names should be loaded from some file to be less prone to errors
            categoryNames[(int)Category.INTRO] = "začátek";
            categoryNames[(int)Category.SEMIFINAL] = "semifinále";
            categoryNames[(int)Category.FINAL] = "finále";
            categoryNames[(int)Category.ONESIDED] = "jednoznačný";
            categoryNames[(int)Category.TURN] = "otočení";
            categoryNames[(int)Category.CLOSE] = "těsný";
            categoryNames[(int)Category.UNEXPECTED] = "nečekaný";
            categoryNames[(int)Category.LENGTH] = "doba";
            categoryNames[(int)Category.RETIRED] = "nedokončený";

            if (messageType == MessageType.RESULT)
            {
                // If score is not completed mark match as retired
                categories[(int)Category.RETIRED] = categories[(int)Category.RETIRED] && !isMatchCompleted;
            }

            int finalRoundPriority = 8;
            bool isFinalRound = rounds[0] != null && rounds[0].Item3 == finalRoundPriority && !isNextRoundOnly;

            for (int sentenceID = 0; sentenceID < sentences.Count; sentenceID++)
            {
                for (int wordID = 0; wordID < sentences[sentenceID].Count; wordID++)
                {
                    string[] tags = sentences[sentenceID][wordID];

                    string word = tags[WORD_INDEX];
                    string lemma = tags[LEMMA_INDEX];
                    string uPosTag = tags[U_POS_TAG_INDEX];
                    string features = tags[FEATURES_INDEX];

                    bool isPolarityNegative = features.Contains("Polarity=Neg");

                    // Skip deleted words
                    if (wordsToDelete.Contains((sentenceID, wordID)))
                    {
                        continue;
                    }

                    if (uPosTag == "VERB")
                    {
                        if (lemma == "dohrát" && isPolarityNegative)
                        {
                            categories[(int)Category.RETIRED] = true;
                        }
                        else if (lemma == "zaváhat" && isPolarityNegative)
                        {
                            categories[(int)Category.ONESIDED] = true;
                        }
                        else if (lemma == "dotáhnout" && isPolarityNegative)
                        {
                            categories[(int)Category.TURN] = true;
                        }

                        if (lemma == "vyhrát")
                        {
                            if (isFinalRound)
                            {
                                categories[(int)Category.FINAL] = true;
                            }
                        }

                        continue;
                    }
                }
            }

            bool isNextRound = false;
            string[] words = templateBuilder.ToString().Split();

            for (int i = 0; i < words.Length; i++)
            {
                bool endsWithPeriod = words[i].EndsWith('.');
                if (endsWithPeriod)
                {
                    words[i] = words[i][..^1];
                }

                if (messageType == MessageType.RESULT && (words[i].Contains("DOBA") || words[i].Contains("DOBOVÝ")))
                {
                    categories[(int)Category.LENGTH] = true;
                }

                // "Vyhrál poprvé Turnaj mistrů" -> "{vyhrál} [4_MÍSTO]" = final category
                if (i + 1 < words.Length && words[i] == "{vyhrál}" && (words[i + 1].Contains("MÍSTO]")))
                {
                    categories[(int)Category.FINAL] = true;
                }

                if (words[i].EndsWith("DALŠÍKOLO]"))
                {
                    isNextRound = true;
                }
            }

            // Issue: "{Zahájil} útok [4_NA_TITULVÍTĚZ]" recognized as final category
            // Solution: Check if category does not contain intro and if so set final to false
            // Also remove final category if "next round" placeholder is present
            if (categories[(int)Category.INTRO] || isNextRound)
            {
                categories[(int)Category.FINAL] = false;
            }

            // Turn already implies close match no need to have theese categories at the same time
            if (categories[(int)Category.TURN])
            {
                categories[(int)Category.CLOSE] = false;
            }

            // E.g.: "Bitvu o postup hladce vyhrál..." -> "těsný" + "jednoznačný" -> "jednoznačný"
            if (categories[(int)Category.CLOSE] && categories[(int)Category.ONESIDED])
            {
                categories[(int)Category.CLOSE] = false;
            }
            // E.g.: "Poražený nedotáhl cestu k titulu, hladce prohrál..." -> "otočení" + "jednoznačný" -> "jednoznačný"
            if (categories[(int)Category.TURN] && categories[(int)Category.ONESIDED])
            {
                categories[(int)Category.TURN] = false;
            }

            bool isRoundKnown = rounds[0] != null;
            categories[(int)Category.FINAL] = categories[(int)Category.FINAL] && (isFinalRound || !isRoundKnown);

            if (categories[(int)Category.INTRO])
            { 
                category += $"{categoryNames[(int)Category.INTRO]} "; 
            }
            if (categories[(int)Category.FINAL]) 
            { 
                category += $"{categoryNames[(int)Category.FINAL]} "; 
            }
            if (categories[(int)Category.SEMIFINAL] && !categories[(int)Category.FINAL] && (!isFinalRound || !isRoundKnown)) 
            { 
                category += $"{categoryNames[(int)Category.SEMIFINAL]} "; 
            }
            if (categories[(int)Category.TURN]) 
            { 
                category += $"{categoryNames[(int)Category.TURN]} "; 
            }
            if (categories[(int)Category.ONESIDED]) 
            { 
                category += $"{categoryNames[(int)Category.ONESIDED]} "; 
            }
            if (categories[(int)Category.CLOSE]) 
            { 
                category += $"{categoryNames[(int)Category.CLOSE]} ";
            }
            if (categories[(int)Category.RETIRED]) 
            { 
                category += $"{categoryNames[(int)Category.RETIRED]} "; 
            }
            if (categories[(int)Category.UNEXPECTED]) 
            { 
                category += $"{categoryNames[(int)Category.UNEXPECTED]} "; 
            }

            // Remove trailing space
            if (category.EndsWith(' '))
            {
                category = category[..^1];
            }
        }

        /// <summary>
        /// Tries to recover from errors from external tools
        /// </summary>
        /// <param name="debugMsg"></param>
        /// <returns>True if the template is valid</returns>
        private bool CheckTemplate(out string debugMsg)
        {
            // Check placeholders for set template
            if (messageType == MessageType.MATCHTITLE)
            {
                // Possible improvement: Do not check whole template but check it word by word
                // But because of the REST API calls it would have almost no effect on the execution time
                if (!templateBuilder.ToString().Contains("SET"))
                {
                    if (char.IsUpper(templateBuilder[0]))
                    {
                        templateBuilder = new StringBuilder($"[6_V_SET] {char.ToLower(templateBuilder[0])}{templateBuilder.ToString(1, templateBuilder.Length - 1)}");
                    }
                    else
                    {
                        templateBuilder = new StringBuilder($"[6_V_SET] {templateBuilder}");
                    }
                }
                if (!templateBuilder.ToString().Contains("SKÓRE"))
                {
                    if (templateBuilder[^1] != ' ')
                    {
                        templateBuilder.Append(isWinnerAndLoserSwapped ? $" [1_SKÓRESETPORAŽENÝ]" : $" [1_SKÓRESETVÍTĚZ]");
                    }
                    else
                    {
                        templateBuilder.Append(isWinnerAndLoserSwapped ? $"[1_SKÓRESETPORAŽENÝ]" : $"[1_SKÓRESETVÍTĚZ]");
                    }
                }

                if (templateBuilder.ToString().Contains("DO_SET") || templateBuilder.ToString().Contains("TITUL"))
                {
                    debugMsg = "Invalid preposition in set";
                    return false;
                }
            }

            bool isNoNameOrMention = playerNames.Count == 0 && playerReferences.Count == 0;
            if ((messageType == MessageType.MATCHTITLE) && isNoNameOrMention)
            {
                debugMsg = "No names or mentions found";
                return false;
            }


            string[] words = templateBuilder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            debugMsg = "";
            bool isSentenceBeginning = true;
            bool isTemplateChanged = false;

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0 && words[i][0] == '[' && words[i].Split('_').Length > 1 && words[i].Split('_')[1] == "OD") // Preposition implies too specific context
                {
                    if (!words[i].Contains("VÍTĚZ") && !words[i].Contains("PORAŽENÝ") && !words[i].Contains("KOLO") && !words[i].Contains("KTERÝ") && !words[i].Contains("JENŽ") && !words[i].Contains("ON"))
                    {
                        debugMsg = "Preposition \"OD\" before placeholder";
                        return false;
                    }
                }
                else if (words[i].Length > 0 && char.IsDigit(words[i][0]))
                {
                    // Any number in regular word implies too specific content -> invalidate template
                    // E.g.: "Williamsová 307. vítězstvím na grandslamu překonala Navrátilovou"
                    // -> "[1_VÍTĚZ] 307. vítězstvím [6_NA_TURNAJ] {překonal} [4_PORAŽENÝ]"
                    debugMsg = "Number in template";
                    return false;
                }
                else if (i + 1 < words.Length && words[i].Contains("TURNAJTYP") && words[i + 1].Contains("TURNAJ]"))
                {
                    // "Na turnaji kategorie challenger" -> "[6_NA_TURNAJ] [2_TURNAJTYP] [1_TURNAJ]" -> "[6_NA_TURNAJ] [2_TURNAJTYP]"
                    if (words[i + 1][^1] == '.' || words[i + 1][^1] == ',')
                    {
                        words[i] += words[i + 1][^1];
                    }
                    words[i + 1] = "";
                    isTemplateChanged = true;
                }
                else if (i + 1 < words.Length && !words[i].EndsWith('.') && !words[i].EndsWith(','))
                {
                    // Do not allow placeholder for player reference and player in a row
                    // Handle it by removing reference placeholder

                    if ((words[i].Contains("VÍTĚZODKAZ]") && words[i + 1].Contains("_VÍTĚZ]")) 
                        || (words[i].Contains("PORAŽENÝODKAZ]") && words[i + 1].Contains("_PORAŽENÝ]")))
                    {
                        // Keep the word case from reference
                        int wordCaseIndex = words[i].IndexOf('_') - 1;
                        int referenceWordCase = words[i][wordCaseIndex] - '0';

                        // Copy preposition from reference
                        string preposition = ParsePlaceholderPreposition(words[i]);
                        string formatedPreposition = "";
                        if (preposition != "")
                        {
                            formatedPreposition = $"_{preposition}";
                        }

                        if (!words[i + 1].Contains("INVALID"))
                        {
                            words[i + 1] = $"{words[i + 1][..wordCaseIndex]}{referenceWordCase}{formatedPreposition}{words[i + 1][(wordCaseIndex + 1)..]}";
                        }
                        words[i] = "";
                        isTemplateChanged = true;
                    }
                    else if ((words[i].Contains("_VÍTĚZ]") && words[i + 1].Contains("VÍTĚZODKAZ]")) 
                        || (words[i].Contains("_PORAŽENÝ]") && words[i + 1].Contains("PORAŽENÝODKAZ]")))
                    {
                        words[i + 1] = "";
                        isTemplateChanged = true;
                    }
                    else if ((words[i].Contains("VÍTĚZODKAZ]") && words[i + 1].Contains("_PORAŽENÝ]"))
                        || (words[i].Contains("PORAŽENÝODKAZ]") && words[i + 1].Contains("_VÍTĚZ]")))
                    {
                        // Try to recover from incorectly recognized player reference
                        // E.g.: "Tenistce Lucii Šafářové vyšel vstup do sezony na trávě".
                        // -> Template: [3_PORAŽENÝODKAZ] [3_VÍTĚZ] {vyšel} vstup [6_NA_TURNAJPOVRCH].
                        // ->                             [3_VÍTĚZ] {vyšel} vstup [6_NA_TURNAJPOVRCH].
                        int wordCaseIndex = words[i].IndexOf('_') - 1;
                        if (words[i][wordCaseIndex] == words[i + 1][wordCaseIndex])
                        {
                            int wordCase = words[i][wordCaseIndex] - '0';
                            // Copy preposition from reference
                            string preposition = ParsePlaceholderPreposition(words[i]);
                            string formatedPreposition = "";
                            if (preposition != "")
                            {
                                formatedPreposition = $"_{preposition}";
                            }
                            if (!words[i + 1].Contains("INVALID") && preposition != "")
                            {
                                words[i + 1] = $"{words[i + 1][..wordCaseIndex]}{wordCase}{formatedPreposition}{words[i + 1][(wordCaseIndex + 1)..]}";
                            }
                            words[i] = "";
                            isTemplateChanged = true;
                        }
                    }
                    else if (messageType == MessageType.MATCHTITLE && words[i] == "do" && words[i + 1].Contains("SET]"))
                    {
                        debugMsg = "Preposition \"DO\" in front of placeholder SET";
                        return false;
                    }
                    else if (words[i] == "do" && words[i + 1] == "[2_KOLO]")
                    {
                        words[i + 1] = "[2_DALŠÍKOLO]";
                        isTemplateChanged = true;
                    }
                    else if (words[i] == "ve" && (words[i + 1].StartsWith("duel") || words[i + 1].StartsWith("souboj") || words[i + 1].StartsWith("bitv") || words[i + 1].StartsWith("utkání")))
                    {
                        words[i] = "v";
                        isTemplateChanged = true;
                    }
                }

                if (i + 1 < words.Length && words[i].EndsWith('.'))
                {
                    bool isNextWordPlaceholder = words[i + 1].StartsWith('[') || words[i + 1].StartsWith('{');
                    bool isNextWordFirstLetterNotCapital = words[i + 1].Length != 0 && !char.IsUpper(words[i + 1][0]);
                    if (!isNextWordPlaceholder && isNextWordFirstLetterNotCapital)
                    {
                        // Fix first letter in the beginning of the sentece not uppercased
                        words[i + 1] = $"{char.ToUpper(words[i+1][0])}{words[i + 1][1..]}";
                        isTemplateChanged = true;
                    }
                }

                if ((words[i].StartsWith('[') && words[i][^1] != ']' && words[i][^2] != ']') 
                    || (words[i].StartsWith('{') && words[i][^1] != '}' && words[i][^2] != '}'))
                {
                    // E.g.: "[1_VÍTĚZ {vyhrál}"
                    debugMsg = "Malmormed placeholder";
                    return false;
                }

                if (i + 1 < words.Length && words[i].Contains("TITUL") && words[i][^1] != '.' && words[i][^1] != ',' && (words[i + 1].Contains("TURNAJTYP") || words[i + 1].Contains("TURNAJPOVRCH")))
                {
                    if (words[i + 1][^1] == '.' || words[i + 1][^1] == ',')
                    {
                        words[i] += words[i + 1][^1];
                    }
                    words[i + 1] = "";
                    isTemplateChanged = true;
                }

                // Check two placeholders in a row
                // If two same placeholders have the same word case delete the second one
                // E.g.: "Podlehla po dvou hodinách a 18 minutách" -> "{Podlehl} [6_PO_DOBA] [6_DOBA]" -> "{podlehl} [6_PO_DOBA]"
                // Otherwise invalidate the template
                // E.g.: "[2_MÍSTO] [6_MÍSTO]"
                if (i > 0 && !words[i - 1].EndsWith('.'))
                {
                    if (words[i - 1].StartsWith('[') && words[i].StartsWith('['))
                    {
                        int firstPlaceholderOffset = words[i - 1].LastIndexOf('_') + 1;
                        int secondPlaceholderOffset = words[i].LastIndexOf('_') + 1;

                        string firstPlaceholderKeyWord = words[i - 1][firstPlaceholderOffset..^1];
                        string secondPlaceHolderKeyWord = words[i][secondPlaceholderOffset..^1];

                        if (firstPlaceholderKeyWord.EndsWith(']'))
                        {
                            firstPlaceholderKeyWord = firstPlaceholderKeyWord[..^1];
                        }
                        if (secondPlaceHolderKeyWord.EndsWith(']'))
                        {
                            secondPlaceHolderKeyWord = secondPlaceHolderKeyWord[..^1];
                        }

                        if (firstPlaceholderKeyWord == secondPlaceHolderKeyWord)
                        {
                            // Check word case
                            int firstWordCaseIndex = words[i - 1].IndexOf('_') - 1;
                            int secondWordCaseIndex = words[i].IndexOf('_') - 1;

                            int firstWordCase = words[i - 1][firstWordCaseIndex] - '0';
                            int secondWordCase = words[i][secondWordCaseIndex] - '0';

                            bool isValidCase = firstWordCase >= 1 && firstWordCase <= 7 && secondWordCase >= 1  && secondWordCase <= 7;
                            bool placeholderException = firstPlaceholderKeyWord == "DOBA" || firstPlaceholderKeyWord == "MÍSTO";

                            // Remove second placeholder
                            if (isValidCase && (firstWordCase == secondWordCase || placeholderException))
                            {
                                if (words[i].EndsWith('.') || words[i].EndsWith(','))
                                {
                                    words[i - 1] += words[i][^1];
                                }

                                // E.g.: "[1_VÍTĚZODKAZ] [1_VÍTĚZ]" -> "[1_VÍTĚZ]"
                                if (firstWordCaseIndex == 2 && secondWordCaseIndex == 1)
                                {
                                    // Remove second mention from first placeholder
                                    words[i - 1] = $"{words[i - 1][..secondWordCaseIndex]}{words[i-1][firstWordCaseIndex..]}";
                                }

                                if (firstPlaceholderKeyWord == "MÍSTO")
                                {
                                    // Try to recover from incorect word case
                                    // E.g.: "Turnaj [1_V_MÍSTO] [6_NA_MÍSTO]" -> "Turnaj [6_V_MÍSTO]"
                                    // Most often place with 6th word case is the correct one
                                    if (firstWordCase == 1 && secondWordCase != 1)
                                    {
                                        words[i - 1] = $"{words[i - 1][..firstWordCase]}{secondWordCase}{words[i - 1][(firstWordCaseIndex + 1)..]}";
                                    }
                                }
                                words[i] = "";
                                isTemplateChanged = true;
                            }
                            else
                            {
                                debugMsg = "Two same placeholders in a row";
                                return false;
                            }
                        }
                    }
                }

                // Invalid phrase "za sebou"
                // Try to recover by deleting it
                if (i > 0 && words[i - 1] == "za" && words[i] == "sebou")
                {
                    words[i - 1] = "";
                    words[i] = "";
                    isTemplateChanged = true;
                }

                // Try to fix bad word case recognition for tournaments
                // E.g.: "Žitko Vyhrál turnaj Futures"
                // "[1_VÍTĚZ] {vyhrál} [4_TURNAJ] [1_TURNAJMÍSTO]" -> "[1_VÍTĚZ] {vyhrál} [4_TURNAJ] [4_TURNAJMÍSTO]"
                if (i + 1 < words.Length && words[i].StartsWith('[') && words[i][^1] != '.' && words[i + 1].StartsWith("[1"))
                {
                    if (words[i].Contains("TURNAJ") && words[i + 1].Contains("TURNAJ"))
                    {
                        int currentWordCase = words[i][1] - '0';

                        words[i + 1] = $"[{currentWordCase}{words[i + 1][2..]}";
                        int lastPlaceholderOffset = words[i].Length;
                        templateBuilder.Length -= lastPlaceholderOffset;
                        templateBuilder.Append(words[i]);
                        isTemplateChanged = true;
                    }
                }
                isSentenceBeginning = words[i].EndsWith('.');
            }

            if (isTemplateChanged)
            {
                // Construct new template from the words
                templateBuilder.Clear();

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "")
                    {
                        continue;
                    }

                    templateBuilder.Append(words[i]);
                    if (i + 1 < words.Length)
                    {
                        templateBuilder.Append(' ');
                    }
                }
            }

            string template = templateBuilder.ToString();
            if (template.Contains("1_TURNAJNÁZEV]") || template.Contains("1_TURNAJMÍSTO"))
            {
                // High probability of incorrectly recognized tournament word case = invalidate template
                return false;
            }

            return true;
        }

        private string ParsePlaceholderPreposition(string placeholder)
        {
            string[] placeholderParts = placeholder.Split('_');
            if (placeholderParts.Length < 3)
            {
                return "";
            }
            return placeholderParts[1];
        }

        private bool CheckCategories()
        {
            if (categories[(int)Category.TURN] || categories[(int)Category.SEMIFINAL] || categories[(int)Category.UNEXPECTED])
            {
                return false;
            }
            return true;
        }

        private string PrepareTemplate(bool isTemplateValid)
        {
            string debugMsg = "";

            if (isDebug && !isTemplateValid)
            {
                CheckTemplate(out debugMsg);
            }
            else
            {
                isTemplateValid = isTemplateValid && CheckTemplate(out debugMsg);
            }

            if (!isTemplateValid && !isDebug) 
            {
                return ""; 
            }

            string result = "";

            if (isDebug)
            {
                if (!isTemplateValid)
                {
                    result += "Invalid: ";
                }
            }

            templateBuilder.RemoveLast(' ');
            
            if (messageType == MessageType.TITLE)
            {
                templateBuilder.RemoveLast('.');
            }

            // If the previous first word was deleted capitalize current first word
            if (char.IsLower(templateBuilder[0]))
            {
                result += $"{char.ToUpper(templateBuilder[0])}{templateBuilder.ToString(1, templateBuilder.Length - 1)}";
            }
            else
            {
                result += templateBuilder.ToString();
            }

            // Fix sentence ending
            if ((messageType == MessageType.MATCHTITLE || messageType == MessageType.RESULT) && !templateBuilder.EndsWith("."))
            {
                result += '.';
            }

            result += $"\n{category}";
                 
            if (isDebug)
            {
                result += '\n';
                if (debugMsg != "")
                {
                    result += $"-> {debugMsg}\n";
                }
            }

            if (isDeletedWordsDebug)
            {
                result += "\nProtected words: ";

                int x = 0;
                foreach (var protectedWord in protectedWords)
                {
                    string newWord = sentences[protectedWord.Item1][protectedWord.Item2][WORD_INDEX];
                    result += newWord;

                    if (x + 1 != protectedWords.Count)
                    {
                        result += ", ";
                    }
                    x++;
                }

                result += '\n';
                if (debugWordsDeleted.Count > 0)
                {
                    result += "Deleted words:\n";
                }
                foreach (var deletedWords in debugWordsDeleted)
                {
                    result += $"{deletedWords.Item1} -> {deletedWords.Item2}'\n'";
                }
            }
            return result;
        }
    }
}