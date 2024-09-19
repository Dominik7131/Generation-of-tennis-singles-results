using System;
using System.Collections.Generic;
using System.Text;
using static TemplateExtractor.RecognizedKeyWords;
using static TemplateExtractor.Flags;
using static Utility.NameTag;


namespace TemplateExtractor
{
    public class NamedEntitiesProcessor
    {
        private List<(string word, string tag)> tokens;
        private MessageType messageType;

        private const char SCORE_SEPARATOR = ':';
        private bool isSentenceStart;
        private bool isQuotationMarksBeginning;
        private StringBuilder sentenceBuilder; // Edited input for UDPipe


        public NamedEntitiesProcessor(MessageType messageType)
        {
            this.messageType = messageType;
            isSentenceStart = true;
            isQuotationMarksBeginning = false;
            sentenceBuilder = new StringBuilder();
        }

        public string ProcessEntities(string input)
        {
            // Prepares only names, round and score for UDPipe because NameTag does not recognize word case and other attributes

            StringBuilder resultBuilder = new StringBuilder();
            if (isDebug)
            {
                resultBuilder.Append($"{input}\n");
            }

            bool isResponseValid = GetResponse(input, out Response response);
            if (!isResponseValid)
            {
                return "Invalid response";
            }
            bool isParsedResponseValid = ParseResponse(response, out tokens);
            if (!isParsedResponseValid)
            {
                return "Invalid parsed response";
            }


            int sentenceID = 0;
            int scoreLength = 0;
            int deletedWords = 0;
            int scoreTemplateLength = 3;
            int sentencesLength = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                string word = tokens[i].word;
                string tag = tokens[i].tag;

                int wordID = i - sentencesLength;
                if (scoreLength != 0)
                {
                    wordID = wordID - scoreLength + scoreTemplateLength;
                }
                if (deletedWords != 0)
                {
                    wordID -= deletedWords;
                }

                string nextWord = "";
                if (i + 1 != tokens.Count)
                {
                    nextWord = tokens[i + 1].word;
                }

                FixTags(word, nextWord, ref tag);

                // French Open or US Open are sometimes incorectly recognized
                if ((word == "French" || word == "US") && nextWord == "Open")
                {
                    deletedWords++;
                    continue;
                }

                // Removes adjective structures
                // E.g.: Djokovič, jenž vloni vypadl už ve druhém kole, vstoupil do utkání -> Djokovič vstoupil do utkání
                if (word == ",")
                {
                    bool isRemoved = TryToRemoveAdjectiveStructure(ref i, ref deletedWords);
                    if (isRemoved)
                    {
                        continue;
                    }
                }

                // Process name
                // Tag first letter: B = Beginning
                if (tag.StartsWith("B-P") || tag.StartsWith("B-ps") || tag.EndsWith("B-ps")) // B-Personal name || B-personal surname
                {
                    ProcessName(sentenceID, wordID, ref i, ref word, ref tag);
                    continue;
                }

                string nextTag = "";
                if (i + 1 < tokens.Count)
                {
                    nextTag = tokens[i + 1].tag;
                }

                // Process inhabitant name (e.g. Švýcarka, Němka, Češka)
                // Incorectly recognized: Japonka, Francouzka
                if (tag.StartsWith("B-pc")) // B-inhabitant name
                {
                    // Remove inhabitant name only if next word is player name (e.g. Češka Karolína Plíšková -> "Karolína Plíšková")
                    // Otherwise this can happen (e.g. "Australanka porazila Dánku" -> "porazila")
                    if (nextTag.StartsWith("B-P") || nextTag.StartsWith("B-ps") || nextTag.EndsWith("B-ps")) // B-Personal name || B-personal surname
                    {
                        deletedWords++;
                        continue;
                    }
                    else if (word == "Rusku") // Fix wrong word case by replacing the word
                    {
                        word = "Češku";
                    }
                }

                // Process country preposition "from" (e.g. z Česka, z Německa)
                if ((word == "z" || word == "ze") && nextTag == "B-gc") // B-geographical name
                {
                    // Remove country and preposition (e.g. Tenistka z Německa -> Tenistka)
                    // Because it would get deleted in UDPipe anyway
                    deletedWords += 2;
                    i++;
                    continue;
                }

                // Process tournament
                bool isTournamentName = tag.StartsWith("B-i") || tag.StartsWith("I-i"); // Institution names
                bool isTournamentPlace = tag.StartsWith("B-g"); // Geographical names
                if (isTournamentName || isTournamentPlace)
                {
                    ProcessTournament(sentenceID, wordID, ref i, ref word, ref tag, ref deletedWords, isTournamentName);
                    continue;
                }

                // Process score
                bool isScoreMultiplier = word == "dvakrát" || word == "třikrát";
                bool isPartOfScore = isScoreMultiplier || (i + 2 < tokens.Count && tokens[i + 1].word.Length > 0 && tokens[i + 1].word.Length > 0 &&
                                    char.IsDigit(word[0]) && tokens[i + 1].word[0] == SCORE_SEPARATOR && char.IsDigit(tokens[i + 2].word[0]));
                if (isPartOfScore) // Cannot use tags because time is also recognized as sport score (E.g. Podlehla za 64 minut 3:6 a 2:6.)
                {
                    ProcessScore(sentenceID, wordID, ref word, ref tag, ref i, ref scoreLength);
                    continue;
                }

                // Process round
                bool isRoundProcessed = ProcessRound(sentenceID, wordID, ref word, ref tag, ref i);
                if (isRoundProcessed)
                {
                    continue;
                }

                if (!AppendWord(word, i))
                {
                    deletedWords++;
                }

                bool sentenceContinuesAfterPeriod = word == "." && i + 1 < tokens.Count && char.IsUpper(tokens[i + 1].word[0]);
                if (sentenceContinuesAfterPeriod)
                {
                    sentenceID++;
                    sentencesLength = i + 1;
                    scoreLength = 0;
                    deletedWords = 0;
                }
            }

