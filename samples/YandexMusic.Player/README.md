# YandexMusic.Player

An interactive terminal music player built on the **YandexMusic** library — a sample that doubles as a
reference for structuring a real TUI app around the client.

```bash
dotnet run --project samples/YandexMusic.Player
```

## Features

- **Sign in** four ways — OAuth token, device-code flow, QR code, or login + password — with the
  session cached to disk so subsequent runs start already authenticated.
- **Search tracks** and start playback from any result (the whole result set becomes the queue).
- **Browse your albums** and drill into an album's tracklist.
- **Now playing** — a live view with an animated equalizer, a real-time progress bar, a volume meter,
  and auto-advance through the queue.

### Keyboard controls (now-playing view)

| Key | Action |
| --- | --- |
| `space` / `p` | play / pause |
| `→` / `n` | next track |
| `←` / `b` | previous track |
| `↑` / `↓` | volume up / down |
| `s` | stop |
| `q` / `Esc` | back to menu |

## Playback & platforms

Audio goes through a single seam, [`IAudioPlayer`](Playback/IAudioPlayer.cs):

- **`NAudioPlayer`** — real playback on **Windows** via NAudio (Media Foundation + WaveOut), with true
  volume control. Compiled only under the `WINDOWS` target.
- **`SimulatedAudioPlayer`** — a silent player that advances a position clock in real time, so the
  whole UI works without a subscription, a token, or any native dependency. This is the default on
  Linux/macOS and the universal fallback.
- **`ResilientAudioPlayer`** — composes the two: it prefers the real backend and transparently falls
  back to the simulation per track if a stream can't be opened.

The project targets a Windows TFM on Windows (so NAudio's Windows assemblies resolve) and plain
`net10.0` elsewhere, so it builds and runs everywhere. Swapping in a cross-platform backend (e.g.
LibVLC) means adding one `IAudioPlayer` and changing one line in [`Program.cs`](Program.cs).

## Architecture

Composition happens in [`Program.cs`](Program.cs); everything else is resolved through DI and depends
only on abstractions.

```
Program (composition root)
  └─ PlayerApp ............ banner, auth gate, main menu, screen dispatch
       ├─ AuthService ..... restore session ▸ pick an IAuthFlow ▸ persist
       │    └─ IAuthFlow .. Token · DeviceCode · Qr · Password
       ├─ Screens ......... Search · Albums · Album · NowPlaying
       ├─ IMusicCatalog ... anti-corruption layer over IYandexMusicClient
       │                    (returns the UI's own view-models, not library models)
       └─ PlaybackController . queue + transport brain over IAudioPlayer
            └─ IAudioPlayer .. Resilient ▸ { NAudio | Simulated }
```

| Layer | Responsibility | Key seam |
| --- | --- | --- |
| `Auth` | Sign-in methods + session persistence | `IAuthFlow`, `ISessionStore` |
| `Catalog` | Talks to the library, maps to view-models | `IMusicCatalog` |
| `Playback` | Queue, transport, audio output | `IAudioPlayer`, `PlaybackController` |
| `Screens` | Spectre.Console UI | (one class per screen) |

### Extending it

- **New audio backend** — implement `IAudioPlayer`, register it in `Program.cs`.
- **New sign-in method** — implement `IAuthFlow`, add one `AddSingleton<IAuthFlow, …>()`.
- **New screen** — add a screen class and a menu entry in `PlayerApp`.
- **Playback features** (shuffle, repeat, gapless) live in `PlaybackController`; the UI is unaffected.

## Dependencies

[Spectre.Console](https://spectreconsole.net/) (TUI) · [QRCoder](https://github.com/codebude/QRCoder)
(QR rendering) · [NAudio](https://github.com/naudio/NAudio) (Windows audio) ·
`Microsoft.Extensions.DependencyInjection` (composition).
