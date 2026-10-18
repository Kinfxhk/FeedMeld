namespace FeedMeld.Tests;

public class FeedLoaderTests
{
    [Fact]
    public void Extracts_xml_urls_from_opml()
    {
        string dir = Path.Combine(Path.GetTempPath(), "feedmeld-opml-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string opml = Path.Combine(dir, "feeds.opml");
        File.WriteAllText(opml, """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
          <body>
            <outline text="A" xmlUrl="https://a.com/feed.xml" />
            <outline text="B" xmlUrl="https://b.com/feed.xml" />
            <outline text="No url" />
          </body>
        </opml>
        """);

        var urls = FeedLoader.ReadOpmlXmlUrls(opml).ToList();

        Assert.Equal(2, urls.Count);
        Assert.Contains("https://a.com/feed.xml", urls);
        Assert.Contains("https://b.com/feed.xml", urls);

        Directory.Delete(dir, true);
    }

    [Fact]
    public void IsUrl_detects_http_and_https()
    {
        Assert.True(FeedLoader.IsUrl("https://a.com/x"));
        Assert.True(FeedLoader.IsUrl("http://a.com/x"));
        Assert.False(FeedLoader.IsUrl("local/file.xml"));
        Assert.False(FeedLoader.IsUrl("C:\\feeds\\a.xml"));
    }
}
