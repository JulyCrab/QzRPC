#!/usr/bin/env bash
#
# Assembles a macOS .app bundle around the self-contained QzRPC binary produced
# by `dotnet publish -r osx-*`.
#
# Usage:  build/macos/make-app-bundle.sh <published-binary> <output-dir>
# Example: build/macos/make-app-bundle.sh dist-macos/QzRPC dist-macos
#
# Uses only `cp`/`chmod`, so it runs on any platform (the icon is a prebuilt
# build/macos/AppIcon.icns committed to the repo). Regenerate that icon with
# build/macos/png_to_icns.py if the source artwork changes.
set -euo pipefail

BIN="${1:?path to the published QzRPC binary is required}"
OUTDIR="${2:?output directory is required}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ICNS="$ROOT/build/macos/AppIcon.icns"
PLIST="$ROOT/build/macos/Info.plist"

for f in "$BIN" "$ICNS" "$PLIST"; do
    if [[ ! -f "$f" ]]; then echo "error: required file not found: $f" >&2; exit 1; fi
done

APP="$OUTDIR/QzRPC.app"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

cp "$ICNS" "$APP/Contents/Resources/AppIcon.icns"
cp "$PLIST" "$APP/Contents/Info.plist"
cp "$BIN" "$APP/Contents/MacOS/QzRPC"
chmod +x "$APP/Contents/MacOS/QzRPC"

echo "Built bundle: $APP"
