# Releasing & repository setup

🌐 **English** below · [Русская версия ниже](#релиз-и-настройка-репозитория)

## 1. First push to GitHub

Create an empty `jrfrigat/YandexMusic` repository on GitHub (no README/license — they already exist here), then:

```bash
git add -A
git commit -m "Initial public release: refactor, multi-targeting, CI/CD, docs"
git branch -M master            # or: git branch -M main
git remote add origin https://github.com/jrfrigat/YandexMusic.git
git push -u origin HEAD
```

> Workflows trigger on both `master` and `main`, so either default branch works.
> To start with a clean history instead, run `rm -rf .git && git init` before the commands above.

## 2. Repository settings (Settings tab)

- **General → Default branch**: `master` (or rename to `main`).
- **General → Features**: enable **Issues** and **Discussions** (the issue template links to Discussions).
- **About** (main page, ⚙): set Description, add topics
  `yandex-music, dotnet, csharp, api-client, nuget, async`, and set the site to the Pages URL below.
- **Pages**: Source = **GitHub Actions** (the `Docs` workflow deploys the DocFX site to
  `https://jrfrigat.github.io/YandexMusic/`).
- **Actions → General**:
  - Actions permissions: **Allow all actions and reusable workflows**.
  - Workflow permissions: **Read repository contents** is enough — each workflow requests the extra
    scopes it needs (`id-token`, `pages`) explicitly.
- **Environments** (created automatically on first run; optionally protect them):
  - `github-pages` — used by the Docs workflow.
  - `nuget` — used by the Release workflow; you may add required reviewers for safety.
- **Branch protection (optional)**: protect `master`, require the **CI** check to pass before merge.

## 3. NuGet publishing (Trusted Publishing / OIDC — no API key)

1. On **nuget.org → Account → Trusted Publishing**, add a policy:
   - Repository owner / repo: `jrfrigat/YandexMusic`
   - Workflow file: `release.yml`
   - Environment: `nuget`
   - Package IDs: `YandexMusic` and `YandexMusic.DependencyInjection` (reserve/own both IDs on your
     account — the release publishes both packages).
2. On GitHub, **Settings → Secrets and variables → Actions → Variables**, add a repository variable:
   - `NUGET_USER` = your nuget.org username.

No API-key secret is required — the workflow exchanges a short-lived token via OIDC.

## 4. Cutting a release

1. Update `CHANGELOG.md` (move items from *Unreleased* to the new version).
2. Create a **GitHub Release** with a tag like `v0.1.0` (the workflow strips the leading `v`).
   - The **Release** workflow builds, tests, packs and pushes both packages to nuget.org, and
     attaches the `.nupkg`/`.snupkg` to the release.
   - The **Docs** workflow publishes the documentation site.

The package version comes from the tag; locally the default is `VersionPrefix` in
`Directory.Build.props`.

---

# Релиз и настройка репозитория

🌐 [English version above](#releasing--repository-setup) · **Русская версия**

## 1. Первый пуш в GitHub

Создайте пустой репозиторий `jrfrigat/YandexMusic` на GitHub (без README/лицензии — они уже здесь), затем:

```bash
git add -A
git commit -m "Initial public release: refactor, multi-targeting, CI/CD, docs"
git branch -M master            # или: git branch -M main
git remote add origin https://github.com/jrfrigat/YandexMusic.git
git push -u origin HEAD
```

> Workflow'ы срабатывают и на `master`, и на `main` — подойдёт любая ветка по умолчанию.
> Если хотите чистую историю, перед командами выполните `rm -rf .git && git init`.

## 2. Настройки репозитория (вкладка Settings)

- **General → Default branch**: `master` (или переименуйте в `main`).
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
- **Branch protection (опционально)**: защитите `master`, требуйте прохождения проверки **CI**.

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

1. Обновите `CHANGELOG.md` (перенесите изменения из *Unreleased* в новую версию).
2. Создайте **GitHub Release** с тегом вида `v0.1.0` (workflow убирает ведущий `v`).
   - Workflow **Release** соберёт, протестирует, упакует и опубликует оба пакета в nuget.org,
     и приложит `.nupkg`/`.snupkg` к релизу.
   - Workflow **Docs** опубликует сайт документации.

Версия пакета берётся из тега; локально по умолчанию используется `VersionPrefix` из
`Directory.Build.props`.
