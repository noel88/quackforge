# 공식 Mod API 분석 (T1.1)

**작성일**: 2026-04-17
**대상 빌드**: Duckov v2.2.0, Unity 2022.3.62f2, Steam AppID 3167020
**근거**: Team Soda Steam 공지 + `TeamSoda.Duckov.Core.dll` 문자열 정적 분석 + 커뮤니티 모드 매니저 문서

> 이 문서는 **커버리지 탐색** 중심. 정확한 필드·타입·메서드 시그니처는 T1.2 (Assembly 역공학) 에서 보강.

## 1. 1차 출처

| 출처 | 링크 | 상태 |
|---|---|---|
| Steam 공지 "Mod Guide" | https://store.steampowered.com/news/app/3167020/view/617681009992796272 | 페이지는 JS 렌더 — WebFetch 로 본문 추출 실패. 수동 열람 필요 |
| Steam Workshop | https://steamcommunity.com/app/3167020/workshop/ | 92+ 모드 활동 중 |
| 커뮤니티 가이드 (NeonLightsMedia) | https://www.neonlightsmedia.com/blog/escape-from-duckov-mod-support-guide | 설치만 서술, 포맷 미기재 |
| Duckov Mod Manager (DMCK96) | https://github.com/DMCK96/Duckov-Mod-Manager | Workshop ID 기반 폴더 스캔, 포맷 세부 미공개 |
| 개발자 연락 폼 | https://usersurvey.wjx.cn/vm/OTlyio5.aspx | 공식 도움 창구 |

> **확정된 사실**: 게임은 `Duckov_Data/Mods/` 폴더 아래 모드를 로드. Steam Workshop 구독 시 자동 다운로드 + 인게임 Mods 메뉴에서 활성화. 공식 개발자 지원 채널 존재.

## 2. 어셈블리 정적 분석

대상: `Duckov.app/Contents/Resources/Data/Managed/TeamSoda.Duckov.Core.dll` (Mono, .NET Standard 2.1, x86_64 슬라이스 기준 로드 확인)

### 2.1 Namespace 구조

```
Duckov.Modding
├── ModManager                 # 메인 로더 (Assets/SubProjects/Modding/Scripts/ModManager.cs)
├── ModBehaviour               # 런타임 스크립트 베이스 (MonoBehaviour 추정)
├── ModInfo                    # 모드 메타데이터 컨테이너
└── SteamWorkshopManager       # Workshop Upload/Query (SteamUGC 바인딩)

Duckov.Modding.UI
├── ModManagerUI               # 인게임 메인 UI (BeginUpload 코루틴)
├── ModChangedWarning          # 모드 변경 경고
├── ModEntry                   # 리스트 아이템
└── ModPathButton              # 모드 폴더 열기 버튼
```

### 2.2 추출한 멤버 시그니처 (부분)

**`ModManager`**
- `public static ModManager get_ModManager()` — 싱글턴 접근자
- `modInfos` — 필드 (`List<ModInfo>` 추정)
- `SortModInfosByPriority` — 우선순위 정렬 (로드 오더 지원)
- `GetActiveModBehaviour` — 활성화된 모드의 `ModBehaviour` 조회

**`ModInfo`**
- `author`, `authorName`, `Author_Contact`, `Author_Name` — 작성자 메타
- `description`
- `previewSprite`, `defaultPreviewTexture` — 썸네일 (Sprite + Texture 둘 다)

**`SteamWorkshopManager` (SteamUGC 직접 호출)**
- `RequestNewWorkshopItemID` (async)
- `UploadWorkshopItem` (async)
- `OnSteamUGCQueryCompleted`
- `ugcDetailsCache`
- `CreateQueryUGCDetailsRequest`, `SendQueryUGCRequest`, `ReleaseQueryUGCRequest`
- `m_bUserNeedsToAcceptWorkshopLegalAgreement` — 약관 동의 플래그

### 2.3 자산 로딩 힌트

- `UnityEngine.JSONSerializeModule` 참조 → **데이터 파일은 JSON 기반** 가능성 높음
- `LoadFrom`, `LoadFromData` 메서드명 → AssetBundle + JSON 혼합 로더 추정
- `ModBehaviour` 존재 → C# 스크립트 내장 가능성 (AssetBundle 에 컴파일된 DLL 포함)

### 2.4 무엇을 아직 모르는가

- 구성 매니페스트 포맷
  - 커뮤니티 문서는 `info.ini` 언급 → 어셈블리 문자열에서는 미발견 (다른 이름일 가능성)
  - 후보: `mod.json`, `manifest.json`, 혹은 어셈블리 번들 내부 메타
