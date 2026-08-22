# Local device discovery, and splitting the repository into three libraries

**Status:** step 1 done — `YandexMusic.Ynison` shipped as its own package in 0.5.0 and the core is
standalone. Discovery and transport are measured on real hardware (see "What the wire says"), and
`YandexMusic.Quasar` now exists on the `local-devices` branch with discovery implemented and
verified. What remains unknown is authentication and the command schema, so nothing can be
**controlled** yet.

## The problem

The Ynison remote lists only the devices that joined the account's Ynison session. A capture from a
live session (see the request journal, menu key `g`) contains exactly four:

| device_id | title | type | offline | can_be_player |
|---|---|---|---|---|
| `71627e95…` | OnePlus CPH2747 | ANDROID | no | yes |
| `<this app>` | YandexMusic .NET | WEB | no | no |
| `5315b78…` | YandexMusic .NET | WEB | yes | no |
| `dbf5653…` | YandexMusic .NET | WEB | yes | no |

No smart speakers, ever — not while idle, and the frame carries no field that would hide them. The
server simply does not put them in the session.

The official Yandex Music app, meanwhile, offers speakers on the same Wi-Fi network **even when the
speaker is signed in to a different account**. An account-scoped protocol cannot do that by
construction. So the official client's device picker is the union of two sources:

1. **Ynison** — devices of *this account* currently in session. Already implemented.
2. **Local discovery** — whatever answers on *this network*, regardless of account. Not implemented.

This issue covers the second source, and the repository restructuring it forces.

## Why this forces a restructuring

Today `YandexMusic` is one package holding both the REST API and Ynison, wired together by
`IYandexMusicClient.CreateYnisonClient()`. Adding a third subsystem to the same package would make
every consumer of the REST API carry a websocket client, an mDNS responder and a local TLS stack
they will never call.

The overwhelming majority of consumers want the REST API and nothing else. That has to stay true.

### Target layout

```
YandexMusic                 REST Music API. Depends on nothing but the BCL.
YandexMusic.Ynison          Account-level real-time state and remote control. Depends on YandexMusic.
YandexMusic.Quasar          Same-network discovery and control. Depends on YandexMusic.
YandexMusic.DependencyInjection   Unchanged: AddYandexMusic() for the core.
```

Rules:

- `YandexMusic` never references the other two. It is complete and useful on its own — this is the
  constraint everything else bends around.
- `YandexMusic.Ynison` and `YandexMusic.Quasar` do not reference each other. A consumer wanting
  the merged device list of the official app takes both and merges them itself; a helper that does
  the merging can come later, in its own package, once both halves exist.
- All three ship from this repository, on one version line, so "the YandexMusic repo covers
  everything you need for this service" holds without splitting the work across repos.

### The one breaking change this requires — done in 0.5.0

`IYandexMusicClient.CreateYnisonClient()` was the coupling that blocked the split: the core interface
named a Ynison type. It moved out of the core and became an extension method supplied by the Ynison
package:

```csharp
// before 0.5.0, on the core interface
IYnisonClient CreateYnisonClient(string? deviceId = null, YnisonClientOptions? options = null);

// since 0.5.0, in YandexMusic.Ynison
public static class YandexMusicClientYnisonExtensions
{
    public static IYnisonClient CreateYnisonClient(
        this IYandexMusicClient client,
        string? deviceId = null,
        YnisonClientOptions? options = null);
}
```

Call sites did not change — `client.CreateYnisonClient()` still compiles — but a `using
YandexMusic.Ynison;` is now necessary, and the method is gone from the core interface. The token
comes from `client.Authentication.Session.AccessToken`, which was already public.

Two smaller moves came with it, both invisible to callers: `YandexMusicYnisonException` now ships
from the Ynison assembly (its namespace is unchanged, so `catch` blocks still compile), and the
protobuf-JSON converters moved along with it — they had no other user. The core no longer references
`System.Net.WebSockets` at all.

`YandexMusic.Quasar` gets the same treatment, so the three entry points read alike.

## Scope of YandexMusic.Quasar

Three separable pieces, in dependency order:

1. **Discovery.** Find the speakers on the current network and report id, name, model, address and
   whether they are reachable. Purely local; no account involved.
2. **Authorization.** Obtain whatever credential the local control channel requires. This is the
   part that touches a Yandex backend, and the part that is least understood.
3. **Control.** Connect to a device and drive it: play/pause, next/previous, volume, status.

