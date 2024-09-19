using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateExtractor
{
    static class TextProcessorInfo
    {
        public static MessageType messageType;
        public static StringBuilder templateBuilder;
        public static bool[] categories; // Each index represents one category according to index of category name in enum Category
        public static (bool isValid, int wordCase)[] playerTemplate;
        public static HashSet<(int, int)> wordsToDelete;
        public static HashSet<(string, string)> debugWordsDeleted;
        public static HashSet<(int, int)> protectedWords; // Key words such as player names that must not be deleted
        public static HashSet<(int, int, List<string>)> heads; // Words that should lose all dependencies
                                                               // <SentenceID, WordID, List of UPosTags to avoid
                                                               // (e.g. NOUN = do not delete nouns depending on this head)>

        public static void Init(MessageType messageTypeInfo)
        {
            messageType = messageTypeInfo;
            templateBuilder = new StringBuilder();
            categories = new bool[Enum.GetNames(typeof(Category)).Length]; // Size = Number of categories
            playerTemplate = new (bool isValid, int wordCase)[2]; // Winner and loser name placeholder
            heads = new HashSet<(int, int, List<string>)>();
            wordsToDelete = new HashSet<(int, int)>();
            protectedWords = new HashSet<(int, int)>();
            debugWordsDeleted = new HashSet<(string, string)>();

        }
    }
}
