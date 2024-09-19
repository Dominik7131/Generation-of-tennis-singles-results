using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;


namespace CTKNewsParser
{
    class Citator
    {
        private readonly string inputFilePath;
        private readonly string outputFilePath;
        private List<(string id, string date, string domicile)> citations = new List<(string, string, string)>();

        private const string SOURCE_NAME = "Infobanka ČTK";
        private const string URL = "http://ib.ctk.cz/";


        public Citator(string inputFilePath, string outputFilePath)
        {
            this.inputFilePath = inputFilePath;
            this.outputFilePath = outputFilePath;
        }

        /// <summary>
        /// Cites ČTK news according to citation norm ISO 690
        /// </summary>
        public void Cite()
        {
            GetInformationFromNews();
            WriteCitationsToFile();
        }

        private void GetInformationFromNews()
        {
            using StreamReader newsReader = new StreamReader(inputFilePath);

            string id = "";
            if (!newsReader.EndOfStream)
            {
                id = newsReader.ReadLine();
            }
            while (!newsReader.EndOfStream)
            {
                newsReader.ReadLine(); // Name
                newsReader.ReadLine(); // Special instruction
                string date = newsReader.ReadLine();
                newsReader.ReadLine(); // Priority
                newsReader.ReadLine(); // Author
                newsReader.ReadLine(); // Region
                newsReader.ReadLine(); // Category
                newsReader.ReadLine(); // Keywords
                newsReader.ReadLine(); // Text
                string textContent = newsReader.ReadLine();

                string parsedID = ParseID(id);
                string parsedDate = ParseDate(date);
                string domicile = ParseDomicile(textContent);

                bool isCTK = textContent.Contains("ČTK");
                bool areAllInformationPresent = id != "" && date != "" && domicile != "" && isCTK;
                if (areAllInformationPresent)
                {
                    citations.Add((parsedID, parsedDate, domicile));
                }

                bool doesFileContinue = FindNextNewsBeginning(newsReader, out id);
            }
        }

        /// <summary>
        /// Skips the rest of the text from the last article and finds beginning of a new article
        /// </summary>
        /// <param name="firstLine">First line of the new article (ID)</param>
        /// <returns>True if the beginning of the new article was found</returns>
        private bool FindNextNewsBeginning(StreamReader reader, out string firstLine)
        {
            string idStart = "ID:";
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.StartsWith(idStart))
                {
                    firstLine = line;
                    return true;
                }
            }
            firstLine = "";
            return false;
        }

        private string ParseID(string id)
        {
            string[] idParts = id.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return idParts[1];
        }

        private string ParseDate(string date)
        {
            if (date == "")
            {
                return date;
            }
            string[] dateParts = date.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (dateParts.Length < 2)
            {
                return "";
            }
            return dateParts[1];
        }

        private string ParseDomicile(string line)
        {
            // Domicile is usually in the beginning of the text

            if (line == "")
            {
                return line;
            }

            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 4)
            {
                // Only check first 4 words because domicile will not be longer
                words = new string[] { words[0], words[1], words[2], words[3] };
            }

            bool isResponseValid = NameTag.GetResponse(line, out NameTag.Response response);
            if (!isResponseValid)
            {
                return "";
            }
            bool isParsedResponseValid = NameTag.ParseResponse(response, out List<(string word, string tag)> tokens);

            if (!isParsedResponseValid)
            {
                return "";
            }

            StringBuilder domicileBuilder = new StringBuilder();

            for (int i = 0; i < tokens.Count; i++)
            {
                bool isPlaceBeginningTag = tokens[i].tag.StartsWith("B-g") || tokens[i].tag.StartsWith("B-i");
                bool isPlaceContinuationTag = tokens[i].tag.StartsWith("I-g") || tokens[i].tag.StartsWith("I-i");
                bool isDash = tokens[i].word == "-";
                int lastIndex = tokens[i].word[^1] == ':' ? 1 : 0;

                if ((tokens[i].word.Length > lastIndex) && ((i == 0 && isPlaceBeginningTag) || isPlaceContinuationTag))
                {
                    if (isDash)
                    {
                        domicileBuilder.RemoveLast(' ');
                    }

                    domicileBuilder.Append($"{char.ToUpper(tokens[i].word[0])}{tokens[i].word[1..^lastIndex].ToLower()} ");

                    if (isDash)
                    {
                        domicileBuilder.RemoveLast(' ');
                    }
                }
                else
                {
                    break;
                }
            }

            domicileBuilder.RemoveLast(' ');
            return domicileBuilder.ToString();
        }

        private void WriteCitationsToFile()
        {
            DateTime currentTime = DateTime.Now;
            int curentYear = currentTime.Year;

            using StreamWriter citationWriter = new StreamWriter(outputFilePath);
            for (int i = 0; i < citations.Count; i++)
            {
                citationWriter.WriteLine($"{SOURCE_NAME}, {curentYear}, {citations[i].domicile} ČTK ({citations[i].date}), ID {citations[i].id}, dostupné z {URL}");
            }
        }
    }
}
