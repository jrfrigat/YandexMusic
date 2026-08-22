#!/bin/sh
# Installs the ymt terminal player (Linux, x86-64).
#
# Downloads the self-contained release build from GitHub, unpacks it under the user's data directory
# and links the binary into ~/.local/bin. Nothing is installed system-wide, no root is needed, and
# .NET does not have to be installed.
#
#     curl -fsSL https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.sh | sh
#
# Options come from the environment, so they survive being piped into a shell:
#
#     YMT_VERSION=v0.4.0 curl -fsSL .../install.sh | sh
#     YMT_INSTALL_DIR=$HOME/opt/ymt curl -fsSL .../install.sh | sh
#     YMT_BIN_DIR=$HOME/bin         curl -fsSL .../install.sh | sh

set -eu

REPO='jrfrigat/YandexMusic'
COMMAND='ymt'
VERSION="${YMT_VERSION:-latest}"
INSTALL_DIR="${YMT_INSTALL_DIR:-${XDG_DATA_HOME:-$HOME/.local/share}/ymt}"
BIN_DIR="${YMT_BIN_DIR:-$HOME/.local/bin}"

step() { printf '==> %s\n' "$1"; }
fail() { printf 'error: %s\n' "$1" >&2; exit 1; }

for tool in curl tar; do
    command -v "$tool" >/dev/null 2>&1 || fail "$tool is required but not installed."
done

arch=$(uname -m)
case "$arch" in
    x86_64 | amd64) ;;
    *) fail "ymt ships for x86-64 Linux only; this machine is $arch. Build from source instead: https://github.com/$REPO" ;;
esac

case "$(uname -s)" in
    Linux) ;;
    *) fail "This script installs the Linux build. On Windows use scripts/install.ps1." ;;
esac

if [ "$VERSION" = 'latest' ]; then
    release_url="https://api.github.com/repos/$REPO/releases/latest"
else
    release_url="https://api.github.com/repos/$REPO/releases/tags/$VERSION"
fi

step "Looking up the $VERSION release of $REPO"
release=$(curl -fsSL -H 'Accept: application/vnd.github+json' -H 'User-Agent: ymt-installer' "$release_url") \
    || fail "cannot reach the GitHub release API ($release_url)."

# The download URL of the linux-x64 archive, without requiring jq.
download_url=$(printf '%s' "$release" |
    tr ',' '\n' |
    grep '"browser_download_url"' |
    sed 's/.*"browser_download_url"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/' |
    grep 'linux-x64\.tar\.gz$' |
    head -n 1)

[ -n "$download_url" ] || fail "this release has no linux-x64 archive."

tag=$(printf '%s' "$release" | tr ',' '\n' | grep '"tag_name"' |
    sed 's/.*"tag_name"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/' | head -n 1)

tmp=$(mktemp -d)
# shellcheck disable=SC2064
trap "rm -rf '$tmp'" EXIT INT TERM

step "Downloading $(basename "$download_url")"
curl -fsSL --proto '=https' --tlsv1.2 -o "$tmp/ymt.tar.gz" "$download_url" \
    || fail "download failed."

step "Unpacking into $INSTALL_DIR"
mkdir -p "$tmp/unpacked"
tar -xzf "$tmp/ymt.tar.gz" -C "$tmp/unpacked"

[ -f "$tmp/unpacked/$COMMAND" ] || fail "the archive did not contain a '$COMMAND' binary."

# Replace the contents, not the directory: it may already be referenced by an existing symlink.
mkdir -p "$INSTALL_DIR"
rm -rf "${INSTALL_DIR:?}/"* 2>/dev/null || true
cp -R "$tmp/unpacked/." "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/$COMMAND"

mkdir -p "$BIN_DIR"
ln -sf "$INSTALL_DIR/$COMMAND" "$BIN_DIR/$COMMAND"

printf '\n%s %s installed.\n' "$COMMAND" "${tag:-$VERSION}"

case ":$PATH:" in
    *":$BIN_DIR:"*)
        printf 'Run it with: %s\n' "$COMMAND"
        ;;
    *)
        printf '%s is not on your PATH. Add this to your shell profile:\n\n    export PATH="%s:$PATH"\n\n' "$BIN_DIR" "$BIN_DIR"
        printf 'Until then run it with: %s\n' "$BIN_DIR/$COMMAND"
        ;;
esac
