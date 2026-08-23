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
- ✅ **Ynison в реальном времени** — подписка на состояние воспроизведения аккаунта на всех устройствах и дистанционное управление (пауза, треки, громкость) по websocket-протоколу официальных клиентов
- ✅ **Умные колонки** — поиск в локальной сети по mDNS и прямое управление, с пиннингом TLS-сертификата; колонка никогда не входит в сессию аккаунта, так что это единственный способ до неё добраться
- ✅ **Три способа входа** — OAuth-токен, официальный OAuth **device-code** flow и best-effort cookie/QR; всё поверх сериализуемой сессии с сохранением/восстановлением
- ✅ Source-generation `System.Text.Json` — экономно к аллокациям, дружелюбно к trim/AOT (`IsAotCompatible`)
- ✅ Типизированные исключения, интеграция с DI, полная XML-документация
- ✅ Чистая расширяемая архитектура: добавил группу эндпоинтов — получил новый домен

### Пример: терминальный музыкальный плеер

[`samples/YandexMusicTerminal`](samples/YandexMusicTerminal) — полноценный интерактивный TUI поверх
библиотеки: поиск, просмотр своих альбомов и плейлистов, экран «сейчас играет» с анимированным
эквалайзером, прогресс-баром в реальном времени и управлением громкостью/воспроизведением с клавиатуры.

**Установка** — одной командой, .NET не нужен (сборки self-contained). Ставится в профиль
пользователя, без прав администратора, и добавляет `ymt` в PATH:

```powershell
irm https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.ps1 | iex
```

```bash
curl -fsSL https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.sh | sh
```

Дальше запуск откуда угодно:

```bash
ymt
```

Повторный запуск той же команды обновляет на месте, но это почти никогда не нужно: плеер проверяет
GitHub на новую версию при запуске и дальше каждые полчаса, пишет об этом в главном меню и ставит её
по `u`. «О программе» (`i`) показывает текущую версию и проверяет обновления по требованию.
`YM_PLAYER_NO_UPDATE_CHECK=1` отключает автоматическую проверку, ручная продолжает работать. Как
закрепить версию или сменить каталог — см. [`scripts/`](scripts).

**Либо** возьмите архивы вручную со страницы
[**Releases**](https://github.com/jrfrigat/YandexMusic/releases) (`ymt-<version>-win-x64.zip`,
`ymt-<version>-linux-x64.tar.gz`), либо запустите из исходников:

```bash
dotnet run --project samples/YandexMusicTerminal
```

- **Вход** по OAuth-токену, через device-code flow, QR-код или логин/пароль; сессия кэшируется, поэтому
  следующий запуск стартует уже авторизованным.
