> 🌐 [English](../en/authentication.md) · **Русский**

# Авторизация

Большинство эндпоинтов требуют авторизации. Клиент поддерживает несколько способов входа: существующий
OAuth-токен, официальный OAuth-флоу **device-code** и — best-effort — cookie, QR-код или логин/пароль.

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

## Вход по cookie, QR и паролю (best-effort)

Кроме токена клиент может входить по сессионным cookie Яндекса, по интерактивному QR-коду или по
логину и паролю.

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

// Логин и пароль (Яндекс часто требует капчу или 2FA — в этом случае метод бросает исключение)
await client.Authentication.SignInWithPasswordAsync("login", "password");
```

> ⚠️ Эти флоу — **best-effort** реализация версионно-хрупкого недокументированного протокола Яндекс
> Паспорта, который часто закрыт капчей. Проверяйте их на живом флоу — пути по токену и device-code
> надёжные и полностью поддерживаемые.

## Выход

```csharp
client.Authentication.SignOut();
```

## Где взять токен

Проще всего — через OAuth implicit flow Яндекса. Откройте в браузере

```
https://oauth.yandex.ru/authorize?response_type=token&client_id=23cabbbdc6cd418abb4b39c32c41195d
```

войдите и скопируйте значение `access_token` из адреса перенаправления
`https://music.yandex.ru/#access_token=…`. Не публикуйте токен и не храните его в коде — используйте
переменную окружения или защищённое хранилище.
