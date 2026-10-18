using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace FeedMeld;

public static class RssWriter
{
    public static void Write(SyndicationFeed feed, string outputPath)
    {
        string fullPath = Path.GetFullPath(outputPath);
        string? dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "  ",
        };

        var formatter = new Rss20FeedFormatter(feed);
        using var writer = XmlWriter.Create(fullPath, settings);
        formatter.WriteTo(writer);
    }
}
