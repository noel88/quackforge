#!/usr/bin/env bash
# One-shot installer for BepInEx 5.4.23.5 macos_universal into Duckov.
# Idempotent: safe to re-run.
#
# Background: see PRD §2.2. Apple Silicon arm64 Duckov + BepInEx 5.4.21 unix_x64
# dies with dyld arch mismatch, and Rosetta via `arch` is defeated by SIP.
# 5.4.23.5 ships a universal libdoorstop.dylib, and run-duckov-mac.sh handles
# the Rosetta+env re-export dance.
#
# Usage: ./scripts/install-bepinex-mac.sh

set -euo pipefail

DUCKOV_DIR="${DUCKOV_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov}"
BEPINEX_TAG="v5.4.23.5"
BEPINEX_ASSET="BepInEx_macos_universal_5.4.23.5.zip"
APPID="3167020"

log() { printf '\033[0;36m[install-bepinex]\033[0m %s\n' "$*"; }
err() { printf '\033[0;31m[install-bepinex]\033[0m %s\n' "$*" >&2; }

if [ ! -d "$DUCKOV_DIR" ]; then
  err "Duckov not found at: $DUCKOV_DIR"
  err "Install Escape from Duckov via Steam first."
  exit 1
fi

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

log "downloading $BEPINEX_ASSET from BepInEx/$BEPINEX_TAG"
if command -v gh >/dev/null 2>&1; then
  gh release download "$BEPINEX_TAG" --repo BepInEx/BepInEx -p "$BEPINEX_ASSET" --dir "$TMP"
else
  curl -fsSL -o "$TMP/$BEPINEX_ASSET" \
    "https://github.com/BepInEx/BepInEx/releases/download/$BEPINEX_TAG/$BEPINEX_ASSET"
fi

log "extracting into $DUCKOV_DIR"
unzip -o -q "$TMP/$BEPINEX_ASSET" -d "$DUCKOV_DIR"

log "configuring run_bepinex.sh executable_name=Duckov.app"
sed -i '' 's|^executable_name=""$|executable_name="Duckov.app"|' "$DUCKOV_DIR/run_bepinex.sh"
chmod +x "$DUCKOV_DIR/run_bepinex.sh"

log "stripping macOS Gatekeeper quarantine flags"
xattr -cr "$DUCKOV_DIR/BepInEx" "$DUCKOV_DIR/libdoorstop.dylib" "$DUCKOV_DIR/run_bepinex.sh"

log "writing steam_appid.txt ($APPID) to bypass Steam DRM RestartAppIfNecessary"
printf "%s" "$APPID" > "$DUCKOV_DIR/steam_appid.txt"

log "done. Launch with: ./scripts/run-duckov-mac.sh"
log "verify BepInEx log at: $DUCKOV_DIR/BepInEx/LogOutput.log"
