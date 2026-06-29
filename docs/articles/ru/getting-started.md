> 🌐 [English](../en/getting-started.md) · **Русский**

# Быстрый старт

## Установка

```bash
dotnet add package YandexMusic
```

Для DI добавьте также `YandexMusic.DependencyInjection`. Таргеты: `net8.0`, `net9.0`, `net10.0`.

## Создание клиента

```csharp
using YandexMusic;

await using var client = new YandexMusicClient();
```

Конструктор принимает `YandexMusicClientOptions` для настройки таймаута, прокси, `User-Agent`,
идентификатора устройства и языка:

```csharp
using var client = new YandexMusicClient(new YandexMusicClientOptions
{
    Timeout = TimeSpan.FromSeconds(30),
    DeviceId = "my-app",
});
```

## Авторизация

```csharp
client.Authentication.SignInWithToken("<oauth-token>");
Console.WriteLine(client.Authentication.IsAuthenticated);
```

Про сохранение сессии — см. [Авторизация](authentication.md).

## Примеры

```csharp
// Трек и прямая ссылка на медиа
var track = await client.Tracks.GetAsync("4");
string? link = await client.Tracks.GetDirectLinkAsync("4");

// Тексты и похожие треки
var supplement = await client.Tracks.GetSupplementAsync("4");
var similar = await client.Tracks.GetSimilarAsync("4");

// Поиск и подсказки
var found = await client.Search.SearchAsync("Queen");
var hints = await client.Search.SuggestAsync("que");

// Альбомы и исполнители (все каталожные id — строки)
var album = await client.Albums.GetWithTracksAsync("3");
var artist = await client.Artists.GetBriefInfoAsync("79215");
var artistTracks = await client.Artists.GetTracksAsync("79215", page: 0, pageSize: 20);

// Плейлисты
var playlist = await client.Playlists.GetAsync("yamusic-daily", "1000");

// Аккаунт и библиотека (чтение и запись)
var status = await client.Account.GetStatusAsync();
var uid = status!.Account.Uid.ToString();
var liked = await client.Library.GetLikedTracksAsync(uid);
await client.Library.AddLikedTracksAsync(uid, ["4"]);

// Открытия: радио, лендинг, чарты
var dashboard = await client.Radio.GetStationsDashboardAsync();
var chart = await client.Landing.GetChartAsync("russia");
var newReleases = await client.Landing.GetNewReleasesAsync();
```

## Отмена операций

Каждый асинхронный метод принимает `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var track = await client.Tracks.GetAsync("4", cts.Token);
```

## Обработка ошибок

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
    Console.WriteLine($"Ошибка авторизации: {ex.Message}");
}
```

Иерархия исключений — в [Архитектуре](architecture.md).
