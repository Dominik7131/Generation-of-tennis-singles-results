using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;


namespace Utility
{
    public static class NameTag
    {
        public const int WORD_INDEX = 0;
        public const int TAG_INDEX = 1;

        const string URL = "https://lindat.mff.cuni.cz/services/nametag/api/recognize?data=";
        const string OPTIONS = "&output=conll";

        public class Response // Structure of NameTag REST API response with conll option
        {
            public string model { get; set; }
            public string[] acknowledgements { get; set; }
            public string result { get; set; }
        }

        public static bool GetResponse(string line, out Response outputResponse)
        {
            RestClient client = new RestClient($"{URL}{line}{OPTIONS}");
            IRestResponse response = client.Execute(new RestRequest());

            while (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Wait two seconds and then try it again
                Thread.Sleep(2000);
                response = client.Execute(new RestRequest());
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Error: Invalid response: {response.StatusCode}");
                outputResponse = null;
                return false;
            }

            outputResponse = JsonConvert.DeserializeObject<Response>(response.Content);
            return true;
        }

        public static bool ParseResponse(Response response, out List<(string word, string tag)> words)
        {
            words = new List<(string word, string tag)>();
            string[] tokens = response.result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                string[] wordAndTag = token.Split("\t");
                if (wordAndTag.Length != 2)
                {
                    return false;
                }
                words.Add((wordAndTag[WORD_INDEX], wordAndTag[TAG_INDEX]));
            }
            return true;
        }
    }
}