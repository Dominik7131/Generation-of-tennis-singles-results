using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CTKNewsParser
{
    class Separator
    {
        private RulesChecker rulesChecker;
        private bool isDebug;

        private StreamReader articlesReader;
        private string outTitlesFileName;
        private string outResultSummaryFileName;
        private string outMatchSummaryFileName;

        private HashSet<string> results;
        private HashSet<string> matches;
        private HashSet<string> titles;

        private string[] lastResultTexts;
        private const string DEBUG_FILE_ENDING = "Debug.txt";

        // Debug
        private HashSet<string> removedTitles;
        private HashSet<string> removedResults;
        private HashSet<string> removedMatches;


        public Separator(string matchesNewsFileName, string outTitlesFileName, string outResultSummaryFileName, string outGameSummaryFileName, bool isDebug)
        {
            rulesChecker = new RulesChecker();
            articlesReader = new StreamReader(matchesNewsFileName);
            this.outTitlesFileName = outTitlesFileName;
            this.outResultSummaryFileName = outResultSummaryFileName;
            this.outMatchSummaryFileName = outGameSummaryFileName;

            results = new HashSet<string>();
            matches = new HashSet<string>();
            titles = new HashSet<string>();
            removedTitles = new HashSet<string>();
            removedResults = new HashSet<string>();
            removedMatches = new HashSet<string>();

            lastResultTexts = new string[3] { "", "", "" }; // Last 3 texts to avoid duplicates
            this.isDebug = isDebug;
        }

        public void DivideArticles()
        {
            while (!articlesReader.EndOfStream)
            {
                bool isNextArticle = SkipUntil(NewsProcessor.ID_BEGINNING);
                if (!isNextArticle)
                {
                    break;
                }

                string title = articlesReader.ReadLine();
                if (title == "" || !title.StartsWith(NewsProcessor.TITLE_BEGINNING))
                {
                    continue;
                }

                if (!ProcessTitle(title))
                {
                    continue;
                }

                SkipUntil(NewsProcessor.KEY_WORDS_BEGINNING);
                string firstTextLine = SkipTextHeader();
                ProcessText(firstTextLine);
            }

            WriteOutput();
        }

        private void WriteOutput(bool isJsonFormat = true)
        {
            // Create debug file names by appending "debugFileEnding" to the end of the file titles, results and matches
            int titlePeriodExtensionIndex = outTitlesFileName.LastIndexOf('.');
            int resultPeriodExtensionIndex = outResultSummaryFileName.LastIndexOf('.');
            int matchPeriodExtensionIndex = outMatchSummaryFileName.LastIndexOf('.');

            string titleDebugPath = $"{outTitlesFileName[..titlePeriodExtensionIndex]}{DEBUG_FILE_ENDING}";
            string resultsDebugPath = $"{outResultSummaryFileName[..resultPeriodExtensionIndex]}{DEBUG_FILE_ENDING}";
            string matchesDebugPath = $"{outMatchSummaryFileName[..matchPeriodExtensionIndex]}{DEBUG_FILE_ENDING}";

            using StreamWriter titleWriter = new StreamWriter(outTitlesFileName),
                               resultWriter = new StreamWriter(outResultSummaryFileName),
                               matchWriter = new StreamWriter(outMatchSummaryFileName),
                               titleDebugWriter = new StreamWriter(titleDebugPath),
                               resultDebugWriter = new StreamWriter(resultsDebugPath),
                               matchDebugWriter = new StreamWriter(matchesDebugPath);

            if (!isJsonFormat)
            {
                foreach (string line in titles)
                {
                    titleWriter.WriteLine(line);
                }
                foreach (string line in results)
                {
                    resultWriter.WriteLine(line);
                }
                foreach (string line in matches)
                {
                    matchWriter.WriteLine(line);
                }
            }
            else
            {
                List<string> acceptedTitles = titles.ToList();
                string titlesJson = JsonConvert.SerializeObject(acceptedTitles);
                titleWriter.WriteLine(titlesJson);

                List<string> acceptedResults = results.ToList();
                string resultsJson = JsonConvert.SerializeObject(acceptedResults);
                resultWriter.WriteLine(resultsJson);

                List<string> acceptedMatches = matches.ToList();
                string matchesJson = JsonConvert.SerializeObject(acceptedMatches);
                matchWriter.WriteLine(matchesJson);
            }


            if (isDebug)
            {
                titleDebugWriter.WriteLine("---- Removed titles ----");
                WriteDebug(removedTitles, titleDebugWriter);

                resultDebugWriter.WriteLine("---- Removed result summaries ----");
                WriteDebug(removedResults, resultDebugWriter);

                matchDebugWriter.WriteLine("---- Removed match summaries ----");
                WriteDebug(removedMatches, matchDebugWriter);
            }
        }

        private void WriteDebug(HashSet<string> removedArticleParts, StreamWriter writer)
        {
            foreach (string removedArticlePart in removedArticleParts)
            {
                writer.WriteLine(removedArticlePart);
                writer.WriteLine();
            }
        }

        private bool ProcessTitle(string title)
        {
            int titleStartIndex = NewsProcessor.TITLE_BEGINNING.Length + 1;
            string parsedTitle = title[titleStartIndex..];

            // Check if title is duplicit
            if (titles.Contains(parsedTitle))
            {
                return true;
            }

            bool result = false;

            bool isTitleValid = rulesChecker.CheckTitle(parsedTitle, out bool isTitleUsable, out string debugMsg);
            if (isTitleValid)
            {
                // Not usable title = we do not want to use it, but we still want to process the rest of the article
                if (isTitleUsable)
                {
                    titles.Add(parsedTitle);
                }
                result = true;
            }

            if (isDebug && !isTitleUsable)
            {
                removedTitles.Add($"{parsedTitle}\n-> {debugMsg}");
            }
            return result;
        }

        private void ProcessText(string firstLine)
        {
            if (string.IsNullOrEmpty(firstLine))
            {
                return;
            }

            int dashIndex = firstLine.IndexOf('-') + 2;
            string line = firstLine[dashIndex..];

            // Issue: Dashes in a place name
            // E.g.: Kao-siung
            // If the dash is close to the beginning of the text find next dash
            int newDashIndexTries = 2;
            for (int i = 0; i < newDashIndexTries && dashIndex < 15; i++)
            {
                dashIndex = line.IndexOf('-');
                if (dashIndex == -1)
                {
                    break;
                }
                int spaceOffset = 2;
                line = line[(dashIndex + spaceOffset)..];
            }

            // Skip same message or similar edited versions
            int firstCharsToCheck = 30;
            if (TextsStartsSame(line, lastResultTexts[0], firstCharsToCheck) || TextsStartsSame(line, lastResultTexts[1], firstCharsToCheck) 
                || TextsStartsSame(line, lastResultTexts[2], firstCharsToCheck))
            {
                if (isDebug)
                {
                    removedResults.Add($"{line}\n-> Text is already present");
                }
                return;
            }

            lastResultTexts[2] = lastResultTexts[1];
            lastResultTexts[1] = lastResultTexts[0];
            lastResultTexts[0] = line;

            // Check if the first line contains score and if so
            // end the sentence at first period and save it into editedLine
            string debugMsg = "";
            if (rulesChecker.ContainsScore(line, out string editedLine) && rulesChecker.CheckResult(editedLine, out debugMsg))
            {
                results.Add(editedLine);
            }
            else if (isDebug)
            {
                string missingScore = "Missing score";
                if (editedLine == "")
                {
                    debugMsg = missingScore;
                    editedLine = line;
                }
                if (debugMsg != missingScore)
                {
                    removedResults.Add($"{editedLine}\n-> {debugMsg}");
                }
            }

            if (!articlesReader.EndOfStream)
            {
                line = articlesReader.ReadLine();
            }

            bool isSummaryStart = false;
            bool doesSummaryContinue = false;
            while (!articlesReader.EndOfStream && line != "")
            {
                doesSummaryContinue = FindMatchSummary(line, out isSummaryStart);
                if (isSummaryStart && !doesSummaryContinue)
                {
                    break;
                }
                line = articlesReader.ReadLine();
            }
        }

        private bool FindMatchSummary(string line, out bool isSummaryStart)
        {
            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sentenceBuilder = new StringBuilder();
            isSummaryStart = false;
            bool isSummary = false;
            bool isSentenceEnd = true;

            for (int i = 0; i < words.Length; i++)
            {
                if (!isSentenceEnd)
                {
                    sentenceBuilder.Append(' ');
                }
                sentenceBuilder.Append(words[i]);

                string word = words[i].ToLower();
                bool isNextWordSet = i + 1 < words.Length && (words[i + 1].StartsWith("set") || words[i + 1].StartsWith("sad"));

                if (!isSummaryStart) // Try to find beginning of the match summary
                {
                    if (word.StartsWith("začal") || word.StartsWith("zahájil") || word.StartsWith("vstoupil") ||
                       (i + 1 < words.Length && (word.StartsWith("úvodní") || word.StartsWith("první")) && isNextWordSet))
                    {
                        isSummaryStart = true;
                        isSummary = true;
                    }
                }
                else // Continue finding match summary
                {
                    bool isSetNumber = word.StartsWith("první") || word.StartsWith("druh") || word.StartsWith("třetí") || word.StartsWith("čtvrt") || word.StartsWith("pát");
                    isSummary = isSummary || isSetNumber || (word.StartsWith("set") || word.StartsWith("sad") || word.StartsWith("gem") || word.StartsWith("gam"));
                }

                bool isPeriod = words[i].EndsWith('.');
                bool isNextCharCapitalOrNone = i + 2 >= words.Length || char.IsUpper(words[i + 1][0]);
                isSentenceEnd = isPeriod && isNextCharCapitalOrNone;
                if (isSentenceEnd)
                {
                    string sentence = sentenceBuilder.ToString();
                    if (isSummary)
                    {
                        if (rulesChecker.CheckMatch(sentence, out string debugMsg))
                        {
                            matches.Add(sentence);
                        }
                        else if (isDebug)
                        {
                            removedMatches.Add($"{sentence}\n-> {debugMsg}");
                        }
                    }
                    else if (isDebug)
                    {
                        removedMatches.Add($"{sentence}\n-> No summary word found");
                    }

                    if (i + 1 == words.Length)
                    {
                        return isSummary;
                    }
                    isSummary = false;
                    sentenceBuilder.Clear();
                }
            }
            return false;
        }

        private bool TextsStartsSame(string line1, string line2, int firstCharsToCheck)
        {
            int length = line1.Length > line2.Length ? line2.Length : line1.Length; // Set length to the shorter one

            for (int i = 0; i < length; i++)
            {
                if (i >= firstCharsToCheck)
                {
                    return true;
                }

                if (line1[i] != line2[i])
                {
                    return false;
                }
            }
            return false;
        }

        private string SkipTextHeader()
        {
            string textContentElement = "(ČTK)";
            while (!articlesReader.EndOfStream)
            {
                string line = articlesReader.ReadLine();

                if (line.Contains(textContentElement))
                {
                    return line; // First line of actual news text
                }
            }
            return "";
        }

        private bool SkipUntil(string lineStart)
        {
            while (!articlesReader.EndOfStream)
            {
                string line = articlesReader.ReadLine();
                if (line.StartsWith(lineStart))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
