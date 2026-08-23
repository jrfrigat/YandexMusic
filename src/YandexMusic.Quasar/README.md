# YandexMusic.Quasar

Yandex smart speakers for the [YandexMusic](https://www.nuget.org/packages/YandexMusic) client: find
them on the local network and drive them directly, without going through the cloud. Targets
**.NET 8, 9 and 10**.

A speaker never joins the account's Ynison session, so this is the only way to reach one.

## Find what is on the network

Discovery is mDNS/DNS-SD (`_yandexio._tcp`) and needs **no account, no token and no internet
connection** — it asks the local network and reports what answers.

```csharp
using YandexMusic.Quasar;

var scanner = new LocalDeviceScanner();
await foreach (var device in scanner.DiscoverAsync(TimeSpan.FromSeconds(3)))
{
    Console.WriteLine($"{device.Platform} {device.DeviceId} at {device.Endpoint}");
}
```

Finding nothing is a normal outcome, not an error: many corporate and guest networks block multicast
outright.

Devices do not advertise the name their owner gave them, so a UI that wants to show "kitchen" has to
get it from the account and match on `DeviceId`.

## Drive one

Control needs the account, for two things the network cannot supply: a per-device token, and the
certificate the speaker is supposed to present.

```csharp
using YandexMusic;
using YandexMusic.Quasar;

await using var music = new YandexMusicClient();
music.Authentication.SignInWithToken("<oauth-token>");

using var quasar = music.CreateQuasarClient();
var speaker = (await quasar.GetDevicesAsync()).First(d => d.Platform == "yandexmini");

await using var control = await quasar.ConnectAsync(speaker);
_ = control.RunAsync();

var state = await control.WaitForStateAsync(TimeSpan.FromSeconds(10));
Console.WriteLine($"{speaker.Name}: {state.State?.PlayerState?.Title}");

await control.PauseAsync();
await control.SetVolumeAsync(0.4);
await control.PlayTrackAsync("33221455");   // the speaker fetches and plays it itself
```

`GetDevicesAsync` returns every Quasar device on the account, which is more than speakers — cameras
and the phone apps signed in to it come back too. Filter on `NetworkInfo` and `Platform`.

## The certificate is pinned, deliberately

A speaker presents a self-signed certificate that names `localhost`, so ordinary validation can never
succeed and the usual shortcut is to trust anything. This package does not: it compares the
certificate against the one the account publishes for that exact device, and refuses anything else.

Expiry is not checked, on purpose. Speakers are shipping certificates that expired years ago and
still work; rejecting them would lock out the real device while proving nothing about any other.

## Three things that will look like bugs

- **A command's reply carries the state from before the command was applied.** Pause a speaker and
  its answer still says `Playing`; the truth arrives in the frames that follow. Confirming a command
  by reading its own answer concludes, every time, that nothing happened.
- **Not every command is answered at all**, and `SUCCESS` is not proof that anything started — an
  unknown track id gets the same answer as a good one. The state stream is the only source of truth.
- **Times here are seconds**, where `YandexMusic.Ynison` uses milliseconds for the same things.

The protocol is undocumented; all of the above was measured against real hardware rather than
assumed. The findings are written up
[in the repository](https://github.com/jrfrigat/YandexMusic/blob/main/docs/proposals/local-devices.md).

Documentation: <https://jrfrigat.github.io/YandexMusic/>

> ⚠️ Unofficial library, not affiliated with Yandex. It talks to undocumented endpoints. Use at your
> own risk and comply with the Yandex Music terms of use.
