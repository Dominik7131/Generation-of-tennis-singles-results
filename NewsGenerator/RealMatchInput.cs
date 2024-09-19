using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NewsGenerator
{
    class MatchScore
    {
        public int loser_games_won { get; set; }
        public string loser_name { get; set; }
        public string loser_player_id { get; set; }
        public string loser_seed { get; set; }
        public int loser_sets_won { get; set; }
        public string loser_slug { get; set; }
        public int loser_tiebreaks_won { get; set; }
        public string match_id { get; set; }
        public int match_order { get; set; }
        public string match_score_tiebreaks { get; set; }
        public string match_stats_url_suffix { get; set; }
        public int round_order { get; set; }
        public int tourney_order { get; set; }
        public string tourney_round_name { get; set; }
        public string tourney_slug { get; set; }
        public string tourney_url_suffix { get; set; }
        public string tourney_year_id { get; set; }
        public int winner_games_won { get; set; }
        public string winner_name { get; set; }
        public string winner_player_id { get; set; }
        public string winner_seed { get; set; }
        public int winner_sets_won { get; set; }
        public string winner_slug { get; set; }
        public int winner_tiebreaks_won { get; set; }
    }

    public class MatchStats
    {
        public int loser_aces { get; set; }
        public int loser_break_points_converted { get; set; }
        public int loser_break_points_return_total { get; set; }
        public int loser_break_points_saved { get; set; }
        public int loser_break_points_serve_total { get; set; }
        public int loser_double_faults { get; set; }
        public int loser_first_serve_points_total { get; set; }
        public int loser_first_serve_points_won { get; set; }
        public int loser_first_serve_return_total { get; set; }
        public int loser_first_serve_return_won { get; set; }
        public int loser_first_serves_in { get; set; }
        public int loser_first_serves_total { get; set; }
        public int loser_return_games_played { get; set; }
        public int loser_return_points_total { get; set; }
        public int loser_return_points_won { get; set; }
        public int loser_second_serve_points_total { get; set; }
        public int loser_second_serve_points_won { get; set; }
        public int loser_second_serve_return_total { get; set; }
        public int loser_second_serve_return_won { get; set; }
        public int loser_service_games_played { get; set; }
        public int loser_service_points_total { get; set; }
        public int loser_service_points_won { get; set; }
        public int loser_total_points_total { get; set; }
        public int loser_total_points_won { get; set; }
        public int match_duration { get; set; }
        public string match_id { get; set; }
        public string match_stats_url_suffix { get; set; }
        public string match_time { get; set; }
        public int tourney_order { get; set; }
        public int winner_aces { get; set; }
        public int winner_break_points_converted { get; set; }
        public int winner_break_points_return_total { get; set; }
        public int winner_break_points_saved { get; set; }
        public int winner_break_points_serve_total { get; set; }
        public int winner_double_faults { get; set; }
        public int winner_first_serve_points_total { get; set; }
        public int winner_first_serve_points_won { get; set; }
        public int winner_first_serve_return_total { get; set; }
        public int winner_first_serve_return_won { get; set; }
        public int winner_first_serves_in { get; set; }
        public int winner_first_serves_total { get; set; }
        public int winner_return_games_played { get; set; }
        public int winner_return_points_total { get; set; }
        public int winner_return_points_won { get; set; }
        public int winner_second_serve_points_total { get; set; }
        public int winner_second_serve_points_won { get; set; }
        public int winner_second_serve_return_total { get; set; }
        public int winner_second_serve_return_won { get; set; }
        public int winner_service_games_played { get; set; }
        public int winner_service_points_total { get; set; }
        public int winner_service_points_won { get; set; }
        public int winner_total_points_total { get; set; }
        public int winner_total_points_won { get; set; }
    }

    static class RealMatchInput
    {
        static string matchScoresPath = $"data{Path.DirectorySeparatorChar}inputs{Path.DirectorySeparatorChar}match_scores_2017_unindexed_json.json";
        static string matchStatsPath = $"data{Path.DirectorySeparatorChar}inputs{Path.DirectorySeparatorChar}match_stats_2017_unindexed_json.json";

        public static MatchInput ParseRandomRealMatch()
        {
            MatchScore currentMatchScore = null;
            MatchStats currentMatchStats = null;

            {
                MatchScore[] matchScore = LoadMatchScore();
                MatchStats[] matchStats = LoadMatchStats();

                Random random = new Random();

                while (currentMatchStats == null)
                {
                    // Few matches do not connect with match stats
                    // -> try different match
                    int randomIndex = random.Next(0, matchScore.Length);
                    currentMatchScore = matchScore[randomIndex];
                    currentMatchStats = FindMatchStats(matchStats, currentMatchScore.match_id);
                }
            }

            MatchInput matchInput = new MatchInput();

            matchInput.score = ParseScore(currentMatchScore.match_score_tiebreaks);
            matchInput.length = ParseLength(currentMatchStats.match_time);
            matchInput.round = ParseRound(currentMatchScore.round_order);
            matchInput.tournamentName = ParseTournamentName(currentMatchScore.tourney_slug);

            return matchInput;
        }

        private static string[] ParseTournamentName(string rawName)
        {
            // rawName: E.g.: australian-open
            // -> split it and capitalize every part
            char nameSeparator = '-';
            string[] nameParts = rawName.Split(nameSeparator, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder tournamentNameBuilder = new StringBuilder();

            for (int i = 0; i < nameParts.Length; i++)
            {
                if (nameParts[i].Length == 2 && nameParts[i] == "us")
                {
                    // Us Open -> US Open
                    tournamentNameBuilder.Append($"{nameParts[i].ToUpper()} ");
                }
                else
                {
                    tournamentNameBuilder.Append($"{char.ToUpper(nameParts[i][0])}{nameParts[i][1..]} ");
                }
            }
            if (tournamentNameBuilder[^1] == ' ')
            {
                tournamentNameBuilder.Length--;
            }
            return tournamentNameBuilder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ParseRound(int roundOrder)
        {
            int roundIndex = InputList.rounds.Count - roundOrder;
            if (roundIndex < 0)
            {
                roundIndex = 0;
            }
            return InputList.rounds[roundIndex];
        }

        private static string ParseLength(string matchTime)
        {
            // matchTime format: hh:mm:ss
            // MatchInput length format: hh:mm
            int secondsAndColonLength = 3;
            string parsedLength = matchTime[..^secondsAndColonLength];
            return parsedLength;
        }

        private static MatchStats FindMatchStats(MatchStats[] matchStats, string targetMatchID)
        {
            for (int i = 0; i < matchStats.Length; i++)
            {
                if (matchStats[i].match_id == targetMatchID)
                {
                    return matchStats[i];
                }
            }
            return null;
        }

        private static string ParseScore(string rawScore)
        {
            if (rawScore.EndsWith("(RET)"))
            {
                // E.g.: 62 61 30 (RET) -> 62 61 30
                int retLength = "(RET)".Length;
                rawScore = rawScore[..^(retLength + 1)];
            }
            string[] tokens = rawScore.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string score = "";
            foreach (string token in tokens)
            {
                int scoreWinner = token[0] - '0';
                int scoreLoser = token[1] - '0';

                score += $"{scoreWinner}:{scoreLoser},";
            }
            score = score[..^1]; // Remove comma at the end
            return score;
        }

        private static MatchScore[] LoadMatchScore()
        {
            using StreamReader reader = new StreamReader(matchScoresPath);
            string json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<MatchScore[]>(json);
        }

        private static MatchStats[] LoadMatchStats()
        {
            using StreamReader reader = new StreamReader(matchStatsPath);
            string json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<MatchStats[]>(json);
        }
    }
}
