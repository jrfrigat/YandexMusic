# Changelog

🌐 **English** below · [Русская версия ниже](#история-изменений)

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

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
- Three sign-in flows: OAuth token, the official OAuth **device-code** flow, and best-effort cookie/QR — all over a serializable `AuthSnapshot` (export/import to persist and resume a session).
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
- Три способа входа: OAuth-токен, официальный OAuth **device-code** flow и best-effort cookie/QR — всё поверх сериализуемого `AuthSnapshot` (export/import для сохранения и восстановления сессии).
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
