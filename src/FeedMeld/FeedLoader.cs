using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace FeedMeld;

public sealed class FeedSource
{
    public string Location { get; }
    public bool IsUrl { get; }

    public FeedSource(string location, bool isUrl)
    {
        Location = location;
        IsUrl = isUrl;
    }
}

public sealed class LoadedFeed
{
    public string SourceTitle { get; set; } = string.Empty;
    public List<SyndicationItem> Items { get; } = new();
}

public sealed class FeedLoader
{
    private readonly HttpClient _http;

    public FeedLoader()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("FeedMeld/1.0");
    }

    public static bool IsUrl(string s)
        => s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    public List<FeedSource> BuildSources(Config config, string configDir, TextWriter stderr)
    {
        var sources = new List<FeedSource>();
        foreach (string f in config.Feeds)
        {
            bool isUrl = IsUrl(f);
            sources.Add(new FeedSource(isUrl ? f : ResolveLocalPath(f, configDir), isUrl));
        }

        if (!string.IsNullOrWhiteSpace(config.Opml))
        {
            string opmlPath = ResolveLocalPath(config.Opml, configDir);
            try
            {
                foreach (string url in ReadOpmlXmlUrls(opmlPath))
                {
                    bool isUrl = IsUrl(url);
                    sources.Add(new FeedSource(isUrl ? url : ResolveLocalPath(url, configDir), isUrl));
                }
            }
            catch (Exception ex)
            {
                stderr.WriteLine($"error: OPML {opmlPath}: {ex.Message}");
            }
        }

        return sources;
    }

    public static string ResolveLocalPath(string path, string configDir)
        => Path.IsPathRooted(path) ? path : Path.Combine(configDir, path);

    public static IEnumerable<string> ReadOpmlXmlUrls(string opmlPath)
    {
        using var stream = File.OpenRead(opmlPath);
        var doc = XDocument.Load(stream);
        return doc.Descendants("outline")
            .Select(e => (string?)e.Attribute("xmlUrl"))
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!.Trim())
            .ToList();
    }

    public async Task<LoadedFeed?> TryLoadAsync(FeedSource source, TextWriter stderr)
    {
        try
        {
            using var stream = source.IsUrl
                ? await _http.GetStreamAsync(source.Location)
                : File.OpenRead(source.Location);
            using var reader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(reader);

            var loaded = new LoadedFeed
            {
                SourceTitle = string.IsNullOrWhiteSpace(feed.Title?.Text)
                    ? source.Location
                    : feed.Title!.Text.Trim(),
            };
            loaded.Items.AddRange(feed.Items);
            return loaded;
        }
        catch (Exception ex)
        {
            stderr.WriteLine($"error: {source.Location}: {ex.Message}");
            return null;
        }
    }
}
