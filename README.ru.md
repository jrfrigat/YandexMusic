<p align="center">
  <img src="assets/icon.svg" width="112" alt="YandexMusic" />
</p>

<h1 align="center">YandexMusic для .NET</h1>

<p align="center">🌐 <a href="README.md">English</a> · <b>Русский</b></p>

[![CI](https://github.com/jrfrigat/YandexMusic/actions/workflows/ci.yml/badge.svg)](https://github.com/jrfrigat/YandexMusic/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/YandexMusic.svg?logo=nuget)](https://www.nuget.org/packages/YandexMusic)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com)

Неофициальная асинхронная библиотека для работы с API Яндекс Музыки. Работает на **.NET 8, .NET 9 и .NET 10**.

> ⚠️ Неофициальный проект, не связанный с Яндексом. Используйте на свой риск и соблюдайте условия использования сервиса.

## Возможности

- ✅ Полностью асинхронный API с поддержкой `CancellationToken` во всех методах
- ✅ **Полное покрытие каталога** — треки (метаданные, **прямая ссылка на скачивание/стрим**, тексты, full-info, похожие, трейлер), поиск (+ подсказки), альбомы, исполнители, плейлисты, жанры, лейблы, клипы, кредиты, дисклеймеры, концерты, мета-теги
- ✅ **Персональные эндпоинты** — аккаунт и настройки, библиотека (лайки/дизлайки на чтение и запись), редактирование плейлистов, радио (rotor), лендинг и фид, очереди между устройствами, пины, пресейвы, история прослушиваний
- ✅ **Три способа входа** — OAuth-токен, официальный OAuth **device-code** flow и best-effort cookie/QR; всё поверх сериализуемой сессии с сохранением/восстановлением
- ✅ Source-generation `System.Text.Json` — экономно к аллокациям, дружелюбно к trim/AOT (`IsAotCompatible`)
- ✅ Типизированные исключения, интеграция с DI, полная XML-документация
- ✅ Чистая расширяемая архитектура: добавил группу эндпоинтов — получил новый домен

## Установка

```bash
# Основной клиент
dotnet add package YandexMusic

# Опционально: интеграция с DI
dotnet add package YandexMusic.DependencyInjection
```

| Пакет | Назначение |
|-------|------------|
| [`YandexMusic`](https://www.nuget.org/packages/YandexMusic) | Клиент `YandexMusicClient`, модели, авторизация и группы эндпоинтов. |
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `AddYandexMusic()` — scoped-клиент поверх пула `IHttpClientFactory`. |

## Быстрый старт

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();

// Авторизация по OAuth-токену (не храните токен в коде — env-переменная или защищённое хранилище)
client.Authentication.SignInWithToken(Environment.GetEnvironmentVariable("YANDEX_MUSIC_TOKEN")!);

// Метаданные трека и прямая ссылка на медиа
var track = await client.Tracks.GetAsync("4");
Console.WriteLine(track?.Title);
var link = await client.Tracks.GetDirectLinkAsync("4");

// Поиск и подсказки
var results = await client.Search.SearchAsync("Queen");
var hints = await client.Search.SuggestAsync("que");

// Альбомы, исполнители, плейлисты (все каталожные id — строки)
var album = await client.Albums.GetWithTracksAsync("3");
var artist = await client.Artists.GetBriefInfoAsync("79215");
var playlist = await client.Playlists.GetAsync("yamusic-daily", "1000");

// Аккаунт и библиотека
var status = await client.Account.GetStatusAsync();
var uid = status!.Account.Uid.ToString();
var liked = await client.Library.GetLikedTracksAsync(uid);
await client.Library.AddLikedTracksAsync(uid, ["4"]);

// Открытия: радио, лендинг, чарты
var dashboard = await client.Radio.GetStationsDashboardAsync();
var chart = await client.Landing.GetChartAsync("russia");
var newReleases = await client.Landing.GetNewReleasesAsync();
```

### Вход через OAuth device-code flow

Без работы с паролем — покажите пользователю короткий код и опрашивайте сервер до подтверждения:

```csharp
await using var client = new YandexMusicClient();
var token = await client.Authentication.SignInWithDeviceFlowAsync(code =>
    Console.WriteLine($"Откройте {code.VerificationUrl} и введите код {code.UserCode}"));
// Клиент авторизован; сохраните token.AccessToken для повторного использования.
```

Все методы принимают `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var track = await client.Tracks.GetAsync("4", cts.Token);
```

## Авторизация и сохранение сессии

Войдите по OAuth-токену, затем экспортируйте сессию для восстановления позже:

```csharp
client.Authentication.SignInWithToken("<oauth-token>");

var snapshot = client.Authentication.Session.Export();      // сериализуемая запись
var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
// ... сохраните json в защищённом хранилище ...
client.Authentication.Session.Import(
    System.Text.Json.JsonSerializer.Deserialize<YandexMusic.Authentication.AuthSnapshot>(json)!);
```

## Внедрение зависимостей

```csharp
services.AddYandexMusic(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DeviceId = "my-app";
});

// IYandexMusicClient регистрируется как scoped, изолированно на scope.
```

## Документация

Полные руководства и справочник API: **<https://jrfrigat.github.io/YandexMusic/>**

- [Быстрый старт](docs/articles/ru/getting-started.md)
- [Авторизация](docs/articles/ru/authentication.md)
- [Архитектура](docs/articles/ru/architecture.md)
- [FAQ](docs/articles/ru/faq.md)

## Структура репозитория

```
.
├── src/
│   ├── YandexMusic/                     # основная библиотека (клиент, модели, эндпоинты, auth, JSON)
│   └── YandexMusic.DependencyInjection/ # интеграция AddYandexMusic()
├── tests/
│   └── YandexMusic.Tests/               # модульные + (по токену) интеграционные тесты (xUnit)
├── docs/                                # сайт документации (DocFX)
└── .github/workflows/                   # CI, релиз (NuGet), публикация документации
```

## Сборка и тесты

```bash
dotnet restore
dotnet build -c Release
dotnet test  -c Release
```

Требуется .NET SDK 10 (он собирает таргеты net8.0/net9.0/net10.0). Интеграционные тесты ходят в
реальный API и **пропускаются автоматически**, если не задана `YANDEX_MUSIC_TOKEN`:

```bash
YANDEX_MUSIC_TOKEN=<ваш-токен> dotnet test -c Release
```

## Лицензия

[MIT](LICENSE) © FrigaT
