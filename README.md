# QuackForge

> Level-based progression + endgame challenge content for **Escape from Duckov** (Team Soda, Unity 2022.3.5f1).

QuackForge is a BepInEx 5 mod that adds:

- **Leveling system** — XP from kills/quests, 5-stat point allocation (VIT / STR / AGI / PRE / SUR)
- **Challenge Mode** — Boss Rush with time limits, limited lives, exclusive rewards
- **New weapons & armors** — 10+ weapons / 5+ armors reusing existing assets, gated by blueprints

See [`docs/QuackForge_Master_PRD_v2.0.md`](docs/QuackForge_Master_PRD_v2.0.md) for the full design.

## Status

**Phase 0 — Bootstrap** (in progress). Hello-world plugin that loads into BepInEx.

## Requirements

- Escape from Duckov (Steam)
- [BepInEx 5.4.21](https://github.com/BepInEx/BepInEx/releases) (unix x64 on Mac, x64 on Windows)
- .NET 8 SDK (for building from source)

## Build

```bash
dotnet restore
dotnet build -c Release
```

Output: `src/QuackForge.Loader/bin/Release/QuackForge.Loader.dll`

## License

MIT (see `LICENSE`).
