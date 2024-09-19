using System;
using System.Collections.Generic;
using System.IO;


namespace TemplateExtractor
{
    static class Lists
    {
        private static string feminineAnimacyFilePath { get; } = $"data{Path.DirectorySeparatorChar}feminineAnimacy.csv";
        public static List<string> feminineAnimacyLemmas { get; }

        // These all lists shoud be loaded from json files similarly to feminineAnimacyLemmas
        public static List<string> roundLemmas { get; } = new List<string> { "úvod", "kolo", "osmifinále", "čtvrtfinále", "semifinále", "finále" };
        public static List<string> surfacesLemmas { get; } = new List<string> { "tráva", "antuka", "beton", "povrch" };
        public static List<string> titlesLemmas { get; } = new List<string> { "titul", "triumf", "vavřín", "trofej" };
        public static List<string> tournamentLemmas { get; } = new List<string> { "turnaj", "grandslam", "challenger", "exhibice" };

        public static List<string> uPosTagsToAvoidLength { get; } = new List<string> { "ADP", "PRON" };
        public static List<string> uPosTagsToAvoidADP { get; } = new List<string> { "ADP" };
        public static List<string> uPosTagsToAvoidCCONJ { get; } = new List<string> { "CCONJ" };
        public static List<string> UPosTagsToAvoidADPCCONJ { get; } = new List<string> { "ADP", "CCONJ" };

        public static List<(string, Category)> modifiers { get; } = new List<(string, Category)>
        {
            ("nečekaně", Category.UNEXPECTED), ("neočekávaně", Category.UNEXPECTED), ("neočekávaný", Category.UNEXPECTED), ("překvapivě", Category.UNEXPECTED), ("dokonce", Category.UNEXPECTED), ("senzace", Category.UNEXPECTED),
            ("hladce", Category.ONESIDED), ("hladký", Category.ONESIDED), ("jednoduše", Category.ONESIDED), ("jednoznačně", Category.ONESIDED), ("jednoznačný", Category.ONESIDED), ("snadno", Category.ONESIDED), ("lehce", Category.ONESIDED), ("přesvědčivě", Category.ONESIDED), ("jasně", Category.ONESIDED), ("suverénní", Category.ONESIDED),
            ("těsně", Category.CLOSE), ("těsný", Category.CLOSE) , ("vybojovaný", Category.CLOSE),
            ("vítězně", Category.NONE), ("vítězný", Category.NONE), ("rozehraný", Category.NONE), ("skvělý", Category.NONE), ("síl", Category.NONE), ("síla", Category.NONE), ("vyřazení", Category.NONE),
            ("slibně", Category.TURN), ("slibný", Category.TURN), ("navzdory", Category.TURN), ("nadějně", Category.TURN), ("ztracený", Category.TURN),
            ("namále", Category.CLOSE), ("těsně", Category.CLOSE), ("těsný", Category.CLOSE), ("šťastný", Category.NONE), ("drama", Category.TURN),
            ("lekce", Category.ONESIDED), ("přehled", Category.ONESIDED), ("debakl", Category.ONESIDED), ("tažení", Category.NONE), ("zbraň", Category.NONE), ("recept", Category.NONE),
            // ("hra", Category.NONE)
            ("utkání", Category.NONE), ("bitva", Category.CLOSE), ("boj", Category.CLOSE), ("přestřelka", Category.CLOSE), ("zápas", Category.NONE), ("vítězství", Category.NONE),
            ("výhra", Category.NONE), ("porážka", Category.NONE), ("výsledek", Category.NONE), ("tráva", Category.NONE), ("raketa", Category.NONE),
            ("antuka", Category.NONE), ("povrch", Category.NONE), ("vedení", Category.NONE), ("překvapení", Category.UNEXPECTED),
            ("obrat", Category.TURN), ("začátek", Category.INTRO), ("dvouhra", Category.NONE), ("duel", Category.NONE),
            ("konec", Category.NONE), ("postup", Category.NONE), ("skreč", Category.RETIRED), ("stav", Category.NONE), ("zisk", Category.NONE),
            ("klání", Category.NONE), ("vstup", Category.INTRO), ("úvod", Category.INTRO), ("úkor", Category.NONE), ("soutěž", Category.NONE), ("účinkování", Category.NONE),
            ("okruh", Category.NONE), ("set", Category.NONE), ("sada", Category.NONE), ("game", Category.NONE), ("gem", Category.NONE),
            ("mečbol", Category.NONE), ("brejkbol", Category.NONE), ("souboj", Category.NONE), ("kategorie", Category.NONE), ("odvrácení", Category.TURN),
            ("cesta", Category.NONE), ("útok", Category.NONE), ("protizbraň", Category.NONE), ("recept", Category.NONE), ("pouť", Category.NONE)
        };

