#!/usr/bin/env bash
# Deploy the minimal official-mod-path test (#65) to Duckov Mods folder on Mac.
#
# Produces:
#   <Duckov.app>/Contents/Resources/Data/Mods/quackforge_test/
#     ├── info.ini
#     └── quackforge_test.dll
#
# 사용자는 이후:
#   1. 덕코프 실행 (Steam 또는 run-duckov-mac.sh)
#   2. 메인 메뉴 → Mods → 약관 수락 (AllowActivatingMod = true)
#   3. quackforge_test 토글 ON
#   4. BepInEx/LogOutput.log (또는 Unity Player.log) 확인
#      → "[QuackForge.TestMod] OnAfterSetup — hello from official Duckov mod path."

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJ_DIR="$REPO_ROOT/src/QuackForge.TestMod"
BUILD_OUTPUT="$PROJ_DIR/bin/Release"

DUCKOV_DIR="${DUCKOV_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov}"
MODS_DIR="$DUCKOV_DIR/Duckov.app/Contents/Resources/Data/Mods"
TARGET_DIR="$MODS_DIR/quackforge_test"

SKIP_BUILD=0
for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=1 ;;
    -h|--help)    sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "unknown arg: $arg" >&2; exit 2 ;;
  esac
done

log() { printf '\033[0;36m[deploy-testmod]\033[0m %s\n' "$*"; }
err() { printf '\033[0;31m[deploy-testmod]\033[0m %s\n' "$*" >&2; }

if [[ ! -d "$DUCKOV_DIR" ]]; then
  err "Duckov not found: $DUCKOV_DIR"
  exit 1
fi

if [[ "$SKIP_BUILD" -eq 0 ]]; then
  log "dotnet build (QuackForge.TestMod, Release)"
  (cd "$PROJ_DIR" && dotnet build -c Release --nologo -v minimal)
fi

DLL="$BUILD_OUTPUT/quackforge_test.dll"
if [[ ! -f "$DLL" ]]; then
  err "build output missing: $DLL"
  exit 1
fi

log "Mods dir: $MODS_DIR"
mkdir -p "$TARGET_DIR"
cp "$DLL" "$TARGET_DIR/"
cp "$PROJ_DIR/info.ini" "$TARGET_DIR/"

log "deployed:"
ls -la "$TARGET_DIR" | sed 's|^|  |'

cat <<'NEXT'

Next steps (in-game):
  1. Launch Duckov (scripts/run-duckov-mac.sh)
  2. Main menu → Mods
  3. Accept the mod agreement if you haven't (AllowActivatingMod = true)
  4. Toggle "QuackForge (Test Mod)" ON (or restart)
  5. Check logs:
     - BepInEx/LogOutput.log
     - ~/Library/Logs/TeamSoda/Duckov/Player.log

Expected log line:
  [QuackForge.TestMod] OnAfterSetup — hello from official Duckov mod path.
NEXT
