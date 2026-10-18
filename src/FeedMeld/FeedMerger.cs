using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;

namespace FeedMeld;

public sealed class FeedEntry
{
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public DateTimeOffset? Date { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
}

public sealed class MergeResult
{
    public SyndicationFeed Feed { get; set; } = null!;
    public int ItemsIn { get; set; }
    public int ItemsOut { get; set; }
}

public sealed class FeedMerger
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private readonly List<Regex> _include;
    private readonly List<Regex> _exclude;
    private readonly int _maxAgeDays;
    private readonly int _maxItems;
    private readonly string _title;
    private readonly string _link;

    public FeedMerger(Config config)
    {
        _include = Compile(config.Include, "include");
        _exclude = Compile(config.Exclude, "exclude");
        _maxAgeDays = config.MaxAgeDays;
        _maxItems = config.MaxItems;
        _title = config.Title;
        _link = config.Link;
    }

    private static List<Regex> Compile(IEnumerable<string> patterns, string which)
    {
        var list = new List<Regex>();
        foreach (string p in patterns)
        {
            if (string.IsNullOrEmpty(p))
                continue;
            try
            {
                list.Add(new Regex(p, RegexOptions.None, RegexTimeout));
            }
            catch (ArgumentException ex)
            {
                throw new ConfigException($"invalid {which} regex '{p}': {ex.Message}");
            }
        }
        return list;
    }

    public MergeResult Merge(IReadOnlyList<LoadedFeed> feeds)
    {
        var entries = new List<FeedEntry>();
        int itemsIn = 0;

        foreach (var feed in feeds)
        {
            foreach (var item in feed.Items)
            {
                itemsIn++;
                var entry = ToEntry(item, feed.SourceTitle);
                if (!PassesFilters(entry))
                    continue;
                entries.Add(entry);
            }
        }

        var deduped = Deduplicate(entries);
        deduped.Sort(CompareNewestFirst);
        if (deduped.Count > _maxItems)
            deduped = deduped.Take(_maxItems).ToList();

        return new MergeResult
        {
            ItemsIn = itemsIn,
            ItemsOut = deduped.Count,
            Feed = BuildFeed(deduped),
        };
    }

    private static FeedEntry ToEntry(SyndicationItem item, string sourceTitle)
    {
        string summary = item.Summary?.Text ?? string.Empty;
        if (summary.Length == 0 && item.Content is TextSyndicationContent tc && tc.Text is not null)
            summary = tc.Text;

        var link = item.Links.FirstOrDefault();
        string linkStr = link?.Uri?.ToString() ?? string.Empty;

        return new FeedEntry
        {
            Title = item.Title?.Text ?? string.Empty,
            Link = linkStr,
            Summary = summary,
            SourceTitle = sourceTitle,
            Date = item.PublishDate == default ? null : item.PublishDate,
        };
    }

    private bool PassesFilters(FeedEntry e)
    {
        string haystack = e.Title + "\n" + e.Summary;

        bool included = _include.Count == 0 || _include.Any(r => r.IsMatch(haystack));
        if (!included)
            return false;

        if (_exclude.Any(r => r.IsMatch(haystack)))
            return false;

        if (e.Date.HasValue)
        {
            var cutoff = DateTimeOffset.UtcNow.AddDays(-_maxAgeDays);
            if (e.Date.Value < cutoff)
                return false;
        }

        return true;
    }

    private static List<FeedEntry> Deduplicate(List<FeedEntry> entries)
    {
        var seenLink = new HashSet<string>(StringComparer.Ordinal);
        var seenTitle = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<FeedEntry>();

        foreach (var e in entries)
        {
            string linkKey = LinkNormalizer.Normalize(e.Link);
            if (linkKey.Length > 0)
            {
                if (!seenLink.Add(linkKey))
                    continue;
            }
            else
            {
                string titleKey = LinkNormalizer.NormalizeTitle(e.Title);
                if (titleKey.Length > 0 && !seenTitle.Add(titleKey))
                    continue;
            }
            result.Add(e);
        }

        return result;
    }

    private static int CompareNewestFirst(FeedEntry a, FeedEntry b)
    {
        var da = a.Date ?? DateTimeOffset.MinValue;
        var db = b.Date ?? DateTimeOffset.MinValue;
        return db.CompareTo(da);
    }

    private SyndicationFeed BuildFeed(List<FeedEntry> entries)
    {
        var linkUri = Uri.TryCreate(_link, UriKind.Absolute, out Uri? u)
            ? u
            : new Uri("https://example.com/");

        var feed = new SyndicationFeed(_title, _title, linkUri);
        var outputItems = new List<SyndicationItem>();

        foreach (var e in entries)
        {
            var item = new SyndicationItem
            {
                Title = new TextSyndicationContent(e.Title),
                Summary = new TextSyndicationContent(e.Summary),
            };

            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri? itemLink))
                item.Links.Add(SyndicationLink.CreateAlternateLink(itemLink));

            if (e.Date.HasValue)
                item.PublishDate = e.Date.Value;

            item.Categories.Add(new SyndicationCategory(e.SourceTitle));

            outputItems.Add(item);
        }

        feed.Items = outputItems;
        return feed;
    }
}
