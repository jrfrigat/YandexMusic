> 🌐 **English** · [Русский](../ru/getting-started.md)

# Getting started

## Installation

```bash
dotnet add package YandexMusic
```

For dependency injection also add `YandexMusic.DependencyInjection`. Targets `net8.0`, `net9.0` and `net10.0`.

## Creating a client

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
```

The constructor accepts `YandexMusicClientOptions` to configure the timeout, proxy, `User-Agent`,
device id and language:

```csharp
using var client = new YandexMusicClient(new YandexMusicClientOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    DeviceId = "my-app",
});
```

## Authorization

```csharp
client.Authentication.SignInWithToken("<oauth-token>");
Console.WriteLine(client.Authentication.IsAuthenticated);
```

See [Authentication](authentication.md) for session persistence.

## Examples

```csharp
// Track and a direct media link
var track = await client.Tracks.GetAsync("4");
string? link = await client.Tracks.GetDirectLinkAsync("4");

// Lyrics and similar tracks
var supplement = await client.Tracks.GetSupplementAsync("4");
var similar = await client.Tracks.GetSimilarAsync("4");

// Search and autocomplete
var found = await client.Search.SearchAsync("Queen");
var hints = await client.Search.SuggestAsync("que");

// Albums and artists (all catalogue ids are strings)
var album = await client.Albums.GetWithTracksAsync("3");
var artist = await client.Artists.GetBriefInfoAsync("79215");
var artistTracks = await client.Artists.GetTracksAsync("79215", page: 0, pageSize: 20);

// Playlists
var playlist = await client.Playlists.GetAsync("yamusic-daily", "1000");

// Account and library (read & write)
var status = await client.Account.GetStatusAsync();
var uid = status!.Account.Uid.ToString();
var liked = await client.Library.GetLikedTracksAsync(uid);
await client.Library.AddLikedTracksAsync(uid, ["4"]);

// Discovery: radio, landing, charts
var dashboard = await client.Radio.GetStationsDashboardAsync();
var chart = await client.Landing.GetChartAsync("russia");
var newReleases = await client.Landing.GetNewReleasesAsync();
```

## Cancellation

Every asynchronous method accepts a `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var track = await client.Tracks.GetAsync("4", cts.Token);
```

## Error handling

```csharp
using YandexMusic;

try
{
    var album = await client.Albums.GetAsync("0");
}
catch (YandexMusicApiException ex)
{
    Console.WriteLine($"{(int)ex.StatusCode}: {ex.ErrorName}");
}
catch (YandexMusicAuthenticationException ex)
{
    Console.WriteLine($"Authorization error: {ex.Message}");
}
```

See the exception hierarchy in [Architecture](architecture.md).
