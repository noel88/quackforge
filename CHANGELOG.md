# Changelog

All notable changes to QuackForge are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), versioning follows [SemVer](https://semver.org/).

## [0.3.0-alpha.1] — 2026-04-29

Phase 3 closeout. UI 통합 + 9종 Harmony 패치 + 설정 확장 + Modifier 시스템 마이그레이션. v0.2.0 의 MaxHealth percent-damage 부작용 (#35 IT2 발견) 해결.

### Added
- **`docs/reverse-engineering/ui.md`** (#29) — Duckov 의 UI 스택 (UGUI / View / ViewTabs) RE.
- **`docs/stat-balance.md`** (#30) — 5종 스탯 효과량 + 게임 stat 매핑.
- **9종 Harmony Postfix → Modifier 시스템 마이그레이션** (#31, #88):
  - VIT (`MaxHealth`) / STR (`MaxWeight`, `MeleeDamageMultiplier`) / AGI (`MaxStamina`, `Moveability`) / PRE (`RecoilControl`, `GunScatterMultiplier`) / SUR (`HealGain`, `EnergyCost`, `WaterCost`)
  - 초기 v0.2.0 패턴(개별 게터 Postfix) 9개를 게임의 `ItemStatsSystem.Modifier` 정식 등록으로 통일 (#88).
  - PercentageMultiply 계열에 floor 가드 (50pt 시 음수 방지).
- **`StatModifierBinder`** (#88): MainCharacter 변경 자동 감지 + 자동 unbind/rebind. raid 진입/탈출/캐릭터 교체 케이스 처리.
- **`CharacterPanel` UGUI** (#83): 코드로 빌드한 5-stat 분배 UI. host GameObject `HideAndDontSave` 보호. 토글 키 `[Debug] CharacterPanelKey` (default U).
- **`LevelUpNotification`** UnityUI (#85): 화면 중앙 "LEVEL UP!" + 우상단 "+N pts" 1.5초 + 1.0초 페이드. cursor lock 비간섭 (raycastTarget=false).
- **`CharacterTabInjector`** (#87): 게임 ViewTabs 에 "Char" 탭 동적 추가 (Harmony-free, clone-existing-entry 패턴). Button 강제 추가 fallback.
- **`StatManager.Deallocate` / `ResetAllocation`** (#83, #84): [-] 버튼 + 리스팩 정책 가드 (`AllowFreeRespec`).
- **`ProgressionSettings`** (#84): 진보 관련 모든 값 한 객체로 + 10 ConfigEntry (`PointsPerLevel`, `MaxPointsPerStat`, `AllowFreeRespec`, `HpPerVit` + 효과량 6종).
- **`DamageActionPatch`** (#88): `Duckov.Effects.DamageAction.OnTriggeredPositive` Prefix + Finalizer (예외 안전 ThreadStatic flag).

### Fixed
- **MaxHealth percent-damage debuff 부작용** (#35 IT2):
  - v0.2.0 의 `Health.MaxHealth` getter Postfix 가 우리 보너스 합산 → 게임의 `DamageAction.OnTriggeredPositive` 의 percent-damage 계산 (`damageValue = percent × MaxHealth × layers`) 까지 증폭 → raid 중 출혈/감염 디버프 효과 가속.
  - **Fix**: Modifier 시스템으로 통합 (보너스는 `stat.Value` 에 정상 반영) + DamageAction 스코프 동안 ThreadStatic flag 켜고 `Health.MaxHealth` getter 가 우리 보너스 빼서 반환 (1 floor 안전).
  - IT5 라운드에서 `DamageAction Prefix #N` 호출 + `VitBonus` 정확 우회 검증.
- **Char 탭 click 미작동** (#35 IT1):
  - 게임 `ViewTabDisplayEntry` prefab 이 IPointerClickHandler 별도 컴포넌트를 쓰는 케이스 → `GetComponentInChildren<Button>` 못 찾음.
  - **Fix** (#88 IT2): clone 에 Button 없으면 강제로 root 에 Button + 투명 Image (raycastTarget) 추가.
- **DebugOverlay IMGUI cursor lock 충돌** (#77):
  - `OverlayEnabled` default `true` → `false` 로 변경 (#85). v0.4 에서 완전 제거 예정.
  - LevelUpNotification + CharacterPanel 이 IMGUI overlay 의 토스트/스탯 표시 역할 흡수.

### Changed
- **`QuackForge.Loader.csproj`**: `UnityEngine.UI` + `TeamSoda.Duckov.Core` 직접 참조 (UGUI + ViewTabs 인젝트용).
- **`QfProgression.Initialize` 시그니처**: `(int, bool)` → `(ProgressionSettings)` (null 이면 기본값) (#84).
- **9개 stat patch 파일 → 1 Binder + 2 patches** (#88). 코드량 약 50% 감소.

### Errata (v0.2.0-alpha.1)
- v0.2.0 의 known issue "VIT 분배 시 percent damage 비례 증폭" 가 사실은 **Health.MaxHealth getter 가산 패턴의 자연 부작용** 이었음. v0.3.0 에서 Modifier 시스템 마이그레이션으로 해결.

### Known Issues / Deferred
- **1/10 stat 등록 실패** (`Stats.Binder] bound to MainCharacter (9/10 stats)` 로그). MeleeDamageMultiplier 추정 — 게임이 무기 stat 으로 분리해 character item 에 없음. 무기 들었을 때만 적용. 별도 PR.
- **무기/방어구 게임 내 spawn 미구현**: Phase 3 까지 데이터/UI/스탯만. AssetBundle 경로 (#65) 후속.
- **Character UI 인디케이터 동기화 안 됨**: 우리 panel 이 `ManagedUIElement` 비상속 → 다른 탭과 active 상태 동기화 X. 후속.
- **Windows VM cross-platform 미검증**: PR #88 의 fix 가 BepInEx win_x64 에서도 동일 동작하는지 별도 검증 필요.

### Migration from v0.2.0-alpha.1
- cfg 새 항목 (`[Progression]` 4개 + `[Effects]` 7개 + `[Debug] CharacterPanelKey` / `LevelUpNotificationEnabled`) 자동 생성 (BepInEx default).
- `[Debug] OverlayEnabled` 기존 값 유지 (default `false` 로 변경됐지만 cfg 에 이미 있는 사용자는 그대로). 권장: 명시적으로 `false`.
- 사이드카 `quackforge.json` 포맷 호환. VIT 분배 그대로 복원.

[0.3.0-alpha.1]: https://github.com/noel88/quackforge/releases/tag/v0.3.0-alpha.1

## [0.2.0-alpha.1] — 2026-04-29

Phase 2 closeout. Hybrid leveling 체인이 인게임에서 처음으로 동작. v0.1.0 의 **잘못 진단된 known issue** ("게임 종료 직후 unloaded") 가 사실은 부팅 단계의 Plugin GameObject sweep 이었음을 발견하고 fix.

### Added
- **`QuackForge.Progression`** 모듈 (#19 #20 #21).
  - `StatType` enum (VIT/STR/AGI/PRE/SUR), `StatManager` (포인트 적립/분배/persist + 이벤트), `XpSubscriber` (`Duckov.EXPManager.onLevelChanged` 구독), `QfProgression` 컴포지션 루트.
  - Hybrid 전략: XP/레벨 계산은 게임에 위임, 레벨업 이벤트만 우리가 활용 (PRD §6.0).
  - `pointsPerLevel = 1`, `autoAllocateVit = true` (MVP — Phase 3 Character UI 진입 시 false 전환).
- **HP 스탯 Harmony 패치** (#24): `Health.MaxHealth` Postfix — MainCharacter 한정 VIT × +10 가산. 인게임 +10 시각 컨펌.
- **`QfRuntime` MonoBehaviour** — F9 키바인드 + 주기적 사이드카 flush. Plugin GameObject 와 분리된 persistent host 에 부착.
- **사이드카 IO `quackforge.json`** (#23): atomic `tmp → target` rename, 15s 주기 flush, 게임 세이브와 분리. Newtonsoft.Json 사용.
- **`DebugOverlay` IMGUI** (#25): 우상단 스탯 요약 + 3초 페이드 토스트. ⚠️ #77 (cursor lock 충돌) 로 cfg `OverlayEnabled = true` 기본이지만 raid 중엔 끌 것 권고.
- **`AddXp` 디버그 키바인드** (#26): 기본 F9, 1000 XP/누름. `EXPManager.AddExp` 래핑.
- **공식 Mod 경로 (`<app>/Contents/Mods/`) 실증** (#65): `QuackForge.TestMod` 최소 모드로 BepInEx 와 공존 가능 확인. v0.3 부터 `quackforge.dll` 공식 래퍼 후보.
- **Unity 프로젝트 스캐폴드** (#65): `unity-project/` URP 2022.3.62f3 + `Assets/Editor/BuildAssetBundles.cs` + `scripts/build-mod.sh` (헤드리스). 실제 prefab 빌드는 후속.
- **문서**: `docs/xp-balance.md` (XP/Level/Stat 곡선 + 정책), `docs/reverse-engineering/mod-system.md` (#74), `docs/assetbundle-workflow.md`.

### Fixed (인게임 통합 — v0.1.0 결함)
v0.1.0 가 인게임에서 한 번도 작동한 적 없었음 (#27 / PR #78, 9 라운드 진단):

- **Plugin GameObject 가 부팅 sweep 으로 disable**. Duckov 가 BepInEx host GameObject 에 `SetActive(false)` 처리 → Update / sceneLoaded / 키바인드 모두 죽음. `DontDestroyOnLoad` 도 활성 scene 없는 시점이라 무효.
  - **Fix**: 별도 host GameObject + `HideFlags.HideAndDontSave` + `DontDestroyOnLoad`. Plugin 은 부트스트랩만, 매 프레임 로직은 `QfRuntime` 으로 분리.
- **Harmony 패치 OnDestroy 시 unpatch**. Plugin GO 가 죽으면 패치도 사라져 HP 가산 효과 소실.
  - **Fix**: `_harmony` static, `OnDestroy` 의 `UnpatchSelf` 호출 제거.
- **`System.Text.Json.Utf8JsonWriter` Mono+Rosetta 환경에서 VTable setup 실패**. 사이드카 직렬화 시 `TypeLoadException` → 후속 핸들러 체인 끊김.
  - **Fix**: 게임 Managed/ 의 `Newtonsoft.Json.dll` 직접 참조, `JsonConvert` 로 교체 (Core 만, Data 의 boot-time deserialize 는 `Utf8JsonReader` 만 써서 영향 없음).
- **XpSubscriber.OnLevelChanged unhandled exception 위험**. 콜백 throw 시 `EXPManager.onLevelChanged` invocation list 가 끊겨 다른 구독자(게임 자체 UI 등)도 같이 죽을 수 있음.
  - **Fix**: try/catch + 로그.

### Changed
- **`QuackForge.Core` 의존성**: `System.Text.Json 8.0.5` PackageReference 제거 → `Newtonsoft.Json` (게임 제공) 직접 참조. (`QuackForge.Data` 는 boot-time deserialize 만 하므로 그대로 유지.)
- **솔루션 5-프로젝트 구성**: `Loader` + `Core` + `Data` + `Progression` + `TestMod`.

### Errata (v0.1.0 known issue 정정)
v0.1.0 CHANGELOG 의 known issue 항목 중 **"게임 종료 직후 `Index was out of range`"** 는 **잘못된 진단**:
- 사실 `QuackForge unloaded` 는 게임 부팅 막바지에 BepInEx Plugin GO 가 disable 되면서 발생하던 것 (종료 시점 아님). v0.2.0 에서 host 분리 fix 로 해소됐지만 BepInEx Plugin GO 자체는 여전히 disable 됨 (의도된 동작).
- `Index was out of range` 콘솔 에러는 게임 자체의 raid 초기화 "Setting up pet" 단계 버그. 우리 모드와 무관, 게임 패치 전까지 무시 가능.

### Known Issues / Deferred
- **DebugOverlay IMGUI 가 raid 중 cursor lock 을 깸** (#77). 임시 회피: cfg `[Debug] OverlayEnabled = false`. Phase 3 #33 의 정식 UnityUI 로 deprecate 예정.
- **무기 / 방어구 게임 내 spawn 미구현**. 데이터 카탈로그(메모리 등록) + 레벨 시스템까지만. 실제 prefab/AssetBundle 등록은 Phase 3 후속.
- **Windows VM cross-platform 검증 미완**. PR #78 의 fix 가 BepInEx win_x64 에서도 같은 boot sweep / VTable 패턴이 나는지 별도 확인 필요.

### Migration from v0.1.0-alpha.1
- `BepInEx/quackforge.json` 포맷은 v0.1.0 과 동일 (있을 수 없지만, v0.1.0 이 인게임 동작 안 했으므로 사실상 신규 파일).
- cfg `[Debug] OverlayEnabled` 기본값 `true` 유지. raid 진입 시 직접 `false` 로 끌 것 권고.

[0.2.0-alpha.1]: https://github.com/noel88/quackforge/releases/tag/v0.2.0-alpha.1

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
