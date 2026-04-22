# 공식 Mod 시스템 역공학 (Duckov.Modding.*)

**작성일**: 2026-04-22
**대상**: Duckov v2.2.0, `TeamSoda.Duckov.Core.dll`
**도구**: `ilspycmd 10.0.0.8330`

> 🎯 **핵심 갱신**: Phase 1 기획 시 가정한 "공식 Part A = AssetBundle 기반" 이 **부정확**. 실제 공식 모드는 **C# DLL + info.ini** 조합이며, AssetBundle 은 DLL 내부에서 선택적으로 사용할 수 있는 도구일 뿐. 이 발견은 #65 재스코프 근거.

## 1. 모드 폴더 규약

### 경로

| OS | 경로 |
|---|---|
| macOS | `<Duckov.app>/Contents/Resources/Data/Mods/<modName>/` |
| Windows | `<Duckov 설치 경로>/Duckov_Data/Mods/<modName>/` |

코드 근거: `ModManager.DefaultModFolderPath => Path.Combine(Application.dataPath, "Mods")`.

`Application.dataPath` 는 Unity 런타임 상수 — 빌드된 게임의 Data 폴더.

### 폴더 레이아웃

```
Mods/
└── quackforge/                  # ← 모드 이름 (폴더명 = info.ini의 name 과 일치해야 함)
    ├── info.ini                 # 필수 메타
    ├── quackforge.dll           # 필수 — <modName>.dll 명명 규약
    └── preview.png              # 선택 (썸네일, 최대 256x256 추정)
```

Steam Workshop 구독분은 `steamapps/workshop/content/3167020/<publishedFileId>/` 에 동일 구조.

## 2. info.ini 포맷

단순한 키=값 텍스트. `[Section]` 헤더는 파싱 시 무시 (단, 줄 트리밍 → `[` 로 시작 안 해야 키로 인식).

```ini
name = quackforge
displayName = QuackForge
description = Level-based progression + endgame challenge content.
version = 0.2.0
tags = progression,challenge,weapons
publishedFileId = 0
```

### 키 규약

| 키 | 필수 | 타입 | 비고 |
|---|---|---|---|
| `name` | ✅ | string | 폴더명·DLL 파일명·클래스 네임스페이스 prefix 와 일치. 누락 시 로드 실패 |
| `displayName` | ✅ | string | UI 표시명. 누락 시 `LogError` + name 으로 폴백 |
| `description` | ✅ | string | Regex.Unescape 처리. `\n` 같은 이스케이프 시퀀스 지원. 누락 시 `LogError` + "?" |
| `publishedFileId` | 선택 | ulong | Steam Workshop 연동. 0 이면 로컬 모드 |
| `version` | 선택 | string | UI 노출 문자열. "?" 기본 |
| `tags` | 선택 | string | 쉼표 분리 추정. 분류·필터링 |

### description 이스케이프 예

`\n` → 줄바꿈, `\t` → 탭. 기본 Regex 지원 문자열 이스케이프.

## 3. DLL 엔트리포인트 규약

### 기대 구조

```csharp
// quackforge.dll 에 반드시 포함되어야 함
namespace quackforge
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        protected override void OnAfterSetup() { /* 초기화 */ }
        protected override void OnBeforeDeactivate() { /* 정리 */ }
    }
}
```

- **타입 이름**: `<name>.ModBehaviour` (점 구분 완전 이름). name 이 `quackforge` 면 타입은 `quackforge.ModBehaviour`.
- **상속**: `Duckov.Modding.ModBehaviour` (abstract, MonoBehaviour 기반).
- **로드 방식**: `Assembly.LoadFrom(dllPath).GetType(...)` → `GameObject.AddComponent(type)` → `ModBehaviour.Setup(...)` 호출.

### `ModBehaviour` 베이스 (추출)

```csharp
public abstract class ModBehaviour : MonoBehaviour
{
    public ModManager master { get; private set; }
    public ModInfo info { get; private set; }

    public void Setup(ModManager master, ModInfo info) { ... }        // ModManager 가 호출
    public void NotifyBeforeDeactivate() { OnBeforeDeactivate(); }

    protected virtual void OnAfterSetup() { }       // 서브클래스 오버라이드 지점 (부팅)
    protected virtual void OnBeforeDeactivate() { } // 서브클래스 오버라이드 지점 (종료)
}
```

