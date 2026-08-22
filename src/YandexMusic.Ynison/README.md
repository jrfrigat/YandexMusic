# YandexMusic.Ynison

Ynison support for the [YandexMusic](https://www.nuget.org/packages/YandexMusic) client.

Ynison is what keeps the web player, the phone apps and the smart speakers of one account in sync.
This package subscribes to that state over a websocket and sends remote-control commands back.

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

await ynison.SetPausedAsync(false);
```

`CreateYnisonClient()` is an extension method on `IYandexMusicClient`, so the core package stays free
of websockets: add this package only when you need the remote.

> ⚠️ Unofficial library, not affiliated with Yandex.
