# Releasing & repository setup

🌐 **English** · [Русский](RELEASING.ru.md)

## 1. First push to GitHub

Create an empty `jrfrigat/YandexMusic` repository on GitHub (no README/license — they already exist here), then:

```bash
git add -A
git commit -m "Initial public release: refactor, multi-targeting, CI/CD, docs"
git branch -M main
git remote add origin https://github.com/jrfrigat/YandexMusic.git
git push -u origin HEAD
```

> CI, Docs and CodeQL workflows trigger on the `main` branch; the Release workflow triggers on version
> tags (`v*`). To start with a clean history instead, run `rm -rf .git && git init` before the commands above.

## 2. Repository settings (Settings tab)

- **General → Default branch**: `main`.
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
- **Branch protection (optional)**: protect `main`, require the **CI** check to pass before merge.

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

1. Update `CHANGELOG.md` and `CHANGELOG.ru.md` (move items from *Unreleased* to the new version).
2. Push a version tag: `git tag v0.1.0 && git push origin v0.1.0`.
   - The **Release** workflow builds, tests, packs and pushes both packages (`.nupkg` and
     `.snupkg`) to nuget.org, then creates the GitHub Release **as a draft**, attaches the player
     archives, and only then makes it visible. The draft step is deliberate: `scripts/install.*`
     resolve "latest release" and download the `ymt-*` archive out of it, so a visible release
     without those archives would break the one-command install for everyone. If a player build
     fails, the release stays a draft and the last good release remains "latest".
   - Do not create the release manually first — let the workflow own it.
   - The **Docs** workflow publishes the documentation site.

### When a release run fails

The workflow is re-runnable. Packages already on NuGet are skipped (`--skip-duplicate`), an
existing draft release is topped up instead of recreated, and the release is undrafted only once
both player archives are attached. So: fix the cause, push the fix, and re-run the failed run — or,
if the tag has to move because the fix is in a new commit and nothing was published yet:

```bash
git push origin main && git tag -f vX.Y.Z && git push origin -f vX.Y.Z
```

Moving a tag is only safe while nothing has been published under it. Once packages are on NuGet,
cut a new patch version instead — NuGet does not allow re-publishing a version.

The package version is derived from the tag by MinVer (the `v` prefix is stripped); an untagged
local build gets a `preview` pre-release (`MinVerDefaultPreReleaseIdentifiers` in
`Directory.Build.props`).
