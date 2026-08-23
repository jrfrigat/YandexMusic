# YandexMusic

Unofficial asynchronous .NET client for the Yandex Music API. Targets **.NET 8, 9 and 10**.

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
client.Authentication.SignInWithToken("<oauth-token>");

var track = await client.Tracks.GetAsync("33221455");
Console.WriteLine(track?.Title);

var found = await client.Search.SearchAsync("Queen");
var link = await client.Tracks.GetDirectLinkAsync("33221455");
```

Covers the catalogue (tracks, search, albums, artists, playlists, genres, labels, clips, credits,
concerts, meta-tag pages) and the personal side (account, library likes, playlist editing, radio,
landing and feed, queues, pins, pre-saves, history). Every method is asynchronous and accepts a
`CancellationToken`.

This package depends on nothing but the BCL, and it is complete on its own — the rest are optional.

| Package | Adds |
|---------|------|
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `services.AddYandexMusic()` — a scoped client over a pooled handler. |
| [`YandexMusic.Ynison`](https://www.nuget.org/packages/YandexMusic.Ynison) | The account's live playback state across its devices, and the remote. |
| [`YandexMusic.Quasar`](https://www.nuget.org/packages/YandexMusic.Quasar) | Yandex speakers on the local network: discovery and direct control. |

Documentation: <https://jrfrigat.github.io/YandexMusic/>

> ⚠️ Unofficial library, not affiliated with Yandex. Use at your own risk and comply with the
> Yandex Music terms of use.
