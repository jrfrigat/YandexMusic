# Changelog

🌐 **English** · [Русский](CHANGELOG.ru.md)

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Changed
- **Breaking**: `IYandexMusicClient.CreateYnisonClient` returns the new `IYnisonClient` interface
  instead of the sealed `YnisonClient` class, so consumers can substitute and test it like every
  other endpoint group. `YnisonClient` implements it unchanged.
- **Breaking**: `TrackSupplement.Id` and `Lyrics.Id` are `string?` instead of `long`. The API sends
  the id as a string on one response and a number on another, and user-uploaded tracks carry ids that
  are not numbers at all — those threw while parsing. Nothing computes on the value.

### Fixed
- Sample player: the remote crashed on open with "Encountered malformed markup tag at position 14".
  The device hotkey badge closed its style with `[/grey]`, which Spectre's markup parser rejects (only
  `[/]` closes a tag) — the 0.3.0 fix escaped the brackets but introduced this.
- Sample player: a message a screen left behind (a screen failure, an unreachable Ynison, an empty
  library, "this track has no lyrics") stayed on screen for the rest of the session. Those were plain
  console writes, and every screen that follows is a live view rendered below them, so nothing ever
  cleared them. They now go through a notice board the main menu renders and expires after four
  seconds, and the lyrics refusal appears as a toast in the now-playing view it returns to.
- Sample player: the search tabs showed "Nothing found" for a query that had results — a tab only
  fetched its first page once `Enter` was pressed on it. Each tab now loads as soon as it is shown,
  beside the render loop, so the "loading" row is actually visible and the list stays live; a failed
  fetch shows the reason and retries on `Enter` instead of hammering the API.
- Sample player: drilling into an album or a playlist from search threw — the detail screens open
  their own interactive display, and Spectre permits only one at a time. The tab view now closes
  first and reopens afterwards with every tab's state intact.
- Sample player: a failed automatic advance (a radio batch that could not be fetched, a stream that
  would not resolve) stopped playback silently, as an unobserved task exception. It is reported
  through the new `PlaybackController.Failed` event and shown by the player and the menu.
- Sample player: the playback queue was a `List<T>` mutated both by the UI thread and by the audio
  backend's "track ended" callback. Queue changes are serialized now, and the queue is an immutable
  snapshot swapped by reference, so a skip racing with a track end can no longer corrupt it.
- Sample player: a queue that ran out and was then stopped by hand reported the same track as "left"
  twice, sending a duplicate skip to the recommendation feedback.
- Sample player: play reporting counted paused time as listening, and a fast skip could stamp a
  track's play-audio event with the next track's play id.
- Sample player: the goodbye line was hardcoded in English instead of using its localized resource,
  and the lyrics view never rendered the hotkey bar it had a resource for.
- Ynison: a fatal handshake failure (a rejected token, a malformed redirect answer) left
  `WaitForStateAsync` sitting out its whole timeout and then reporting a timeout, hiding the real
  reason. The first-state wait now fails with the failure that actually happened.
- Ynison: a command sent as the socket dropped escaped `SendAsync` as a raw `WebSocketException` or
  `ObjectDisposedException`; both are wrapped in `YandexMusicYnisonException` now, which is what
  callers already handle.
- Ynison: connections were always aborted, never closed — `IYnisonSocket.CloseAsync` existed but was
  never called. Shutdown now runs a bounded close handshake before aborting.
- Ynison: the reconnect backoff never reset after a healthy connection, so a drop hours into a
  session waited out the 64-second ceiling instead of retrying promptly.

### Removed
- Sample player: eleven resource strings left over from the pre-live-view screens, which no code read.

## [0.3.0] - 2026-08-19

