#!/usr/bin/env bash
# Headless Unity batchmode AssetBundle 빌드.
# CI / 일괄 릴리즈 빌드용. 평소 개발은 Unity 에디터 메뉴로 충분.
#
# 실행:
#   ./scripts/build-mod.sh                # Mac + Windows 둘 다 빌드
#   UNITY_BIN=<path> ./scripts/build-mod.sh   # Unity 바이너리 경로 오버라이드
#
# 기본 Unity 경로:
#   macOS: /Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity
#
# 산출물:
#   mod/quackforge/mac/*.bundle
#   mod/quackforge/windows/*.bundle

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
UNITY_PROJECT="$REPO_ROOT/unity-project"

# Unity 에디터 버전은 ProjectSettings/ProjectVersion.txt 에서 읽어서 경로 구성.
VERSION=$(awk '/m_EditorVersion:/ {print $2}' "$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt" 2>/dev/null || echo "")
if [[ -z "$VERSION" ]]; then
  echo "[build-mod] failed to read Unity version from ProjectVersion.txt" >&2
  exit 1
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity}"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[build-mod] Unity binary not found: $UNITY_BIN" >&2
  echo "[build-mod] override with UNITY_BIN=<path> ./scripts/build-mod.sh" >&2
  exit 1
fi

LOG_FILE="$(mktemp -t quackforge-unity-build.XXXXXX.log)"
echo "[build-mod] Unity: $UNITY_BIN"
echo "[build-mod] project: $UNITY_PROJECT"
echo "[build-mod] log: $LOG_FILE"
echo "[build-mod] building… (Unity 시작에 수십 초 소요)"

set +e
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$UNITY_PROJECT" \
  -executeMethod QuackForge.Editor.BuildAssetBundles.BuildAllCli \
  -logFile "$LOG_FILE"
EXIT=$?
set -e

if [[ "$EXIT" -ne 0 ]]; then
  echo "[build-mod] FAILED (exit $EXIT). Last 40 lines of log:" >&2
  tail -40 "$LOG_FILE" >&2
  exit "$EXIT"
fi

echo "[build-mod] done."
echo "[build-mod] artifacts:"
find "$REPO_ROOT/mod/quackforge" -type f -name '*.bundle' 2>/dev/null | sort | sed 's|^|  |'
