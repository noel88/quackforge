#!/usr/bin/env bash
# Build QuackForge.Loader in Release mode and deploy to local Duckov BepInEx plugins.
# Usage: ./scripts/deploy-mac.sh [--skip-build] [--watch]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DUCKOV_DIR="${DUCKOV_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov}"
PLUGINS_DIR="$DUCKOV_DIR/BepInEx/plugins/QuackForge"
BUILD_OUTPUT="$REPO_ROOT/src/QuackForge.Loader/bin/Release"

SKIP_BUILD=0
WATCH=0
for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=1 ;;
    --watch)      WATCH=1 ;;
    -h|--help)
      sed -n '2,4p' "$0"
      exit 0
      ;;
    *)
      echo "unknown arg: $arg" >&2
      exit 2
      ;;
  esac
done

log() { printf '\033[0;36m[deploy-mac]\033[0m %s\n' "$*"; }
err() { printf '\033[0;31m[deploy-mac]\033[0m %s\n' "$*" >&2; }

check_duckov() {
  if [[ ! -d "$DUCKOV_DIR" ]]; then
    err "Duckov not found at: $DUCKOV_DIR"
    err "Set DUCKOV_DIR env var to override."
    exit 1
  fi
  if [[ ! -d "$DUCKOV_DIR/BepInEx" ]]; then
    err "BepInEx not installed at: $DUCKOV_DIR/BepInEx"
    err "Install BepInEx 5.4.21 first (see SETUP.md)."
    exit 1
  fi
}

build() {
  log "dotnet build -c Release"
  (cd "$REPO_ROOT" && dotnet build -c Release --nologo -v minimal)
}

deploy() {
  local loader="$BUILD_OUTPUT/QuackForge.Loader.dll"
  if [[ ! -f "$loader" ]]; then
    err "Build output missing: $loader"
    exit 1
  fi
  local duckov_managed="$DUCKOV_DIR/Duckov.app/Contents/Resources/Data/Managed"
  mkdir -p "$PLUGINS_DIR"
  local count=0
  for dll in "$BUILD_OUTPUT"/*.dll; do
    [[ -f "$dll" ]] || continue
    local base
    base=$(basename "$dll")
    case "$base" in
      QuackForge.*.dll)
        cp "$dll" "$PLUGINS_DIR/"
        log "deployed → $base"
        count=$((count + 1))
        ;;
      *)
        # Ship third-party deps only if the game doesn't already provide them
        # (avoids type-forwarding conflicts with Mono runtime assemblies).
        if [[ -f "$duckov_managed/$base" ]]; then
          continue
        fi
        cp "$dll" "$PLUGINS_DIR/"
        log "dep      → $base"
        count=$((count + 1))
        ;;
    esac
  done
  log "$count DLLs deployed"
}

run_once() {
  check_duckov
  [[ "$SKIP_BUILD" -eq 0 ]] && build
  deploy
}

if [[ "$WATCH" -eq 1 ]]; then
  if ! command -v fswatch >/dev/null 2>&1; then
    err "fswatch not installed. brew install fswatch"
    exit 1
  fi
  run_once
  log "watching src/ — Ctrl+C to stop"
  fswatch -o "$REPO_ROOT/src" | while read -r _; do
    run_once || err "build failed, continuing to watch"
  done
else
  run_once
fi