### Added
- Ynison real-time client (`YandexMusic.Ynison`): subscribe to the account's playback state across
  all devices and remote-control it (pause/resume, next/previous track, per-device volume, "play on
  device") over the same websocket protocol the official clients use. Full protobuf-JSON state
  models (the protocol's original snake_case wire names), reconnect with capped exponential
  backoff, ready-made request builders, and `IYandexMusicClient.CreateYnisonClient()` wiring the
  session token automatically. Verified against the live service.
- Sample player: the Ynison remote control screen — live playback state of every device of the
  account with pause/tracks/volume commands and "play on this device" via keys `1-9`.
- Sample player: per-track actions on the now-playing screen — like (`l`), dislike with an auto-skip
  (`x`), lyrics (`t`), and an endless "similar tracks" radio seeded from the current track (`i`).
- Sample player: play reporting — every playback start sends a play-audio event, and radio queues
  (My Wave, similar tracks) report radio-started/track-started/finished/skipped feedback, so
  recommendations react to what is actually played.
- Sample player: search as a tabbed screen — a horizontal tab bar (tracks, albums, playlists) over
  a result list in one live view, per-tab paging via a "load more" row, and drill-in to a picked
  album's or playlist's tracklist; radio queues keep fetching batches instead of stopping at the
  end of the first one.
- Sample player: status toasts (liked, errors) are grey and fade out after four seconds.
- The sample player now ships as self-contained single-file builds for **Linux** alongside Windows
  (a zip for win-x64, a tar.gz for linux-x64), attached to releases and CI artifacts.

### Fixed
- Sample player: the remote no longer crashes on Spectre markup — the device hotkey badges
  ("[1]"-style) are rendered as literal brackets now.
- Sample player: lyrics load again — the supplement endpoint returns its id as a string, which the
  model now accepts (number or string).
- Ynison: server frames use the protocol's original snake_case field names (`player_state`,
  `redirect_ticket`); the models now deserialize them correctly (previously only camelCase was
  accepted, so every live frame parsed empty and the redirector answer was rejected).
- `Authentication.PollDeviceTokenAsync` now signs the client in when the user confirms the device
  code, as its documentation always promised. Previously it returned the token without applying it,
  leaving `IsAuthenticated` false and every subsequent call silently unauthenticated.
- Sample player: the lyrics view now reads the text from the supplement endpoint, which carries it
  directly. The signed `/lyrics` endpoint currently answers 403 "Invalid Sign" even with the exact
  reference signature, so `Tracks.GetLyricsAsync` is documented accordingly.
- Sample player: a failing screen (network drop, API error) shows an error line and returns to the
  menu instead of exiting the whole app; the same applies to the Ynison remote teardown.
- Sample player: launching offline (or pressing Ctrl+C during the startup session check) no longer
  deletes the saved session. The stored session is now dropped only when the API itself refuses it;
  network errors keep it.
- Sample player: the stored session file (`session.json`, token and cookies) is now written
  atomically and restricted to the owner (`0600`) on Linux/macOS instead of the default
  world-readable mode.
- Release workflow: requests the `nuget` environment as the Trusted Publishing policy expects, and
  attaches `.snupkg` symbol packages to the GitHub Release (they already reached NuGet.org, but
  were missing from the release assets).

### Changed
- `RELEASING.md` now exists per language (`RELEASING.md` + `RELEASING.ru.md`), like the READMEs; the
  changelog follows the same split (`CHANGELOG.md` + `CHANGELOG.ru.md`). The versioning description
  now matches reality (MinVer tag-driven; push a tag — the workflow creates the release).

## [0.2.0] - 2026-07-03

### Added
- `YandexMusicClientOptions.ApiBaseUri` — override the API base address for a reverse proxy, a regional
  mirror, or a local stub server in tests; defaults to the official host.
- Opportunistic HTTP/2 (falls back to HTTP/1.1 transparently) on every `HttpClient` the library creates,
  enabling connection multiplexing.
- `AddYandexMusic(..., configureHttpClient)` overload and a public `HttpClientName` constant, so consumers
  can attach a resilience handler, logging, or a custom primary handler to the pooled `IHttpClientFactory`
  client without depending on an internal name.

### Fixed
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

### Changed
- `YandexMusic.DependencyInjection`: HTTP client options (timeout, headers, proxy, base address) now
  resolve from the DI container consistently everywhere, instead of partly from a locally-captured copy —
  fixes a split-brain where a pre-registered `YandexMusicClientOptions` silently lost its
  proxy/timeout/headers configuration.
- CI now builds and tests on both `ubuntu-latest` and `windows-latest` (previously Linux-only), so
  OS-dependent regressions like the two fixes above are caught automatically.
- The zero-warning build bar (`TreatWarningsAsErrors`) now also covers the test project; NU1900 (audit
  feed unreachable) is demoted so an offline build cannot fail CI.

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
  - **Queue**, **Pins**, **Presaves**, `MusicHistory` — personal cross-device state.
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
