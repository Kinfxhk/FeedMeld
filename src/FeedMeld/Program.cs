using System.Text.Json;

namespace FeedMeld;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--example")
        {
            Console.WriteLine(ExampleConfig());
            return 0;
        }

        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: feedmeld <config.json>");
            Console.Error.WriteLine("       feedmeld --example");
            return 1;
        }

        try
        {
            string configPath = Path.GetFullPath(args[0]);
            Config config = Config.Load(configPath);
            config.Validate();

            string configDir = Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory();

            // Compile regexes up front so an invalid pattern is reported as bad
            // config (exit 1) before any network work.
            var merger = new FeedMerger(config);

            var loader = new FeedLoader();
            List<FeedSource> sources = loader.BuildSources(config, configDir, Console.Error);

            var loaded = new List<LoadedFeed>();
            foreach (var source in sources)
            {
                LoadedFeed? lf = await loader.TryLoadAsync(source, Console.Error);
                if (lf is not null)
                    loaded.Add(lf);
            }

            if (loaded.Count == 0)
            {
                Console.Error.WriteLine("error: no feeds could be loaded");
                return 2;
            }

            MergeResult result = merger.Merge(loaded);

            string outputPath = Path.IsPathRooted(config.Output)
                ? config.Output
                : Path.Combine(configDir, config.Output);
            RssWriter.Write(result.Feed, outputPath);

            Console.WriteLine(
                $"feeds={sources.Count} ok={loaded.Count} failed={sources.Count - loaded.Count} " +
                $"items_in={result.ItemsIn} items_out={result.ItemsOut}");
            return 0;
        }
        catch (ConfigException ex)
        {
            Console.Error.WriteLine($"bad config: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static string ExampleConfig()
    {
        var example = new
        {
            title = "My tech news",
            link = "https://example.com/",
            output = "out/merged.xml",
            maxItems = 100,
            maxAgeDays = 30,
            feeds = new[] { "https://example.com/feed.xml", "local/file.xml" },
            opml = "optional/path.opml",
            include = new[] { "(?i)release|security" },
            exclude = new[] { "(?i)sponsored" },
        };
        return JsonSerializer.Serialize(example, new JsonSerializerOptions { WriteIndented = true });
    }
}

