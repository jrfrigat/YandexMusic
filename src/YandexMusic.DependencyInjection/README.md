# YandexMusic.DependencyInjection

Dependency-injection integration for the [YandexMusic](https://www.nuget.org/packages/YandexMusic)
client. Targets **.NET 8, 9 and 10**.

```csharp
services.AddYandexMusic(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DeviceId = "my-app";
});
```

Registers a **scoped** `IYandexMusicClient` over a handler pooled by `IHttpClientFactory`. Each scope
— an HTTP request, a signed-in user — gets its own authentication session, so access tokens never
leak between users, while the underlying connection pool is shared and long-lived. The client is
disposed with the scope.

Under DI the pooled handler is deliberately **cookie-free**: requests authenticate with the
per-scope OAuth token, so a long-lived shared handler cannot carry one user's cookies into another
user's request. The interactive cookie and QR sign-in flows use their own clients inside the library
and are unaffected.

## Related packages

| Package | Adds |
|---------|------|
| [`YandexMusic.Ynison`](https://www.nuget.org/packages/YandexMusic.Ynison) | The account's live playback state across its devices, and the remote. |
| [`YandexMusic.Quasar`](https://www.nuget.org/packages/YandexMusic.Quasar) | Yandex speakers on the local network: discovery and direct control. |

Documentation: <https://jrfrigat.github.io/YandexMusic/>

> ⚠️ Unofficial library, not affiliated with Yandex. Use at your own risk and comply with the
> Yandex Music terms of use.
