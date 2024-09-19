using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ufal.MorphoDiTa;

namespace Utility
{
    public static class MorphoditaGenerator
    {
        private const string DICTIONARY_PATH = "ufal/czech-morfflex-161115.dict";
        private static readonly string morphoditaComplementPath = $"data{Path.DirectorySeparatorChar}database{Path.DirectorySeparatorChar}morphoditaComplement.txt";
        public static Morpho morpho { get; set; }
        private static TaggedLemmasForms lemmasForms;


        private static List<string> lemmasToSkip = new List<string>
        {
            "roland", "masters"
        };

        public static void Init()
        {
            morpho = Morpho.load(DICTIONARY_PATH);
            lemmasForms = new TaggedLemmasForms();
        }

        public static bool CreateWordForm(string word, int wordCase, out string result, bool isSingular = true, bool isAdjective = false, bool isPronoun = false, bool isMale = true)
        {
            bool isGender = isAdjective || isPronoun;
            if ((wordCase == 1 && isSingular && !isGender) || lemmasToSkip.Contains(word.ToLower()))
            {
                result = word;
                return true;
            }

            string targetForm = "??";

            if (isGender)
            {
                if (word != "on" && word != "který")
                {
                    targetForm += isMale ? 'M' : 'F';
                }
                else
                {
                    targetForm += isMale ? 'Z' : 'F';
                }
            }
            else
            {
                targetForm += '?';
            }

            targetForm += isSingular ? 'S' : 'P';
            targetForm += wordCase.ToString();

            if (isAdjective)
            {
                targetForm += "????1A----";
            }

            bool isFirstLetterLowered = false;
            if (word == "Turnaj")
            {
                word = word.ToLower();
                isFirstLetterLowered = true;
            }

            result = "";
            bool skipWord = word == "Rus";

            if (!skipWord)
            {
                int generateResult = morpho.generate(word, targetForm, Morpho.GUESSER, lemmasForms);
                if (Generate(wordCase, out result, isFirstLetterLowered, isPronoun, isAdjective))
                {
                    return true;
                }
            }

            if (result == "" && isAdjective)
            {
                // Try different target form for adjectives
                targetForm = $"{targetForm[0..9]}------";
                int generateResult = morpho.generate(word, targetForm, Morpho.GUESSER, lemmasForms);
                if (Generate(wordCase, out result, isFirstLetterLowered, isPronoun, isAdjective))
                {
                    return true;
                }
            }

            result = SearchComplement(morphoditaComplementPath, word, wordCase);
            if (result != "")
            {
                return true;
            }

            result = word;
            return false;
        }

        private static bool Generate(int wordCase, out string result, bool isFirstLetterLowered, bool isPronoun = false, bool isAdjective = false)
        {
            foreach (TaggedLemmaForms lemmaForms in lemmasForms)
            {
                foreach (TaggedForm form in lemmaForms.forms)
                {
                    result = form.form;

                    // "Podlehl Berdychu" -> "Podlehl Berdychovi"
                    // "Podlehl Čechu" -> "Podlehl Čechovi"
                    if (wordCase == 3 && result.EndsWith('u') && !isPronoun && !isAdjective)
                    {
                        result = $"{result[..^1]}ovi";
                    }
                    // Wrong inflection of word "veterán" in 4th word case
                    if (wordCase == 4 && result == "veterán")
                    {
                        result = "veterána";
                    }

                    if (isFirstLetterLowered)
                    {
                        result = $"{char.ToUpper(result[0])}{result[1..]}";
                    }

                    return true;
                }
            }
            result = "";
            return false;
        }

        /// <summary>
        /// Tries to find the wanted word form in morphoditaComplement file
        /// </summary>
        /// <returns>The new word form if the word was found in the file, empty string otherwise</returns>
        private static string SearchComplement(string path, string word, int wordCase, bool isSingular = true)
        {
            string result = "";
            int wordCasesInCzech = 7;

            using (StreamReader reader = new StreamReader(path))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (line != word)
                    {
                        continue;
                    }

                    // Word found
                    if (wordCase == 1)
                    {
                        return line;
                    }

                    for (int i = 1; i < wordCasesInCzech && !reader.EndOfStream; i++)
                    {
                        line = reader.ReadLine();
                        if (i + 1 == wordCase)
                        {
                            return line;
                        }
                    }
                }
            };
            return result;
        }
    }
}