A useful first milestone is **discovery alone** — a `LocalDeviceScanner` that returns what is on the
network. It is independently valuable (the remote can list the speakers greyed out, and say why they
cannot be controlled yet), it is verifiable without any credentials, and it de-risks the rest.

### The surface, as built

```csharp
public interface ILocalDeviceScanner
{
    IAsyncEnumerable<LocalDevice> DiscoverAsync(TimeSpan window, CancellationToken cancellationToken = default);
}
```

`IAsyncEnumerable` rather than a list: discovery is a stream of answers arriving over a window, and
a UI wants to show each device the moment it replies.

`LocalDevice` came out of the sketch changed in one way worth noting: the sketched `Name` is gone.
Devices do not advertise the name their owner gave them, so a field for it could only ever have held
something invented. What is there instead is `DeviceId`, `Platform`, `Endpoint`, `Host` and the raw
`Attributes` — all of it measured rather than assumed.

## What the wire says

Measured against four speakers (Mini, Station 2, Station 3, Station Midi) on one home Wi-Fi network,
2026-08-22. Everything in this section was observed directly, not inferred: discovery answers, a TLS
handshake and a websocket upgrade. No command was sent to any device.

### Discovery — mDNS/DNS-SD, `_yandexio._tcp`

The hypothesis was right. `_services._dns-sd._udp.local` lists `_yandexio._tcp.local`, and a PTR
query for it returns one instance per speaker. Each instance resolves to:

| Record | Content |
|---|---|
| PTR | `YandexIOReceiver-<deviceId>._yandexio._tcp.local` |
| SRV | `<model>-<deviceId>.local` : **1961**, priority 0, weight 0 |
| TXT | `deviceId=<id>`, `platform=<model>`, `cluster=yes` |
| A / AAAA | the LAN address, plus a link-local IPv6 |

`platform` is the model key — `yandexmini`, `yandexstation_2`, `orion` (Station 3), `cucumber`
(Station Midi). It is what a UI should map to a display name; the mDNS instance name carries no
user-visible label, so the friendly name a user recognizes ("Speaker in the kitchen") is **not**
available locally and has to come from an account-level source.

Two practical notes for the implementation. The port is in the SRV record and must be read from
there, not hard-coded — and 1961 is what these four report. And one device advertised a generic
hostname (`Android.local`) rather than a model-derived one, so the hostname is not a reliable
identity: `deviceId` from TXT is.

### Transport — TLS 1.2 with a self-signed certificate, then WebSocket

Port 1961 answers with a TLS 1.2 handshake, `TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256`, presenting:

```
subject: O=Yandex, CN=localhost, C=RU
issuer:  O=Yandex, CN=localhost, C=RU     (self-signed)
```

Validation fails on two counts, by construction and permanently: `RemoteCertificateChainErrors`
(self-signed, no path to a public root) and `RemoteCertificateNameMismatch` (`CN=localhost` can never
match the address being dialled). **And expiry is real, not hypothetical**: of the four speakers,
one presented a certificate that expired in 2024 and is still serving traffic today. A validation
policy that rejects expired certificates makes that speaker permanently uncontrollable.

So certificate validation has to be a deliberate, documented decision. It cannot be the default, and
"accept everything silently" is not acceptable either. This is the one design question in the package
that deserves the most care.

On top of TLS the device speaks **WebSocket** — `Server: WebSocket++/0.8.2`. The upgrade is accepted
**with no credentials of any kind**: no token, no header, no prior pairing. Immediately after the
101, the device starts sending websocket Ping frames with the payload `server_ping`.

That places authentication at the message level, not at the connection level, and it means discovery
and connection can be implemented and tested with no account involved at all.

### The credential — a per-device token from the quasar backend

Two undocumented endpoints, both answering `200` with `status: ok` when called with the account's
OAuth token in an `Authorization: OAuth <token>` header:

```
GET https://quasar.yandex.net/glagol/device_list
GET https://quasar.yandex.net/glagol/token?device_id=<id>&platform=<platform>
```

`device_list` returns every Quasar device on the account — **not just speakers**: a run on a
four-speaker household also returned a camera (`platform: mike`) and seven phone-app registrations
(`alice_app_android`, `iot_app_android`). Only some carry a `networkInfo`, and those are the ones
that were recently reachable on a LAN. Anything driving playback has to filter.

Each entry carries, among a large `config` block:

