# YandexMusic

Unofficial asynchronous .NET client for the Yandex Music API. Targets **.NET 8, 9 and 10**.

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
client.Authentication.SignInWithToken("<oauth-token>");

var track = await client.Tracks.GetAsync("33221455");
Console.WriteLine(track?.Title);
```

Every method is asynchronous and accepts a `CancellationToken`. For dependency-injection use the
[`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection)
package (`services.AddYandexMusic()`).

Documentation: <https://jrfrigat.github.io/YandexMusic/>

> ⚠️ Unofficial library, not affiliated with Yandex. Use at your own risk and comply with the
> Yandex Music terms of use.
