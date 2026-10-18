using System.ServiceModel.Syndication;
using System.Xml;

namespace FeedMeld.Tests;

public class EvidenceTests
{
    [Fact]
    public void Evidence_merged_xml_parses_back_with_syndication_feed()
    {
        string repoRoot = FindRepoRoot();
        string path = Path.Combine(repoRoot, "evidence", "merged.xml");
        Assert.True(File.Exists(path), $"expected evidence file at {path}");

        using var reader = XmlReader.Create(path);
        var feed = SyndicationFeed.Load(reader);

        var items = feed.Items.ToList();
        Assert.True(items.Count > 0, "merged feed should contain items");
        foreach (var item in items)
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Title?.Text));
            Assert.False(string.IsNullOrWhiteSpace(item.Links.FirstOrDefault()?.Uri?.ToString()));
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "FeedMeld.slnx")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return dir!.FullName;
    }
}
