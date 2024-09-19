using System;
using System.Collections.Generic;
using System.Text;


namespace NewsGenerator
{
    static class TextUtility
    {
        public static string RemoveAccents(string text)
        {
            // Code from https://www.c-sharpcorner.com/code/2855/remove-accentsdiacritics-from-a-string-with-c-sharp.aspx
            StringBuilder builder = new StringBuilder();
            char[] textWithoutAccents = text.Normalize(NormalizationForm.FormD).ToCharArray();

            foreach (char letter in textWithoutAccents)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(letter) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(letter);
                }
            }
            return builder.ToString();
        }

        public static string GetFemininumNounLemmaFromNumber(int number)
        {
            if (number == 1)
            {
                return "jednička";
            }
            else if (number == 2)
            {
                return "dvojka";
            }
            else if (number == 3)
            {
                return "trojka";
            }
            else if (number == 4)
            {
                return "čtyřka";
            }
            else if (number == 5)
            {
                return "pětka";
            }
            else if (number == 6)
            {
                return "šestka";
            }
            else if (number == 7)
            {
                return "sedmička";
            }
            else if (number == 8)
            {
                return "osmička";
            }
            else if (number == 9)
            {
                return "devítka";
            }
            else if (number == 10)
            {
                return "desítka";
            }
            else
            {
                return "nula";
            }
        }

        private static string GetRoundFromNumber(int number)
        {
            if (number == 1)
            {
                return "první";
            }
            else if (number == 2)
            {
                return "druhé";
            }
            else if (number == 3)
            {
                return "třetí";
            }
            else if (number == 4)
            {
                return "čtvrté";
            }
            else
            {
                return "nultý";
            }
        }

        public static string GetMasculinumNounLemmaFromNumber(int number)
        {
            if (number == 1)
            {
                return "první";
            }
            else if (number == 2)
            {
                return "druhý";
            }
            else if (number == 3)
            {
                return "třetí";
            }
            else if (number == 4)
            {
                return "čtvrtý";
            }
            else if (number == 5)
            {
                return "pátý";
            }
            else if (number == 6)
            {
                return "šestý";
            }
            else if (number == 7)
            {
                return "sedmí";
            }
            else if (number == 8)
            {
                return "osmý";
            }
            else if (number == 9)
            {
                return "devátý";
            }
            else if (number == 10)
            {
                return "desátý";
            }
            return "nultý";
        }

        public static string CountryToAdjective(string country, bool isMale)
        {
            string result = "";

            if (country == "Afghánistán")
            {
                result = "afghánistánský";
            }
            else if (country == "Albánie")
            {
                result = "albánský";
            }
            else if (country == "Alžírsko")
            {
                result = "alžírský";
            }
            else if (country == "Argentina")
            {
                result = "argentinský";
            }
            else if (country == "Arménie")
            {
                result = "arménský";
            }
            else if (country == "Austrálie")
            {
                result = "australský";
            }
            else if (country == "Rakousko")
            {
                result = "rakouský";
            }
            else if (country == "Azerbajdžán")
            {
                result = "azerbajdžánský";
            }
            else if (country == "Bangladéš")
            {
                result = "bangladéšský";
            }
            else if (country == "Barbados")
            {
                result = "barbadoský";
            }
            else if (country == "Bělorusko")
            {
                result = "běloruský";
            }
            else if (country == "Belgie")
            {
                result = "belgický";
            }
            else if (country == "Bolívie")
            {
                result = "bolívijský";
            }
            else if (country == "Botswana")
            {
                result = "botswanský";
            }
            else if (country == "Brazílie")
            {
                result = "brazilský";
            }
            else if (country == "Brunej")
            {
                result = "brunejský";
            }
            else if (country == "Bulharsko")
            {
                result = "bulharský";
            }
            else if (country == "Burundi")
            {
                result = "burundský";
            }
            else if (country == "Kameron")
            {
                result = "kameronský";
            }
            else if (country == "Kanada")
            {
                result = "kanadský";
            }
            else if (country == "Česko")
            {
                result = "český";
            }
            else if (country == "Čína")
            {
                result = "čínský";
            }
            else if (country == "Kolubie")
            {
                result = "kolumbijský";
            }
            else if (country == "Dánsko")
            {
                result = "dánský";
            }
            else if (country == "Dominikánská republika")
            {
                result = "dominikánský";
            }
            else if (country == "Egypt")
            {
                result = "egyptský";
            }
            else if (country == "Estonsko")
            {
                result = "estonský";
            }
            else if (country == "Etiopie")
            {
                result = "etiopský";
            }
            else if (country == "Finsko")
            {
                result = "finský";
            }
            else if (country == "Francie")
            {
                result = "francouzský";
            }
            else if (country == "Gruzie")
            {
                result = "gruzínský";
            }
            else if (country == "Chile")
            {
                result = "chilský";
            }
            else if (country == "Chorvatsko")
            {
                result = "chorvatský";
            }
            else if (country == "Německo")
            {
                result = "německý";
            }
            else if (country == "Řecko")
            {
                result = "řecký";
            }
            else if (country == "Granada")
            {
                result = "granadský";
            }
            else if (country == "Island")
            {
                result = "islandský";
            }
            else if (country == "Indie")
            {
                result = "indský";
            }
            else if (country == "Indonésie")
            {
                result = "indonéský";
            }
            else if (country == "Irsko")
            {
                result = "irský";
            }
            else if (country == "Írán")
            {
                result = "íránský";
            }
            else if (country == "Izrael")
            {
                result = "izraelský";
            }
            else if (country == "Itálie")
            {
                result = "italský";
            }
            else if (country == "Jamajka")
            {
                result = "jamajský";
            }
            else if (country == "Japonsko")
            {
                result = "japonský";
            }
            else if (country == "Kazachstán")
            {
                result = "kazašský";
            }
            else if (country == "Korea")
            {
                result = "korejský";
            }
            else if (country == "Lotyšsko")
            {
                result = "lotyšský";
            }
            else if (country == "Lucembursko")
            {
                result = "lucemburský";
            }
            else if (country == "Mexiko")
            {
                result = "mexický";
            }
            else if (country == "Maroko")
            {
                result = "marocký";
            }
            else if (country == "Nizozemsko")
            {
                result = "nizozemský";
            }
            else if (country == "Nový Zéland")
            {
                result = "novozélandský";
            }
            else if (country == "Norsko")
            {
                result = "norský";
            }
            else if (country == "Pákistán")
            {
                result = "pákistánský";
            }
            else if (country == "Polsko")
            {
                result = "polský";
            }
            else if (country == "Portugalsko")
            {
                result = "portugalský";
            }
            else if (country == "Romunsko")
            {
                result = "romunský";
            }
            else if (country == "Rusko")
            {
                result = "ruský";
            }
            else if (country == "Salvador")
            {
                result = "salvadorský";
            }
            else if (country == "Srbsko")
            {
                result = "srbský";
            }
            else if (country == "Singapur")
            {
                result = "singapurský";
            }
            else if (country == "Slovinsko")
            {
                result = "slovinský";
            }
            else if (country == "Slovensko")
            {
                result = "slovenský";
            }
            else if (country == "Somálsko")
            {
                result = "somálský";
            }
            else if (country == "Jihoafrická republika")
            {
                result = "jihoafrický";
            }
            else if (country == "Švédsko")
            {
                result = "švédský";
            }
            else if (country == "Španělsko")
            {
                result = "španělský";
            }
            else if (country == "Švýcarsko")
            {
                result = "švýcarský";
            }
            else if (country == "Taiwan")
            {
                result = "taiský";
            }
            else if (country == "Tunisko")
            {
                result = "tuniský";
            }
            else if (country == "Ukrajina")
            {
                result = "ukrajinský";
            }
            else if (country == "Velká Británie")
            {
                result = "britský";
            }
            else if (country == "Amerika")
            {
                result = "americký";
            }
            else if (country == "Vietnam")
            {
                result = "vietnamský";
            }
            else if (country == "Uruguay")
            {
                result = "uruguayský";
            }
            else if (country == "Bosna a Hercegovina")
            {
                result = "bosenský";
            }
            else if (country == "Monako")
            {
                result = "monacký";
            }

            if (result != "" && !isMale)
            {
                result = $"{result[0..^2]}á";
            }

            if (result == "")
            {
                //MessageBox.Show($"{country} to adjective not defined");
            }
            return result;
        }

        public static string CountryToNation(string country, bool isMale)
        {
            string result = "";

            if (country == "Česko")
            {
                result += isMale ? "Čech" : "Češka";
            }
            else if (country == "Srbsko")
            {
                result += isMale ? "Srb" : "Srbka";
            }
            else if (country == "Rusko")
            {
                result += isMale ? "Rus" : "Ruska";
            }
            else if (country == "Amerika")
            {
                result += isMale ? "Američan" : "Američanka";
            }
            else if (country == "Kanada")
            {
                result += isMale ? "Kanaďan" : "Kanaďanka";
            }

            if (result == "")
            {
                Console.WriteLine($"Warning: Country to nation not defined: {country}");
            }
            return result;
        }

        public static string FormatPreposition(string preposition, string wordAfterPreposition)
        {
            // Rules for preposition vocalization: https://prirucka.ujc.cas.cz/?id=770
            string vowels = "aáeéěiíoóuůúAÁÉĚEIÍOÓUŮÚ";
            bool isFirstLetterVowel = vowels.IndexOf(wordAfterPreposition[0]) >= 0;
            bool isSecondLetterVowel = wordAfterPreposition.Length > 1 && vowels.IndexOf(wordAfterPreposition[1]) >= 0;

            if (char.IsDigit(wordAfterPreposition[0]))
            {
                int number = wordAfterPreposition[0] - '0';
                wordAfterPreposition = GetRoundFromNumber(number);
            }

            char wordFirstLetter = char.ToLower(wordAfterPreposition[0]);
            char wordSecondLetter = wordAfterPreposition.Length > 1 ? wordAfterPreposition[1] : ' ';

            bool vocalize = !isFirstLetterVowel && !isSecondLetterVowel && wordSecondLetter != 'r' && wordSecondLetter != 'l';

            List<char> similarPronunciationToV = new List<char> { 'v', 'w', 'f' };
            List<char> similarPronunciationToS = new List<char> { 's', 'z', 'š', 'ž' };
            List<char> similarPronunciationToK = new List<char> { 'k', 'g' };


            if (preposition == "v")
            {
                if (similarPronunciationToV.Contains(wordFirstLetter) || vocalize)
                {
                    return "ve";
                }
                else
                {
                    return "v";
                }
            }
            else if (preposition == "s")
            {
                if (similarPronunciationToS.Contains(wordFirstLetter) || vocalize)
                {
                    return "se";
                }
                else
                {
                    return "s";
                }
            }
            else if (preposition == "z")
            {
                if (similarPronunciationToS.Contains(wordFirstLetter) || vocalize)
                {
                    return "ze";
                }
                else
                {
                    return "z";
                }
            }
            else if (preposition == "k")
            {
                if (similarPronunciationToK.Contains(wordFirstLetter) || vocalize)
                {
                    return "ke";
                }
                else
                {
                    return "k";
                }
            }
            return preposition;
        }

        public static string ProcessPlacePreposition(string preposition, string wordAfterPreposition)
        {
            if (preposition != "na" && preposition != "v")
            {
                return preposition;
            }

            List<string> placesWithPrepositionNa = new List<string>
            {
                "Štvanice"
            };

            if (preposition == "v" && placesWithPrepositionNa.Contains(wordAfterPreposition))
            {
                return "na";
            }
            else
            {
                return "v";
            }
        }


        public static string[] GetNextRound(string[] round)
        {
            string[] nextRound = null;

            if (round.Length == 1)
            {
                if (round[0].StartsWith("osmi"))
                {
                    nextRound = new string[] { "čtvrtfinále" };
                }
                else if (round[0].StartsWith("čtvrt"))
                {
                    nextRound = new string[] { "semifinále" };
                }
                else if (round[0].StartsWith("semi"))
                {
                    nextRound = new string[] { "finále" };
                }
            }
            else if (round.Length == 2)
            {
                string firstPart = round[0];
                if (char.IsDigit(firstPart[0]))
                {
                    int roundNumber = firstPart[0] - '0';

                    if (roundNumber == 1 || roundNumber == 2)
                    {
                        string firstRoundPart = $"{((firstPart[0] - '0') + 1)}.";
                        nextRound = new string[] { firstRoundPart, "kolo" };
                    }
                    else
                    {
                        nextRound = new string[] { "osmifinále" };
                    }
                }
            }
            return nextRound;
        }

        public static string GetSetCountAdjective(int setCount)
        {
            string result = "";

            if (setCount == 2)
            {
                result = "dvou";
            }
            else if (setCount == 3)
            {
                result = "tří";
            }
            else if (setCount == 4)
            {
                result = "čtyř";
            }
            else if (setCount == 5)
            {
                result = "pěti";
            }

            return result;
        }
    }
}