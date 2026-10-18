# FeedMeld

FeedMeld is a command-line tool that merges and filters multiple RSS/Atom feeds
into a single RSS 2.0 feed — a replacement for the feed-merge and
filtering workflows that Yahoo Pipes and FeedDemon once provided.

## Features

- Reads RSS 2.0 and Atom feeds from HTTP(S) URLs and local files, plus every
  `xmlUrl` inside an OPML file.
- Filters items with include/exclude regular expressions (matched against
  title + summary) and drops items older than `maxAgeDays`.
- Deduplicates by normalised link (lowercased scheme/host, trailing slash and
  `utm_*` query params stripped), or by title when the link is missing.
- Sorts newest first, caps the result at `maxItems`, and writes UTF-8
  (no BOM) RSS 2.0. Each item keeps title, link, date and summary, and gains a
  `<category>` set to the source feed title.
- `feedmeld --example` prints an example config.
- Prints a one-line summary: `feeds=N ok=N failed=N items_in=N items_out=N`.

## Install

1. Download `FeedMeld-v1.0.0-win-x64.zip` from the latest release (or build it,
   see below).
2. Unzip it.
3. Run `feedmeld.exe <config.json>`.

No install required — the exe is a self-contained single file (it uses the
installed .NET runtime).

## Usage

```json
{
  "title": "My tech news",
  "link": "https://example.com/",
  "output": "out/merged.xml",
  "maxItems": 100,
  "maxAgeDays": 30,
  "feeds": ["https://...", "local/file.xml"],
  "opml": "optional/path.opml",
  "include": ["(?i)release|security"],
  "exclude": ["(?i)sponsored"]
}
```

Local feed paths and the `output` path are resolved relative to the config
file's directory. Exit codes: `0` success, `1` bad config, `2` no feed loaded.

## Build

```powershell
./build.ps1
```

This restores, builds Release, runs the test suite, publishes the portable
single-file exe, and packages `dist/FeedMeld-v1.0.0-win-x64.zip`.

## Scheduling on GitHub Actions (example)

The following workflow is an **example** only — it is not part of this
repository's live CI. Copy it into your own repo to run FeedMeld on a schedule
and publish `merged.xml` to GitHub Pages:

```yaml
# .github/workflows/feed.yml  (example — adapt the feeds and path to your needs)
name: FeedMeld

on:
  schedule:
    - cron: '0 */6 * * *'
  workflow_dispatch:

permissions:
  contents: write

jobs:
  feed:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Build FeedMeld
        run: |
          dotnet publish src/FeedMeld/FeedMeld.csproj -c Release -r linux-x64 \
            --self-contained false -p:PublishSingleFile=true -o out
      - name: Write config
        run: |
          cat > config.json <<'EOF'
          {
            "title": "My tech news",
            "link": "https://example.com/",
            "output": "docs/merged.xml",
            "maxItems": 100,
            "maxAgeDays": 30,
            "feeds": ["https://hnrss.org/frontpage"]
          }
          EOF
      - name: Run FeedMeld
        run: ./out/feedmeld config.json
      - name: Commit to GitHub Pages
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
          git add docs/merged.xml
          git commit -m "Update merged feed" || echo "No changes"
          git push
```

Then enable GitHub Pages on the `main` branch and serve `docs/merged.xml`.

## Privacy

FeedMeld reads the config file, the feed URLs listed in it, and any local feed
or OPML files you point it at. It writes only the merged feed file. It sends
nothing to any server other than fetching the feed URLs you configured.

## Licence

MIT. See [LICENSE](LICENSE).

---

# FeedMeld（繁體中文）

FeedMeld 是一個命令列工具，可將多個 RSS/Atom 資訊源合併並過濾為單一 RSS 2.0
資訊源——用以取代 Yahoo Pipes 與 FeedDemon 過去提供的資訊源合併與過濾功能。

## 功能

- 從 HTTP(S) 網址與本機檔案讀取 RSS 2.0 與 Atom 資訊源，並讀取 OPML 檔案中每個
  `xmlUrl`。
