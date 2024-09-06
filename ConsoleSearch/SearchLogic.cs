using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        HttpClient api = new() { BaseAddress = new Uri("http://word-service") };

        Dictionary<string, int> mWords;

        public SearchLogic()
        {
            var url = "Word/GetAll";
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, url));
            var content = response.Content.ReadAsStringAsync().Result;
            mWords = JsonSerializer.Deserialize<Dictionary<string, int>>(content);
        }

        public int GetIdOf(string word)
        {
            if (mWords.ContainsKey(word))
                return mWords[word];
            return -1;
        }

        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            var url = "Document/GetByWordIds?wordIds=" + string.Join("&wordIds=", wordIds);
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, url));
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<Dictionary<int, int>>(content);
        }

        public List<string> GetDocumentDetails(List<int> docIds)
        {
            var url = "Document/GetByDocIds?docIds=" + string.Join("&docIds=", docIds);
            var response = api.Send(new HttpRequestMessage(HttpMethod.Get, url));
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<string>>(content);
        }
    }
}