Phase 2/3 스탯·레벨링 기능은 `OnAfterSetup` 에서 `Harmony.PatchAll` 호출하면 BepInEx 없이도 공식 모드 경로로 동일하게 동작 가능.

## 4. 전역 활성화 플래그

```csharp
public static bool AllowActivatingMod { get; set; }
// 내부: SavesSystem.LoadGlobal/SaveGlobal("AllowLoadingMod")
```

사용자가 인게임 UI (모드 매니저 화면) 에서 **약관 동의** 해야 true 가 됨. false 면 `ActivateMod` 전부 거부.

즉 모드 배포 시 사용자 측 행동 1회 필요: 모드 메뉴 진입 → 약관 수락.

## 5. 활성화·비활성화 라이프사이클

### 스캔 흐름
```
Rescan()
  └─ Directory.GetDirectories(Mods/)
      └─ TryProcessModFolder(path)        # info.ini 파싱 + preview.png 로드
          └─ ModInfo 생성 (dllFound = File.Exists(dllPath) 체크)
SortModInfosByPriority()
  └─ priority_<name> 저장값으로 정렬
```

### 개별 모드 활성화
```
ActivateMod(info)
  1. AllowActivatingMod 체크 (false 면 abort)
  2. 이미 활성이면 abort
  3. Assembly.LoadFrom(info.dllPath)
  4. GetType("<name>.ModBehaviour") → MonoBehaviour 상속 확인
  5. GameObject 생성 → AddComponent<ModBehaviour>
  6. modBehaviour.Setup(this, info)   → OnAfterSetup 호출
  7. activeMods[info.name] = modBehaviour
  8. OnModActivated 이벤트 + ModActive_<name> = true 저장
```

### 비활성화
```
DeactivateMod(info)
  1. activeMods[info.name] 조회
  2. NotifyBeforeDeactivate → OnBeforeDeactivate
  3. GameObject.Destroy(modBehaviour.gameObject)
  4. activeMods 에서 제거
  5. ModActive_<name> = false 저장
```

### 이벤트 API (다른 시스템이 구독 가능)

- `ModManager.OnModActivated(info, behaviour)`
- `ModManager.OnModWillBeDeactivated(info, behaviour)`
- `ModManager.OnModStatusChanged()`
- `ModManager.OnScan(modInfos)`
- `ModManager.OnReorder()`
- `ModManager.OnModLoadingFailed(path, message)`

### 우선순위

```csharp
ModManager.SetModPriority(name, int)   // priority_<name> 키로 저장
ModManager.GetModPriority(name)        // default: int.MaxValue (미지정 → 가장 뒤)
ModManager.Reorder(fromIdx, toIdx)     // 순서 교체 + RegeneratePriorities
```

## 6. Steam Workshop 연동

- `SteamWorkshopManager` 클래스 (같은 네임스페이스) 가 SteamUGC 직접 호출
- `RequestNewWorkshopItemID` / `UploadWorkshopItem` (async)
- `TryProcessModFolder(path, info, isSteamItem: true, publishedFileId)` 으로 Workshop 구독분 동일 처리
- 구독한 모드는 `steamapps/workshop/content/3167020/<id>/` 에 같은 구조로 다운로드됨

## 7. AssetBundle 은 어디에?

`ModManager` 내부에는 AssetBundle 로드 로직이 **없음**. 즉:

- **DLL 이 자체적으로** `AssetBundle.LoadFromFile(mod폴더/번들경로)` 를 호출해서 prefab/스프라이트 등을 꺼내야 함
- 권장 구조:
  ```
  Mods/quackforge/
    ├── info.ini
    ├── quackforge.dll                    # ModBehaviour + AssetBundle 로더 코드
    ├── preview.png
    └── bundles/
        ├── quackforge_mac.bundle         # Mac 플랫폼용
        └── quackforge_win.bundle         # Windows 플랫폼용
  ```
