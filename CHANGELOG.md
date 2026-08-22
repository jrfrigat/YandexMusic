# Changelog

🌐 **English** · [Русский](CHANGELOG.ru.md)

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added
- **`YandexMusic.LocalDevices`** — a new package that finds Yandex speakers on the current network
  over mDNS/DNS-SD (`_yandexio._tcp`). `ILocalDeviceScanner.DiscoverAsync` streams each device as it
  answers, with its identifier, hardware platform and the endpoint to reach it. Discovery needs no
  account, no token and no internet connection, and it takes no dependency: the slice of the DNS wire
  format it needs is about 200 lines. Controlling a discovered device is **not** implemented — see
  [the proposal](docs/proposals/local-devices.md) for what is still unknown.
- Sample player: an **"About"** screen (`i`) — the running version, the data directory, the state of
  the request journal, and an update check on demand (`r`). The automatic check can only ever report
  bad news; this is the place that answers "am I up to date?" either way.
- Sample player: an **"Update"** entry on the main menu (`u`), shown while an update is available. It
  runs the same installer the README documents, so acting on the notice no longer means finding and
  retyping the install command. On Windows the installer is started detached and waits for the player
  to exit, because a running `ymt.exe` cannot be overwritten; on Linux it runs inline in the terminal
  and the player exits afterwards. Either way the player ends, since the process in memory is still
  the old build.
- Sample player: the version is shown next to the subtitle in the startup banner.

### Changed
- Sample player: the update check now runs at startup and every 30 minutes, instead of at most once
  a day. Two requests an hour are nowhere near GitHub's unauthenticated rate limit, and a release
  published while the player is open is noticed without restarting it. The daily stamp file is gone;
  a leftover one is deleted on first run.
- Sample player: `YM_PLAYER_NO_UPDATE_CHECK` now only disables the automatic schedule. An explicit
  check from the "About" screen still runs — opting out of being told is not the same as asking for
  the button to do nothing.
- Sample player: the main menu tracks the selected entry by action rather than by index, so the
  arrival of the "Update" entry no longer moves the cursor under the user's hands.

## [0.5.0] - 2026-08-22

### Added
- **`YandexMusic.Ynison`** — a new package holding the real-time playback state and the remote. The
  core `YandexMusic` package no longer contains a websocket client of any kind: it references nothing
  but the BCL, which is what the overwhelming majority of consumers actually want. Both packages ship
  from this repository on one version line.

### Changed
- **Breaking**: `CreateYnisonClient` is no longer a member of `IYandexMusicClient`. It is an
  extension method in `YandexMusic.Ynison`, so `client.CreateYnisonClient()` still compiles — add the
  package and a `using YandexMusic.Ynison;`. This is the coupling that kept the core from being
  standalone: an interface in the core named a Ynison type, so every REST consumer carried one.
- **Breaking**: `YandexMusicYnisonException` ships from the `YandexMusic.Ynison` assembly. Its
  namespace (`YandexMusic.Exceptions`) and shape are unchanged, so `catch` blocks recompile as they
  are; only a binary reference has to be updated.

### Removed
- `RELEASING.md` / `RELEASING.ru.md`. They were a maintainer runbook — repository settings, nuget.org
  account setup, tagging — with nothing in them for anyone using or contributing to the library.

## [0.4.0] - 2026-08-22

### Added
- The sample terminal installs with one command and runs as `ymt`. `scripts/install.ps1`
  (`irm … | iex`) and `scripts/install.sh` (`curl … | sh`) fetch the self-contained release build,
  unpack it into the user profile and put it on PATH — no administrator rights and no .NET install.
  Re-running either command updates in place.
- The terminal checks GitHub for a newer release, at most once a day and detached from startup, and
  shows a line on the main menu when one exists. `YM_PLAYER_NO_UPDATE_CHECK=1` turns it off.
- `YandexMusicClientOptions.HandlerFactory` — an optional `DelegatingHandler` placed in front of the
  client's own handler, so a directly-constructed client can be logged or traced. Consumers on
  `AddYandexMusic` already had `configureHttpClient` for this.
- `IYnisonClient.FrameReceived` / `FrameSent` — the raw text of every Ynison frame, before parsing.
  Diagnostics: when the parsed state looks wrong, this is the only way to see what actually arrived.
- Sample player: search covers artists — a fourth tab next to tracks, albums and playlists, drilling
  into the artist's popular tracks to play from.
- Sample player: a request journal, toggled from the main menu with `g` and off by default. It
  records the HTTP traffic (request line, headers, bodies, status and timing) and the raw Ynison
  frames to `%APPDATA%\yandexmusic-player\requests.log`. Every line is scrubbed of OAuth tokens,
  bearer tokens, cookies and passwords before it is written, because a journal is written to be
  handed to somebody. The interactive sign-in flows use their own HTTP clients inside the library and
  are deliberately not recorded.

### Changed
- The sample project is now **`YandexMusicTerminal`** (was `YandexMusic.Player`), its binary and
  release archives are named `ymt`, and its data directory moved from `yandexmusic-player` to `ymt`.
  An existing directory is carried over on first run, so saved sessions survive the rename.
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

- Release workflow: the GitHub Release is created as a draft and made visible only after both player
  archives are attached, so "latest release" is never a state the install scripts cannot download
  from. A failed run can be re-run: existing packages are skipped and an existing draft is topped up
  instead of failing on re-creation.
- CI: the install scripts are linted (ShellCheck, PSScriptAnalyzer) and guarded against CRLF line
  endings, which would make the shell installer unrunnable on Linux.

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
