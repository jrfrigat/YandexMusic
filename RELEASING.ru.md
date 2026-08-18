# Релиз и настройка репозитория

🌐 [English](RELEASING.md) · **Русский**

## 1. Первый пуш в GitHub

Создайте пустой репозиторий `jrfrigat/YandexMusic` на GitHub (без README/лицензии — они уже здесь), затем:

```bash
git add -A
git commit -m "Initial public release: refactor, multi-targeting, CI/CD, docs"
git branch -M main
git remote add origin https://github.com/jrfrigat/YandexMusic.git
git push -u origin HEAD
```

> Workflow'ы CI, Docs и CodeQL срабатывают на ветке `main`; workflow релиза — на тегах версий (`v*`).
> Если хотите чистую историю, перед командами выполните `rm -rf .git && git init`.

## 2. Настройка репозитория (вкладка Settings)

- **General → Default branch**: `main`.
- **General → Features**: включите **Issues** и **Discussions** (шаблон issue ссылается на Discussions).
- **About** (⚙ на главной): задайте Description, добавьте topics
  `yandex-music, dotnet, csharp, api-client, nuget, async`, в Website укажите адрес Pages (ниже).
- **Pages**: Source = **GitHub Actions** (workflow `Docs` публикует сайт DocFX по адресу
  `https://jrfrigat.github.io/YandexMusic/`).
- **Actions → General**:
  - Actions permissions: **Allow all actions and reusable workflows**.
  - Workflow permissions: достаточно **Read repository contents** — каждый workflow сам запрашивает
    нужные ему права (`id-token`, `pages`).
- **Environments** (создаются автоматически при первом запуске; можно защитить):
  - `github-pages` — для workflow документации.
  - `nuget` — для workflow релиза; можно добавить обязательных ревьюеров.
- **Branch protection (опционально)**: защитите `main`, требуйте прохождения проверки **CI**.

## 3. Публикация в NuGet (Trusted Publishing / OIDC — без API-ключа)

1. На **nuget.org → Account → Trusted Publishing** добавьте политику:
   - Owner/repo: `jrfrigat/YandexMusic`
   - Workflow file: `release.yml`
   - Environment: `nuget`
   - Package IDs: `YandexMusic` и `YandexMusic.DependencyInjection` (зарезервируйте/владейте обоими
     ID — релиз публикует оба пакета).
2. В GitHub, **Settings → Secrets and variables → Actions → Variables**, добавьте переменную:
   - `NUGET_USER` = ваш логин на nuget.org.

Секрет с API-ключом не нужен — workflow получает временный токен через OIDC.

## 4. Выпуск релиза

1. Обновите `CHANGELOG.md` и `CHANGELOG.ru.md` (перенесите изменения из *Unreleased* в новую версию).
2. Запушьте тег версии: `git tag v0.1.0 && git push origin v0.1.0`.
   - Workflow **Release** соберёт, протестирует, упакует и опубликует оба пакета (`.nupkg` и
     `.snupkg`) в nuget.org, затем сам создаст GitHub Release и приложит пакеты и архивы плеера.
     Не создавайте релиз вручную заранее — шаг создания в workflow упадёт уже после публикации
     пакетов в NuGet.
   - Workflow **Docs** опубликует сайт документации.

Версия пакета вычисляется из тега средствами MinVer (префикс `v` отбрасывается); локальная сборка
без тега получает pre-release `preview` (`MinVerDefaultPreReleaseIdentifiers` в
`Directory.Build.props`).
