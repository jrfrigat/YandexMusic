> 🌐 [English](../en/faq.md) · **Русский**

# FAQ

## Это официальная библиотека?

Нет. Это неофициальный клиент, не связанный с Яндексом, и оригинальная clean-room реализация,
написанная по публичному HTTP-API. Используйте на свой риск и соблюдайте условия использования сервиса.

## Какие версии .NET поддерживаются?

`net8.0`, `net9.0` и `net10.0`.

## Как скачать или стримить трек?

Получите прямую ссылку через `GetDirectLinkAsync` или сырые варианты через `GetDownloadInfoAsync`:

```csharp
string? url = await client.Tracks.GetDirectLinkAsync("4");
var variants = await client.Tracks.GetDownloadInfoAsync("4");
```

Требуется авторизованная сессия с подходящей подпиской.

## Как использовать прокси?

Передайте `IWebProxy` через опции:

```csharp
using System.Net; // WebProxy

using var client = new YandexMusicClient(new YandexMusicClientOptions
{
    Proxy = new WebProxy("http://127.0.0.1:8080"),
});
```

## Как интегрировать с DI?

Используйте пакет `YandexMusic.DependencyInjection`:

```csharp
services.AddYandexMusic(o => o.Timeout = TimeSpan.FromSeconds(30));
```

Он регистрирует scoped `IYandexMusicClient`, изолированный на scope.

## Как обрабатывать ошибки API?

Ловите `YandexMusicApiException` (и при необходимости `YandexMusicAuthenticationException`,
`YandexMusicSerializationException`). Все наследуются от `YandexMusicException`.

## Почему не запускаются интеграционные тесты?

Они ходят в реальный API и пропускаются, если не задана `YANDEX_MUSIC_TOKEN`:

```bash
YANDEX_MUSIC_TOKEN=<ваш-токен> dotnet test
```
