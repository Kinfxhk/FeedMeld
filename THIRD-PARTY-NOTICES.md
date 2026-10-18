# Third-Party Notices

FeedMeld ships a self-contained list of dependencies. Runtime and build-time
components are listed below with their licence and source.

## Runtime dependencies

### System.ServiceModel.Syndication

- Version: 10.0.13
- Purpose: RSS 2.0 / Atom feed parsing and RSS 2.0 serialisation
  (`SyndicationFeed`, `SyndicationItem`, `Rss20FeedFormatter`).
- Licence: MIT
- Copyright: .NET Foundation and Contributors
- Source: https://github.com/dotnet/runtime

## Build / test-only dependencies (not shipped in the published binary)

### xUnit

- Version: 2.9.3
- Licence: Apache-2.0
- Source: https://github.com/xunit/xunit

### xunit.runner.visualstudio

- Version: 3.1.4
- Licence: Apache-2.0
- Source: https://github.com/xunit/visualstudio.xunit

### Microsoft.NET.Test.Sdk

- Version: 17.14.1
- Licence: MIT
- Source: https://github.com/microsoft/vstest

### coverlet.collector

- Version: 6.0.4
- Licence: MIT
- Source: https://github.com/coverlet-coverage/coverlet
