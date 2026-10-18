using System.Text.Json;

namespace FeedMeld;

public sealed class ConfigException : Exception
{
    public ConfigException(string message) : base(message) { }
}

public sealed class Config
{
    public string Title { get; set; } = "FeedMeld";
    public string Link { get; set; } = "https://example.com/";
    public string Output { get; set; } = "out/merged.xml";
    public int MaxItems { get; set; } = 100;
    public int MaxAgeDays { get; set; } = 30;
    public List<string> Feeds { get; set; } = new();
    public string? Opml { get; set; }
    public List<string> Include { get; set; } = new();
    public List<string> Exclude { get; set; } = new();

    public static Config Load(string path)
    {
        string json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try
        {
            Config? config = JsonSerializer.Deserialize<Config>(json, options);
            return config ?? throw new ConfigException("config is empty");
        }
        catch (JsonException ex)
        {
            throw new ConfigException($"invalid JSON: {ex.Message}");
        }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Output))
            throw new ConfigException("'output' must not be empty");
        if (MaxItems < 1)
            throw new ConfigException("'maxItems' must be >= 1");
        if (MaxAgeDays < 0)
            throw new ConfigException("'maxAgeDays' must be >= 0");
        if ((Feeds is null || Feeds.Count == 0) && string.IsNullOrWhiteSpace(Opml))
            throw new ConfigException("no sources: set 'feeds' and/or 'opml'");
    }
}
