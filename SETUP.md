# Development Setup

See [`docs/QuackForge_Master_PRD_v2.0.md`](docs/QuackForge_Master_PRD_v2.0.md) **Part II** for the full environment spec. This file is a quick reference.

## Prerequisites

- **macOS (primary dev)**: Homebrew, .NET 8 SDK, JetBrains Rider, fswatch
- **Windows 11 VM (QA)**: UTM + Windows 11 ARM evaluation
- **Escape from Duckov** installed on both platforms
- **BepInEx 5.4.21**
  - Mac: `BepInEx_unix_x64_5.4.21.zip`
  - Windows: `BepInEx_x64_5.4.21.zip`

Detailed install steps live in PRD §2.2 (Mac), §2.3 (VM), §2.4 (Windows).

## Build from source

```bash
dotnet restore
dotnet build -c Release
```

## Deploy (Mac)

```bash
./scripts/deploy-mac.sh
```

Copies `QuackForge.Loader.dll` into `~/Library/Application Support/Steam/steamapps/common/Escape From Duckov/BepInEx/plugins/`.

## Verify

Launch Duckov and check `BepInEx/LogOutput.log`:

```
[Info   :   BepInEx] Loading [QuackForge 0.0.1]
[Info   : QuackForge] 🦆 QuackForge is awake. Forging begins. (v0.0.1)
```
