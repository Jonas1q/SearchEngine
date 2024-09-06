using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using Polly;
using Polly.Retry;
using System.Text.Json;
using System.Threading.Tasks;

namespace Indexer;

public class Crawler
{
    private readonly char[] sep = " \\\n\t\"$'!,?;.:-_**+=)([]{}<>/@&%€#".ToCharArray();
    private Dictionary<string, int> words = new Dictionary<string, int>();
    private Dictionary<string, int> documents = new Dictionary<string, int>();
    private readonly HttpClient _api;

    public Crawler()
    {
        _api = new HttpClient { BaseAddress = new Uri("http://word-service") };
    }

    private async Task<ISet<string>> ExtractWordsInFileAsync(FileInfo file)
    {
        ISet<string> res = new HashSet<string>();
        var content = await File.ReadAllLinesAsync(file.FullName);
        foreach (var line in content)
        {
            foreach (var aWord in line.Split(sep, StringSplitOptions.RemoveEmptyEntries))
            {
                res.Add(aWord);
            }
        }
        return res;
    }

    private ISet<int> GetWordIdFromWords(ISet<string> src)
    {
        ISet<int> res = new HashSet<int>();
        foreach (var p in src)
        {
            if (words.TryGetValue(p, out int id))
            {
                res.Add(id);
            }
        }
        return res;
    }

    public async Task IndexFilesInAsync(DirectoryInfo dir, List<string> extensions)
    {
        Console.WriteLine("Crawling " + dir.FullName);

        foreach (var file in dir.EnumerateFiles())
        {
            if (extensions.Contains(file.Extension))
            {
                documents.Add(file.FullName, documents.Count + 1);

                var documentMessage = new HttpRequestMessage(HttpMethod.Post, "Documents?id=" + documents[file.FullName] + "&url=" + Uri.EscapeDataString(file.FullName));
                await _api.SendAsync(documentMessage);

                var newWords = new Dictionary<string, int>();
                var wordsInFile = await ExtractWordsInFileAsync(file);

                foreach (var aWord in wordsInFile)
                {
                    if (!words.ContainsKey(aWord))
                    {
                        words.Add(aWord, words.Count + 1);
                        newWords.Add(aWord, words[aWord]);
                    }
                }

                var wordMessage = new HttpRequestMessage(HttpMethod.Post, "Word");
                wordMessage.Content = JsonContent.Create(newWords);
                await _api.SendAsync(wordMessage);

                var occurrenceMessage = new HttpRequestMessage(HttpMethod.Post, "Occurrence?docId=" + documents[file.FullName]);
                occurrenceMessage.Content = JsonContent.Create(GetWordIdFromWords(wordsInFile));
                await _api.SendAsync(occurrenceMessage);
            }
        }

        foreach (var d in dir.EnumerateDirectories())
        {
            await IndexFilesInAsync(d, extensions);
        }
    }
}