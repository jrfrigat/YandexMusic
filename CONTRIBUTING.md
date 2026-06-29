# Contributing

🌐 **English** · [Русский](CONTRIBUTING.ru.md)

Thanks for your interest in the project! Here is a short guide.

## Environment

- .NET SDK **10** (it can build the `net8.0`/`net9.0`/`net10.0` targets).
- To run the tests for every target, the .NET 8, 9 and 10 runtimes are recommended.

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test  -c Release
```

Run tests for a specific target:

```bash
dotnet test -c Release -f net10.0
```

### Integration tests

`tests/YandexMusic.Tests/Integration` contains tests that hit the real API. They require a Yandex
Music OAuth token and are **skipped automatically when no token is set**.

1. Copy `.env.example` to `.env` and put your token in it:
   ```dotenv
   YANDEX_MUSIC_TOKEN=your_token
   ```
   (`.env` is in `.gitignore` — never commit the token. You can also set the `YANDEX_MUSIC_TOKEN` environment variable instead of `.env`.)
2. Run only the integration tests:
   ```bash
   dotnet test -c Release --filter Category=Integration
   ```
   Or only the unit tests (no network):
   ```bash
   dotnet test -c Release --filter Category!=Integration
   ```

## Structure

- `src/YandexMusic.API` — the low-level library.
- `src/YandexMusic` — the high-level client.
- `samples/YaMusicCli` — a usage example.
- `tests/YandexMusic.Tests` — unit and integration tests (xUnit).
- `docs/` — documentation (DocFX).

## Conventions

- Follow the style from `.editorconfig` (`dotnet format` helps).
- Public methods are asynchronous and accept a `CancellationToken`; use `ConfigureAwait(false)` in library code.
- Throw typed exceptions from `YandexMusic.API.Exceptions`, not `System.Exception`.
- NuGet package versions are managed centrally in `Directory.Packages.props`.
- Accompany new functionality with tests and, when needed, documentation in `docs/`.

## Pull request

1. Create a branch off `master`.
2. Make sure `dotnet build` and `dotnet test` pass without errors.
3. Describe your change in the PR and add an entry to `CHANGELOG.md` (the `Unreleased` section).

## License

By contributing, you agree that your contribution will be distributed under the [MIT](LICENSE) license.
