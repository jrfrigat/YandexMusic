# Local device discovery, and splitting the repository into three libraries

**Status:** draft — the package split is decided in principle; the local protocol is not yet
investigated. Nothing below the "Open questions" heading should be treated as settled.

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
YandexMusic.LocalDevices    Same-network discovery and control. Depends on YandexMusic.
YandexMusic.DependencyInjection   Unchanged: AddYandexMusic() for the core.
```

Rules:

- `YandexMusic` never references the other two. It is complete and useful on its own — this is the
  constraint everything else bends around.
- `YandexMusic.Ynison` and `YandexMusic.LocalDevices` do not reference each other. A consumer wanting
  the merged device list of the official app takes both and merges them itself; a helper that does
  the merging can come later, in its own package, once both halves exist.
- All three ship from this repository, on one version line, so "the YandexMusic repo covers
  everything you need for this service" holds without splitting the work across repos.

### The one breaking change this requires

`IYandexMusicClient.CreateYnisonClient()` is the coupling that blocks the split: the core interface
names a Ynison type. It has to move out of the core and become an extension method that the Ynison
package supplies:

```csharp
// today, on the core interface
IYnisonClient CreateYnisonClient(string? deviceId = null, YnisonClientOptions? options = null);

// after the split, in YandexMusic.Ynison
public static class YandexMusicClientYnisonExtensions
{
    public static IYnisonClient CreateYnisonClient(
        this IYandexMusicClient client,
        string? deviceId = null,
        YnisonClientOptions? options = null);
}
```

Call sites do not change — `client.CreateYnisonClient()` still compiles — but a `using
YandexMusic.Ynison;` becomes necessary, and the method disappears from the core interface. The
extension needs the session's token, which is already reachable publicly through
`client.Authentication.Session`; confirm that it exposes the access token before committing to this
shape.

`YandexMusic.LocalDevices` gets the same treatment, so the three entry points read alike.

## Scope of YandexMusic.LocalDevices

Three separable pieces, in dependency order:

1. **Discovery.** Find the speakers on the current network and report id, name, model, address and
   whether they are reachable. Purely local; no account involved.
2. **Authorization.** Obtain whatever credential the local control channel requires. This is the
   part that touches a Yandex backend, and the part that is least understood.
3. **Control.** Connect to a device and drive it: play/pause, next/previous, volume, status.

A useful first milestone is **discovery alone** — a `LocalDeviceScanner` that returns what is on the
network. It is independently valuable (the remote can list the speakers greyed out, and say why they
cannot be controlled yet), it is verifiable without any credentials, and it de-risks the rest.

### Sketch of the surface

```csharp
public interface ILocalDeviceScanner
{
    IAsyncEnumerable<LocalDevice> DiscoverAsync(TimeSpan window, CancellationToken cancellationToken = default);
}

public sealed record LocalDevice(string DeviceId, string Name, string Model, IPEndPoint Endpoint);
```

`IAsyncEnumerable` rather than a list: discovery is a stream of answers arriving over a window, and
a UI wants to show each device the moment it replies.

## Open questions — none of these are answered yet

These need investigation with a packet capture from the official app before any code is written. I
have not verified any of it, and the issue should not pretend otherwise.

- **Discovery mechanism.** mDNS/DNS-SD is the strong hypothesis (the service type is commonly cited
  as `_yandexio._tcp`), but this needs confirming on the wire, along with what the TXT records carry.
- **Dependency cost.** .NET has no mDNS in the BCL. Either take a dependency (Makaretu.Dns,
  Zeroconf) or implement the subset needed. For a library that advertises a BCL-only core, this
  choice deserves its own discussion — it lands in `LocalDevices` only, but it is still a dependency.
- **Transport and TLS.** Local control is reportedly a websocket to the device itself, with a
  certificate that will not chain to a public root. Whatever validation is used must be a deliberate,
  documented decision, not a blanket "accept everything".
- **Credential.** What the device requires to accept commands, where it comes from, and its lifetime.
  If it must be fetched from a Yandex backend, that endpoint is undocumented and outside the Music
  API — which is an argument for keeping it in this package rather than the core.
- **Cross-account control.** The official app controls speakers signed in to other accounts. Whether
  that holds for any device on the network or only in some pairing state changes the security posture
  of this feature substantially, and should be understood before shipping it.
- **Platform reach.** mDNS behaviour differs across Windows/Linux/macOS and breaks on many corporate
  and guest networks. The library must degrade to "found nothing" cleanly rather than hanging.

## Non-goals

- Wrapping the smart-home platform in general (lights, sockets, scenarios). This is about music
  playback endpoints only.
- Merging the two device lists inside either package. That belongs above both of them.
- Voice/assistant interaction.

## Suggested order of work

1. Split the packages and move `CreateYnisonClient` to an extension. Mechanical, unblocks everything,
   and worth doing on its own merits — it is the change that makes the core standalone.
2. Capture the official app's traffic and answer the open questions above. Write the findings down
   here before writing code.
3. Discovery only, behind `ILocalDeviceScanner`, with the sample listing what it finds.
4. Authorization and control, once (2) is actually known.
