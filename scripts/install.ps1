<#
.SYNOPSIS
    Installs the ymt terminal player (Windows, x64).

.DESCRIPTION
    Downloads the self-contained release build from GitHub, unpacks it into the user's programs
    directory and puts that directory on the user PATH. Nothing is installed machine-wide and no
    administrator rights are needed; .NET does not have to be installed either.

    Run it directly:

        irm https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.ps1 | iex

    To pass options, fetch the script into a scriptblock first:

        & ([scriptblock]::Create((irm https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.ps1))) -Version v0.4.0

.PARAMETER Version
    The release tag to install, for example "v0.4.0". Defaults to the latest release.

.PARAMETER InstallDir
    Where to unpack. Defaults to %LOCALAPPDATA%\Programs\ymt.

.PARAMETER NoPathUpdate
    Skip adding the install directory to the user PATH.
#>
[CmdletBinding()]
param(
    [string] $Version = 'latest',
    [string] $InstallDir = (Join-Path $env:LOCALAPPDATA 'Programs\ymt'),
    [switch] $NoPathUpdate
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = 'jrfrigat/YandexMusic'
$command = 'ymt'

function Write-Step([string] $message) { Write-Host "==> $message" -ForegroundColor Cyan }

if ([Environment]::Is64BitOperatingSystem -eq $false) {
    throw "ymt ships for 64-bit Windows only; this system is 32-bit."
}

# TLS 1.2 for Windows PowerShell 5.1, whose default still excludes it on older builds.
try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 } catch { }

$headers = @{ 'User-Agent' = 'ymt-installer'; 'Accept' = 'application/vnd.github+json' }
$releaseUrl = if ($Version -eq 'latest') {
    "https://api.github.com/repos/$repo/releases/latest"
} else {
    "https://api.github.com/repos/$repo/releases/tags/$Version"
}

Write-Step "Looking up the $Version release of $repo"
try {
    $release = Invoke-RestMethod -Uri $releaseUrl -Headers $headers
}
catch {
    throw "Cannot reach the GitHub release API ($releaseUrl): $($_.Exception.Message)"
}

$asset = $release.assets | Where-Object { $_.name -like 'ymt-*-win-x64.zip' } | Select-Object -First 1
if (-not $asset) {
    throw "Release $($release.tag_name) has no win-x64 archive. Assets: $(($release.assets | ForEach-Object name) -join ', ')"
}

$temp = Join-Path ([IO.Path]::GetTempPath()) ("ymt-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $archive = Join-Path $temp $asset.name
    Write-Step "Downloading $($asset.name) ($([math]::Round($asset.size / 1MB, 1)) MB)"
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $archive -Headers $headers

    Write-Step "Unpacking into $InstallDir"
    $staging = Join-Path $temp 'unpacked'
    Expand-Archive -Path $archive -DestinationPath $staging -Force

    # Replace the contents rather than the directory itself: the directory may already be on PATH,
    # and a running shell keeps resolving the path it was given.
    if (Test-Path $InstallDir) {
        Get-ChildItem -Path $InstallDir -Force | Remove-Item -Recurse -Force
    }
    else {
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    }

    Copy-Item -Path (Join-Path $staging '*') -Destination $InstallDir -Recurse -Force
}
finally {
    Remove-Item -Path $temp -Recurse -Force -ErrorAction SilentlyContinue
}

$exe = Join-Path $InstallDir "$command.exe"
if (-not (Test-Path $exe)) {
    throw "The archive did not contain $command.exe. Contents: $((Get-ChildItem $InstallDir | ForEach-Object Name) -join ', ')"
}

if (-not $NoPathUpdate) {
    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $entries = if ($userPath) { $userPath.Split(';', [StringSplitOptions]::RemoveEmptyEntries) } else { @() }
    if ($entries -notcontains $InstallDir) {
        Write-Step "Adding $InstallDir to the user PATH"
        $updated = (@($entries) + $InstallDir) -join ';'
        [Environment]::SetEnvironmentVariable('Path', $updated, 'User')
        # So the current session can run it without reopening the terminal.
        $env:Path = "$env:Path;$InstallDir"
        Write-Host "    Open a new terminal for PATH to apply everywhere." -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "ymt $($release.tag_name) installed." -ForegroundColor Green
Write-Host "Run it with: " -NoNewline
Write-Host $command -ForegroundColor Yellow
