using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Utility
{
    public static class UDPipe
	{
        public class Response // Structure of UDPipe REST API response
        {
            public string model { get; set; }
            public string[] acknowledgements { get; set; }
            public string result { get; set; }
        }

        public const int WORD_INDEX = 0;
        public const int LEMMA_INDEX = 1;
        public const int U_POS_TAG_INDEX = 2;
        public const int X_POS_TAG_INDEX = 3;
        public const int FEATURES_INDEX = 4;
        public const int HEAD_INDEX = 5;
        public const int DEP_REL_INDEX = 6;
        public const int MISC_INDEX = 8;
        public const int WORD_CASE_INDEX = 4;
        public const int GRAMMATICAL_NUMBER_INDEX = 3; // Singular or plural

        public enum MessageType { TITLE, RESULT, MATCHTITLE }

        private const string URL = "https://lindat.mff.cuni.cz/services/udpipe/api/process?tokenizer&tagger&parser&data=";
        private const string HEADER = "# sent_id";
        private const int HEADER_LENGTH = 7;
        private const int TAGS_LENGTH = 9;
        

        public static bool GetResponse(string input, out Response outputResponse)
        {
            RestClient client = new RestClient($"{URL}{input}");
            IRestResponse response = client.Execute(new RestRequest());

            while (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Wait two seconds and then try it again
                Thread.Sleep(2000);
                response = client.Execute(new RestRequest());
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Error: Invalid response - {response.StatusCode}");
                outputResponse = null;
                return false;
            }

            outputResponse = JsonConvert.DeserializeObject<Response>(response.Content);
            return true;
        }

        public static bool ParseResponse(Response response, string rawMessageType, out List<List<string[]>> sentences)
        {
            MessageType messageType = ParseMessageType(rawMessageType);
            sentences = new List<List<string[]>>(); // List of sentences of their words of their tags
            string[] tokens = response.result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            List<string[]> words = new List<string[]>();

            for (int i = HEADER_LENGTH; i < tokens.Length; i++)
            {
                string[] parts = tokens[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1 && parts[0].StartsWith(HEADER))
                {
                    if ((messageType == MessageType.RESULT) && words[^1][0] != ".")
                    {
                        // If previous sentence does not end with period it probably got splited into more parts
                        // which is unwanted because it means that dependencies were incorrectly recognized
                        return false;
                    }
                    // Add to the list previous sentence
                    sentences.Add(words);
                    words = new List<string[]>();
                    i++;
                    continue;
                }
                string[] tags = new string[TAGS_LENGTH];
                int headIndex = 6;

                if (parts[0].Contains('-'))
                {
                    // E.g.: "Dva velké obraty pøedvedla tenistka Karolína Muchová, aby si poprvé zahrála osmifinále grandslamového US Open."
                    // For some reason word "aby" has ID "9-10"
                    return false;
                }

                for (int x = 0; x < TAGS_LENGTH; x++)
                {
                    if (x + 1 == headIndex)
                    {
                        bool isValid = int.TryParse(parts[x + 1], out int headID);
                        if (isValid && headID != 0)
                        {
                            headID--; // UDPipe counts words ID from 1
                        }
                        tags[x] = headID.ToString();
                    }
                    else
                    {
                        tags[x] = parts[x + 1];
                    }
                }
                words.Add(tags);
            }
            sentences.Add(words);

            if ((messageType == MessageType.TITLE || messageType == MessageType.MATCHTITLE) && sentences.Count > 1)
            {
                // Title got divided into more parts
                return false;
            }
            return true;
        }

        private static MessageType ParseMessageType(string rawMessageType)
        {
            if (rawMessageType == MessageType.TITLE.ToString())
            {
                return MessageType.TITLE;
            }
            else if (rawMessageType == MessageType.RESULT.ToString())
            {
                return MessageType.RESULT;
            }
            else
            {
                return MessageType.MATCHTITLE;
            }
        }
    }
}