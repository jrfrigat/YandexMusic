> 🌐 [English](../en/architecture.md) · **Русский**

# Архитектура

Библиотека — это один основной пакет плюс опциональный пакет для DI, разнесённые по слоям через
namespace.

```
YandexMusic.DependencyInjection   // AddYandexMusic(): scoped-клиент поверх пула IHttpClientFactory
        │
        ▼
YandexMusic                       // YandexMusicClient + группы эндпоинтов (Tracks, Search, …)
   Endpoints  →  Http (Connection)  →  Serialization (source-gen JSON)
   Models.*      Authentication        Exceptions
```

## Клиент и группы эндпоинтов

`YandexMusicClient` (`IYandexMusicClient`) — точка входа. Каждый домен доступен как типизированная
группа эндпоинтов через интерфейс: `client.Tracks` — это `ITracksClient`, `client.Search` —
`ISearchClient` и т.д. Это упрощает тестирование и позволяет зависеть от абстракций. Добавить домен —
это новый интерфейс эндпоинта + реализация, зарегистрированная на клиенте.

## Конвейер запроса

Единый внутренний `Connection` (`IYandexMusicConnection`) — движок запросов, общий для всех групп
эндпоинтов. Он формирует запрос, добавляет OAuth-заголовок и идентификатор устройства, отправляет,
разворачивает конверт `ApiResponse<T>` (`{ result, error, invocationInfo }`) и маппит ошибки в
типизированные исключения. Никакого билдер-класса на эндпоинт — методы описывают запросы декларативно.

При прямом создании `YandexMusicClient` его `HttpClient` настраивается
`YandexMusicHttpClientFactory`: пул `SocketsHttpHandler`, автодекомпрессия и контейнер кук сессии.
Под DI handler пулится через `IHttpClientFactory` и **не хранит куки** — API-запросы авторизуются
per-scope OAuth-токеном, поэтому общий долгоживущий handler сохраняет изоляцию пользователей в разных
скоупах.

## Сериализация JSON

Вся сериализация идёт через source-generated `JsonSerializerContext`. Единый общий «замороженный»
`JsonSerializerOptions` переиспользуется для каждого запроса (без аллокаций на вызов), а ответы
десериализуются прямо из UTF-8 потока через `JsonTypeInfo<T>`. Горячий путь экономен к аллокациям и
дружелюбен к trim/AOT (включён `IsAotCompatible`).

Перечисления терпимы: конвертер сопоставляет kebab- и `UPPER_SNAKE`-написания без учёта регистра и на
неизвестном значении возвращает член `Unknown`, так что новое значение на стороне сервера не ломает
весь ответ. Полиморфный `best` поиска обрабатывается отдельным конвертером.

## Авторизация

`IAuthSession` хранит токен, куки и идентичность устройства и может быть экспортирован в сериализуемый
`AuthSnapshot` и восстановлен позже. `IAuthenticationClient` (`client.Authentication`) предоставляет
операции входа/выхода.

## Исключения

Все ошибки наследуются от `YandexMusicException`:

| Тип | Когда |
|-----|-------|
| `YandexMusicApiException` | Ответ API не 2xx; содержит `StatusCode`, `ErrorName`, `Error`, `RawResponse`. |
| `YandexMusicAuthenticationException` | Ошибки авторизации. |
| `YandexMusicSerializationException` | Не удалось разобрать ответ. |

## Асинхронность и мультитаргетинг

Все публичные методы асинхронны, принимают `CancellationToken` и используют `ConfigureAwait(false)` в
коде библиотеки. Сборка под `net8.0;net9.0;net10.0`, включены Source Link и символьные пакеты.
