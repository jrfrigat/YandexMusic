> 🌐 **English** · [Русский](../ru/faq.md)

# FAQ

## Is this an official library?

No. This is an unofficial client, not affiliated with Yandex, and an original clean-room
implementation written from the public HTTP API. Use it at your own risk and comply with the
service's terms of use.

## Which .NET versions are supported?

`net8.0`, `net9.0` and `net10.0`.

## How do I download or stream a track?

Resolve a direct media URL with `GetDirectLinkAsync`, or fetch the raw variants with
`GetDownloadInfoAsync`:

```csharp
string? url = await client.Tracks.GetDirectLinkAsync("4");
var variants = await client.Tracks.GetDownloadInfoAsync("4");
```

These require an authenticated session with the appropriate subscription.

## How do I use a proxy?

Pass an `IWebProxy` through the options:

```csharp
using var client = new YandexMusicClient(new YandexMusicClientOptions
{
    Proxy = new WebProxy("http://127.0.0.1:8080"),
});
```

## How do I integrate with dependency injection?

Use the `YandexMusic.DependencyInjection` package:

```csharp
services.AddYandexMusic(o => o.Timeout = TimeSpan.FromSeconds(30));
```

It registers a scoped `IYandexMusicClient`, isolated per scope.

## How do I handle API errors?

Catch `YandexMusicApiException` (and, if needed, `YandexMusicAuthenticationException`,
`YandexMusicSerializationException`). They all derive from `YandexMusicException`.

## Why do the integration tests not run?

They hit the live API and are skipped unless `YANDEX_MUSIC_TOKEN` is set:

```bash
YANDEX_MUSIC_TOKEN=<your-token> dotnet test
```
