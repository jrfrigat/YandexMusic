> 🌐 **English** · [Русский](../ru/architecture.md)

# Architecture

The library is a single core package plus an optional dependency-injection package, organized into
clear layers by namespace.

```
YandexMusic.DependencyInjection   // AddYandexMusic(): scoped client over an IHttpClientFactory pool
        │
        ▼
YandexMusic                       // YandexMusicClient + endpoint groups (Tracks, Search, …)
   Endpoints  →  Http (Connection)  →  Serialization (source-gen JSON)
   Models.*      Authentication        Exceptions
```

## Client and endpoint groups

`YandexMusicClient` (`IYandexMusicClient`) is the entry point. Each domain is exposed as a typed
endpoint group through an interface — `client.Tracks` is `ITracksClient`, `client.Search` is
`ISearchClient`, and so on. This keeps the surface testable and lets consumers depend on
abstractions. Adding a domain is just a new endpoint interface + implementation registered on the
client.

## Request pipeline

A single internal `Connection` (`IYandexMusicConnection`) is the request engine shared by every
endpoint group. It builds the request, applies the OAuth header and device id, sends it, unwraps the
`ApiResponse<T>` envelope (`{ result, error, invocationInfo }`) and maps failures to typed
exceptions. There is no per-endpoint builder class — endpoint methods describe requests declaratively.

When you construct a `YandexMusicClient` directly, its `HttpClient` is configured by
`YandexMusicHttpClientFactory` with a pooled `SocketsHttpHandler`, automatic decompression and the
session cookie container. Under dependency injection the handler is instead pooled by
`IHttpClientFactory` and is **cookie-free** — API requests authenticate with the per-scope OAuth
token, so the long-lived shared handler keeps users in different scopes isolated.

## JSON serialization

All serialization goes through a source-generated `JsonSerializerContext`. A single shared, frozen
`JsonSerializerOptions` is reused for every request — no per-call allocation — and responses are
deserialized straight from the UTF-8 stream via `JsonTypeInfo<T>`. This keeps the hot path
allocation-friendly and trim/AOT-safe (the library sets `IsAotCompatible`).

Enums are tolerant: a converter matches `[EnumMember]`-style kebab and `UPPER_SNAKE` spellings
case-insensitively and falls back to the enum's `Unknown` member on unrecognized input, so a new
server-side value never breaks a whole response. A custom converter handles the polymorphic search
`best` match.

## Authentication

`IAuthSession` holds the access token, cookies and device identity, and can be exported to a
serializable `AuthSnapshot` and restored later. `IAuthenticationClient` (`client.Authentication`)
exposes the sign-in/out operations.

## Exceptions

All errors derive from `YandexMusicException`:

| Type | When |
|------|------|
| `YandexMusicApiException` | API response is non-2xx; carries `StatusCode`, `ErrorName`, `Error`, `RawResponse`. |
| `YandexMusicAuthenticationException` | Authentication errors. |
| `YandexMusicSerializationException` | Failed to parse a response. |

## Asynchrony and multi-targeting

Every public method is asynchronous, accepts a `CancellationToken` and uses `ConfigureAwait(false)`
in library code. The library is built for `net8.0;net9.0;net10.0` with Source Link and symbol
packages enabled.
