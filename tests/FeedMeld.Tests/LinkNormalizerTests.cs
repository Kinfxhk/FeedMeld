namespace FeedMeld.Tests;

public class LinkNormalizerTests
{
    [Fact]
    public void Lowercases_scheme_and_host()
    {
        Assert.Equal("https://example.com/path", LinkNormalizer.Normalize("HTTPS://EXAMPLE.COM/path"));
    }

    [Fact]
    public void Strips_trailing_slash()
    {
        Assert.Equal("https://example.com/path", LinkNormalizer.Normalize("https://example.com/path/"));
    }

    [Fact]
    public void Keeps_root_slash()
    {
        Assert.Equal("https://example.com/", LinkNormalizer.Normalize("https://example.com/"));
    }

    [Fact]
    public void Strips_utm_params()
    {
        Assert.Equal(
            "https://example.com/path?keep=1",
            LinkNormalizer.Normalize("https://example.com/path?utm_source=a&keep=1&utm_medium=b"));
    }

    [Fact]
    public void Empty_link_normalizes_to_empty()
    {
        Assert.Equal(string.Empty, LinkNormalizer.Normalize(string.Empty));
        Assert.Equal(string.Empty, LinkNormalizer.Normalize(null));
    }
}
