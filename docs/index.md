# YandexMusic for .NET

Unofficial asynchronous client for the Yandex Music API. Supports **.NET 8, .NET 9 and .NET 10**.

🌐 **English** · Русская версия — ниже ↓

## Packages

| Package | Description |
|---------|-------------|
| [`YandexMusic`](https://www.nuget.org/packages/YandexMusic) | The `YandexMusicClient`, models, authentication and endpoint groups. |
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `AddYandexMusic()` — a scoped client over an `IHttpClientFactory` pool. |

## Where to start

- [Getting started](articles/en/getting-started.md)
- [Authentication](articles/en/authentication.md)
- [Architecture](articles/en/architecture.md)
- [FAQ](articles/en/faq.md)
- [API Reference](api/index.md)

## Installation

```bash
dotnet add package YandexMusic
```

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
client.Authentication.SignInWithToken("<oauth-token>");

var track = await client.Tracks.GetAsync("4");
Console.WriteLine(track?.Title);
```

> [!WARNING]
> This is an unofficial library. Use it at your own risk and comply with the Yandex Music terms of use.

---

# YandexMusic для .NET

Неофициальная асинхронная библиотека для работы с API Яндекс Музыки. Поддерживает **.NET 8, .NET 9 и .NET 10**.

🌐 English version — above ↑ · **Русский**

## Пакеты

| Пакет | Описание |
|-------|----------|
| [`YandexMusic`](https://www.nuget.org/packages/YandexMusic) | Клиент `YandexMusicClient`, модели, авторизация и группы эндпоинтов. |
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `AddYandexMusic()` — scoped-клиент поверх пула `IHttpClientFactory`. |

## С чего начать

- [Быстрый старт](articles/ru/getting-started.md)
- [Авторизация](articles/ru/authentication.md)
- [Архитектура](articles/ru/architecture.md)
- [FAQ](articles/ru/faq.md)
- [Справочник API](api/index.md)

## Установка

```bash
dotnet add package YandexMusic
```

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
client.Authentication.SignInWithToken("<oauth-token>");

var track = await client.Tracks.GetAsync("4");
Console.WriteLine(track?.Title);
```

> [!WARNING]
> Это неофициальная библиотека. Используйте её на свой риск и соблюдайте условия использования Яндекс Музыки.
