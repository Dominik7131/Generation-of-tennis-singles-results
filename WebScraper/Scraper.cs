using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;


namespace WebScraper
{
    class Scraper
    {
        private const string RANKING_URL = "https://www.atptour.com/en/rankings/singles?rankDate=2021-12-20&rankRange=1-5000";
        private const string TOURNAMENTS_URL = "https://en.wikipedia.org/wiki/List_of_tennis_tournaments";
        private const int COUNTRY_INDEX = 2;
        private const int COUNTRY_BEGINNING = 5;


        public void CreateMenRankingDatabase()
        {
            string result = GetHTMLAsync(RANKING_URL).Result;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            var dateNodes = document.DocumentNode.SelectNodes("//div[@class='dropdown-label']");
            string date = dateNodes[0].ChildNodes[0].InnerText.Trim();

            var nameNodes = document.DocumentNode.SelectNodes("//td[@class='player-cell']");
            var countryNodes = document.DocumentNode.SelectNodes("//div[@class='country-item']");
            var ageNodes = document.DocumentNode.SelectNodes("//td[@class='age-cell']");

            if (nameNodes.Count != countryNodes.Count || nameNodes.Count != ageNodes.Count)
            {
                Console.WriteLine("Incorrect number of nodes");
                return;
            }

            int rank = 1;
            List<Player> players = new List<Player>();

            for (int i = 0; i < nameNodes.Count; i++)
            {
                string name = nameNodes[i].InnerText.Trim();
                string country = countryNodes[i].InnerHtml;
                string[] coutryTokens = country.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                country = coutryTokens[COUNTRY_INDEX][COUNTRY_BEGINNING..^1];
                string ageText = ageNodes[i].InnerText.Trim();
                bool isAgeValid = int.TryParse(ageText, out int age);

                if (name.Length != 0 && char.IsUpper(name[0]) && isAgeValid)
                {
                    string[] playerName = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string countryName = CountryCodeToCountryName(country);
                    players.Add(new Player(playerName, rank, countryName, age));
                    rank++;
                }
            }

            SerializePlayersToJson(date, players);
        }

        private static async Task<string> GetHTMLAsync(string url)
        {
            HttpClient httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url);
        }

        private void SerializePlayersToJson(string date, List<Player> players)
        {
            string json = $"{JsonConvert.SerializeObject(date)}\n{JsonConvert.SerializeObject(players)}";
            File.WriteAllText("playerInfo.json", json);
        }