        public static List<(string, Category)> modifiersNegative { get; } = new List<(string, Category)>
        {
            ("čekaný", Category.UNEXPECTED)
        };

        public static List<(string, Category)> modifiersEndsWith { get; } = new List<(string, Category)>
        {
            ("hodinový", Category.LENGTH), ("setový", Category.NONE)
        };

        public static List<string> verbsSubjectLoser { get; } = new List<string>
        {
            "prohrát", "rozloučit", "podlehnout", "skončit", "ztroskotat", "vypadnout", "zahodit", "utrpět", "poroučet", "snažit",
            "loučit", "dohrát", "vzdát", "odstoupit", "končit", "skrečovat", "zaváhat", "usnadnit", "selhat", "opustit", "padnout"
        };
        public static List<string> verbsSubjectLoserNegativePolarity { get; } = new List<string>
        {
            "stačit", "nastoupit", "vyhrát", "projít", "dosáhnout", "získat", "zvládnout", "oslavit", "postoupit", "zahrát", "připsat",
            "odvrátit", "dohrát", "dotáhnout", "vyjít", "vydařit", "uspět", "porazit", "přejít", "překvapit", "zaskočit"
        };
        public static List<string> setInvalidVerbs { get; } = new List<string> // Invalid verbs from title and results when transforming their templates into match templates
        {
            "vypadnout", "oslavit", "postoupit", "prostoupit", "proniknout", "zahrát", "připsat", "rozloučit", "dosáhnout", "získat", "zvládnout",
            "odvrátit", "vyřadit", "skončit", "dohrát", "poroučet", "loučit", "odstoupit", "končit", "skrečovat",
            "udělat", "dojít", "přidat", "mířit", "projít", "překazit", "zastavit", "přejít", "opustit", "jít", "pokračovat",
            "vyprovodit", "postupovat", "uniknout", "dokonat", "poslat", "zajistit", "proměnit", "ukončit", "probojovat", "utéci",
            "dostat", "znamenat", "udržet", "hostit", "udělat", "překazit", "odcházet", "živit", "moci", "utkat", "překřížit", "zavřít",
            "strávit", "zůstat", "být", "zahájit", "prolétnout", "ovládnout", "potrápit", "propadnout", "trápit", "triumfovat", "vyhrát" /*E.g.: "titul"*/
        };

        public static List<string> matchInvalidNouns { get; } = new List<string>
        {
            "výhra", "postup", "místo", "zápas", "utkání", "dvouhra", "duel"
        };

        public static List<string> matchbalLoserVerbLemmas { get; } = new List<string>
        {
            "odvrátit", "zahodit", "utéct"
        };
        public static List<string> matchbalLoserVerbLemmasNegative { get; } = new List<string>
        {
            "proměnit", "využít"
        };

        public static List<string> prepositionLemmasToInsertInPlaceholders { get; } = new List<string>
        {
            "k", "s", "v", "z", "na"
        };

        public static List<string> tennisFeminineLemmas { get; } = new List<string>
        {
            "jednička", "dvojka", "trojka", "čtyřka", "pětka", "šestka", "sedmička", "osmička", "devítka", "desítka", "jedenáctka",
            "dvanáctka", "třináctka", "čtrnáctka", "patnáctka", "šestnáctka", "sedmnáctka", "osmnáctka", "devatenáctka", "dvacítka",
            "dvacetjednička", "dvacetdvojka", "dvacettrojka", "dvacetčtyřka", "dvacetpětka"
        };


