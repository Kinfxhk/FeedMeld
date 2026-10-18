using System.ServiceModel.Syndication;

namespace FeedMeld.Tests;

public class MergerTests
{
    private static Config BaseConfig() => new()
    {
        Title = "Test",
        Link = "https://example.com/",
        Output = "out.xml",
        MaxItems = 100,
        MaxAgeDays = 30,
        Feeds = new List<string> { "https://example.com/feed" },
    };

    private static SyndicationItem MakeItem(string title, string? link, DateTimeOffset? date, string summary)
    {
        var item = new SyndicationItem
        {
            Title = new TextSyndicationContent(title),
            Summary = new TextSyndicationContent(summary),
        };
        if (link is not null)
            item.Links.Add(new SyndicationLink(new Uri(link)));
        if (date.HasValue)
            item.PublishDate = date.Value;
        return item;
    }

    private static LoadedFeed MakeFeed(string sourceTitle, params SyndicationItem[] items)
    {
        var feed = new LoadedFeed { SourceTitle = sourceTitle };
        foreach (var item in items)
            feed.Items.Add(item);
        return feed;
    }

    [Fact]
    public void Include_regex_keeps_matching_items()
    {
        var config = BaseConfig();
        config.Include.Add("(?i)release|security");
        var merger = new FeedMerger(config);

        var feed = MakeFeed("Feed A",
            MakeItem("Release 1.0", "https://a.com/1", null, "new release"),
            MakeItem("Random post", "https://a.com/2", null, "nothing here"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(2, result.ItemsIn);
        Assert.Equal(1, result.ItemsOut);
        Assert.Equal("Release 1.0", result.Feed.Items.First().Title.Text);
    }

    [Fact]
    public void Exclude_regex_drops_matching_items()
    {
        var config = BaseConfig();
        config.Exclude.Add("(?i)sponsored");
        var merger = new FeedMerger(config);

        var feed = MakeFeed("Feed A",
            MakeItem("Good post", "https://a.com/1", null, "hello"),
            MakeItem("Sponsored deal", "https://a.com/2", null, "sponsored content"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(2, result.ItemsIn);
        Assert.Equal(1, result.ItemsOut);
        Assert.Equal("Good post", result.Feed.Items.First().Title.Text);
    }

    [Fact]
    public void Old_items_dropped_but_undated_kept()
    {
        var config = BaseConfig();
        config.MaxAgeDays = 30;
        var merger = new FeedMerger(config);

        var now = DateTimeOffset.UtcNow;
        var feed = MakeFeed("Feed A",
            MakeItem("Old", "https://a.com/old", now.AddDays(-60), "old"),
            MakeItem("New", "https://a.com/new", now.AddDays(-1), "new"),
            MakeItem("Undated", "https://a.com/undated", null, "no date"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(3, result.ItemsIn);
        Assert.Equal(2, result.ItemsOut);
        Assert.Contains(result.Feed.Items, i => i.Title.Text == "New");
        Assert.Contains(result.Feed.Items, i => i.Title.Text == "Undated");
        Assert.DoesNotContain(result.Feed.Items, i => i.Title.Text == "Old");
    }

    [Fact]
    public void Deduplicates_by_normalized_link()
    {
        var config = BaseConfig();
        var merger = new FeedMerger(config);

        var now = DateTimeOffset.UtcNow;
        var feed = MakeFeed("Feed A",
            MakeItem("Same", "https://a.com/post/", now.AddDays(-1), "x"),
            MakeItem("Same", "HTTPS://A.COM/post", now.AddDays(-2), "x"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(2, result.ItemsIn);
        Assert.Equal(1, result.ItemsOut);
    }

    [Fact]
    public void Deduplicates_by_title_when_link_missing()
    {
        var config = BaseConfig();
        var merger = new FeedMerger(config);

        var feed = MakeFeed("Feed A",
            MakeItem("Same title", null, null, "x"),
            MakeItem("Same title", null, null, "y"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(1, result.ItemsOut);
    }

    [Fact]
    public void Sorts_newest_first_and_applies_max_items()
    {
        var config = BaseConfig();
        config.MaxItems = 2;
        var merger = new FeedMerger(config);

        var now = DateTimeOffset.UtcNow;
        var feed = MakeFeed("Feed A",
            MakeItem("Oldest", "https://a.com/1", now.AddDays(-3), "x"),
            MakeItem("Newest", "https://a.com/2", now.AddDays(-1), "x"),
            MakeItem("Middle", "https://a.com/3", now.AddDays(-2), "x"));

        var result = merger.Merge(new[] { feed });

        Assert.Equal(2, result.ItemsOut);
        Assert.Equal("Newest", result.Feed.Items.First().Title.Text);
    }

    [Fact]
    public void Categories_are_source_titles()
    {
        var config = BaseConfig();
        var merger = new FeedMerger(config);

        var feed = MakeFeed("Source Name",
            MakeItem("Item", "https://a.com/1", null, "x"));

        var result = merger.Merge(new[] { feed });

        var category = result.Feed.Items.First().Categories.First();
        Assert.Equal("Source Name", category.Name);
    }
}
