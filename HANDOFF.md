# HANDOFF — FeedMeld v1.0.0

## What was built

A .NET 10 console app (`src/FeedMeld`, `net10.0`) that merges and filters
RSS 2.0 / Atom feeds into one RSS 2.0 feed, plus an xUnit test project
(`tests/FeedMeld.Tests`). Only two runtime NuGet packages are used:
`System.ServiceModel.Syndication` (feed parse/serialise) and, for tests,
xUnit. Config uses `System.Text.Json`; regexes use
`System.Text.RegularExpressions` with a 1-second match timeout.

Behaviour implemented exactly per the task:

- `feedmeld <config.json>` loads feeds from `feeds` plus every `xmlUrl` in the
  OPML file. HTTP(S) fetched with `HttpClient` (20 s timeout, UA
  `FeedMeld/1.0`), local paths read relative to the config file.
- RSS 2.0 and Atom parsed via `SyndicationFeed`. Failed feeds are reported on
  stderr and skipped; run succeeds if ≥1 feed loaded (exit 0), else exit 2.
  Bad config = exit 1 with a clear message.
- Filtering: keep if (include empty OR any include matches title+summary) AND
  no exclude matches; drop items older than `maxAgeDays` (undated kept).
- Dedup by normalised link (lowercase scheme/host, strip trailing slash and
  `utm_*` query params), else by title.
- Sort newest first, take `maxItems`, write UTF-8 (no BOM) RSS 2.0. Each item
  keeps title/link/date/summary and gets `<category>` = source feed title.
- Summary line `feeds=N ok=N failed=N items_in=N items_out=N`.
- `feedmeld --example` prints an example config.

## Commands run (real output)

Build + test + package (via `pwsh -File build.ps1`):

```
== test ==
已通過! - 失敗: 0，通過: 19，略過: 0，總計: 19 ...
== publish ==
FeedMeld -> ...\dist\publish\
== zip ==
Built ...\dist\FeedMeld-v1.0.0-win-x64.zip
```

Real check (published exe against the two live public feeds):

```
./dist/publish/feedmeld.exe evidence/real.json
feeds=2 ok=2 failed=0 items_in=30 items_out=29
exit code 0
```

`evidence/merged.xml` (60,072 bytes) parses back with `SyndicationFeed` in
`EvidenceTests.Evidence_merged_xml_parses_back_with_syndication_feed`.
`evidence/merged-head.txt` holds the first 40 lines; `evidence/console.txt`
holds the stdout summary line; `evidence/console-stderr.txt` is empty.

Exit codes verified manually:

- `--example` → 0
- no args → 1 (usage)
- missing config file → 1
- invalid JSON → 1 (`bad config: invalid JSON: ...`)
- invalid regex → 1 (`bad config: invalid include regex ...`)
- unreachable feed (only source) → 2 (`error: no feeds could be loaded`)

## What could NOT be verified

- GitHub Actions CI was not run (no push permitted by the task). The workflow
  is written per the common spec (`windows-latest`, `setup-dotnet@v4` with
  `10.0.x`, `./build.ps1`, `upload-artifact@v4`).
- The app was only run on Windows x64; the linux-x64 GitHub Pages example in
  the README is example text only and was not executed.

## Decisions (simplest option that satisfies the spec)

- `output` is resolved relative to the config file directory (the spec only
  states this for local feed paths; config-relative was chosen for
  consistency and determinism regardless of CWD).
- `maxAgeDays` is applied literally: with `0`, every dated item is older than
  the cutoff and dropped (undated items kept).
- An item with no link and no title is kept as-is (nothing to dedup by).
- A feed item link is written to the output `<link>` only when absolute;
  relative links are still used for dedup but omitted from output.
- OPML `xmlUrl` entries are treated as URLs; a non-http entry is treated as a
  local path resolved against the config dir.

## Known limits

- Full-text fetching, web UI, database, webhooks and JSON Feed output are out
  of scope (not in v1), as specified.
- Item `<link>` is emitted with `CreateAlternateLink` so Rss20FeedFormatter
  writes a standard RSS 2.0 `<link>` (not an `atom:link` extension); this was
  verified against the .NET formatter source and the real output.


## Reviewer verification (it-dog)

- KinTest VM (release zip only): `feedmeld real.json` exit 0, `feeds=2 ok=2 failed=0 items_in=30 items_out=29`, merged.xml re-parsed with 29 items. Evidence: `evidence/vm/`.
