# YandexMusic.DependencyInjection

Dependency-injection integration for the [YandexMusic](https://www.nuget.org/packages/YandexMusic) client.

```csharp
services.AddYandexMusic(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DeviceId = "my-app";
});
```

Registers a **scoped** `IYandexMusicClient` over a handler pooled by `IHttpClientFactory`. Each scope
(for example, an HTTP request or a signed-in user) gets its own authentication session, so access
tokens never leak between users, while the underlying connection pool is shared and long-lived.

> ⚠️ Unofficial library, not affiliated with Yandex.
