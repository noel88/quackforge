#!/usr/bin/env bash
# Launch Duckov on macOS Apple Silicon with BepInEx 5.4.23.5 loaded.
#
# Why this script exists instead of the stock run_bepinex.sh:
#
#   Duckov ships as a Universal Binary (x86_64 + arm64). On Apple Silicon the
#   default exec selects arm64, which rules out BepInEx 5 because MonoMod /
#   HarmonyX 2.10 native patching is not reliable on arm64 Mono yet — the
#   preloader loads but hangs before the log system comes up.
#
#   The fix is to run the game under Rosetta (x86_64 slice) while still
#   injecting libdoorstop. But macOS SIP strips DYLD_* across the `arch`
#   boundary, even with `arch -e KEY=val`. So we do:
#
#     arch -x86_64  ->  bash wrapper (re-exports DYLD_*)  ->  exec Duckov
#
#   The bash -> exec step is not a SIP boundary, so DYLD_INSERT_LIBRARIES
#   survives into the game process.
#
#   Duckov additionally requires steam_appid.txt (AppID 3167020) in the game
#   directory so Steamworks.NET RestartAppIfNecessary does not kill us.
#
# Usage: ./scripts/run-duckov-mac.sh

set -u

DUCKOV_DIR="${DUCKOV_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov}"
EXE="$DUCKOV_DIR/Duckov.app/Contents/MacOS/Duckov"
DYLIB="$DUCKOV_DIR/libdoorstop.dylib"
PRELOADER="$DUCKOV_DIR/BepInEx/core/BepInEx.Preloader.dll"
APPID_FILE="$DUCKOV_DIR/steam_appid.txt"

err() { printf '\033[0;31m[run-duckov]\033[0m %s\n' "$*" >&2; }
log() { printf '\033[0;36m[run-duckov]\033[0m %s\n' "$*"; }

for f in "$EXE" "$DYLIB" "$PRELOADER"; do
  if [ ! -f "$f" ]; then
    err "missing: $f"
    err "Run: ./scripts/install-bepinex-mac.sh (or see PRD §2.2)"
    exit 1
  fi
done

if [ ! -f "$APPID_FILE" ]; then
  log "writing $APPID_FILE (Steam AppID 3167020 bypasses RestartAppIfNecessary)"
  printf "3167020" > "$APPID_FILE"
fi

# Stage 1: re-export Rosetta-safe env, exec Duckov.
# This runs only when we are already under arch -x86_64 (stage 2 below).
if [ "${QUACKFORGE_ROSETTA_STAGE:-0}" = "1" ]; then
  export DYLD_INSERT_LIBRARIES="$DYLIB"
  export DYLD_LIBRARY_PATH="$DUCKOV_DIR"
  export DOORSTOP_ENABLED=1
  export DOORSTOP_TARGET_ASSEMBLY="$PRELOADER"
  export DOORSTOP_IGNORE_DISABLED_ENV=0
  export DOORSTOP_MONO_DEBUG_ENABLED=0
  export DOORSTOP_MONO_DEBUG_ADDRESS="127.0.0.1:10000"
  export DOORSTOP_MONO_DEBUG_SUSPEND=0
  export DOORSTOP_MONO_DLL_SEARCH_PATH_OVERRIDE=""
  export DOORSTOP_BOOT_CONFIG_OVERRIDE=""
  export DOORSTOP_CLR_RUNTIME_CORECLR_PATH=".dylib"
  export DOORSTOP_CLR_CORLIB_DIR=""
  exec "$EXE"
fi

# Stage 2: re-invoke self under Rosetta.
log "launching Duckov under Rosetta (x86_64) with BepInEx"
export QUACKFORGE_ROSETTA_STAGE=1
exec arch -x86_64 "$0" "$@"
