using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Polly;
using System.Threading.Tasks;

namespace ConsoleSearch;

public class SearchLogic
{
    private readonly HttpClient _api;
    private readonly IAsyncPolicy<Dictionary<int, int>> _documentsFallbackPolicy;
    private readonly IAsyncPolicy<List<string>> _documentDetailsFallbackPolicy;
    private readonly Dictionary<string, int> _mWords;

    public SearchLogic()
    {
        _api = new HttpClient { BaseAddress = new Uri("http://word-service") };

        _documentsFallbackPolicy = Policy<Dictionary<int, int>>
            .Handle<HttpRequestException>()
            .FallbackAsync(
                new Dictionary<int, int> { { 0, -1 } }, 
                async (exception, context) =>
                {

                    await Task.CompletedTask;
                }
            );

        _documentDetailsFallbackPolicy = Policy<List<string>>
            .Handle<HttpRequestException>()
            .FallbackAsync(
                new List<string> { "Fallback document detail" },
                async (exception, context) =>
                {

                    await Task.CompletedTask;
                }
            );

        var url = "Word/GetAll";
        var response = _api.GetAsync(url).GetAwaiter().GetResult();
        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        _mWords = JsonSerializer.Deserialize<Dictionary<string, int>>(content);
    }

    public int GetIdOf(string word)
    {
        return _mWords.ContainsKey(word) ? _mWords[word] : -1;
    }

    public Dictionary<int, int> GetDocuments(List<int> wordIds)
    {
        var url = "Document/GetByWordIds?wordIds=" + string.Join("&wordIds=", wordIds);


        var result = _documentsFallbackPolicy.ExecuteAsync(async () =>
        {
            var response = await _api.GetAsync(url);
            response.EnsureSuccessStatusCode(); 
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<int, int>>(content);
        }).GetAwaiter().GetResult();

        return result;
    }

    public List<string> GetDocumentDetails(List<int> docIds)
    {
        var url = "Document/GetByDocIds?docIds=" + string.Join("&docIds=", docIds);


        var result = _documentDetailsFallbackPolicy.ExecuteAsync(async () =>
        {
            var response = await _api.GetAsync(url);
            response.EnsureSuccessStatusCode(); 
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content);
        }).GetAwaiter().GetResult();

        return result;
    }
}