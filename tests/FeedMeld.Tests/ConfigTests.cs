namespace FeedMeld.Tests;

public class ConfigTests
{
    [Fact]
    public void Parses_camel_case_config()
    {
        string json = """
        {
          "title": "T",
          "link": "https://example.com/",
          "output": "out/x.xml",
          "maxItems": 50,
          "maxAgeDays": 7,
          "feeds": ["https://a/feed.xml"],
          "include": ["(?i)release"],
          "exclude": ["(?i)spam"]
        }
        """;
        string dir = Path.Combine(Path.GetTempPath(), "feedmeld-cfg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "c.json");
        File.WriteAllText(path, json);

        var config = Config.Load(path);

        Assert.Equal("T", config.Title);
        Assert.Equal(50, config.MaxItems);
        Assert.Equal(7, config.MaxAgeDays);
        Assert.Single(config.Feeds);
        Assert.Single(config.Include);
        Assert.Single(config.Exclude);

        Directory.Delete(dir, true);
    }

    [Fact]
    public void Validate_rejects_empty_sources()
    {
        var config = new Config { Feeds = new List<string>(), Opml = null };
        var ex = Assert.Throws<ConfigException>(() => config.Validate());
        Assert.Contains("feeds", ex.Message);
    }

    [Fact]
    public void Validate_rejects_bad_max_items()
    {
        var config = new Config { MaxItems = 0, Feeds = new List<string> { "https://a/" } };
        Assert.Throws<ConfigException>(() => config.Validate());
    }
}
