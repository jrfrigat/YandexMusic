# YandexMusic.LocalDevices

Finds Yandex speakers on the current network, for the
[YandexMusic](https://www.nuget.org/packages/YandexMusic) client.

```csharp
using YandexMusic.LocalDevices;

var scanner = new LocalDeviceScanner();
await foreach (var device in scanner.DiscoverAsync(TimeSpan.FromSeconds(3)))
{
    Console.WriteLine($"{device.Platform} {device.DeviceId} at {device.Endpoint}");
}
```

Discovery is mDNS/DNS-SD (`_yandexio._tcp`) and needs **no account, no token and no internet
connection** — it asks the local network and reports what answers. Finding nothing is a normal
outcome: many corporate and guest networks block multicast.

Note that devices do not advertise the name their owner gave them, so a UI that wants to show
"kitchen" has to get it from an account-level source and match on `DeviceId`.

> ⚠️ Unofficial library, not affiliated with Yandex. Controlling a discovered device is not
> implemented yet — see the [proposal](https://github.com/jrfrigat/YandexMusic/blob/main/docs/proposals/local-devices.md).