            TextProcessorInfo.Init(messageType);
            TextProcessor textProcessor = new TextProcessor();
            resultBuilder.Append(textProcessor.Process(sentenceBuilder.ToString()));
            return resultBuilder.ToString();
        }

        private void FixTags(string word, string nextWord, ref string tag)
        {
            // Inconsistent tag in nation noun from NameTag
            // Can be improved by checking lemmas with Morphodita
            if (tag == "B-ps" && word.StartsWith("Lotyšk") || word.StartsWith("Francouzk") || word.StartsWith("Japonk")
                || word.StartsWith("Rumunk") || word.StartsWith("Dánk") || word.StartsWith("Srbk") || word.StartsWith("Češk") || word.StartsWith("Kazaš"))
            {
                tag = "B-pc";
            }
            else if (tag == "B-ps" && word.StartsWith("Antukov")) // Antukový král is not a name
            {
                tag = "O";
            }
            else if (word == "Strýcová" || word == "Testudová" || word.StartsWith("Balcell")) // Not recognized names
            {
                tag = "B-ps";
            }
            else if ((word == "Roland" && nextWord == "Garros") || word.StartsWith("Wimbledon")) // Incorrectly recognized tournament names
            {
                tag = "B-ia";
            }
            else if (word == "All" && nextWord == "England") // Incorrectly recognized tournament place
            {
                tag = "B-gu";
            }
            else if (word == "smetl" || word == "Loni") // Incorrectly recognized common word
            {
                tag = "O";
            }
        }

