# Development Setup

See [`docs/QuackForge_Master_PRD_v2.0.md`](docs/QuackForge_Master_PRD_v2.0.md) **Part II** for the full environment spec. This file is a quick reference.

## Prerequisites

- **macOS (primary dev)**: Homebrew, .NET 8 SDK, JetBrains Rider, fswatch
- **Windows 11 VM (QA)**: Parallels Desktop + Windows 11 ARM (x64 emulation)
- **Escape from Duckov** installed on both platforms
- **BepInEx**
  - Mac (arm64/x64): `BepInEx_macos_universal_5.4.23.5.zip` (native Apple Silicon)
  - Windows (x64): `BepInEx_win_x64_5.4.21.0.zip` (or 5.4.23.5)
- **Runtime note**: Duckov on Mac requires `steam_appid.txt` (AppID `3167020`) in the game directory to allow `run_bepinex.sh` standalone launch (bypasses Steam DRM `RestartAppIfNecessary` check).

Detailed install steps live in PRD §2.2 (Mac), §2.3 (VM), §2.4 (Windows). See also [`docs/vm-setup-guide.md`](docs/vm-setup-guide.md) for Parallels flow.

## Build from source

```bash
dotnet restore
dotnet build -c Release
```

## One-shot BepInEx install (Mac)

```bash
./scripts/install-bepinex-mac.sh
```

Downloads BepInEx 5.4.23.5 universal, extracts into Duckov dir, configures `run_bepinex.sh`, strips Gatekeeper xattrs, writes `steam_appid.txt` (AppID 3167020).

## Deploy (Mac)

```bash
./scripts/deploy-mac.sh
```

Copies `QuackForge.Loader.dll` into `~/Library/Application Support/Steam/steamapps/common/Escape From Duckov/BepInEx/plugins/QuackForge/`.

## Launch Duckov with BepInEx loaded (Mac)

```bash
./scripts/run-duckov-mac.sh
```

Runs Duckov under Rosetta (x86_64) so `libdoorstop.dylib` can inject. On Apple Silicon this wrapper is required — BepInEx 5's MonoMod/HarmonyX native patching is not stable on arm64 Mono. The script handles the `arch -x86_64 → bash → exec` chain needed to survive macOS SIP stripping DYLD env vars.

## Verify

Check `BepInEx/LogOutput.log`:

```
[Message: BepInEx] BepInEx 5.4.23.5 - Duckov
[Info   : BepInEx] Detected Unity version: v2022.3.62f2
[Message: BepInEx] Chainloader startup complete
[Info   : QuackForge] 🦆 QuackForge is awake. Forging begins. (v0.0.1)
```
