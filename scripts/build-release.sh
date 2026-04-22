#!/usr/bin/env bash
# Build a distributable QuackForge ZIP for Nexus / manual install / beta testers.
#
# Produces: dist/quackforge-<version>.zip
#
# Layout inside the zip (what extracts into BepInEx/plugins/):
#   QuackForge/
#     QuackForge.Loader.dll
#     QuackForge.Core.dll
#     QuackForge.Data.dll
#     <third-party deps not shipped by the game>
#     README.txt  (install instructions)
#
# Usage:
#   ./scripts/build-release.sh                 # build Release + package
#   ./scripts/build-release.sh --skip-build    # package existing output
#   ./scripts/build-release.sh --duckov <path> # override Mac Duckov dir used
#                                              #   for dep-dedup check

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BUILD_OUTPUT="$REPO_ROOT/src/QuackForge.Loader/bin/Release"
DIST_DIR="$REPO_ROOT/dist"
DUCKOV_DIR="${DUCKOV_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov}"

SKIP_BUILD=0
for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=1 ;;
    --duckov)     shift; DUCKOV_DIR="$1" ;;
    -h|--help)    sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "unknown arg: $arg" >&2; exit 2 ;;
  esac
done

log() { printf '\033[0;36m[build-release]\033[0m %s\n' "$*"; }
err() { printf '\033[0;31m[build-release]\033[0m %s\n' "$*" >&2; }

# Pull version from Loader csproj — single source of truth.
VERSION=$(grep -oE '<Version>[^<]+</Version>' \
            "$REPO_ROOT/src/QuackForge.Loader/QuackForge.Loader.csproj" \
          | head -1 | sed -E 's|</?Version>||g')
if [[ -z "${VERSION:-}" ]]; then
  err "failed to parse <Version> from QuackForge.Loader.csproj"
  exit 1
fi
log "version: $VERSION"

if [[ "$SKIP_BUILD" -eq 0 ]]; then
  log "dotnet build -c Release"
  (cd "$REPO_ROOT" && dotnet build -c Release --nologo -v minimal)
fi

if [[ ! -f "$BUILD_OUTPUT/QuackForge.Loader.dll" ]]; then
  err "build output missing: $BUILD_OUTPUT/QuackForge.Loader.dll"
  exit 1
fi

# Stage contents that will extract as BepInEx/plugins/QuackForge/.
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
PLUGIN_DIR="$STAGE/QuackForge"
mkdir -p "$PLUGIN_DIR"

DUCKOV_MANAGED="$DUCKOV_DIR/Duckov.app/Contents/Resources/Data/Managed"
use_managed_filter=0
if [[ -d "$DUCKOV_MANAGED" ]]; then
  use_managed_filter=1
  log "dedup check against: $DUCKOV_MANAGED"
else
  log "Duckov Managed dir not found — shipping all deps (safer default)"
fi

count=0
for dll in "$BUILD_OUTPUT"/*.dll; do
  [[ -f "$dll" ]] || continue
  base=$(basename "$dll")
  case "$base" in
    QuackForge.*.dll)
      cp "$dll" "$PLUGIN_DIR/"
      count=$((count + 1))
      ;;
    *)
      if [[ "$use_managed_filter" -eq 1 && -f "$DUCKOV_MANAGED/$base" ]]; then
        continue
      fi
      cp "$dll" "$PLUGIN_DIR/"
      count=$((count + 1))
      ;;
  esac
done
log "staged $count DLLs"

# Install instructions inside the zip.
cat > "$PLUGIN_DIR/README.txt" <<EOF
QuackForge v$VERSION — Install Instructions

Requirements:
  1. Escape from Duckov (Steam)
  2. BepInEx 5.4.23.5
       - macOS (Apple Silicon): BepInEx_macos_universal_5.4.23.5.zip
       - Windows x64:           BepInEx_win_x64_5.4.23.5.zip

Installation:
  1. Install BepInEx into the Duckov game directory (where Duckov.exe / Duckov.app lives).
  2. Run Duckov once so BepInEx creates BepInEx/plugins/.
  3. Extract this archive so the 'QuackForge' folder sits inside BepInEx/plugins/.
     Expected path:
       BepInEx/plugins/QuackForge/QuackForge.Loader.dll
  4. Launch Duckov. Verify BepInEx/LogOutput.log contains:
       [Info :QuackForge] 🦆 QuackForge is awake. Forging begins. (v$VERSION)

macOS note:
  Apple Silicon requires running Duckov under Rosetta (x86_64). Use the bundled
  scripts/run-duckov-mac.sh from the repo, or see docs/vm-setup-guide.md.

Source: https://github.com/noel88/quackforge
EOF

mkdir -p "$DIST_DIR"
OUT="$DIST_DIR/quackforge-v$VERSION.zip"
rm -f "$OUT"
(cd "$STAGE" && zip -qr "$OUT" QuackForge)

size=$(wc -c < "$OUT" | tr -d ' ')
log "built → $OUT ($size bytes, $count DLLs + README.txt)"
log "SHA256:"
if command -v shasum >/dev/null 2>&1; then
  shasum -a 256 "$OUT"
else
  sha256sum "$OUT"
fi
