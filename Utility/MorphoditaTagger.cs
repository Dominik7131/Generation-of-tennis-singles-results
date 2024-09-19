using System;
using System.Collections.Generic;
using Ufal.MorphoDiTa;


namespace Utility
{
    public static class MorphoditaTagger
    {
        public const int U_POS_TAG_INDEX = 0;
        public const int DETAILED_U_POS_TAG_INDEX = 1;
        public const int GENDER_INDEX = 2;
        public const int GRAMMATICAL_NUMBER_INDEX = 3;
        public const int VERB_TENSE_INDEX = 8;
        public const int MASCULINUM_GENDER = 'M';
        public const int FEMININUM_GENDER = 'F';
        public const int NEUTRUM_GENDER = 'N';
        public const char SINGULAR = 'S';
        public const char PLURAL = 'P';
        public const char VERB = 'V';
        public const char PREPOSITION = 'R';
        public const char CONJUNCTION = 'J';
        public const char TENSE_FUTURE = 'F';
        public const char TENSE_PAST = 'R';
        public const char CONDITIONAL = 'c';

        private const string TAGGER_PATH = "ufal/czech-morfflex-pdt-161115.tagger";
        private static Tagger tagger { get; set; }
        private static Forms forms { get; set; }
        private static TaggedLemmas lemmas { get; set; }
        private static TokenRanges tokens { get; set; }
        private static Tokenizer tokenizer { get; set; }

        public static void Init()
        {
            tagger = Tagger.load(TAGGER_PATH);
            forms = new Forms();
            lemmas = new TaggedLemmas();
            tokens = new TokenRanges();
            tokenizer = tagger.newTokenizer();
        }

        public static bool TagWords(string words, out List<string> rawLemmas, out List<string> tags)
        {
            rawLemmas = new List<string>();
            tags = new List<string>();

            tokenizer.setText(words);

            while (tokenizer.nextSentence(forms, tokens))
            {
                tagger.tag(forms, lemmas);

                for (int i = 0; i < lemmas.Count; i++)
                {
                    TaggedLemma lemma = lemmas[i];

                    rawLemmas.Add(StripLemma(lemma.lemma));
                    tags.Add(lemma.tag);
                }
            }
            return true;
        }

        private static string StripLemma(string lemma)
        {
            int underscoreIndex = lemma.IndexOf('_');
            if (underscoreIndex != -1)
            {
                lemma = lemma[0..underscoreIndex];
            }

            if (lemma == "-")
            {
                return lemma;
            }

            int dashIndex = lemma.IndexOf('-');
            if (dashIndex != -1)
            {
                lemma = lemma[0..dashIndex];
            }

            return lemma;
        }

        public static bool GetLemma(string word, out string rawLemma)
        {
            rawLemma = "";
            tokenizer.setText(word);

            while (tokenizer.nextSentence(forms, tokens))
            {
                tagger.tag(forms, lemmas);

                for (int i = 0; i < lemmas.Count; i++)
                {
                    TaggedLemma lemma = lemmas[i];
                    rawLemma = StripLemma(lemma.lemma);
                }
            }
            return true;
        }

        public static bool IsWordFemininum(string word)
        {
            TagWords(word, out List<string> _, out List<string> tags);
            char gender = tags[0][GENDER_INDEX];
            return gender == FEMININUM_GENDER;
        }
    }
}