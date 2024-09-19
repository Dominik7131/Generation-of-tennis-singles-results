using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Utility.UDPipe;

namespace TemplateExtractor
{
    static class TextProcessorUtility
    {
        public static bool IsNameCountValid(int namesCount, MessageType type, bool isDebug, out string debugMsg)
        {
            debugMsg = "";

            // Input is from one match only (= 2 player names max, 1 tournament max, etc.)
            if (namesCount == 0 && type != MessageType.MATCHTITLE)
            {
                if (isDebug)
                {
                    debugMsg = "-> No player name\n";
                }
                return false;
            }
            else if (namesCount > 2)
            {
                if (isDebug)
                {
                    debugMsg = "-> Too many player names\n";
                }
                return false;
            }

            return true;
        }

        public static bool ContainsID(this HashSet<(int, int, List<string>)> set, int sentenceID, int wordID)
        {
            foreach (var s in set)
            {
                if (s.Item1 == sentenceID && s.Item2 == wordID)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetWordCase(List<List<string[]>> sentences, int sentenceID, int wordID)
        {
            string xPosTag = sentences[sentenceID][wordID][X_POS_TAG_INDEX];
            return xPosTag[WORD_CASE_INDEX] - '0';
        }

        public static int GetHeadID(List<List<string[]>> sentences, int sentenceID, int wordID)
        {
            string head = sentences[sentenceID][wordID][HEAD_INDEX];
            int.TryParse(head, out int headID);
            return headID;
        }
    }
}