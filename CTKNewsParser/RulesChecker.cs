using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using static Utility.MorphoditaTagger;

namespace CTKNewsParser
{
    class RulesChecker
    {
        /// <summary>
        /// Validates "title"
        /// </summary>
        /// <param name="isTitleUsable">False if we do not want to use the title</param>
        /// <param name="debugMsg">Reason for rejection</param>
        /// <returns>True if title meets given criteria</returns>
        public bool CheckTitle(string title, out bool isTitleUsable, out string debugMsg)
        {
            isTitleUsable = false;
            bool result = ContainsPlayerName(title, out List<int> nameIndexes, out debugMsg);

            if (result)
            {
                result = IsTitleSuitable(title, nameIndexes, out isTitleUsable, out debugMsg);
            }
            return result;
        }

        /// <summary>
        /// Checks rules for "title"
        /// </summary>
        /// <returns>True if title meets given criteria</returns>
        public bool IsTitleSuitable(string title, List<int> nameIndexes, out bool isTitleUsable, out string debugMsg)
        {
            isTitleUsable = false;
            bool isVerbFound = false;
            bool isPreviousWordVerb = false;
            bool containsVerbInPastTense = false;

            MorphoditaTagger.TagWords(title, out List<string> lemmas, out List<string> tags);

            debugMsg = "";
            if (lemmas.Count < 6)
            {
                // Typical for older titles which are made by taking first few words from the actual message
                // But still this actual message can be useful so continue with processing
                debugMsg = "Title is too short but continue";
                return true;
            }

            if (lemmas[^1] == ",")
            {
                debugMsg = "Ends with comma";
                return false;
            }
            else if (tags[^1][U_POS_TAG_INDEX] == PREPOSITION)
            {
                debugMsg = "Last word is preposition but continue";
                return true;
            }

            for (int i = 0; i < lemmas.Count; i++)
            {
                string lemma = lemmas[i].ToLower();
                string tag = tags[i];

                if (BlackLists.titles.Contains(lemma) || BlackLists.global.Contains(lemma))
                {
                    debugMsg = $"Forbidden lemma: {lemma}";
                    return false;
                }
                else if (BlackLists.titlesButContinue.Contains(lemma))
                {
                    debugMsg = $"Forbidden lemma but continue: {lemma}";
                    return true;
                }

                string nextLemma = GetNextLemmaLower(lemmas, i);
                bool isPhraseValid = CheckPhrases(lemma, nextLemma);
                if (!isPhraseValid)
                {
                    debugMsg = $"Invalid phrase: {lemma} {nextLemma}";
                    return false;
                }

                char uPosTag = tag[U_POS_TAG_INDEX];
                bool isSingular = tag[GRAMMATICAL_NUMBER_INDEX] == SINGULAR;
                bool isPlural = tag[GRAMMATICAL_NUMBER_INDEX] == PLURAL; // Not singular does not always mean plural
                bool isFutureTense = tag[VERB_TENSE_INDEX] == TENSE_FUTURE;
                bool isPastTense = tag[VERB_TENSE_INDEX] == TENSE_PAST;

                // Check name indexes to prevent this:
                // E.g.: Player "Nadal" recognized as verb (nadat) instead of name
                bool isVerb = tag[0] == VERB && !nameIndexes.Contains(i);

                if (!isVerb)
                {
                    // Tennis player in plural implies too specific context
                    // E.g.: "Z českých TENISTEK zůstala ve Varšavě ve hře už jen Chládková"
                    if ((lemma == "tenista" || lemma == "tenistka" || lemma == "hráč") && !isSingular)
                    {
                        debugMsg = $"Tennis player in plural: {lemma}";
                        return false;
                    }

                    isPreviousWordVerb = false;
                    bool isWantedConjunction = uPosTag == CONJUNCTION && lemma == "a";
                    bool isPlayerConjunction = lemma == "s" || lemma == ","; // E.g.: "Kvitová S Plíškovou"

                    if (isWantedConjunction || isPlayerConjunction)
                    {
                        if (isWantedConjunction)
                        {
                            // Reset verb count
                            isVerbFound = false;
                        }

                        // If conjunction is between two names discard message
                        // E.g.: "České souboje v Birminghamu vyhrály Kvitová A Strýcová"
                        if (i > 0 && i + 1 < lemmas.Count)
                        {
                            int previousWordIndex = i - 1;
                            int nextWordIndex = i + 1;

                            if (nameIndexes.Contains(previousWordIndex) && nameIndexes.Contains(nextWordIndex))
                            {
                                debugMsg = "Conjunction between two names";
                                return false;
                            }
                        }
                    }
                    continue;
                }

                // Discard auxiliary verb "bude"
                if (lemma == "být" && isFutureTense)
                {
                    debugMsg = $"Future form of auxiliary verb: {lemma}";
                    return false;
                }
                else if (lemma == "být" && tag[DETAILED_U_POS_TAG_INDEX] == CONDITIONAL)
                {
                    // E.g.: "BY měl"
                    debugMsg = $"Invalid form of verb: \"by\"";
                    return false;
                }

                if (isVerbFound && !isPreviousWordVerb)
                {
                    debugMsg = "Two non-consecutive verbs";
                    return false;
                }

                if (isPlural)
                {
                    debugMsg = $"Verb in plural: {lemma}";
                    return false;
                }

                isVerbFound = true;

                if (!containsVerbInPastTense)
                {
                    containsVerbInPastTense = isPastTense;
                }

                if (isFutureTense)
                {
                    debugMsg = "Verb in future tense";
                    return false;
                }

                isPreviousWordVerb = true;
            }

            // At least one verb must be in past tense
            if (!containsVerbInPastTense)
            {
                debugMsg = "No verb in past tense";
                return false;
            }

            isTitleUsable = true;
            return true;
        }

