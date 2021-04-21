using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ConsoleDump;
using PersistentMemoryCache;
using RestSharp;
using System;

namespace cached_resty_linq_example
{
    class Program
    {
        void Main()
        {
            var LiteDbFile = Path.Join(Path.GetTempPath(), "commits_cache.db");
            // File.Delete(LiteDbFile); // clear the cache
            var cache = new PersistentMemoryCache.PersistentMemoryCache(new PersistentMemoryCacheOptions("queries",
                new LiteDbStore(new LiteDbOptions(
                    LiteDbFile
            ))));
            LiteDbFile.Dump();

            var commits = cache.GetOrCreate("commits", _ => Commits());

            commits
                .Select(c => new
                {
                    c.Sha,
                    Message = c.Commit.Message.Shorten()
                })
                .Dump()
            ;
        }

        static List<CommitDetails> Commits()
        {
            const string url = @"https://api.github.com/repos/microsoft/vscode/commits";

            $"Fetching from {url}".Dump();
            var client = new RestClient(url);
            var request = new RestRequest();
            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<List<CommitDetails>>(response.Content).ToList();
        }

        class CommitDetails
        {
            public string Sha { get; set; }
            public CommitInfo Commit { get; set; }
        }

        class CommitInfo
        {
            public string Message { get; set; }
        }

        static void Main(string[] args)
        {
            new Program().Main();
        }
    }
}

public static class Extensions
{
    public static string Shorten(this string message)
    {
        message = message.Replace("\n", " ");
        const int max_length = 60;
        return
            message.Length > max_length
            ?
            message.Substring(0, Math.Min(message.Length, 80)) + "..."
            :
            message;
    }
}