        public static List<(string, Category)> verbCategories { get; } = new List<(string, Category)>
        {
            /* Intro */ ("vstoupit", Category.INTRO), ("začít", Category.INTRO), ("zahájit", Category.INTRO), ("vykročit", Category.INTRO), ("vlétnout", Category.INTRO), ("prolétnout", Category.INTRO), ("rozjet", Category.INTRO), ("vkročit", Category.INTRO),
            /* Semifinal */ ("sahat", Category.SEMIFINAL),
            /* Final */ ("slavit", Category.FINAL), ("připsat", Category.FINAL), ("zakončit", Category.FINAL), ("završit", Category.FINAL), ("dopřát", Category.FINAL), ("triumfovat", Category.FINAL), ("ovládnout", Category.FINAL),
            /* OneSided */ ("deklasovat", Category.ONESIDED), ("rozdrtit", Category.ONESIDED), ("ztroskotat", Category.ONESIDED), ("smést", Category.ONESIDED), ("zničit", Category.ONESIDED), ("utrpět", Category.ONESIDED), ("propadnout", Category.ONESIDED), ("selhat", Category.ONESIDED), ("uštědřit", Category.ONESIDED), ("patřit", Category.ONESIDED), ("vyškolit", Category.ONESIDED), ("pohrát", Category.ONESIDED), ("vyřídit", Category.ONESIDED), ("kralovat", Category.ONESIDED),
            /* Close */ ("vydřít", Category.CLOSE), ("nadřít", Category.CLOSE), ("trápit", Category.CLOSE), ("potrápit", Category.CLOSE), ("protrápit", Category.CLOSE), ("natrápit", Category.CLOSE), ("probít", Category.CLOSE), ("vybojovat", Category.CLOSE), ("udolat", Category.CLOSE), ("přetlačit", Category.CLOSE), ("proklestit", Category.CLOSE), ("bojovat", Category.CLOSE), ("potřebovat", Category.CLOSE), ("vynaložit", Category.CLOSE), ("vzdorovat", Category.CLOSE),
            /* Turn */ ("otočit", Category.TURN), ("odvrátit", Category.TURN), ("promarnit", Category.TURN), ("zahodit", Category.TURN), ("zdramatizovat", Category.TURN), ("čelit", Category.TURN), ("odvracet", Category.TURN), ("obrátit", Category.TURN), ("zachránit", Category.TURN), ("udržet", Category.TURN), ("propást", Category.TURN),
            /* Unexpected */ ("překvapit", Category.UNEXPECTED), ("zaskočit", Category.UNEXPECTED), ("zrodit", Category.UNEXPECTED), ("blýsknout", Category.UNEXPECTED), ("proklouznout", Category.UNEXPECTED), ("padnout", Category.UNEXPECTED),
            /* Retired */ ("skrečovat", Category.RETIRED), ("vzdát", Category.RETIRED), ("odstoupit", Category.RETIRED), ("uvolnit", Category.RETIRED), ("ulehčit", Category.RETIRED), ("usnadnit", Category.RETIRED)
            /* Length */ // trvat, stačit
        };

        public static List<(string, Category)> verbCategoriesNegative { get; } = new List<(string, Category)>
        {
            ("dohrát", Category.RETIRED), ("zaváhat", Category.ONESIDED), ("dotáhnout", Category.TURN)
        };

        static Lists()
        {
            feminineAnimacyLemmas = ParseFeminineAnimacyLemmas(feminineAnimacyFilePath);
        }

        private static List<string> ParseFeminineAnimacyLemmas(string filePath)
        {
            List<string> feminineAnimacyLemmas = new List<string>();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File: {filePath} not found.");
                Environment.Exit(1);
            }
            StreamReader reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split('\t');

                if (tokens.Length > 2 && tokens[2] == "human")
                {
                    feminineAnimacyLemmas.Add(tokens[0]);
                }
            }
            return feminineAnimacyLemmas;
        }
    }
}
