using System;
using System.Collections.Generic;
using System.Text;

namespace TemplateExtractor
{
    static class RecognizedKeyWords
    {
        // 1 = This round, 2 = Next round
        // Next-next round could be added if needed but then the sentence would be too complicated
        public const int ROUND_COUNT = 2;

        // Lists filled by NameTag
        // sentenceID and wordID belongs to the first part of the player name
        public static List<(int sentenceID, int wordID, int length)> playerNames { get; set; }

        // Same as playerNames but with tournaments
        public static List<(int sentenceID, int wordID, int length)> tournamentNames { get; set; }
        public static List<(int sentenceID, int wordID, int length)> tournamentPlaces { get; set; }

        public static List<(int sentenceID, int wordID, bool isScoreWinner)> scores { get; set; }

        // { sentenceID, wordID, roundValue, isNextRound}> }, roundValue for detecting which round is higher
        public static Tuple<int, int, int, bool>[] rounds { get; set; }


        // List used only by UDPipeProcessor
        public static List<(int sentenceID, int wordID, bool isMale)> verbsSingleGender { get; set; }
        public static List<(int sentenceID, int wordID)> playerReferences;


        public static void Init()
        {
            playerNames = new List<(int sentenceID, int wordID, int length)>();
            tournamentNames = new List<(int sentenceID, int wordID, int length)>();
            tournamentPlaces = new List<(int sentenceID, int wordID, int length)>();
            scores = new List<(int sentenceID, int wordID, bool isScoreWinner)>();
            rounds = new Tuple<int, int, int, bool>[ROUND_COUNT];

            verbsSingleGender = new List<(int sentenceID, int wordID, bool isMale)>();
            playerReferences = new List<(int sentenceID, int wordID)>();
        }
    }
}
