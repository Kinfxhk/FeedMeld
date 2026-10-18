using System.ServiceModel.Syndication;
using System.Xml;

namespace FeedMeld.Tests;

public class RssWriterTests
{
    [Fact]
    public void Writes_valid_rss_without_bom_that_roundtrips()
    {
        string dir = Path.Combine(Path.GetTempPath(), "feedmeld-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "sub", "out.xml"); // nested, to exercise directory creation

        var config = new Config
        {
            Title = "T",
            Link = "https://example.com/",
            MaxItems = 10,
            MaxAgeDays = 30,
            Feeds = new List<string> { "https://x/" },
        };

        var feed = new LoadedFeed { SourceTitle = "Source" };
        var item = new SyndicationItem
        {
            Title = new TextSyndicationContent("Item"),
            Summary = new TextSyndicationContent("Summary text"),
        };
        item.Links.Add(new SyndicationLink(new Uri("https://example.com/item")));
        item.PublishDate = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
        item.Categories.Add(new SyndicationCategory("Source"));
        feed.Items.Add(item);

        var result = new FeedMerger(config).Merge(new[] { feed });
        RssWriter.Write(result.Feed, path);

        byte[] bytes = File.ReadAllBytes(path);
        bool hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        Assert.False(hasBom, "file must not start with a UTF-8 BOM");

        SyndicationItem parsedItem;
        using (var reader = XmlReader.Create(path))
        {
            var parsed = SyndicationFeed.Load(reader);
            parsedItem = Assert.Single(parsed.Items);
        }
        Assert.Equal("Item", parsedItem.Title.Text);
        Assert.Equal("Source", parsedItem.Categories.First().Name);

        Directory.Delete(dir, true);
    }
}