- AssetBundle 타겟 플랫폼
  - Mac arm64/x64, Windows x64, Linux x64 별 별도 번들 필요한지
- C# 모드 스크립트 컴파일 환경
  - Unity Editor 필요 여부, 참조 가능한 API 범위
- 데이터 오버라이드 스코프
  - 기존 무기/방어구 스탯 덮어쓰기 vs 신규 ID 추가만 가능

## 3. 공식 API vs BepInEx — 커버리지 매트릭스

> PRD §3.2 의 초기 결정 유지. 아래는 이번 분석으로 확정 강화된 부분만 표기.

| 기능 | 공식 Mod API | BepInEx + Harmony | 채택 경로 |
|---|---|---|---|
| 신규 무기 데이터 추가 | ✅ (AssetBundle + JSON) | ✅ | **Part A (공식)** |
| 신규 방어구 데이터 추가 | ✅ | ✅ | **Part A (공식)** |
| 도면(Blueprint) 레시피 | ✅ 추정 (Craft/Recipe 구조 Part A로 편입 가능) | ✅ | **Part A** |
| 아이콘·프리뷰 | ✅ (`ModInfo.previewSprite`) | ✅ | **Part A** |
| 기존 스탯 실시간 변경 | ⚠️ 데이터 덮어쓰기로 가능할 수도 (미검증) | ✅ Harmony | **Part B** |
| 레벨링 시스템 | ❌ 런타임 시스템 추가 불가 | ✅ | **Part B** |
| 챌린지 모드 (GameMode) | ❌ | ✅ (Phase 4, 고난도) | **Part B** |
| UI 추가 (Character 탭 등) | ❌ Unity UI 재사용 한계 | ✅ | **Part B** |
| Steam Workshop 배포 | ✅ (`SteamWorkshopManager`) | ❌ | **Part A 단독 배포 가능** |
| 패치 안정성 (게임 업데이트) | ✅ 공식 계약 | ⚠️ 버전 의존 | 경로별 위험 분산 |

### 3.1 확정 사항

- **Part A**: `Data 모듈` (무기 10종, 방어구 5종, 블루프린트) 는 공식 Mod API 번들로 출시. Workshop + Nexus 동시 배포 가능.
- **Part B**: `Progression` (레벨/스탯), `Challenge` 모드, `UI` 확장, Harmony 패치는 BepInEx 단독.
- 두 파트 **느슨하게 결합**: Part B 만 설치해도 레벨링은 기본 아이템으로 동작.

## 4. R1-2 재평가

> PRD R1-2: "공식 Mod API가 무기 추가 미지원 → BepInEx 단독 경로로 피벗"

- **위험 낮아짐**: `Duckov.Modding.ModManager` + Workshop upload 경로 확인, 커뮤니티 모드 92+ 실사례 존재 → 무기/방어구 추가는 Part A 로 90%+ 확률 가능.
- **잔여 위험**: AssetBundle 빌드 체인(Unity Editor 필요) 이 Mac 에서 얼마나 순조로울지 미검증.
- **완화**: T1.2 (Assembly 역공학) + 작은 테스트 모드 1개 (더미 무기) 로 end-to-end 검증하는 단계를 **T1.3 앞쪽에 추가** 권고.

## 5. T1.2 진입 전 체크리스트

1. [ ] Steam 공지 Mod Guide 본문 수동 열람 (`gh` 로 접근 불가, 브라우저에서 복사)
2. [ ] Workshop 모드 1개 구독 → `$STEAM_LIBRARY/steamapps/workshop/content/3167020/<id>/` 내부 구조 관찰
3. [ ] Rider 로 `TeamSoda.Duckov.Core.dll` 디컴파일 → `ModManager.LoadFrom(path)` 본문 정독
4. [ ] 매니페스트 파일명 및 스키마 확정 (`info.ini` vs `mod.json` vs 어셈블리 메타)
5. [ ] AssetBundle 빌드 워크플로우 검토 (Unity Editor 버전, 타겟 플랫폼)

## 6. 결론

- **Part A (공식) + Part B (BepInEx) 혼합 전략 유효**. PRD §3.2 결정 변경 없음.
- 공식 API는 **데이터·자산·Workshop 배포** 전담, BepInEx 는 **런타임 시스템 확장** 전담.
- 다음 단계는 **T1.2 역공학** 에서 `ModManager.LoadFrom` 내부와 매니페스트 포맷 확정.