        /// <summary>
        /// Tries to remove adjective structure (words separated by commas from both sides)
        /// </summary>
        /// <returns>True if adjective structure was removed</returns>
        private bool TryToRemoveAdjectiveStructure(ref int i, ref int deletedWords)
        {
            bool result = false;

            for (int x = i + 1; x < tokens.Count; x++)
            {
                string word = tokens[x].word;

                string nextWord = "";
                if (x + 1 < tokens.Count)
                {
                    nextWord = tokens[x + 1].word;
                }
                bool isSentenceEnd = nextWord == "" || char.IsUpper(nextWord[0]);
                if (word == "." && isSentenceEnd)
                {
                    break;
                }
                if (word == ",")
                {
                    // Avoid comma in score: 6:4, 6:3
                    string previousWord = tokens[x - 1].word;
                    if (char.IsDigit(previousWord[0]))
                    {
                        break;
                    }

                    deletedWords += x - i + 1;
                    i = x;
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Recognizes names and saves it into playerNames field
        /// </summary>
        private void ProcessName(int sentenceID, int wordID, ref int i, ref string word, ref string tag)
        {
            AppendWord(word, i);

            int nameLength = 1;

            // Process rest of the name
            for (int y = i + 1; y < tokens.Count; y++)
            {
                word = tokens[y].word;
                tag = tokens[y].tag;

                FixTags(word, "", ref tag);

                if (word == "-")
                {
                    string nextTag = "";
                    if (y + 1 != tokens.Count)
                    {
                        nextTag = tokens[y + 1].tag;
                    }
                    bool isNextWordName = nextTag.StartsWith("I-P") || nextTag.StartsWith("I-ps") || nextTag.StartsWith("B-ps");
                    if (isNextWordName)
                    {
                        tag = "I-ps"; // Another name comes after the dash (E.g.: "Záhlavová-Strýcová")
                    }
                }

                if (!tag.StartsWith("I-P") && !tag.StartsWith("I-ps") && !tag.StartsWith("B-ps")) // Name does not continue
                {
                    break;
                }

                AppendWord(word, y);
                nameLength++;
            }

            playerNames.Add((sentenceID, wordID, nameLength));
            i += nameLength - 1; // Skip all processed names

            if (i == tokens.Count)
            {
                word = "";
            }
        }

        private bool ProcessRound(int sentenceID, int wordID, ref string word, ref string tag, ref int i)
        {
            string wordLower = word.ToLower();

            string nextWord = ""; string nextNextWord = "";
            string nextTag = ""; string nextNextTag = "";

            if (i + 1 < tokens.Count)
            {
                nextWord = tokens[i + 1].word;
                nextTag = tokens[i + 1].word;
            }
            if (i + 2 < tokens.Count)
            {
                nextNextWord = tokens[i + 2].word;
                nextNextTag = tokens[i + 2].word;
            }

            bool isRoundTag = tag == "O" || tag == "B-no";
            bool isWordRoundModifier = wordLower.StartsWith("první") || wordLower.StartsWith("úvod") || wordLower.StartsWith("druh") || wordLower.StartsWith("třetí") || wordLower.StartsWith("čtvrt");
            bool isNextWordRound = nextWord.StartsWith("kol") || (wordLower.StartsWith("úvod") && (nextWord.StartsWith("duel") || nextWord.StartsWith("zápas")));
            bool isNumberAndRound = char.IsDigit(wordLower[0]) && nextWord == "." && nextNextWord.StartsWith("kol"); // E.g.: 1. kolo

            bool isPartOfRound = isRoundTag && (wordLower.EndsWith("finále") || isNumberAndRound || (isWordRoundModifier && isNextWordRound));

            if (isPartOfRound)
            {
                CheckRound(sentenceID, wordID, ref word, ref tag, ref i, nextWord, nextTag, nextNextWord, nextNextTag);
                return true;
            }
            return false;
        }

        private void ProcessTournament(int sentenceID, int wordID, ref int i, ref string word, ref string tag, ref int deletedWords, bool isName)
        {
            if (word == "ATP" || word == "WTA" || word == "Tour") 
            {
                AppendWord(word, i);
                return; 
            }

            AppendWord(word, i);

            int tournamentLength = 1;

            for (int x = i + 1; x < tokens.Count; x++)
            {
                word = tokens[x].word;
                tag = tokens[x].tag;

                if ((!tag.StartsWith("I-g") && !tag.StartsWith("I-i")) || tag.EndsWith("B-ps")) // Tournament name does not continue
                {
                    break;
                }

                AppendWord(word, x);
                tournamentLength++;
            }

            if (isName)
            {
                tournamentNames.Add((sentenceID, wordID, tournamentLength));
            }
            else
            {
                tournamentPlaces.Add((sentenceID, wordID, tournamentLength));
            }

            i += tournamentLength - 1; // Skip processed words

            if (i == tokens.Count)
            {
                word = "";
            }
        }

        private void ProcessScore(int sentenceID, int wordID, ref string word, ref string tag, ref int i, ref int scoreLength)
        {
            string score = "";
            string previousWord = "";
            int scoreTokenLength = 0;

            int firstNumber = 0;
            int secondNumber = 0;
            bool isSetParsed = false;
            List<Tuple<int, int>> sets = new List<Tuple<int, int>>();
            List<bool> isHigherScoreFirst = new List<bool>();
            int scoreMultiplier = 1; // How many times should score be repeated (E.g.: "Vyhrál dvakrát 6:4" -> "Vyhrál 6:4, 6:4")

            for (int x = i; x < tokens.Count; x++)
            {
                word = tokens[x].word;
                tag = tokens[x].tag;

                bool isNumber = int.TryParse(word, out int scoreNumber);
                if (isNumber)
                {
                    firstNumber = secondNumber;
                    secondNumber = scoreNumber;

                    if (isSetParsed)
                    {
                        for (int y = 0; y < scoreMultiplier; y++)
                        {
                            sets.Add(new Tuple<int, int>(firstNumber, secondNumber));
                            isHigherScoreFirst.Add(firstNumber > secondNumber);
                        }
                        scoreMultiplier = 1;
                    }

                    isSetParsed = !isSetParsed;
                }

                if (word == "dvakrát")
                {
                    scoreMultiplier = 2;
                }
                else if (word == "třikrát")
                {
                    scoreMultiplier = 3;
                }

                bool isScorePart = SCORE_SEPARATOR == word[0] || word == "," || word == "a" || word == "(" || word == ")";
                bool doesScoreContinue = isNumber || isScorePart || scoreMultiplier != 1;
                if (!doesScoreContinue)
                {
                    // "X vedl 2:0, ale" -> comma in the end is not part of the score
                    // "X vedl 2:0 a" -> conjunction "a" is not part of the score
                    if (previousWord == "," || previousWord == "a")
                    {
                        score = score[..^1];
                        scoreTokenLength--;
                    }
                    break;
                }

                score += word;
                previousWord = word;
                scoreTokenLength++;
            }

            if (score.Contains(SCORE_SEPARATOR))
            {
                isMatchCompleted = firstNumber >= 6 || secondNumber >= 6; // Last set is completed

                // Distinguish "SKÓREVÍTĚZ" and "SKÓREPORAŽENÝ" by result in last set
                bool isScoreWinner = firstNumber > secondNumber;

                AppendWord("0:0", i);
                scores.Add((sentenceID, wordID, isScoreWinner));

                i += scoreTokenLength - 1; // Skip processed words
                scoreLength += scoreTokenLength;
            }
        }

        private void CheckRound(int sentenceID, int wordID, ref string word, ref string tag, ref int i, string nextWord, string nextTag, string nextNextWord, string nextNextTag)
        {
            int roundValue = 0;
            List<string> roundText;
            string wordLower = word.ToLower();

            if (char.IsDigit(word[0])) // E.g.: "1. kolo"
            {
                roundValue = word[0] - '0';

                roundText = new List<string> { roundValue.ToString(), nextWord, nextNextWord };

                word = tokens[i].word;
                tag = tokens[i].tag;
            }
            else if (wordLower.EndsWith("finále")) // E.g: "Osmifinále", "finále"
            {
                if (wordLower.StartsWith("osmi"))
                {
                    roundValue = 5;
                }
                else if (wordLower.StartsWith("čtvrt"))
                {
                    roundValue = 6;
                }
                else if (wordLower.StartsWith("semi"))
                {
                    roundValue = 7;
                }
                else
                {
                    roundValue = 8;
                }

                roundText = new List<string> { word };

                word = nextWord;
                tag = nextTag;
            }
            else // E.g.: "První kolo", "druhé kolo"
            {
                string wordLowerCase = word.ToLower();
                if (wordLowerCase.StartsWith("první"))
                {
                    roundValue = 1;
                }
                else if (wordLowerCase.StartsWith("druh"))
                {
                    roundValue = 2;
                }
                else if (wordLowerCase.StartsWith("třetí"))
                {
                    roundValue = 3;
                }
                else if (wordLowerCase.StartsWith("čtvrt"))
                {
                    roundValue = 4;
                }

                roundText = new List<string> { word, nextWord };

                word = nextNextWord;
                tag = nextNextTag;
            }

            int wordIDOffset = roundText.Count - 1;

            // "Sort" rounds by "roundValue"
            bool isNextRound = true;
            if (rounds[0] != null)
            {
                if (rounds[0].Item3 < roundValue)
                {
                    rounds[1] = Tuple.Create(sentenceID, wordID + wordIDOffset, roundValue, isNextRound);
                }
                else if (rounds[0].Item3 >= roundValue)
                {
                    var t = rounds[0];
                    rounds[0] = rounds[1] = Tuple.Create(sentenceID, wordID + wordIDOffset, roundValue, !isNextRound);
                    rounds[1] = t;
                }
            }
            else
            {
                rounds[0] = Tuple.Create(sentenceID, wordID + wordIDOffset, roundValue, !isNextRound);
            }

            for (int x = 0; x < roundText.Count; x++)
            {
                AppendWord(roundText[x], i + x);
            }
            i += wordIDOffset; // Skip processed words
        }

        /// <summary>
        /// Appends words from named entities processing for UDPipe input
        /// </summary>
        /// <returns>True if "word" was appended</returns>
        private bool AppendWord(string word, int i)
        {
            string previousWord = "";
            string nextWord = "";
            string nextNextWord = "";

            if (i - 1 > 0)
            {
                previousWord = tokens[i - 1].word;
            }

            if (i + 2 < tokens.Count)
            {
                nextWord = tokens[i + 1].word;
                nextNextWord = tokens[i + 2].word;
            }
            else if (i + 1 < tokens.Count)
            {
                nextWord = tokens[i + 1].word;
            }

            // Skip words that ends with "krát" if they are immediately before score (E.g. "dvakrát" 6:4, "třikrát" 6:3 -> 6:4, 6:3)
            bool isMultiplierBeforeScore = word.EndsWith("krát") && nextWord.Length > 0 && char.IsDigit(nextWord[0])
                                            && nextNextWord.Length > 0 && nextNextWord[0] == SCORE_SEPARATOR;

            if (isMultiplierBeforeScore)
            {
                return false;
            }

            if (word == "\"")
            {
                isQuotationMarksBeginning = !isQuotationMarksBeginning;
            }

            // Do not make space before first word, after first quotation mark or before last quotation mark
            bool isQuotationMarkNoSpace = (previousWord == "\"") || (word == "\"" && !isQuotationMarksBeginning);
            bool isApostrophe = word == "\'" || previousWord == "\'";
            bool isDash = word == "-" || previousWord == "-";

            if (!(isSentenceStart || word == "." || word == "," || isDash || isQuotationMarkNoSpace || isApostrophe))
            {
                sentenceBuilder.Append(' ');
            }

            isSentenceStart = false;
            sentenceBuilder.Append(word);
            return true;
        }
    }
}