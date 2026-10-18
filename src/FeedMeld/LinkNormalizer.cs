using System.Text;

namespace FeedMeld;

public static class LinkNormalizer
{
    public static string Normalize(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return string.Empty;

        link = link.Trim();
        if (!Uri.TryCreate(link, UriKind.Absolute, out Uri? uri))
            return link.ToLowerInvariant();

        var sb = new StringBuilder();
        sb.Append(uri.Scheme.ToLowerInvariant()).Append("://").Append(uri.Host.ToLowerInvariant());
        if (!uri.IsDefaultPort && uri.Port > 0)
            sb.Append(':').Append(uri.Port);

        string path = uri.AbsolutePath;
        if (path.Length > 1 && path.EndsWith('/'))
            path = path[..^1];
        sb.Append(path);

        string query = uri.Query.TrimStart('?');
        if (query.Length > 0)
        {
            var kept = new List<string>();
            foreach (string part in query.Split('&'))
            {
                if (part.Length == 0)
                    continue;
                int eq = part.IndexOf('=');
                string key = eq < 0 ? part : part[..eq];
                if (key.StartsWith("utm_", StringComparison.OrdinalIgnoreCase))
                    continue;
                kept.Add(part);
            }
            if (kept.Count > 0)
                sb.Append('?').Append(string.Join('&', kept));
        }

        return sb.ToString();
    }

    public static string NormalizeTitle(string? title)
        => (title ?? string.Empty).Trim().ToLowerInvariant();
}