        private string CountryCodeToCountryName(string code)
        {
            string country = "";
            switch (code)
            {
                case "CZE":
                    country = "Česko";
                    break;
                case "GER":
                    country = "Německo";
                    break;
                case "GRE":
                    country = "Řecko";
                    break;
                case "ESP":
                    country = "Španělsko";
                    break;
                case "SRB":
                    country = "Srbsko";
                    break;
                case "RUS":
                    country = "Rusko";
                    break;
                case "ITA":
                    country = "Itálie";
                    break;
                case "GBR":
                    country = "Velká Británie";
                    break;
                case "ARG":
                    country = "Argentina";
                    break;
                case "CAN":
                    country = "Kanada";
                    break;
                case "AUT":
                    country = "Rakousko";
                    break;
                case "SUI":
                    country = "Švýcarsko";
                    break;
                case "CHI":
                    country = "Chile";
                    break;
                case "FRA":
                    country = "Francie";
                    break;
                case "GEO":
                    country = "Gruzie";
                    break;
                case "USA":
                    country = "Amerika";
                    break;
                case "BUL":
                    country = "Bulharsko";
                    break;
                case "CRO":
                    country = "Chorvatsko";
                    break;
                case "RSA":
                    country = "Jihoafrická republika";
                    break;
                case "AUS":
                    country = "Austrálie";
                    break;
                case "KAZ":
                    country = "Kazachstán";
                    break;
                case "BEL":
                    country = "Belgie";
                    break;
                case "HUN":
                    country = "Maďarsko";
                    break;
                case "BLR":
                    country = "Bělorusko";
                    break;
                case "JPN":
                    country = "Japonsko";
                    break;
                case "KOR":
                    country = "Korea";
                    break;
                case "NED":
                    country = "Nizozemsko";
                    break;
                case "SWE":
                    country = "Švédsko";
                    break;
                case "FIN":
                    country = "Finsko";
                    break;
                case "BRA":
                    country = "Brazílie";
                    break;
                case "URU":
                    country = "Uruguay";
                    break;
                case "NOR":
                    country = "Norsko";
                    break;
                case "POL":
                    country = "Polsko";
                    break;
                case "SVK":
                    country = "Slovensko";
                    break;
                case "LTU":
                    country = "Litva";
                    break;
                case "SLO":
                    country = "Slovinsko";
                    break;
                case "DEN":
                    country = "Dánsko";
                    break;
                case "COL":
                    country = "Kolumbie";
                    break;
                case "BOL":
                    country = "Bolívie";
                    break;
                case "PER":
                    country = "Peru";
                    break;
                case "MDA":
                    country = "Moldavsko";
                    break;
                case "POR":
                    country = "Portugalsko";
                    break;
                case "TUR":
                    country = "Turecko";
                    break;
                case "ECU":
                    country = "Ekvádor";
                    break;
                case "ALG":
                    country = "Alžírsko";
                    break;
                case "EGY":
                    country = "Egypt";
                    break;
                case "UKR":
                    country = "Ukrajina";
                    break;
                case "MEX":
                    country = "Mexiko";
                    break;
                case "CHN":
                    country = "Čína";
                    break;
                case "HAI":
                    country = "Haiti";
                    break;
                case "ISR":
                    country = "Israel";
                    break;
                case "PHI":
                    country = "Filipíny";
                    break;
                case "THA":
                    country = "Thajsko";
                    break;
                case "ZIM":
                    country = "Zimbabwe";
                    break;
                case "KUW":
                    country = "Kuvajt";
                    break;
                case "SGP":
                    country = "Singapur";
                    break;
                case "MAS":
                    country = "Malajsie";
                    break;
                case "LUX":
                    country = "Lucembursko";
                    break;
                case "MKD":
                    country = "Makedonie";
                    break;
                case "ROU":
                    country = "Rumunsko";
                    break;
                case "UZB":
                    country = "Uzbekistán";
                    break;
                case "TOG":
                    country = "Togo";
                    break;
                case "IND":
                    country = "Indie";
                    break;
                case "EST":
                    country = "Estosnko";
                    break;
                case "QAT":
                    country = "Katar";
                    break;
                case "MAR":
                    country = "Maroko";
                    break;
                case "LAT":
                    country = "Lotyšsko";
                    break;
                case "VEN":
                    country = "Venezuela";
                    break;
                case "BAH":
                    country = "Bahamy";
                    break;
                case "BIH":
                    country = "Bosna a Hercegovina";
                    break;
                case "SEN":
                    country = "Senegal";
                    break;
                case "NZL":
                    country = "Nový Zéland";
                    break;
                case "MAD":
                    country = "Madagaskar";
                    break;
                case "BEN":
                    country = "Benin";
                    break;
                case "IRI":
                    country = "Írán";
                    break;
                case "NGR":
                    country = "Nigérie";
                    break;
                case "GHA":
                    country = "Ghana";
                    break;
                case "MNE":
                    country = "Černá Hora";
                    break;
                case "CYP":
                    country = "Kypr";
                    break;
                case "DOM":
                    country = "Dominikánská republika";
                    break;
                case "TUN":
                    country = "Tunisko";
                    break;
                case "INA":
                    country = "Indonésie";
                    break;
                case "NAM":
                    country = "Namibie";
                    break;
                case "PAK":
                    country = "Pákistán";
                    break;
                case "JOR":
                    country = "Jordán";
                    break;
                case "TPE":
                    country = "Tchaj-wan";
                    break;
                case "KEN":
                    country = "Keňa";
                    break;
                case "SYR":
                    country = "Sýrie";
                    break;
                case "CUW":
                    country = "Kurakao";
                    break;
                case "JAM":
                    country = "Jamajka";
                    break;
                case "ANT":
                    country = "Antigua a Barbuda";
                    break;
                case "SMR":
                    country = "San Marino";
                    break;
                case "IRL":
                    country = "Irsko";
                    break;
                case "GUA":
                    country = "Guatemala";
                    break;
                case "MON":
                    country = "Monako";
                    break;
                case "BAR":
                    country = "Barbados";
                    break;
                case "LIB":
                    country = "Libye";
                    break;
                case "CIV":
                    country = "Pobřeží slonoviny";
                    break;
                case "ESA":
                    country = "Salvador";
                    break;
                case "BDI":
                    country = "Burundi";
                    break;
                case "VIE":
                    country = "Vietnam";
                    break;
                case "NMI":
                    country = "Severní Mariany";
                    break;
                case "CRC":
                    country = "Kostarika";
                    break;
                case "HKG":
                    country = "Hongkong";
                    break;
                default:
                    Console.WriteLine($"Not defined: {code}");
                    break;
            }
            return country;
        }
    }

    class Player
    {
        public string[] name { get; set; }
        public int rank { get; set; }
        public string country { get; set; }
        public int age { get; set; }

        public Player(string[] name, int rank, string country, int age)
        {
            this.name = name;
            this.rank = rank;
            this.country = country;
            this.age = age;
        }
    }
}