| Field | Content |
|---|---|
| `id` | the same identifier mDNS advertises as `deviceId` — the two match exactly |
| `name` | **the name the owner gave the speaker**, which mDNS does not carry |
| `platform` | the same model key mDNS advertises |
| `glagol.security.server_certificate` | the device's TLS certificate, ~1050 characters |
| `glagol.security.server_private_key` | the device's TLS private key, ~1675 characters |
| `networkInfo` | `external_port` (1961), `ip_addresses`, `mac_addresses`, `wifi_ssid`, `ts` |

`glagol/token` then returns a per-device token of roughly 200-230 characters. It differs per device.

Two notes for whoever implements the models. The JSON mixes conventions — `networkInfo` is camelCase
while `external_port` and `ip_addresses` inside it are snake_case — so a single naming policy will
not do; the snake_case members need explicit property names. And `device_list` carries a great deal
that has nothing to do with music: the household's GPS coordinates, Wi-Fi SSID, MAC addresses and
activation codes. The models should bind what the feature needs and leave the rest unread.

### This settles the TLS question: pin, do not trust

`server_certificate` is the device's own certificate, handed over by the backend. So a client does
not have to choose between "accept everything" and "reject everything": it can compare the
certificate the speaker presents against the one the backend says that speaker has, and refuse
anything else. That is real pinning, and it is the design this package will use.

It also disposes of the expired-certificate problem. Pinning compares the certificate itself, so a
speaker still serving a certificate that expired in 2024 stays perfectly usable, while a substituted
certificate is still rejected. Validating dates would have achieved the opposite of security here:
locking out the real device while proving nothing about any other.

`server_private_key` is a different matter. It is the device's private key, and why the backend
hands it to clients at all is unclear. Pinning does not need it, so **this package will neither
request it nor store it**. That is a deliberate decision, recorded here so it is not quietly undone
later by someone who assumes an unused field is safe to keep.

Pinning is not just plausible, it is measured: the certificate `device_list` reports for a given
speaker and the certificate that speaker presents on port 1961 have the same thumbprint, checked on
two devices across separate runs. The comparison is `X509Certificate2.GetCertHashString()` on the
PEM from `device_list` against the certificate handed to the validation callback.

### The system proxy must be turned off explicitly

`ClientWebSocket` honours the machine's configured proxy by default, and it does so for LAN
addresses too. On a machine with a system proxy this turns a connection to `wss://192.168.x.x:1961`
into a `CONNECT` tunnel request that the proxy answers with `403`, and the failure surfaces as the
thoroughly unhelpful "Unable to connect to the remote server" — with the certificate callback never
firing, because the handshake never begins.

So the control client must set `Options.Proxy = null`. Nothing on the local network should ever be
proxied, and leaving this to the machine's configuration means the feature works on the developer's
laptop and fails on every corporate one.

### Speakers say nothing until spoken to

A client that connects and stays silent receives no application data at all — twenty seconds of an
open socket produced zero frames. The device does send websocket Ping control frames, but those are
handled by the protocol layer and never reach an application.

This rules out learning the message schema by observation alone. It also means a status query has to
be the first thing a control client sends.

### Dependency cost — answered: no dependency needed

The whole discovery path above was driven by a hand-written DNS-SD query and parser of roughly 150
lines. One PTR query plus parsing of PTR/SRV/TXT/A records — including name compression — is all the
mDNS this package needs. Taking Makaretu.Dns or Zeroconf for that is not worth the dependency in a
library whose selling point is a BCL-only core.

### The message schema

Measured by sending a single `ping` — a status query that changes nothing — and reading what came
back. A request is one JSON text frame:

```json
{
  "conversationToken": "<the per-device token from glagol/token>",
  "id": "<a uuid the caller invents>",
  "sentTime": 1787425216425,
  "payload": { "command": "ping" }
}
```

The reply echoes `id` as `requestId` and `sentTime` as `requestSentTime`, so responses can be matched
to requests, and carries `status` (`SUCCESS`), `sentTime`, `processingTime`, two large blocks that a
music client has no use for (`experiments`, and an `extra` holding a base64 protobuf `appState`), the
device's capability lists, and the part that matters:

```
state
  aliceState                IDLE
  playing                   true
  volume                    0.3            0..1
  canStop                   true
  timeSinceLastVoiceActivity 373
  hdmi { capable, present }
  playerState
    id, title, subtitle     the track, its title and its artist
    type                    Track
    duration                168            SECONDS, not milliseconds
    progress                67.11          seconds, fractional
    hasPlay, hasPause       what is possible RIGHT NOW, not what the device can do:
    hasNext, hasPrev        while playing, hasPause is true and hasPlay is false
    hasProgressBar
    playerType              music_thin
    playlistId, playlistType, playlistPuid, playlistDescription
    entityInfo { id, type, repeatMode, next { id, type }, prev { id, type } }
    extra { coverURI, radioSessionID, requestID, stateType }
    liveStreamText, showPlayer
```

