# Участие в разработке

🌐 [English](CONTRIBUTING.md) · **Русский**

Спасибо за интерес к проекту! Ниже — короткое руководство.

## Окружение

- .NET SDK **10** (умеет собирать таргеты `net8.0`/`net9.0`/`net10.0`).
- Для запуска тестов под все таргеты желательны рантаймы .NET 8, 9 и 10.

## Сборка и тесты

```bash
dotnet restore
dotnet build -c Release
dotnet test  -c Release
```

Запуск тестов под конкретный таргет:

```bash
dotnet test -c Release -f net10.0
```

### Интеграционные тесты

В `tests/YandexMusic.Tests/Integration` есть тесты, обращающиеся к реальному API. Они требуют
OAuth-токен Яндекс Музыки и **автоматически пропускаются, если токен не задан**.

1. Скопируйте `.env.example` в `.env` и впишите токен:
   ```dotenv
   YANDEX_MUSIC_TOKEN=ваш_токен
   ```
   (Файл `.env` в `.gitignore` — не коммитьте токен. Вместо `.env` можно задать переменную окружения `YANDEX_MUSIC_TOKEN`.)
2. Запустите только интеграционные тесты:
   ```bash
   dotnet test -c Release --filter Category=Integration
   ```
   Или только юнит-тесты (без сети):
   ```bash
   dotnet test -c Release --filter Category!=Integration
   ```

## Структура

- `src/YandexMusic.API` — низкоуровневая библиотека.
- `src/YandexMusic` — высокоуровневый клиент.
- `samples/YaMusicCli` — пример использования.
- `tests/YandexMusic.Tests` — модульные тесты (xUnit).
- `docs/` — документация (DocFX).

## Соглашения

- Соблюдайте стиль из `.editorconfig` (`dotnet format` поможет привести код в порядок).
- Публичные методы — асинхронные, принимают `CancellationToken`; в библиотечном коде используйте `ConfigureAwait(false)`.
- Бросайте типизированные исключения из `YandexMusic.API.Exceptions`, а не `System.Exception`.
- Версии NuGet-пакетов задаются централизованно в `Directory.Packages.props`.
- Новый функционал сопровождайте тестами и, при необходимости, документацией в `docs/`.

## Pull request

1. Создайте ветку от `master`.
2. Убедитесь, что `dotnet build` и `dotnet test` проходят без ошибок.
3. Опишите изменения в PR и добавьте запись в `CHANGELOG.md` (раздел `Unreleased`).

## Лицензия

Отправляя вклад, вы соглашаетесь с тем, что он будет распространяться под лицензией [MIT](LICENSE).