        /// <summary>
        /// Checks rules for "result"
        /// </summary>
        /// <returns>True if result meets given criteria</returns>
        public bool CheckResult(string result, out string debugMsg)
        {
            debugMsg = "";
            MorphoditaTagger.TagWords(result, out List<string> lemmas, out List<string> tags);

            for (int i = 0; i < lemmas.Count; i++)
            {
                string lemma = lemmas[i].ToLower();
                string tag = tags[i];

                char uPosTag = tag[U_POS_TAG_INDEX];
                bool isPlural = tag[GRAMMATICAL_NUMBER_INDEX] == PLURAL;
                bool isFutureTense = tag[VERB_TENSE_INDEX] == TENSE_FUTURE;

                // Skips adjective structures
                // E.g.: Djokovič, JENŽ VLONI VYPADL UŽ VE DRUHÉM KOLE, vstoupil do utkání
                // Do not check words inside the structure because they will be deleted
                if (lemma == ",")
                {
                    bool isSkip = SkipAdjectiveStructure(lemmas, ref i);
                    if (isSkip)
                    {
                        continue;
                    }
                }

                if (BlackLists.results.Contains(lemma) || BlackLists.global.Contains(lemma))
                {
                    debugMsg = $"Forbidden lemma: {lemma}";
                    return false;
                }

                string nextLemma = GetNextLemmaLower(lemmas, i);
                bool isPhraseValid = CheckPhrases(lemma, nextLemma);
                if (!isPhraseValid)
                {
                    debugMsg = $"Invalid phrase: {lemma} {nextLemma}";
                    return false;
                }

                bool isVerb = uPosTag == VERB;
                if (isVerb && isPlural)
                {
                    debugMsg = $"Verb in plural: {lemma}";
                    return false;
                }

                // Discard auxiliary verb "bude"
                if (lemma == "být" && isFutureTense)
                {
                    debugMsg = $"Future form of auxiliary verb: {lemma}";
                    return false;
                }
                else if (lemma == "být" && tag[DETAILED_U_POS_TAG_INDEX] == CONDITIONAL)
                {
                    // E.g. "BY měl"
                    debugMsg = $"Invalid form of verb: \"by\"";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks rules for "match"
        /// </summary>
        /// <returns>True if match meets given criteria</returns>
        public bool CheckMatch(string match, out string debugMsg)
        {
            debugMsg = "";
            MorphoditaTagger.TagWords(match, out List<string> lemmas, out List<string> tags);

            for (int i = 0; i < lemmas.Count; i++)
            {
                string lemma = lemmas[i].ToLower();
                string tag = tags[i];

                char uPosTag = tag[U_POS_TAG_INDEX];
                bool isPlural = tag[GRAMMATICAL_NUMBER_INDEX] == PLURAL;
                bool isFutureTense = tag[VERB_TENSE_INDEX] == TENSE_FUTURE;

                // Skips adjective structures
                if (lemma == ",")
                {
                    bool isSkip = SkipAdjectiveStructure(lemmas, ref i);
                    if (isSkip)
                    {
                        continue;
                    }
                }

                if (BlackLists.matches.Contains(lemma) || BlackLists.global.Contains(lemma))
                {
                    debugMsg = $"Forbidden lemma: {lemma}";
                    return false;
                }

                string nextLemma = GetNextLemmaLower(lemmas, i);
                bool isPhraseValid = CheckPhrases(lemma, nextLemma);
                if (!isPhraseValid)
                {
                    debugMsg = $"Invalid phrase: {lemma} {nextLemma}";
                    return false;
                }

                bool isVerb = uPosTag == VERB;
                if (isVerb && isPlural)
                {
                    debugMsg = $"Verb in plural: {lemma}";
                    return false;
                }

                // Discard auxiliary verb "bude"
                if (lemma == "být" && isFutureTense)
                {
                    debugMsg = $"Future form of auxiliary verb: {lemma}";
                    return false;
                }
                else if (lemma == "být" && tag[DETAILED_U_POS_TAG_INDEX] == CONDITIONAL)
                {
                    // E.g. "BY měl"
                    debugMsg = $"Invalid form of verb: \"by\"";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if "line" contains player name
        /// </summary>
        /// <param name="nameIndexes">Start positions of the found names</param>
        /// <param name="debugMsg">Reason for rejection</param>
        /// <returns>True if "line" contains at least one player name</returns>
        private bool ContainsPlayerName(string line, out List<int> nameIndexes, out string debugMsg)
        {
            nameIndexes = new List<int>();
            debugMsg = "";

            bool isResponseValid = NameTag.GetResponse(line, out NameTag.Response response);
            if (!isResponseValid)
            {
                debugMsg = "Invalid response";
                return false;
            }
            bool isParsedResponseValid = NameTag.ParseResponse(response, out List<(string word, string tag)> tokens);
            if (!isParsedResponseValid)
            {
                debugMsg = "Invalid parsed response";
                return false;
            }

            int playerCount = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].tag.StartsWith("B-P") || tokens[i].tag.StartsWith("B-ps")) // Player name
                {
                    playerCount++;
                    nameIndexes.Add(i);
                }
            }

            if (playerCount == 0)
            {
                debugMsg = "No player found";
                return false;
            }
            else if (playerCount > 2)
            {
                debugMsg = "More than 2 players found";
                return false;
            }
            return true;
        }

        private bool CheckPhrases(string lemma, string nextLemma)
        {
            foreach ((string firstWord, string secondWord) in BlackLists.blackListTwoWordsPhrases)
            {
                if (firstWord == lemma && secondWord == nextLemma)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetNextLemmaLower(List<string> lemmas, int i)
        {
            string nextLemma = "";
            if (i + 1 < lemmas.Count)
            {
                nextLemma = lemmas[i + 1].ToLower();
            }
            return nextLemma;
        }

        /// <summary>
        /// Skips adjective structure if "lemmas" contains it
        /// </summary>
        /// <param name="i">Position to start checking from</param>
        /// <returns>True if adjective structure was skipped and sets "i" to the last word of the structure</returns>
        private bool SkipAdjectiveStructure(List<string> lemmas, ref int i)
        {
            for (int x = i + 1; x < lemmas.Count; x++)
            {
                string lemma = lemmas[x];
                bool isPreviousLemmaDigit = int.TryParse(lemmas[x - 1], out int _);
                if (lemma == "." && !isPreviousLemmaDigit)
                {
                    break; // End of sentence
                }
                if (lemma == ",")
                {
                    // Avoid comma in score: 6:4, 6:3
                    string previousLemma = lemmas[x - 1];
                    if (char.IsDigit(previousLemma[0]))
                    {
                        break;
                    }

                    i = x;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if "line" contains score in format X:Y where X and Y are digits
        /// </summary>
        /// <param name="editedLine">"line" ended at first period after the score</param>
        /// <returns></returns>
        public bool ContainsScore(string line, out string editedLine)
        {
            editedLine = "";
            int scoreIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                // Line contains score in format digit:digit - E.g.: 6:4
                // Check also char after score to avoid time - E.g.: 9:00
                bool isScore = i + 2 < line.Length && char.IsDigit(line[i]) && line[i + 1] == ':' && char.IsDigit(line[i + 2]) && (i + 3 >= line.Length || !char.IsDigit(line[i + 3]));
                if (isScore)
                {
                    scoreIndex = i;
                    break;
                }
            }

            bool isScoreFound = scoreIndex != 0;
            if (isScoreFound)
            {
                // Edit the line so it ends with the first period after score
                for (int i = scoreIndex + 1; i < line.Length; i++)
                {
                    bool isNextCharCapitalOrNone = i + 2 >= line.Length || char.IsUpper(line[i + 2]);
                    bool isPeriod = line[i] == '.';

                    bool isSentenceEnd = isPeriod && isNextCharCapitalOrNone;
                    if (isSentenceEnd)
                    {
                        editedLine = line[0..(i + 1)];
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
