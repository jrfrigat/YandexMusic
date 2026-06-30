<p align="center">
  <img src="assets/icon.svg" width="112" alt="YandexMusic" />
</p>

<h1 align="center">YandexMusic for .NET</h1>

<p align="center">🌐 <b>English</b> · <a href="README.ru.md">Русский</a></p>

[![CI](https://github.com/jrfrigat/YandexMusic/actions/workflows/ci.yml/badge.svg)](https://github.com/jrfrigat/YandexMusic/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/YandexMusic.svg?logo=nuget)](https://www.nuget.org/packages/YandexMusic)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com)

Unofficial asynchronous library for the Yandex Music API. Runs on **.NET 8, .NET 9 and .NET 10**.

> ⚠️ Unofficial project, not affiliated with Yandex. Use at your own risk and comply with the service's terms of use.

## Features

- ✅ Fully asynchronous API with `CancellationToken` support on every call
- ✅ **Full catalogue coverage** — tracks (metadata, **direct download/stream link**, lyrics, full-info, similar, trailer), search (+ autocomplete), albums, artists, playlists, genres, labels, clips, credits, disclaimers, concerts, meta-tag pages
- ✅ **Personalised endpoints** — account & settings, library likes/dislikes (read & write), playlist editing, radio (rotor) stations, landing & feed, cross-device queues, pins, pre-saves, listening history
- ✅ **Three sign-in flows** — OAuth token, the official OAuth **device-code** flow, and best-effort cookie/QR; all over a serializable session you can persist and restore
- ✅ `System.Text.Json` source generation — allocation-conscious and trim/AOT-friendly (`IsAotCompatible`)
- ✅ Typed exceptions, first-class dependency-injection integration, full XML documentation
- ✅ Clean, extensible design: add an endpoint group and you have a new domain

## Installation

```bash
# Core client
dotnet add package YandexMusic

# Optional: dependency-injection integration
dotnet add package YandexMusic.DependencyInjection
```

| Package | Purpose |
|---------|---------|
| [`YandexMusic`](https://www.nuget.org/packages/YandexMusic) | The `YandexMusicClient`, models, authentication and endpoint groups. |
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `AddYandexMusic()` — a scoped client over an `IHttpClientFactory` pool. |

## Quick start

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();

// Authorize with an OAuth token (never hardcode it — use an environment variable or a secure store)
client.Authentication.SignInWithToken(Environment.GetEnvironmentVariable("YANDEX_MUSIC_TOKEN")!);

// Track metadata and a direct media link
var track = await client.Tracks.GetAsync("4");
Console.WriteLine(track?.Title);
var link = await client.Tracks.GetDirectLinkAsync("4");

// Search and autocomplete
var results = await client.Search.SearchAsync("Queen");
var hints = await client.Search.SuggestAsync("que");

// Albums, artists, playlists (all catalogue ids are strings)
var album = await client.Albums.GetWithTracksAsync("3");
var artist = await client.Artists.GetBriefInfoAsync("79215");
var playlist = await client.Playlists.GetAsync("yamusic-daily", "1000");

// Account and library
var status = await client.Account.GetStatusAsync();
var uid = status!.Account.Uid.ToString();
var liked = await client.Library.GetLikedTracksAsync(uid);
await client.Library.AddLikedTracksAsync(uid, ["4"]);

// Discovery: radio, landing, charts
var dashboard = await client.Radio.GetStationsDashboardAsync();
var chart = await client.Landing.GetChartAsync("russia");
var newReleases = await client.Landing.GetNewReleasesAsync();
```

### Sign in with the OAuth device-code flow

No password handling — show the user a short code, then poll until they confirm it:

```csharp
await using var client = new YandexMusicClient();
var token = await client.Authentication.SignInWithDeviceFlowAsync(code =>
    Console.WriteLine($"Open {code.VerificationUrl} and enter code {code.UserCode}"));
// The client is now authenticated; persist token.AccessToken if you want to reuse it.
```

Every method accepts a `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var track = await client.Tracks.GetAsync("4", cts.Token);
```

## Authentication & session persistence

Sign in with an OAuth token, then export the session to resume it later:

```csharp
client.Authentication.SignInWithToken("<oauth-token>");

var snapshot = client.Authentication.Session.Export();      // serializable record
var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
// ... store json securely ...
client.Authentication.Session.Import(
    System.Text.Json.JsonSerializer.Deserialize<YandexMusic.Authentication.AuthSnapshot>(json)!);
```

## Dependency injection

```csharp
services.AddYandexMusic(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DeviceId = "my-app";
});

// IYandexMusicClient is registered as scoped, isolated per scope.
```

## Documentation

Full guides and API reference: **<https://jrfrigat.github.io/YandexMusic/>**

- [Getting started](docs/articles/en/getting-started.md)
- [Authentication](docs/articles/en/authentication.md)
- [Architecture](docs/articles/en/architecture.md)
- [FAQ](docs/articles/en/faq.md)

## Repository layout

```
.
├── src/
│   ├── YandexMusic/                     # core library (client, models, endpoints, auth, JSON)
│   └── YandexMusic.DependencyInjection/ # AddYandexMusic() integration
├── tests/
│   └── YandexMusic.Tests/               # unit + (token-gated) integration tests (xUnit)
├── samples/
│   └── YandexMusic.Cli/                 # console demo (tracks, albums, artists, search, …)
├── docs/                                # documentation site (DocFX)
└── .github/workflows/                   # CI, release (NuGet), docs publishing
```

Try the sample against the live catalogue (no token needed for public data):

```bash
dotnet run --project samples/YandexMusic.Cli -- search "Queen"
dotnet run --project samples/YandexMusic.Cli -- album 3
dotnet run --project samples/YandexMusic.Cli -- artist 79215
# token-gated commands (link, liked):
YANDEX_MUSIC_TOKEN=<token> dotnet run --project samples/YandexMusic.Cli -- liked
```

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test  -c Release
```

The .NET SDK 10 is required (it builds the net8.0/net9.0/net10.0 targets). Integration tests hit the
real API and are **skipped automatically** unless `YANDEX_MUSIC_TOKEN` is set:

```bash
YANDEX_MUSIC_TOKEN=<your-token> dotnet test -c Release
```

## License

[MIT](LICENSE) © FrigaT
