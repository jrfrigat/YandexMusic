# YandexMusic.Ynison

Real-time playback state and remote control for the
[YandexMusic](https://www.nuget.org/packages/YandexMusic) client. Targets **.NET 8, 9 and 10**.

Ynison is what keeps the web player and the phone apps of one account in sync. This package
subscribes to that state over a websocket and sends remote-control commands back, so an application
can see what the account is playing anywhere and drive it.

```csharp
using YandexMusic;
using YandexMusic.Ynison;

await using var client = new YandexMusicClient();
client.Authentication.SignInWithToken("<oauth-token>");

await using var ynison = client.CreateYnisonClient();
_ = ynison.RunAsync();

var state = await ynison.WaitForStateAsync(TimeSpan.FromSeconds(10));
foreach (var device in state.Devices)
{
    Console.WriteLine(device.Info?.Title);
}

ynison.StateReceived += (_, s) => Console.WriteLine(s.PlayerState?.Status?.Paused);

await ynison.SetPausedAsync(paused: false);
await ynison.NextTrackAsync();
await ynison.PlayOnDeviceAsync("<device-id>");
```

`CreateYnisonClient()` is an extension method on `IYandexMusicClient`, so the core package stays free
of websockets: add this one only when you need the remote.

## Smart speakers are not in this list

Worth knowing before you go looking for the bug: a Yandex speaker **never joins the account's Ynison
session**, so it will not appear among `state.Devices` no matter what it is doing. The session lists
the web player, the phone apps and other API clients — nothing else.

Speakers answer on the local network instead, and reaching them is what
[`YandexMusic.Quasar`](https://www.nuget.org/packages/YandexMusic.Quasar) is for. An application
that wants the device picker the official app shows takes both packages and merges the two lists.

## Two things the protocol will not tell you

Ynison sends a frame only when something changes — never a progress tick. Rendering
`PlayingStatus.ProgressMs` verbatim shows a counter frozen at the moment of the last change; add
`TimeSinceLatestState` to it.

Times here are **milliseconds**. `YandexMusic.Quasar` reports the same things in seconds, so anything
showing both on one progress bar has to convert.

Documentation: <https://jrfrigat.github.io/YandexMusic/>

> ⚠️ Unofficial library, not affiliated with Yandex. Use at your own risk and comply with the
> Yandex Music terms of use.
