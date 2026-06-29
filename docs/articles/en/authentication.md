> 🌐 **English** · [Русский](../ru/authentication.md)

# Authentication

Most endpoints require authorization. The client supports three sign-in flows: an existing OAuth
token, the official OAuth **device-code** flow, and best-effort cookie/QR.

## Sign in with a token

```csharp
client.Authentication.SignInWithToken("<oauth-token>");
Console.WriteLine(client.Authentication.IsAuthenticated);
```

The token is stored and sent as `Authorization: OAuth <token>` with every request.

> Keep the token out of source code — use an environment variable or a secure store.

## Sign in with the OAuth device-code flow

The device-code flow is the official, fully-supported way to sign a user in interactively without
ever handling their password: the user confirms a short code on a Yandex page while the library
polls for the token.

```csharp
var token = await client.Authentication.SignInWithDeviceFlowAsync(code =>
    Console.WriteLine($"Open {code.VerificationUrl} and enter code {code.UserCode}"));

// The client is now authenticated; persist token.AccessToken to reuse it later.
```

For finer control you can drive the two steps yourself — request a code with
`RequestDeviceCodeAsync`, then poll `PollDeviceTokenAsync` (it returns `null` while the user has not
yet confirmed). `DeviceAuthOptions` lets you override the OAuth client, device name, poll interval
and timeout.

## Persisting and resuming a session

The authentication state (`client.Authentication.Session`) can be exported to a serializable
`AuthSnapshot` and restored later — including the cookies — so users do not have to sign in again:

```csharp
var snapshot = client.Authentication.Session.Export();
var json = System.Text.Json.JsonSerializer.Serialize(snapshot);
// ... store json securely ...

client.Authentication.Session.Import(
    System.Text.Json.JsonSerializer.Deserialize<YandexMusic.Authentication.AuthSnapshot>(json)!);
```

## Cookie and QR sign-in (best-effort)

In addition to a token, the client can sign in with Yandex session cookies or an interactive QR code.

```csharp
// Cookies obtained elsewhere (for example from a browser/app login)
await client.Authentication.SignInWithCookiesAsync(cookies);

// QR code
var qr = await client.Authentication.StartQrSignInAsync();
// render qr.Url as a QR code for the user to scan with the Yandex app, then poll:
while (!await client.Authentication.TryCompleteQrSignInAsync(qr))
{
    await Task.Delay(TimeSpan.FromSeconds(2));
}
```

> ⚠️ These flows are a **best-effort** implementation of a version-sensitive, undocumented Yandex
> Passport protocol. Verify them against the live flow — the token path (`SignInWithToken`) is the
> robust, fully-supported option.

## Sign out

```csharp
client.Authentication.SignOut();
```

## Getting a token

You can extract an OAuth token from the official client/web. Do not publish the token or store it in
source code — use environment variables or a secure store.
