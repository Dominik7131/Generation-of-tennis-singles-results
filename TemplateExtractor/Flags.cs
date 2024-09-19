using static TemplateExtractor.RecognizedKeyWords;


namespace TemplateExtractor
{
    static class Flags
    {
        public static bool isDebug;
        public static bool isDeletedWordsDebug;

        public static bool isWinnerAndLoserSwapped;
        public static bool isNextRoundOnly; // Does text mention only next round and not the current one
        public static bool isNoNextRound;
        public static bool isMatchBallWinner; // Is mentioned matchball connected to winner
        public static bool isMatchCompleted; // False if score is incomplete (e.g. 6:4, 4:0 is incomplete match = false), set by NamedEntitiesProcessor

        public static void Init(bool isDebugFlag, bool isDeletedWordsDebugFlag)
        {
            isDebug = isDebugFlag;
            isDeletedWordsDebug = isDeletedWordsDebugFlag;

            isWinnerAndLoserSwapped = false;
            isNextRoundOnly = false;
            isNoNextRound = false;
            isMatchBallWinner = true;
            isMatchCompleted = true;
        }
    }
}
