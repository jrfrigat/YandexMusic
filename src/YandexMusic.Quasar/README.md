# YandexMusic.Quasar

Yandex smart speakers for the [YandexMusic](https://www.nuget.org/packages/YandexMusic) client: find
them on the local network and control them directly, without going through the cloud.

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

Finding nothing is a normal outcome: many corporate and guest networks block multicast.

## Control a speaker

Control needs the account, for two things the network cannot supply: a per-device token, and the
certificate the speaker is supposed to present — without which the connection could only be trusted
blindly.

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
```

Two things about the protocol are worth knowing before you write against it. A command's own reply
carries the state from **before** it was applied, so confirming a command by reading its answer
always concludes that nothing happened — watch the frames that follow. And most frames are pushed by
the device rather than answering anything, so they carry no `Status` at all.

> ⚠️ Unofficial library, not affiliated with Yandex. The endpoints it uses are undocumented.