- 以 include/exclude 正規表示式（比對標題＋摘要）過濾項目，並捨棄超過
  `maxAgeDays` 的舊項目。
- 依正規化連結（通訊協定／主機名改為小寫、移除結尾斜線與 `utm_*` 查詢參數）去重，
  缺少連結時改以標題去重。
- 由新到舊排序，以 `maxItems` 截斷結果，輸出 UTF-8（無 BOM）的 RSS 2.0。每個項目
  保留標題、連結、日期與摘要，並加上 `<category>`（設為來源資訊源的標題）。
- `feedmeld --example` 可印出範例設定檔。
- 印出一行摘要：`feeds=N ok=N failed=N items_in=N items_out=N`。

## 安裝

1. 從最新 release 下載 `FeedMeld-v1.0.0-win-x64.zip`（或自行建置，見下文）。
2. 解壓縮。
3. 執行 `feedmeld.exe <config.json>`。

無需安裝——執行檔為自足式的單一檔案（使用已安裝的 .NET 執行環境）。

## 用法

```json
{
  "title": "My tech news",
  "link": "https://example.com/",
  "output": "out/merged.xml",
  "maxItems": 100,
  "maxAgeDays": 30,
  "feeds": ["https://...", "local/file.xml"],
  "opml": "optional/path.opml",
  "include": ["(?i)release|security"],
  "exclude": ["(?i)sponsored"]
}
```

本機資訊源路徑與 `output` 路徑皆相對於設定檔所在目錄解析。結束代碼：
`0` 成功、`1` 設定檔錯誤、`2` 沒有任何資訊源載入成功。

## 建置

```powershell
./build.ps1
```

此指令會還原、以 Release 建置、執行測試、發佈可攜式單檔執行檔，並封裝
`dist/FeedMeld-v1.0.0-win-x64.zip`。

## 在 GitHub Actions 上排程（範例）

以下工作流程**僅為範例**——不屬於本儲存庫的正式 CI。可複製到自己的儲存庫，以排程
執行 FeedMeld 並將 `merged.xml` 發佈到 GitHub Pages：

```yaml
# .github/workflows/feed.yml  (範例 — 請依需要調整資訊源與路徑)
name: FeedMeld

on:
  schedule:
    - cron: '0 */6 * * *'
  workflow_dispatch:

permissions:
  contents: write

jobs:
  feed:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Build FeedMeld
        run: |
          dotnet publish src/FeedMeld/FeedMeld.csproj -c Release -r linux-x64 \
            --self-contained false -p:PublishSingleFile=true -o out
      - name: Write config
        run: |
          cat > config.json <<'EOF'
          {
            "title": "My tech news",
            "link": "https://example.com/",
            "output": "docs/merged.xml",
            "maxItems": 100,
            "maxAgeDays": 30,
            "feeds": ["https://hnrss.org/frontpage"]
          }
          EOF
      - name: Run FeedMeld
        run: ./out/feedmeld config.json
      - name: Commit to GitHub Pages
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
          git add docs/merged.xml
          git commit -m "Update merged feed" || echo "No changes"
          git push
```

接著在 `main` 分支啟用 GitHub Pages，並提供 `docs/merged.xml`。

## 隱私

FeedMeld 會讀取設定檔、其中列出的資訊源網址，以及你指定的本機資訊源或 OPML 檔案。
它只會寫出合併後的資訊源檔案；除了擷取你設定的資訊源網址外，不會傳送任何資料到
任何伺服器。

## 授權

MIT。詳見 [LICENSE](LICENSE)。

## Sponsor / 贊助

If this tool helps you, you can buy me a coffee. 如果這個工具對你有幫助，歡迎請我喝杯咖啡。

- Buy Me a Coffee: https://buymeacoffee.com/kinfxhk
- PayPal: https://paypal.me/sc2nohope
- PayMe (HSBC): https://payme.hsbc/kinfx
