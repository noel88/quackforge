# Changelog

All notable changes to QuackForge are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), versioning follows [SemVer](https://semver.org/).

## [0.1.0-alpha.1] — 2026-04-22

First tagged alpha. 데이터 파이프라인 + 인프라 + Mac/VM 양쪽 end-to-end 로드 검증 완료. 실제 게임 내 아이템 스폰/제작은 AssetBundle 경로(#65) 후속.

### Added
- **`QuackForge.Core`** — 공통 인프라 모듈.
  - `QfLogger` 모듈 태그 로거 (BepInEx `ManualLogSource` 래퍼).
  - `QfConfig` BepInEx `ConfigFile` 래퍼 + typed `Bind<T>`.
  - `QfEventBus` 타입 기반 pub/sub (thread-safe).
  - `QfSaveContext` in-memory 키-값 저장소 (Phase 2 에서 사이드카 JSON 로 확장 예정).
  - `QfCore` 컴포지션 루트 싱글턴.
- **`QuackForge.Data`** — 데이터 스키마 + 로더.
  - `WeaponDefinition` + `WeaponRegistry` (embedded JSON, System.Text.Json).
  - `ArmorDefinition` + `ArmorRegistry`.
  - `BlueprintDefinition` + `BlueprintRegistry` (해금 상태 persist + 이벤트 발행).
- **10종 무기 카탈로그**: AR 3 (ar_ranger / ar_duckstorm / ar_silencer_vx), SMG 2 (smg_vector_custom / smg_quickdraw), Shotgun 2 (sg_scattergun / sg_duckpunch), Sniper 1 (sr_quackshot), Pistol 2 (pistol_sidekick / pistol_thunderbill).
- **5종 방어구 카탈로그**: chest_featherweight / chest_stealth_vest / chest_bulwark_plate / head_nightowl_helm / backpack_scavenger. `movementPenalty` 음수 허용으로 민첩 빌드 차별화.
- **10종 블루프린트 카탈로그**: 각 무기 1:1 대응. Lv 1-22 구간, 보스 드랍 + 챌린지 guaranteed 조합, `consumeOnUse=true` 1개 포함.
- **Mac 런처 스크립트**: `install-bepinex-mac.sh`, `run-duckov-mac.sh`, `deploy-mac.sh`. Apple Silicon arm64 → Rosetta x86_64 체인 + DYLD SIP 우회 + Steam DRM (`steam_appid.txt`) 자동 처리.
- **Windows VM 런처 스크립트**: `install-bepinex-vm.ps1`, `run-duckov-vm.ps1`, `link-plugin-vm.ps1`. Parallels Shared Folders 기반 Mac↔VM 심볼릭 링크 파이프라인.
- **문서**: `docs/official-mod-api-analysis.md`, `docs/reverse-engineering/weapons.md`, `docs/vm-setup-guide.md`, `docs/phase1-qa-report.md`.

### Changed
- **BepInEx**: PRD 초판 `5.4.21 unix_x64` → 런타임 `5.4.23.5 macos_universal`. Apple Silicon arm64 호환성 확보.
- **Unity 타겟**: 2022.3.5f1 (PRD 초판) → 실 게임 2022.3.62f2 확인.

### Infrastructure
- 솔루션 3-프로젝트 구성: `QuackForge.Loader` (BepInEx 엔트리) + `QuackForge.Core` + `QuackForge.Data`. netstandard2.1 공통.
- GitHub Issues 기반 Phase 0/1 전 작업 추적 완료 (#1-#18 closed).

### Known Issues / Deferred
- **AssetBundle 경로 미확립** (#65). 실 게임의 `CraftingFormulaCollection` (ScriptableObject) + Item Prefab 병합 경로는 Unity Editor 기반 모드 빌드가 필요. Part A (공식 Mod API) 실증 후속 sprint.
- **게임 내 콘솔 스폰/워크벤치 제작 미검증**. 데이터 파이프라인 단계까지만 이번 alpha 에 포함.
- 게임 종료 직후 `Index was out of range` 콘솔 에러 — `QuackForge unloaded` 이후 발생, 게임 내부 셧다운 이슈로 판단. 모니터링 대상.

### Migration from v0.0.1 dev
이 릴리즈 전에는 태그/리리즈 없음. 최초 태그.

[0.1.0-alpha.1]: https://github.com/noel88/quackforge/releases/tag/v0.1.0-alpha.1
