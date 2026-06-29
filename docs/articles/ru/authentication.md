> 🌐 [English](../en/authentication.md) · **Русский**

# Авторизация

Большинство эндпоинтов требуют авторизации. Клиент поддерживает три способа входа: существующий
OAuth-токен, официальный OAuth-флоу **device-code** и best-effort cookie/QR.

## Вход по токену

```csharp
client.Authentication.SignInWithToken("<oauth-token>");
Console.WriteLine(client.Authentication.IsAuthenticated);
```

Токен сохраняется и отправляется как `Authorization: OAuth <token>` с каждым запросом.

> Не храните токен в коде — используйте переменную окружения или защищённое хранилище.

## Вход через OAuth device-code flow

Device-code flow — официальный, полностью поддерживаемый способ интерактивно авторизовать
пользователя, не работая с его паролем: пользователь подтверждает короткий код на странице Яндекса,
а библиотека опрашивает сервер до получения токена.

```csharp
var token = await client.Authentication.SignInWithDeviceFlowAsync(code =>
    Console.WriteLine($"Откройте {code.VerificationUrl} и введите код {code.UserCode}"));

// Клиент авторизован; сохраните token.AccessToken для повторного использования.
```

Для точного контроля можно выполнить два шага самостоятельно: запросить код через
`RequestDeviceCodeAsync`, затем опрашивать `PollDeviceTokenAsync` (возвращает `null`, пока
пользователь не подтвердил). `DeviceAuthOptions` позволяет переопределить OAuth-клиент, имя
устройства, интервал опроса и таймаут.

## Сохранение и восстановление сессии

Состояние авторизации (`client.Authentication.Session`) можно экспортировать в сериализуемый
`AuthSnapshot` и восстановить позже — включая куки — чтобы пользователю не пришлось входить заново:

```csharp
var snapshot = client.Authentication.Session.Export();
var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
// ... сохраните json в защищённом хранилище ...

client.Authentication.Session.Import(
    System.Text.Json.JsonSerializer.Deserialize<YandexMusic.Authentication.AuthSnapshot>(json)!);
```

## Вход по cookie и QR (best-effort)

Кроме токена клиент может входить по сессионным cookie Яндекса или по интерактивному QR-коду.

```csharp
// Cookie, полученные иным способом (например, из браузера/приложения)
await client.Authentication.SignInWithCookiesAsync(cookies);

// QR-код
var qr = await client.Authentication.StartQrSignInAsync();
// отрисуйте qr.Url как QR-код, пользователь сканирует его приложением Яндекса, затем опрашивайте:
while (!await client.Authentication.TryCompleteQrSignInAsync(qr))
{
    await Task.Delay(TimeSpan.FromSeconds(2));
}
```

> ⚠️ Эти флоу — **best-effort** реализация версионно-хрупкого недокументированного протокола Яндекс
> Паспорта. Проверьте их на живом флоу — путь по токену (`SignInWithToken`) надёжный и полностью
> поддерживаемый.

## Выход

```csharp
client.Authentication.SignOut();
```

## Где взять токен

OAuth-токен можно извлечь из официального клиента/веба. Не публикуйте токен и не храните его в коде —
используйте переменные окружения или защищённое хранилище.
