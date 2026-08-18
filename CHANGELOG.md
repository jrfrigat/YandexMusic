# Changelog

🌐 **English** below · [Русская версия ниже](#история-изменений)

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Fixed
- `Authentication.PollDeviceTokenAsync` now signs the client in when the user confirms the device
  code, as its documentation always promised. Previously it returned the token without applying it,
  leaving `IsAuthenticated` false and every subsequent call silently unauthenticated.
- Sample player: launching offline (or pressing Ctrl+C during the startup session check) no longer
  deletes the saved session. The stored session is now dropped only when the API itself refuses it;
  network errors keep it.
- Sample player: the stored session file (`session.json`, token and cookies) is now written
  atomically and restricted to the owner (`0600`, and a `0700` directory) on Linux/macOS instead of
  the default world-readable mode.
- **Critical**: every `Pins` call (`PinAlbumAsync`, `UnpinAlbumAsync`, `PinArtistAsync`, `UnpinArtistAsync`,
  `PinPlaylistAsync`, `UnpinPlaylistAsync`, `PinWaveAsync`, `UnpinWaveAsync`) threw at runtime on both
  Windows and Linux. They built a relative-path request and sent it through the raw transport method,
  which never resolves relative paths against the API host (no `HttpClient.BaseAddress` was ever set).
  Routed through the same request pipeline as every other endpoint instead.
- Absolute-URI detection in the request pipeline now requires an `http`/`https` scheme. On Linux, `Uri`
  parsed a leading-slash relative path like `/users/1/playlists/2` as an absolute `file://` URI (Windows
  parsed the same string as relative), so requests built that way failed with "The 'file' scheme is not
  supported" outside Windows.
- `Account.SetSettingsAsync(IReadOnlyDictionary<string, string>, ...)` disposed its request body before
  the request finished sending, which could intermittently throw or send a truncated body under real
  network latency.

### Added
- `YandexMusicClientOptions.ApiBaseUri` — override the API base address for a reverse proxy, a regional
  mirror, or a local stub server in tests; defaults to the official host.
- Opportunistic HTTP/2 (falls back to HTTP/1.1 transparently) on every `HttpClient` the library creates,
  enabling connection multiplexing.
- `AddYandexMusic(..., configureHttpClient)` overload and a public `HttpClientName` constant, so consumers
  can attach a resilience handler, logging, or a custom primary handler to the pooled `IHttpClientFactory`
  client without depending on an internal name.

### Changed
- `YandexMusic.DependencyInjection`: HTTP client options (timeout, headers, proxy, base address) now
  resolve from the DI container consistently everywhere, instead of partly from a locally-captured copy —
  fixes a split-brain where a pre-registered `YandexMusicClientOptions` silently lost its
  proxy/timeout/headers configuration.
- CI now builds and tests on both `ubuntu-latest` and `windows-latest` (previously Linux-only), so
  OS-dependent regressions like the two fixes above are caught automatically.
- The zero-warning build bar (`TreatWarningsAsErrors`) now also covers the test project.

## [0.1.0] - 2026-06-30

First public release of the original, clean-room implementation.

### Added
- Support for `net8.0`, `net9.0` and `net10.0` (multi-targeting).
- High-level `YandexMusicClient` (`IYandexMusicClient`) with typed endpoint groups:
  - **Tracks** — metadata (single & batch), direct download/stream link, lyrics, supplement, full-info, trailer, similar, play-audio, after-track.
  - **Search** — full search with a polymorphic best match and per-category sections, plus autocomplete (`suggest`).
  - **Albums** — album, album-with-tracks, batch, similar-entities, trailer.
  - **Artists** — brief info, paged tracks/albums, similar, links, about, info, clips, donations, skeleton, trailer, discography.
  - **Playlists** — read, create, delete, rename, change visibility/description, edit tracks, recommendations, similar-entities, trailer, and more.
  - **Account** — status, settings (get/set), permission alerts, A/B experiments, promo codes.
  - **Library** — liked & disliked tracks/albums/artists/playlists/clips, including add/remove.
  - **Genres**, **Labels**, **Clips**, **Credits**, **Disclaimers** — catalogue metadata.
  - **Landing** — feed, landing blocks, charts, new releases/playlists, podcasts, tags.
  - **Radio** (rotor) — station dashboard, list, info, tracks, settings, feedback.
  - **Concerts**, **Metatags** — events and curated collection pages.
  - **Queue**, **Pins**, **Presaves**, **MusicHistory** — personal cross-device state.
- Multiple sign-in flows: OAuth token, the official OAuth **device-code** flow, and best-effort cookie, QR or login + password — all over a serializable `AuthSnapshot` (export/import to persist and resume a session).
- `YandexMusic.DependencyInjection` package: `AddYandexMusic()` registers a scoped client over a handler pooled by `IHttpClientFactory`.
- Typed exception hierarchy (`YandexMusicException` and descendants).
- Full XML documentation on the public API; unit tests plus token-gated integration tests (xUnit).
- Source Link, symbol packages (`snupkg`), README in packages, DocFX documentation, GitHub Actions CI/CD.

### Performance
- `System.Text.Json` source generation throughout (single shared, frozen `JsonSerializerOptions`); responses
  deserialized straight from the UTF-8 stream via `JsonTypeInfo<T>` — allocation-conscious and trim/AOT-clean
  (`IsAotCompatible`).
- Tolerant enum handling (kebab/UPPER_SNAKE, default on unknown) so new server-side values never break a response.

### Note
- This is an original, clean-room implementation written from the public HTTP API. It does not derive from
  any third-party Yandex Music client.

---

# История изменений

🌐 [English version above](#changelog) · **Русская версия**

Все значимые изменения в этом проекте документируются в этом файле.
Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/),
проект следует [семантическому версионированию](https://semver.org/lang/ru/).

## [Не выпущено]

### Исправлено
- **Критично**: любой вызов `Pins` (`PinAlbumAsync`, `UnpinAlbumAsync`, `PinArtistAsync`, `UnpinArtistAsync`,
  `PinPlaylistAsync`, `UnpinPlaylistAsync`, `PinWaveAsync`, `UnpinWaveAsync`) падал в рантайме и на Windows,
  и на Linux. Эти методы строили запрос с относительным путём и отправляли его через низкоуровневый метод
  транспорта, который не резолвит относительные пути против хоста API (у `HttpClient` никогда не был
  выставлен `BaseAddress`). Теперь они идут через тот же конвейер запросов, что и все остальные эндпоинты.
- Определение абсолютного URI в конвейере запросов теперь требует схему `http`/`https`. На Linux `Uri`
  разбирал путь вида `/users/1/playlists/2` как абсолютный `file://`-URI (на Windows та же строка
  считалась относительной), из-за чего такие запросы падали с ошибкой «схема 'file' не поддерживается»
  вне Windows.
- `Account.SetSettingsAsync(IReadOnlyDictionary<string, string>, ...)` освобождал тело запроса до
  завершения его отправки, что могло изредка приводить к ошибке или обрезанному телу при реальной
  сетевой задержке.

### Добавлено
- `YandexMusicClientOptions.ApiBaseUri` — переопределение базового адреса API для обратного прокси,
  регионального зеркала или локального стаб-сервера в тестах; по умолчанию — официальный хост.
- Оппортунистический HTTP/2 (с прозрачным откатом на HTTP/1.1) на каждом `HttpClient`, который создаёт
  библиотека — включает мультиплексирование соединений.
- Перегрузка `AddYandexMusic(..., configureHttpClient)` и публичная константа `HttpClientName` — теперь
  можно подключить resilience-handler, логирование или свой primary handler к клиенту в пуле
  `IHttpClientFactory`, не полагаясь на внутреннее имя.

### Изменено
- `YandexMusic.DependencyInjection`: настройки HTTP-клиента (таймаут, заголовки, прокси, базовый адрес)
  теперь везде последовательно резолвятся из контейнера DI, а не частично из локально захваченной копии —
  исправлен «раздвоенный» баг, при котором заранее зарегистрированный `YandexMusicClientOptions` незаметно
  терял свои настройки прокси/таймаута/заголовков.
- CI теперь собирает и тестирует и на `ubuntu-latest`, и на `windows-latest` (раньше — только на Linux),
  так что подобные ОС-специфичные регрессии ловятся автоматически.
- Планка нулевых предупреждений (`TreatWarningsAsErrors`) теперь распространяется и на тестовый проект.

## [0.1.0] - 2026-06-30

Первый публичный релиз оригинальной clean-room реализации.

### Добавлено
- Поддержка `net8.0`, `net9.0` и `net10.0` (мультитаргетинг).
- Высокоуровневый `YandexMusicClient` (`IYandexMusicClient`) с типизированными группами эндпоинтов:
  - **Tracks** — метаданные (одиночно и пачкой), прямая ссылка, тексты, supplement, full-info, трейлер, похожие, play-audio, after-track.
  - **Search** — полный поиск с полиморфным best и секциями по категориям, плюс подсказки (`suggest`).
  - **Albums** — альбом, альбом-с-треками, пачкой, similar-entities, трейлер.
  - **Artists** — brief-info, пагинируемые треки/альбомы, похожие, ссылки, about, info, клипы, донаты, skeleton, трейлер, дискография.
  - **Playlists** — чтение, создание, удаление, переименование, видимость/описание, правка треков, рекомендации, similar-entities, трейлер и др.
  - **Account** — статус, настройки (get/set), permission-alerts, A/B-эксперименты, промокоды.
  - **Library** — лайкнутые и дизлайкнутые треки/альбомы/исполнители/плейлисты/клипы, включая add/remove.
  - **Genres**, **Labels**, **Clips**, **Credits**, **Disclaimers** — метаданные каталога.
  - **Landing** — фид, блоки лендинга, чарты, новые релизы/плейлисты, подкасты, теги.
  - **Radio** (rotor) — dashboard станций, список, info, треки, настройки, фидбэк.
  - **Concerts**, **Metatags** — события и подборки (мета-теги).
  - **Queue**, **Pins**, **Presaves**, **MusicHistory** — личное состояние между устройствами.
- Несколько способов входа: OAuth-токен, официальный OAuth **device-code** flow и best-effort cookie, QR или логин/пароль — всё поверх сериализуемого `AuthSnapshot` (export/import для сохранения и восстановления сессии).
- Пакет `YandexMusic.DependencyInjection`: `AddYandexMusic()` регистрирует scoped-клиент поверх handler'а в пуле `IHttpClientFactory`.
- Типизированная иерархия исключений (`YandexMusicException` и наследники).
- Полная XML-документация публичного API; модульные и интеграционные (по токену) тесты (xUnit).
- Source Link, символьные пакеты (`snupkg`), README в пакетах, документация DocFX, CI/CD на GitHub Actions.

### Производительность
- Source-generation `System.Text.Json` целиком (единый общий «замороженный» `JsonSerializerOptions`); ответы
  десериализуются прямо из UTF-8 потока через `JsonTypeInfo<T>` — экономно к аллокациям, чисто к trim/AOT (`IsAotCompatible`).
- Терпимая обработка перечислений (kebab/UPPER_SNAKE, значение по умолчанию на неизвестном) — новые значения на стороне сервера не ломают ответ.

### Примечание
- Это оригинальная clean-room реализация, написанная по публичному HTTP-API. Она не является производной от
  какого-либо стороннего клиента Яндекс Музыки.