- `ModBehaviour.OnAfterSetup()` 에서 `Application.platform` 분기 후 해당 .bundle 로드 → 번들 내 prefab 인스턴스화 → 게임 시스템 (ItemAssetsCollection, CraftingFormulaCollection 등) 에 병합

**QuackForge 로 번역:**
- `QuackForge.Data.WeaponRegistry.LoadAll()` (이미 embedded JSON 로드) + AssetBundle 로부터 Prefab 참조 매칭 → 게임 시스템 등록
- `BlueprintRegistry.Unlock` → `CraftingManager.UnlockFormula` 브릿지

## 8. BepInEx vs 공식 모드 경로 — 재평가

| 항목 | BepInEx (현재) | 공식 모드 (DLL + info.ini) | 혼합 |
|---|---|---|---|
| 게임 부트 시 로드 | doorstop 주입 | `AllowActivatingMod` + 약관 후 ActivateMod | - |
| 사용자 설치 복잡도 | BepInEx 설치 필요 | Workshop 구독 1클릭 | - |
| Harmony 패칭 | ✅ | ✅ (DLL 코드가 직접 PatchAll) | - |
| UI 통합 | ❌ 인게임 모드 매니저 UI 에 안 보임 | ✅ 인게임 UI 에서 토글 | - |
| Steam Workshop 배포 | ❌ | ✅ | - |
| 여러 모드 간 우선순위 | ❌ 랜덤 | ✅ priority_ 관리 | - |
| 약관 수락 필요 | ❌ | ✅ 1회 필요 | - |
| 현 QuackForge 사용 상태 | ✅ 동작 검증됨 | ❌ 미탐색 | — |

### 전략 제안

**단기 (Phase 1-3)**: BepInEx 경로 유지. 개발 속도 우선, 기능 검증 중심.
**중기 (v0.3 이후)**: 공식 모드 경로 병행 배포. 같은 코드 베이스에서 두 엔트리포인트 빌드:
- `QuackForge.Loader.dll` (BepInEx 용, 현재)
- `quackforge.dll` (공식 모드 용, `quackforge.ModBehaviour` 래퍼)
두 엔트리 모두 공통 `QfCore.Initialize(...)` 호출하게 하면 코드 중복 최소화.

Workshop 배포로 "Subscribe 만 하면 바로 작동" UX 달성 → 알파 테스터 확장.

## 9. 당장의 실증 단계 (#65 재스코프)

1. **최소 공식 모드 1개 빌드 시도** — AssetBundle 없이:
   - `Mods/quackforge_test/info.ini` 작성
   - `quackforge_test.dll` = `Duckov.Modding.ModBehaviour` 상속 클래스 1개, `OnAfterSetup` 에서 `Debug.Log("hello from official mod")` 만
   - 게임 실행 → 약관 수락 → 활성화 → 로그 확인
2. **성공하면 AssetBundle 로 prefab 1개 로드 시도** — 기존 게임 무기 clone 참조
3. **typeID 등록 실증** — `ItemStatsSystem.ItemAssetsCollection` 에 추가 경로 확인

## 10. 남은 open questions (#65 진행 중 확인)

- Q-MOD-1: `Application.dataPath + "/Mods/"` 가 macOS .app 구조에서 정확히 어느 경로인지 실기 확인
- Q-MOD-2: Mac 에서 Mono DLL 로드 (Assembly.LoadFrom) 시 arm64/x64 제약
- Q-MOD-3: Workshop 업로드 시 번들 플랫폼별 분리 필요 여부
- Q-MOD-4: `ItemAssetsCollection` 증분 등록 메커니즘

## 11. PRD 업데이트 권고

- §3.2 Part A 정의 수정: "공식 Mod API = DLL + info.ini + (선택) AssetBundle" (기존은 "AssetBundle + 매니페스트")
- §5.4.4 워크벤치 통합: `CraftingFormulaCollection` 리스트 확장 경로 확정. Harmony 대신 직접 List 주입 가능 여부 #65 실증 후 결정
- §3.3 모듈 구조: `mod/quackforge/{name}.dll` 이 공식 엔트리포인트로 빌드되도록 장기 계획
