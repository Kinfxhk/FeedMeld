$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$dist = Join-Path $root "dist"
$publishDir = Join-Path $dist "publish"
$version = "1.0.0"
$zip = Join-Path $dist "FeedMeld-v$version-win-x64.zip"

Write-Host "== restore =="
dotnet restore "$root\FeedMeld.slnx"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "== build =="
dotnet build "$root\FeedMeld.slnx" -c Release --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "== test =="
dotnet test "$root\tests\FeedMeld.Tests\FeedMeld.Tests.csproj" -c Release --no-build --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "== publish =="
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish "$root\src\FeedMeld\FeedMeld.csproj" -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $publishDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "== zip =="
New-Item -ItemType Directory -Force -Path $dist | Out-Null
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zip

Write-Host "Built $zip"
