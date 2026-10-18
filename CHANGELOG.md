## 1.0.0 - 2026-10-19

- Merge multiple RSS 2.0 / Atom feeds into a single RSS 2.0 feed.
- Filter items with include/exclude regular expressions and a max age in days.
- Deduplicate by normalised link, or by title when the link is missing.
- Sort newest first and cap the result at `maxItems`.
- Load feeds over HTTP(S) and from local files, plus every `xmlUrl` in an OPML file.
- Write UTF-8 (no BOM) RSS 2.0; each item keeps title, link, date and summary and gains a `<category>` set to the source feed title.
- `feedmeld --example` prints an example config.