- **Воспроизведение** использует [NAudio](https://github.com/naudio/NAudio) на Windows; на остальных
  платформах (и как fallback) работает беззвучная симуляция, управляющая тем же интерфейсом. Аудио-бэкенд —
  единственный шов [`IAudioPlayer`](samples/YandexMusicTerminal/Playback/IAudioPlayer.cs), так что замена
  на кроссплатформенный движок меняет одну строку.
- **Пульт** — экран показывает и Ynison-устройства аккаунта, и колонки, которые отвечают в этой сети,
  единой нумерацией. Выбираете клавишами `1-9`, и управление уходит туда; `0` возвращает клавиши
  сессии, `r` перезапускает поиск.
- **Действия с треком** (экран «сейчас играет») — `l` лайк · `x` дизлайк (и пропуск) · `t` текст
  песни · `i` бесконечное радио похожих треков · `r` отправить трек на колонку, дальше она берёт его
  из каталога сама. Старты и скипы отчитываются в API, поэтому «Моя волна» учится на том, что вы
  слушаете.
- **Поиск** — вкладки треков, артистов, альбомов и плейлистов, пагинация строкой «ещё», drill-in в
  треки выбранного артиста, альбома или плейлиста.
- **Главное меню** управляется курсором, снизу — строка горячих клавиш; одиночные клавиши сразу
  открывают раздел (`s` поиск · `a` альбомы · `l` плейлисты · `f` любимое · `w` волна · `p` плеер ·
  `r` пульт · `g` журнал запросов · `q` выход), а `Esc` всегда возвращает назад.
- **Управление** (экран «сейчас играет»): `space` пауза · `←/→` пред/след · `↑/↓` громкость · `s` стоп ·
  `q` назад — плюс `l`/`x`/`t`/`i`: лайк, дизлайк, текст, похожие (см. выше).

Архитектура описана в [README примера](samples/YandexMusicTerminal/README.md).

## Установка

```bash
# Основной клиент
dotnet add package YandexMusic

# Опционально: состояние в реальном времени и пульт
dotnet add package YandexMusic.Ynison

# Опционально: колонки в локальной сети
dotnet add package YandexMusic.Quasar

# Опционально: интеграция с DI
dotnet add package YandexMusic.DependencyInjection
```

| Пакет | Назначение |
|-------|------------|
| [`YandexMusic`](https://www.nuget.org/packages/YandexMusic) | Клиент `YandexMusicClient`, модели, авторизация и группы эндпоинтов. |
| [`YandexMusic.Ynison`](https://www.nuget.org/packages/YandexMusic.Ynison) | `CreateYnisonClient()` — живое состояние воспроизведения аккаунта и пульт. |
| [`YandexMusic.Quasar`](https://www.nuget.org/packages/YandexMusic.Quasar) | Находит колонки Яндекса в локальной сети и управляет ими напрямую. |
| [`YandexMusic.DependencyInjection`](https://www.nuget.org/packages/YandexMusic.DependencyInjection) | `AddYandexMusic()` — scoped-клиент поверх пула `IHttpClientFactory`. |

Основной пакет самодостаточен и не зависит ни от чего, кроме BCL: большинству нужен именно REST API,
поэтому пульт на websocket и поддержка колонок вынесены в отдельные пакеты и не попадают в дерево
зависимостей всем подряд.

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

### Управление воспроизведением в реальном времени (Ynison)

Ynison — это то, что синхронизирует веб-плеер, телефонные приложения и умные колонки. Клиент
подписывается на состояние воспроизведения аккаунта и может управлять любым устройством сессии.
Поставляется отдельно, в пакете `YandexMusic.Ynison`:

```csharp
using YandexMusic.Ynison;

await using var ynison = client.CreateYnisonClient();
var run = Task.Run(() => ynison.RunAsync());

var state = await ynison.WaitForStateAsync(TimeSpan.FromSeconds(10));
Console.WriteLine(state.Devices.Count + " устройств в сессии");
ynison.StateReceived += (_, s) => Console.WriteLine(s.PlayerState?.PlayerQueue?.PlayableList[
    Math.Max(0, s.PlayerState.PlayerQueue.CurrentPlayableIndex)]?.Title);

await ynison.SetPausedAsync(paused: false);   // дистанционное управление
await ynison.NextTrackAsync();
```

### Управление колонкой в локальной сети (Quasar)

Умная колонка никогда не входит в Ynison-сессию аккаунта, поэтому единственный способ до неё
добраться — говорить с ней напрямую. `YandexMusic.Quasar` находит колонки по mDNS и подключается к
каждой сам:

```csharp
using YandexMusic.Quasar;

// Одному обнаружению не нужны ни аккаунт, ни токен, ни интернет.
var scanner = new LocalDeviceScanner();
await foreach (var found in scanner.DiscoverAsync(TimeSpan.FromSeconds(3)))
{
    Console.WriteLine($"{found.Platform} на {found.Endpoint}");
}

// А управлению нужны: аккаунт даёт имя колонки, её сертификат и токен на устройство.
using var quasar = client.CreateQuasarClient();
var speaker = (await quasar.GetDevicesAsync()).First(d => d.Platform == "yandexmini");

await using var control = await quasar.ConnectAsync(speaker);
_ = control.RunAsync();
await control.WaitForStateAsync(TimeSpan.FromSeconds(10));

await control.PlayTrackAsync("38633712");   // колонка сама возьмёт трек из каталога
await control.SetVolumeAsync(0.4);
```

Соединение пиннит TLS-сертификат колонки к тому, который публикует для неё аккаунт: сертификат
самоподписанный и говорит `localhost`, так что обычная проверка не прошла бы никогда, а
единственной альтернативой было бы «доверять чему угодно».

Все методы принимают `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var track = await client.Tracks.GetAsync("4", cts.Token);
```

## Авторизация и сохранение сессии

### Получение OAuth-токена

Проще всего получить токен через OAuth implicit flow Яндекса. Откройте эту ссылку в браузере, войдите
и подтвердите доступ:

```
https://oauth.yandex.ru/authorize?response_type=token&client_id=23cabbbdc6cd418abb4b39c32c41195d
```

Вас перенаправит на адрес `music.yandex.ru`, где токен будет во фрагменте (после `#`):

```
https://music.yandex.ru/#access_token=y0__xExampleFAKEtokenDoNotUse000000000000000000&token_type=bearer&expires_in=24752795&cid=ab1cd23efghij4klmn5opqrs6
```

Скопируйте значение **`access_token`** (здесь `y0__xExampleFAKEtokenDoNotUse000000000000000000`) — это и
есть ваш токен. Храните его в секрете; передавайте через переменную окружения `YANDEX_MUSIC_TOKEN`
(или вставьте в способ входа **OAuth-токен** в примере-плеере). Части `token_type`, `expires_in` и `cid`
не нужны.

### Вход и сохранение сессии

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
│   ├── YandexMusic.Ynison/              # состояние в реальном времени и пульт (websocket)
│   ├── YandexMusic.Quasar/              # колонки в локальной сети (mDNS + websocket)
│   └── YandexMusic.DependencyInjection/ # интеграция AddYandexMusic()
├── tests/
│   └── YandexMusic.Tests/               # модульные + (по токену) интеграционные тесты (xUnit)
├── samples/
│   └── YandexMusicTerminal/              # интерактивный терминальный плеер (TUI-демо)
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