Note the units: `duration` and `progress` are **seconds**, where Ynison uses milliseconds. A client
that shares a progress bar between the two sources has to convert.

`supported_features` lists 29 capability names — `absolute_volume_change`, `relative_volume_change`,
`multiroom`, `stereo_pair`, `bluetooth_player`, `audio_bitrate320` and so on. It describes what the
hardware can do, not the command vocabulary.

### It is a state stream, not a request/response API

One `ping` produced **fourteen** replies over the following twelve seconds. So the device does not
answer a question and fall silent: once a client has sent one valid message, it starts pushing state
on every change. That makes the shape of this client much closer to `IYnisonClient` than to the REST
endpoint groups — connect, send one message to open the conversation, then subscribe.

### The command vocabulary

Each of these was sent to a real speaker and its effect confirmed three seconds later with a fresh
`ping`, rather than trusted from the reply:

| `payload.command` | Effect | Status |
|---|---|---|
| `ping` | returns state, changes nothing | `SUCCESS` |
| `play` | resumes | `SUCCESS` |
| `stop` | **pauses** — this is the pause command | `SUCCESS` |
| `next` | skips to the next track | `SUCCESS` |
| `prev` | goes back to the previous track | `SUCCESS` |
| `setVolume` + `volume` (0..1) | sets the volume | `SUCCESS` |
| `rewind` + `position`? | accepted, but no seek was observed | `SUCCESS` |
| `pause` | **not a command** | `UNSUPPORTED` |
| anything unrecognised | | `UNSUPPORTED` |

`pause` deserves its own line because it is the obvious guess and it is wrong: the device answers
`UNSUPPORTED`, and pausing is `stop`. An unknown command is refused cleanly with the same status
rather than being silently dropped, which gives the library a real error signal to surface.

`rewind` is accepted rather than refused, so it exists — but sending `position: 5` produced no
observable seek, so the argument's name is a guess and stays unverified.

### Two traps in the reply, both worth knowing before writing a client

**A reply carries the state from before the command was applied.** Sending `stop` returns a frame
saying `playing: true`; three seconds later the device reports `playing: false`. The command worked;
the reply is simply a snapshot taken when the message arrived. Reading the command's own reply to
confirm what it did will conclude, every single time, that nothing happened.

**Not every command produces a correlated reply at all.** `play` was answered by no frame carrying
its `requestId` on either of two runs, while plainly working. So `requestId` is useful for matching
when a reply comes, but a client must never wait on one as an acknowledgement. The state stream is
the only reliable source of truth about what the device is doing.

## Open questions — still unanswered

- **The `rewind` argument.** The command exists; how to tell it where to seek does not follow from
  anything measured.
- **Cross-account control.** The official app controls speakers signed in to other accounts. Whether
  that holds for any device on the network or only in some pairing state changes the security posture
  of this feature substantially, and should be understood before shipping it. Note that everything
  measured here went through `device_list`, which is account-scoped — so this path does **not** yet
  explain how the official app reaches a stranger's speaker.
- **Platform reach.** Discovery is confirmed working on Windows. mDNS behaviour differs across
  Linux/macOS and breaks on many corporate and guest networks; the library must degrade to "found
  nothing" cleanly rather than hanging. Note that on a machine with several adapters (Hyper-V, WSL,
  VPN) a socket bound to `0.0.0.0` sends the query out whichever adapter wins on route metric — which
  found nothing at all here. Discovery must query **every** interface explicitly.

## Non-goals

- Wrapping the smart-home platform in general (lights, sockets, scenarios). This is about music
  playback endpoints only.
- Merging the two device lists inside either package. That belongs above both of them.
- Voice/assistant interaction.

## Suggested order of work

1. ~~Split the packages and move `CreateYnisonClient` to an extension.~~ **Done in 0.5.0** — the
   core is standalone and BCL-only.
2. Answer the open questions and write the findings down here before writing code. Discovery and
   transport are **done**; the credential and the command schema are what remain, and they need a
   capture of the official app talking to a speaker.
3. ~~Discovery only, behind `ILocalDeviceScanner`.~~ **Done** on the `local-devices` branch, verified
   against four real speakers. It needs no credential, no
   account and no answers from (2), and it is independently useful — the remote can list the speakers
   greyed out and say why they cannot be driven yet.
4. Authorization and control, once (2) is actually known.
