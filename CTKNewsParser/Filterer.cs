using System;
using System.Collections.Generic;
using System.IO;


namespace CTKNewsParser
{
    class Filterer
    {
        private readonly string inputFilePath;
        private readonly string outputFilePath;

        private List<List<string>> acceptedArticles;
        private HashSet<string> ids = new HashSet<string>();

        public Filterer(string inputFilePath, string outputFilePath)
        {
            this.inputFilePath = inputFilePath;
            this.outputFilePath = outputFilePath;

            acceptedArticles = new List<List<string>>();
        }

        public void Filter()
        {
            using StreamReader newsReader = new StreamReader(inputFilePath);

            string id = "";
            
            if (!newsReader.EndOfStream)
            {
                newsReader.ReadLine();
            }

            while (!newsReader.EndOfStream)
            {
                string name = newsReader.ReadLine();
                string specialInstruction = newsReader.ReadLine();
                string date = newsReader.ReadLine();
                string priority = newsReader.ReadLine();
                string author = newsReader.ReadLine();
                string region = newsReader.ReadLine();
                string category = newsReader.ReadLine();
                string kws = newsReader.ReadLine();
                string text = newsReader.ReadLine();

                List<string> article = new List<string>()
                { 
                    id, name, specialInstruction, date, priority, author, region, category, kws
                };
                bool isID = ids.Contains(id);
                string currentID = id;

                while (!newsReader.EndOfStream)
                {
                    string line = newsReader.ReadLine();
                    if (line.StartsWith(NewsProcessor.ID_BEGINNING))
                    {
                        id = line;
                        break;
                    }
                    if (!string.IsNullOrEmpty(line))
                    {
                        text += $"\n'{line}";
                    }
                }
                article.Add(text);

                if (!kws.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
                {
                    kws = FindKeyWords(specialInstruction, date, priority, author, region, category, text);
                }

                if (string.IsNullOrEmpty(kws))
                {
                    continue;
                }

                bool isAnyKeyWordInBlackList = IsAnyKeyWordInBlackList(kws);

                if (!isID && !isAnyKeyWordInBlackList)
                {
                    acceptedArticles.Add(article);
                    ids.Add(currentID);
                }
            }

            acceptedArticles.Sort((a, b) => b[0].CompareTo(a[0]));

            WriteOutput();
        }

        private string FindKeyWords(string specialInstruction, string date, string priority, string author, string region, string category, string text)
        {
            if (specialInstruction.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return specialInstruction;
            }
            else if (date.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return date;
            }
            else if (priority.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return priority;
            }
            else if (author.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return author;
            }
            else if (region.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return region;
            }
            else if (category.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return category;
            }
            else if (text.StartsWith(NewsProcessor.KEY_WORDS_BEGINNING))
            {
                return text;
            }
            return "";
        }

        private bool IsAnyKeyWordInBlackList(string rawKeyWords)
        {
            int keyWordsOffset = NewsProcessor.KEY_WORDS_BEGINNING.Length + 1;
            string[] keyWords = rawKeyWords[keyWordsOffset..].Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (string keyWord in keyWords)
            {
                if (Array.IndexOf(BlackLists.keyWords, keyWord.ToLower()) != -1)
                {
                    // Keyword is in black list
                    return true;
                }
            }
            return false;
        }

        private void WriteOutput()
        {
            using StreamWriter titleWriter = new StreamWriter(outputFilePath);
            {
                foreach (List<string> article in acceptedArticles)
                {
                    foreach (string line in article)
                    {
                        titleWriter.WriteLine(line);
                    }
                    titleWriter.WriteLine();
                    titleWriter.WriteLine();
                }
            }
        }
    }
}
