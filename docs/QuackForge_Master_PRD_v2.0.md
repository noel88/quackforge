# QuackForge — Master PRD v2.0

> 덕코프(Escape from Duckov)에 레벨 기반 성장 시스템과 엔드게임 챌린지 콘텐츠를 추가하는 모드 프로젝트의 통합 PRD

**코드네임**: QuackForge
**버전**: v2.0 (전 Phase 통합)
**작성일**: 2026-04-17
**대상 게임**: Escape from Duckov (Team Soda, Unity 2022.3.5f1)
**개발자**: N (Solo, Backend 7y)
**개발 환경**: Mac M1 Studio (메인) + Windows 11 VM (QA) + Windows PC (릴리즈 빌드)

---

## 목차

- [Part I. 프로젝트 개요](#part-i-프로젝트-개요)
- [Part II. 개발 환경 설계](#part-ii-개발-환경-설계)
- [Part III. 기술 스택 & 아키텍처](#part-iii-기술-스택--아키텍처)
- [Part IV. Phase 0 — Bootstrap & Hello World](#part-iv-phase-0--bootstrap--hello-world)
- [Part V. Phase 1 — 무기 & 방어구 데이터 시스템](#part-v-phase-1--무기--방어구-데이터-시스템)
- [Part VI. Phase 2 — 레벨링 시스템 MVP](#part-vi-phase-2--레벨링-시스템-mvp)
- [Part VII. Phase 3 — 레벨링 시스템 확장](#part-vii-phase-3--레벨링-시스템-확장)
- [Part VIII. Phase 4 — 챌린지 모드](#part-viii-phase-4--챌린지-모드)
- [Part IX. Phase 5 — 배포 & 안정화](#part-ix-phase-5--배포--안정화)
- [Part X. 리스크 레지스터](#part-x-리스크-레지스터)
- [Part XI. 부록](#part-xi-부록)

---

# Part I. 프로젝트 개요

## 1.1 One-liner

> 덕코프에 **레벨 기반 성장 시스템**과 **엔드게임 챌린지 콘텐츠**를 추가해, 농장마을 이후 떨어지는 게임의 후반 긴장감을 되살린다.

## 1.2 Problem Statement

덕코프는 초반 탐험·파밍 루프는 훌륭하지만, 나무위키 및 커뮤니티 평가에서 공통적으로 지적되는 결함이 있다:

> "파밍을 하면서 캐릭터를 성장시키는 재미가 농장마을까지만 이어지고, 그 이후부터는 더 상위 아이템이 나오는 것도 아니고 퀘스트도 거의 없어져 다소 게임이 지루해진다."

이를 분해하면:

- **P1**: 현재 레벨은 표시되지만 **레벨업 시 선택권·보상이 없어** 성장 동기가 약함
- **P2**: **엔드게임 콘텐츠 부재**. 종결 장비를 갖춘 유저가 "도전"할 무대가 없음
- **P3**: 보상 풀(무기/방어구) 다양성이 한정적이라 반복 플레이 유인이 낮음

## 1.3 Vision & Core Hypotheses

| ID | 가설 | 검증 방법 |
|----|------|----------|
| H1 | 레벨업 시 스탯 투자 선택지가 있으면 **캐릭터 빌드의 다양성**이 생겨 반복 플레이 동기가 회복된다 | Beta 테스터 5명 중 3명 이상이 "빌드를 바꿔가며 다시 플레이함" 피드백 |
| H2 | 보스 러시 챌린지 모드가 있으면, 종결 장비를 갖춘 유저에게 **실력 검증 무대**가 된다 | Nexus 다운로드 대비 챌린지 클리어 리포트 비율 20% 이상 |
| H3 | 신규 무기/방어구는 위 두 시스템의 **보상 풀**로 기능해 피드백 루프를 완성한다 | 레벨링만 설치한 유저 vs 전체 설치 유저의 평균 플레이 시간 차이 |

## 1.4 Non-Goals

명시적으로 스코프에서 제외:

- 멀티플레이 지원 (게임 자체가 싱글 전용)
- PvP 밸런스 (대상 아님)
- 완전히 새로운 맵 제작 (기존 맵 재활용 전제)
- Mac 네이티브 배포 지원 (유저의 95%가 Windows)
- 3D 모델·사운드·애니메이션 신규 제작 (v1.0에서는 기존 에셋만 재활용)
- 타 모드와의 딥 인티그레이션 (느슨한 공존만 보장)
- 리더보드·랭킹 시스템 (싱글 게임 특성상 불필요)

## 1.5 타겟 유저 & 배포 전략

### Primary Persona: "후반 권태 유저"

- 덕코프 플레이타임 **100시간+**
- 익스트랙션 슈터 경험자 (타르코프·제로시버트 플레이 이력)
- 덕코프의 얕은 성장 커브에 **명시적 아쉬움**을 느낌
- Nexus Mods / Steam Workshop 사용 경험 있음
- 영어 또는 한국어 구사

### Secondary Persona: "빌드 실험가"

- RPG의 스탯 분배·빌드 구성을 즐기는 유저
- 타르코프의 하드코어 영구사망 모드에 관심
- 커뮤니티 포럼·리딧에 빌드 공유를 즐김

### 배포 채널 전략 (단계적)

| 단계 | 채널 | 타겟 | 배포 시점 |
|------|------|------|----------|
| Alpha | 본인 + 지인 2~3명 | 핵심 버그 검증 | Phase 3 중반 |
| Beta | Nexus Mods (비공개/한정) | 50~100명 얼리 어답터 | Phase 4 완료 |
| GA v1.0 | Nexus Mods 공개 | 일반 유저 | Phase 5 완료 |
| GA v1.1+ | Steam Workshop 확장 | 비(非) Nexus 유저 | GA 후 1주 안정화 후 |

## 1.6 성공 지표

### 필수 지표 (학습 목적)

- [ ] C# / Unity / BepInEx 실전 경험 확보 (Phase 3까지 도달)
- [ ] 역공학 기반 모드 개발 파이프라인 구축 (디컴파일 → Harmony 패치 → QA)
- [ ] 1인 서비스 가능성 검증 (Solo 개발로 GA 도달)

### 선택 지표 (프로젝트 성공)

| 지표 | Alpha | Beta | GA v1.0 | GA v1.3 |
|------|-------|------|---------|---------|
| Nexus 다운로드 | — | 100+ | 1,000+ | 5,000+ |
| 별점 평균 | — | 4.0+ | 4.2+ | 4.3+ |
| 크리티컬 버그 | ≤5 | ≤2 | 0 | 0 |
| Workshop 구독 | — | — | — | 2,000+ |

---

# Part II. 개발 환경 설계

## 2.1 개발 환경 아키텍처 (B안)

3단 구성으로 로컬 개발 → QA → 릴리즈 파이프라인을 분리. Spring 백엔드의 **dev → staging → prod** 개념과 동일.

```
┌─────────────────┐    빌드·일상 테스트     ┌──────────────────┐
│ Mac M1 Studio   │  ─────────────────▶   │ Mac 덕코프         │
│ (메인 개발)      │                       │ (빠른 iteration)  │
└────────┬────────┘                       └──────────────────┘
         │ 공유 폴더 (VirtFS)
         ▼
┌─────────────────┐    마일스톤 QA         ┌──────────────────┐
│ UTM Windows 11  │  ─────────────────▶   │ VM 덕코프         │
│ (가상화 QA)      │                       │ (크로스 플랫폼 검증)│
└─────────────────┘                       └──────────────────┘
         │ Git push
         ▼
┌─────────────────┐    릴리즈 빌드         ┌──────────────────┐
│ Windows 실기 PC  │  ─────────────────▶   │ Nexus/Workshop    │
│ (최종 빌드)      │                       │ 배포 산출물        │
└─────────────────┘                       └──────────────────┘
```

### 왜 이 구성인가

**대안 비교**:

| 옵션 | 장점 | 단점 | 결정 |
|------|------|------|------|
| Mac 단독 | 환경 단순, 빠름 | Windows 유저 테스트 불가 (유저의 95%) | ❌ |
| Windows 실기 단독 | 환경 일관성 | Mac이 메인 머신인데 모든 개발을 실기 PC로 강제 | ❌ |
| **Mac + VM + 실기 (B안)** | 일상성·안정성·품질 3박자 | 초기 셋업 복잡 | ✅ |

## 2.2 Mac 개발 환경

### 설치 요구사항

| 도구 | 버전 | 설치 명령 | 용도 |
|------|------|---------|------|
| Homebrew | latest | [공식 스크립트] | 패키지 매니저 |
| .NET SDK | 8.0 LTS | `brew install --cask dotnet-sdk` | C# 컴파일 |
| JetBrains Rider | 2024.x+ | `brew install --cask rider` | IDE + 디컴파일러 |
| Git | 2.40+ | (이미 설치됨) | 버전 관리 |
| fswatch | latest | `brew install fswatch` | watch 모드 자동 빌드 |
| Steam + 덕코프 | — | (Steam에서 설치) | Mac 네이티브 실행 |
| BepInEx 5 (unix x64) | 5.4.21 | GitHub Releases | 모드 로더 |

### Rider vs VS Code

**Rider 선택 이유**:
1. **내장 디컴파일러**가 Phase 2부터 핵심 도구 (dnSpy Mac 버전 부재)
2. M1 네이티브 바이너리 제공, 성능 우수
3. Unity 프로젝트 통합 (Phase 4에서 유용)

**VS Code 대안**: 무료. 단, 디컴파일은 별도 CLI 도구(ilspycmd) 필요.

> **결정**: Rider 우선. 개인 라이선스 구매 또는 JetBrains OSS/학생 라이선스 확인.

### 덕코프 경로 (Mac)

```
~/Library/Application Support/Steam/steamapps/common/Escape From Duckov/
```

### BepInEx 설치 절차 (Mac)

```bash
DUCKOV="$HOME/Library/Application Support/Steam/steamapps/common/Escape From Duckov"
cd "$DUCKOV"

# 1. BepInEx_unix_x64_5.4.21.zip 다운로드 (GitHub Releases)
unzip ~/Downloads/BepInEx_unix_x64_5.4.21.zip

# 2. 실행 권한 부여
chmod +x run_bepinex.sh

# 3. macOS 차단 플래그 제거
xattr -cr "$DUCKOV"

# 4. 첫 실행 (BepInEx 폴더 자동 생성 트리거)
./run_bepinex.sh
```

확인: `BepInEx/LogOutput.log` 에 BepInEx 로드 메시지가 보여야 함.

## 2.3 Windows VM QA 환경

### VM 소프트웨어 선택 우선순위

| 순위 | 소프트웨어 | 비용 | 선택 기준 |
|------|----------|------|----------|
| 1 | UTM | 무료 | 우선 시도. 덕코프가 가벼워 충분할 가능성 |
| 2 | VMware Fusion | 개인용 무료 | UTM에서 게임 성능 부족 시 |
| 3 | Parallels Desktop | 연 $99~ | 위 둘 다 실패 시 최후 수단 |
| 4 | 실기 PC 원격 | 무료 | 실기 PC 네트워크 상시 연결 가능할 때 |

### UTM 권장 VM 스펙

```
CPU: 4 cores
RAM: 8 GB (여유 있으면 16 GB)
Disk: 64 GB
Display: virtio-ramfb-gl (GPU Supported)
Shared Folder: VirtFS mode, ~/Dev/quackforge
```

### Windows 11 라이선스

**Evaluation 버전 (90일 무료)** 사용.
- https://www.microsoft.com/en-us/evalcenter/evaluate-windows-11-enterprise
- ARM64 (Apple Silicon용) 선택
- 90일 후 재설치 또는 정품 키 구매 판단

### 덕코프 Windows 경로

```
C:\Program Files (x86)\Steam\steamapps\common\Escape From Duckov\
```

## 2.4 실기 PC 배포 환경

Phase 5 전까지는 거의 쓰지 않음. 역할은 다음 두 가지:

1. **릴리즈 빌드 최종 검증** — VM에서 재현 안 되는 GPU/드라이버 엣지 케이스
2. **AssetBundle 빌드** (향후 v2.0+) — Unity Editor 작업 (3D 에셋 추가 시)

**사전 준비**:
- Steam 덕코프 설치
- Git 클라이언트
- .NET 8 SDK
- (선택) Visual Studio 2022 또는 Rider

## 2.5 파일 공유 전략

목표: Mac에서 빌드한 `QuackForge.Loader.dll` 이 VM 덕코프 plugins 폴더에 자동 반영.

### 옵션 A: UTM VirtFS (우선)

```
[Mac]
~/Dev/quackforge/src/QuackForge.Loader/bin/Release/netstandard2.1/
  │
  ├── (VirtFS 공유)
  ▼
[VM Windows]
\\.host.local\share\...\QuackForge.Loader.dll
  │
  ├── (심볼릭 링크)
  ▼
C:\Program Files (x86)\Steam\...\BepInEx\plugins\QuackForge.Loader.dll
```

**VM PowerShell (관리자)**:

```powershell
New-Item -ItemType SymbolicLink `
    -Path "C:\...\plugins\QuackForge.Loader.dll" `
    -Target "\\.host.local\share\...\QuackForge.Loader.dll"
```

### 옵션 B: HTTP 서버 (폴백)

Mac 에서:
```bash
cd src/QuackForge.Loader/bin/Release/netstandard2.1
python3 -m http.server 8000
```

VM 에서:
```powershell
Invoke-WebRequest -Uri "http://<Mac IP>:8000/QuackForge.Loader.dll" `
    -OutFile "C:\...\plugins\QuackForge.Loader.dll"
```

### 옵션 C: Git + VM git pull

작업 산출물을 Git에 포함시키는 방법. **비권장** — 빌드 아티팩트가 Git 히스토리 오염.

> **결정**: A → B 순으로 시도. C는 배제.

## 2.6 Steam 계정 운용

### 제약

Steam은 **동시에 한 계정에서 한 게임만** 실행 가능. Mac 덕코프 실행 중 VM 덕코프 실행 시 Mac 측 강제 종료.

### 운용 규칙

| 단계 | Mac 덕코프 | VM 덕코프 |
|------|-----------|-----------|
| 일상 개발 | ON | OFF |
| 마일스톤 QA | OFF | ON |
| 크로스 플랫폼 검증 | 순차 실행 (한 번에 한 쪽) | 순차 실행 |

### Family Sharing 고려

Phase 3 이후 QA 빈도가 높아지면 Family Sharing + 서브 계정으로 동시 실행 고려. 단, 덕코프가 Family Sharing을 허용하는지 확인 필요 (Open Question 참조).

## 2.7 전체 셋업 체크리스트 (최초 1회)

- [ ] Homebrew 설치
- [ ] .NET 8 SDK 설치
- [ ] Rider 설치 및 라이선스 활성화
- [ ] Git 설정 (이름, 이메일)
- [ ] 덕코프 Mac 클라이언트 설치
- [ ] BepInEx Mac 설치 및 깡통 동작 확인
- [ ] UTM 설치
- [ ] Windows 11 평가판 ISO 다운로드
- [ ] VM 생성 및 Windows 11 설치
- [ ] VM 내 Steam + 덕코프 설치 **(최대 리스크)**
- [ ] VM BepInEx 설치 및 깡통 동작 확인
- [ ] Mac↔VM 파일 공유 (VirtFS) 설정
- [ ] Git 저장소 생성 (GitHub 비공개)

---

# Part III. 기술 스택 & 아키텍처

## 3.1 기술 스택

| 레이어 | 기술 | 버전 | 선택 이유 |
|--------|------|------|----------|
| 언어 | C# | 9.0+ | 덕코프가 Unity/Mono 기반 |
| 런타임 | .NET Standard | 2.1 | BepInEx 5 + Unity 2022 공통 타겟 |
| 게임 엔진 | Unity | 2022.3.5f1 | 게임 버전과 반드시 일치 |
| 모드 로더 | BepInEx | 5.4.21 | 커뮤니티 표준, Harmony 내장 |
| 런타임 패치 | HarmonyX | 2.10+ | AOP 스타일 메서드 훅 |
| 설정 시스템 | BepInEx.Configuration | 5.4.21 | F1 런타임 편집, 파일 저장 |
| UI 프레임워크 | Unity UGUI | 2022.3.5 | 게임 자체 UI 스택 재활용 |
| JSON 직렬화 | System.Text.Json | 8.0 | 세이브 확장 파일 |
| 테스트 | xUnit + Moq | 최신 | Core 모듈 단위 테스트 (선택) |

## 3.2 모드 로더 선택 — 혼합 전략

### 선택지 비교

| 항목 | 공식 Mod API | BepInEx + Harmony | 혼합 (채택) |
|------|-------------|-------------------|------------|
| 무기/방어구 데이터 추가 | ✅ 적합 | ✅ 가능 | 공식 API 사용 |
| 새 시스템(레벨링) 구축 | ❌ 불가 | ✅ 가능 | BepInEx 사용 |
| 게임 업데이트 안정성 | ✅ 높음 | ⚠️ 버전 의존 | 경로별 위험 분산 |
| Steam Workshop 배포 | ✅ | ❌ | 공식 API 파트만 Workshop |
| Nexus 배포 | ✅ | ✅ | 전체 패키지 Nexus |
| 개발 난이도 | 낮음 | 높음 | 평균 |

### 결정

**Part A (공식 Mod API)** → 무기·방어구 데이터, 도면 레시피, 아이콘
**Part B (BepInEx)** → 레벨링, 챌린지 모드, UI, Harmony 패치

두 파트는 **느슨하게 결합**. Part B만 설치한 유저도 레벨링은 작동 (기본 무기/방어구만 보상 가능).

## 3.3 모듈 구조

```
QuackForge (Solution)
│
├── src/
│   ├── QuackForge.Loader/           # [Phase 0] BepInEx 엔트리포인트
│   │   └── Plugin.cs
│   │
│   ├── QuackForge.Core/             # [Phase 1] 공통 인프라
│   │   ├── Logging/
│   │   ├── Config/
│   │   ├── Events/                  # 내부 이벤트 버스
│   │   └── Save/                    # 세이브 확장 파일 IO
│   │
│   ├── QuackForge.Data/             # [Phase 1] 무기·방어구 데이터
│   │   ├── Weapons/
│   │   │   ├── WeaponRegistry.cs
│   │   │   └── Definitions/         # JSON 데이터
│   │   ├── Armors/
│   │   ├── Blueprints/              # 도면 시스템
│   │   └── Patches/                 # 워크벤치 Harmony 패치
│   │
│   ├── QuackForge.Progression/      # [Phase 2~3] 레벨링
│   │   ├── Xp/
│   │   │   ├── XpCollector.cs       # 이벤트 훅
│   │   │   └── XpCurve.cs           # 레벨 → XP 테이블
│   │   ├── Stats/
│   │   │   ├── StatManager.cs       # 중앙 상태
│   │   │   ├── StatType.cs
│   │   │   └── Patches/             # 스탯 적용 지점
│   │   └── UI/
│   │       └── StatScreen.cs        # 스탯 분배 창
│   │
│   ├── QuackForge.Challenge/        # [Phase 4] 챌린지 모드
│   │   ├── GameModes/
│   │   │   └── BossRushMode.cs
│   │   ├── Spawning/
│   │   │   └── BossSpawner.cs
│   │   ├── Rewards/
│   │   └── UI/
│   │       └── ChallengeMenu.cs
│   │
│   └── QuackForge.Common/           # 여러 모듈이 공유하는 DTO, 상수
│
├── mod/                              # 공식 Mod API 배포용 (Part A)
│   ├── manifest.json
│   ├── weapons/
│   ├── armors/
│   └── icons/
│
├── scripts/
│   ├── deploy-mac.sh
│   ├── deploy-vm.ps1
│   └── build-release.sh
│
└── docs/
    ├── PRD.md (이 문서)
    ├── ARCHITECTURE.md
    └── CHANGELOG.md
```

**설계 원칙**:
- Spring의 멀티모듈 프로젝트 구조
- `Core`만 다른 모듈이 참조 가능, 나머지는 서로 의존 금지
- 유저가 `Progression`만 쓰고 `Challenge` 뺀 경우에도 정상 동작

## 3.4 데이터 흐름

### 게임 부팅 시

```
BepInEx 로드
  ↓
Plugin.Awake()
  ↓
Core.Bootstrap.Initialize()     ← 설정 로드, 로거 초기화
  ↓
Data.WeaponRegistry.LoadAll()   ← 무기/방어구 JSON 읽기
  ↓
Data.Patches.Apply(_harmony)    ← 워크벤치 레시피 주입 패치
  ↓
Progression.LevelingSystem.Attach(_harmony)  ← XP 훅 + 스탯 패치
  ↓
Challenge.GameModeRegistry.Register()        ← 챌린지 모드 등록
  ↓
게임 메인 메뉴 표시
```

### 플레이 중 (적 처치 예시)

```
[게임]
Enemy.TakeDamage(100) → Enemy.Die()
  ↓ (Harmony Postfix)
[Progression.XpCollector]
OnEnemyKilled(enemy)
  ↓
StatManager.AddXp(xpAmount)
  ↓
if (NewLevel) → EventBus.Publish(LevelUpEvent)
  ↓ (UI 구독자)
StatScreen.ShowLevelUpNotification()
```

### 세이브 시점

```
[게임]
SaveManager.Save()
  ↓ (Harmony Postfix)
[Core.Save]
QuackForgeSaveWriter.Write(gameSlot)
  ↓
{gameSlotFolder}/quackforge.json 작성
```

## 3.5 저장 호환성 설계 (중요)

### 3가지 옵션 재평가

| 옵션 | 장점 | 단점 |
|------|------|------|
| A. 원본 세이브 확장 | 유저 경험 매끄러움 | 모드 제거 시 세이브 오염 |
| **B. 별도 파일 분리** | 원본 무결성, 제거 안전 | 세이브 동기화 로직 필요 |
| C. 전용 세이브 슬롯 | 가장 안전 | 기존 세이브 이관 불가 |

**결정: B안**. `{세이브폴더}/quackforge.json` 병치.

### quackforge.json 스키마 (v1)

```json
{
  "version": 1,
  "createdAt": "2026-04-17T12:00:00Z",
  "updatedAt": "2026-04-17T12:00:00Z",
  "gameSlotId": "save_slot_3",
  "progression": {
    "level": 12,
    "currentXp": 4250,
    "availablePoints": 2,
    "stats": {
      "vitality": 3,
      "strength": 1,
      "agility": 2,
      "precision": 4,
      "survival": 2
    }
  },
  "challenge": {
    "completedRuns": [
      { "mode": "boss_rush_farm", "time": 423.5, "date": "2026-04-15T20:12:00Z" }
    ]
  }
}
```

### 버전 마이그레이션 전략

Spring Flyway 개념과 동일:
- 파일 상단 `version` 필드
- `SaveMigrator.Migrate(oldJson, targetVersion)` — 순차 업그레이드
- 하위 호환 깨뜨리는 변경은 **major version up + 마이그레이션 로직 필수**

### 세이브 슬롯 매칭

덕코프 세이브 폴더 구조 확인 필요 (Open Question).

예상 위치 (Mac):
```
~/Library/Application Support/com.TeamSoda.Duckov/saves/
```

각 세이브 슬롯별 서브 디렉토리가 있다면 그 안에 `quackforge.json` 병치. 플랫 구조면 슬롯 ID를 파일명에 포함 (`quackforge_slot3.json`).

## 3.6 타 모드 호환성

### 잠재 충돌 모드 목록

| 모드 | 영역 | 충돌 가능성 | 대응 |
|------|------|------------|------|
| VanillaAttachmentsExpanded | 무기 추가 | 중 | 데이터 ID 분리, 어태치먼트 슬롯 공유 금지 |
| New Weapon Customizer | 무기 파라미터 | 낮음 | 공존 가능 |
| 인벤토리 확장 모드 | 캐리 용량 | 중 | 레벨링 strength 스탯과 연동 방식 선택 |
| 3rd person camera | 카메라 | 없음 | 완전 독립 |
| 각종 치트 모드 | 전반 | 낮음 | 사용자 책임 |

### 네임스페이스 규칙

```
[아이템 ID]
quackforge_weapon_<name>
quackforge_armor_<name>
quackforge_blueprint_<name>

[Harmony Patch ID]
com.returntrue.quackforge.<module>.<patch>

[Config Section]
[QuackForge.<Module>]
```

### Harmony 우선순위

```csharp
[HarmonyPatch(typeof(Target), nameof(Target.Method))]
[HarmonyPriority(Priority.Normal)]  // 명시적
[HarmonyBefore("com.other_mod.id")]  // 필요 시
```

우선순위 명시는 필수. 생략 시 로드 순서 의존성으로 디버깅 어려워짐.

## 3.7 네이밍 컨벤션

| 대상 | 규칙 | 예시 |
|------|------|------|
| C# 네임스페이스 | PascalCase | `QuackForge.Progression.Stats` |
| 클래스 | PascalCase | `StatManager` |
| 메서드 | PascalCase | `AddXp` |
| 로컬 변수 | camelCase | `currentLevel` |
| Private 필드 | _camelCase | `_harmony` |
| Const | PascalCase | `MaxLevel` |
| Mod 아이템 ID | snake_case + 프리픽스 | `quackforge_weapon_ak74` |
| 파일 이름 | kebab-case (문서), PascalCase (코드) | `deploy-mac.sh`, `Plugin.cs` |
| Harmony Patch | `<Target>_<Method>_Patch` | `Enemy_Die_Patch` |
| 로그 태그 | `[module]` | `[Progression]` |

---

# Part IV. Phase 0 — Bootstrap & Hello World

**기간**: 2~3일 (주말 집중 or 평일 저녁 분산)
**작업자**: Solo (N)
**전제**: Part II 개발 환경 체크리스트 완료

## 4.1 Phase 0 목표

"Mac에서 빌드한 플러그인이 Mac과 Windows VM 양쪽 덕코프에서 동일한 환영 메시지를 로그에 남긴다."

이 한 줄이 완성되면 **개발 파이프라인이 양쪽으로 뚫린 상태**. 이후 모든 Phase가 이 파이프라인 위에서 진행된다.

## 4.2 Done 기준

- [ ] Mac에서 `./scripts/deploy-mac.sh` 실행 → 자동 빌드·배포 성공
- [ ] Mac 덕코프 실행 시 `LogOutput.log` 에 `🦆 QuackForge is awake. Forging begins.` 출력
- [ ] VM 내 Windows 덕코프에서도 동일 로그 출력
- [ ] Git 저장소 초기 커밋 + GitHub push 완료
- [ ] Rider에서 프로젝트 열고 중단점 설정 가능 (디버깅 기초)

## 4.3 Scope

### In Scope
- BepInEx 플러그인 엔트리포인트 작성
- 빌드 스크립트 (Mac)
- Git 저장소 초기화
- 양 플랫폼 (Mac/VM) 동작 확인
- 최소 설정 엔트리 (EnableMod 마스터 스위치)

### Out of Scope (Phase 1 이후)
- 실제 Harmony 패치 (빈 `PatchAll()` 만 호출)
- UI 요소
- 세이브 파일 조작
- 어떤 기능적 동작이든

## 4.4 작업 분해 (Task Breakdown)

### T0.1 Mac 개발 도구 설치 (30분)
- Homebrew, .NET 8 SDK, Rider, fswatch 설치
- `dotnet --version` 확인

### T0.2 Mac BepInEx 설치 및 검증 (1시간)
- Unix x64 zip 다운로드, 덕코프 폴더 압축 해제
- `xattr -cr` 로 macOS Gatekeeper 플래그 제거
- `./run_bepinex.sh` 실행 → 메뉴 진입 → 종료 → `LogOutput.log` 확인
- **체크포인트**: "BepInEx 5.4.21.0 - Duckov" 메시지 확인

### T0.3 UTM + Windows 11 VM 생성 (2~3시간)
- UTM 설치, VM 생성 (스펙: 4C/8G/64G)
- Windows 11 ARM 평가판 ISO 설치
- Windows 업데이트 1회 적용
- VirtFS 공유 폴더 활성화

### T0.4 VM 내 덕코프 설치 및 실행 검증 ⚠️ **최대 리스크** (1~3시간)
- VM에 Steam 설치 → 로그인
- Mac 덕코프 종료 확인 후 VM에서 덕코프 설치
- 게임 메인 메뉴 진입 + 1회 레이드 입장 테스트
- **체크포인트**: 30 FPS 이상 유지되면 성공

**실패 시 대응** (순차 폴백):
1. UTM 설정 조정 (GPU 드라이버, 디스플레이 옵션)
2. VMware Fusion으로 전환
3. Parallels Desktop 구매
4. VM 전략 포기, 실기 PC로 QA 환경 대체

### T0.5 VM BepInEx 설치 (30분)
- Windows x64 zip 다운로드
- 압축 해제, 게임 1회 실행, 로그 확인

### T0.6 프로젝트 구조 생성 (1시간)
- 폴더 구조 생성: `src/`, `scripts/`, `docs/`
- `QuackForge.sln`, `QuackForge.Loader.csproj`, `Plugin.cs` 작성
- `.gitignore`, `README.md`, `SETUP.md` 배치

### T0.7 빌드 & 로컬 검증 (1시간)
- `dotnet restore` → NuGet 패키지 복원 성공
- `dotnet build -c Release` → DLL 생성 확인
- `ls -la src/QuackForge.Loader/bin/Release/netstandard2.1/QuackForge.Loader.dll`

### T0.8 Mac 배포 스크립트 작성 및 테스트 (1시간)
- `scripts/deploy-mac.sh` 작성
- `chmod +x`
- 실행 → 덕코프 플러그인 폴더에 DLL 복사 확인
- 덕코프 실행 → 로그에 환영 메시지 확인
- **체크포인트**: `🦆 QuackForge is awake.` 출력

### T0.9 Mac↔VM 파일 공유 설정 (1~2시간)
- UTM VirtFS 공유 폴더 VM 마운트
- VM PowerShell로 심볼릭 링크 생성 (관리자 권한)
- Mac에서 빌드 → VM 덕코프 실행 → 로그 확인
- **체크포인트**: Mac에서 빌드한 DLL이 VM에서 로드됨

### T0.10 Git 저장소 초기화 (30분)
- `git init` → `git add .` → 첫 커밋
- GitHub 비공개 레포 생성
- `git remote add` → `git push -u origin main`

## 4.5 산출물

| 산출물 | 위치 | 용도 |
|--------|------|------|
| `QuackForge.Loader.dll` | Mac 덕코프 plugins | 로드 테스트 |
| Plugin.cs | `src/QuackForge.Loader/` | 엔트리포인트 |
| deploy-mac.sh | `scripts/` | 자동 배포 |
| .gitignore | 루트 | 커밋 필터 |
| README.md, SETUP.md | 루트 | 문서 |
| Git 히스토리 | GitHub | 버전 관리 |

## 4.6 주요 기술 결정 (Phase 0)

### 결정 0-1: BepInEx 5 vs 6

**결정**: BepInEx 5.4.21 사용.

**근거**:
- BepInEx 6은 Beta 단계, 안정성 미확인
- 덕코프 기존 모드 전부 BepInEx 5 기반
- 유저 호환성 관점에서 5가 안전

### 결정 0-2: 설정 시스템

**결정**: BepInEx 기본 Configuration 사용 (JSON 아님).

**근거**:
- 유저가 F1 ConfigurationManager 로 런타임 편집 가능
- 별도 JSON 파싱 코드 불필요
- 복잡한 스키마 (중첩, 배열)가 필요한 시점에만 JSON 도입 (Phase 2의 quackforge.json)

### 결정 0-3: 로거 패턴

**결정**: `Plugin.Log` 정적 싱글턴 공개.

**근거**:
- BepInEx `BaseUnityPlugin.Logger` 는 인스턴스 멤버
- 하위 모듈에서 `Plugin.Log.LogInfo(...)` 로 쉽게 접근
- Spring의 SLF4J 정적 Logger와 유사 패턴

### 결정 0-4: Harmony 인스턴스 시점

**결정**: Plugin.Awake() 에서 즉시 생성, Phase 0에서는 빈 `PatchAll()` 호출.

**근거**:
- Phase 1+에서 패치 추가 시 추가 작업 최소화
- `OnDestroy` 에서 `UnpatchSelf` 로 클린업 보장

## 4.7 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R0-1 | VM에서 덕코프 실행 불가 | 중 | 상 | VMware → Parallels → 실기 PC 대체 (4단 폴백) |
| R0-2 | macOS Gatekeeper로 BepInEx 로드 실패 | 저 | 중 | xattr 플래그 제거, Rosetta 강제 실행 |
| R0-3 | .NET SDK 버전 충돌 | 저 | 저 | netstandard2.1 타겟 고정, global.json 추가 |
| R0-4 | BepInEx NuGet 피드 접근 실패 | 저 | 중 | `dotnet nuget add source https://nuget.bepinex.dev/v3/index.json` |
| R0-5 | VirtFS 권한 이슈 | 중 | 저 | HTTP 서버 폴백 (scripts/serve-mac.sh) |

## 4.8 Open Questions (Phase 0)

- [ ] Q0-1: UTM VirtFS가 실제로 VM에서 안정적인가? (실측 필요)
- [ ] Q0-2: Mac 덕코프가 Apple Silicon 네이티브인지 x86 Rosetta인지?
- [ ] Q0-3: BepInEx Mac 빌드와 덕코프 Mac 클라이언트 호환성 검증됨?
- [ ] Q0-4: Rider 라이선스 경로 (개인/학생/OSS)

## 4.9 Phase 0 완료 후 다음 액션

1. Phase 1 킥오프 전 **공식 Mod Guide 공지 전문 분석**
2. `Assembly-CSharp.dll` Rider 디컴파일러로 열어 **무기·방어구 클래스 구조 스케치**
3. 이를 근거로 Phase 1 PRD(§Part V) 세부사항 확정

---

# Part V. Phase 1 — 무기 & 방어구 데이터 시스템

**기간**: 1~2주
**전제**: Phase 0 완료
**난이도**: Tier 1 (데이터 중심) + Tier 2 (일부 Harmony 패치)

## 5.1 Phase 1 목표

"덕코프의 워크벤치에서 도면을 사용해 QuackForge 전용 무기·방어구를 제작할 수 있고, 적에게 드랍되거나 시장에서 거래될 수 있다."

## 5.2 Done 기준

- [ ] 최소 **10종 신규 무기** 정의 (기존 모델 재활용, 파라미터만 신규)
- [ ] 최소 **5종 신규 방어구** 정의
- [ ] 각 무기/방어구별 **도면 아이템** 존재 및 워크벤치에서 제작 가능
- [ ] 도면 드랍 로직 동작 (챌린지 클리어 or 보스 드랍 조건, Phase 4 연동 대비)
- [ ] 기존 `VanillaAttachmentsExpanded` 모드와 **공존 확인**
- [ ] 데이터 JSON 파일 → 런타임 등록 파이프라인 완성
- [ ] 공식 Mod API와 BepInEx 혼합 방식 검증

## 5.3 Scope

### In Scope
- 무기/방어구 데이터 정의 시스템
- 도면(Blueprint) 아이템 & 제작 레시피
- 워크벤치 UI 확장 (기존 워크벤치에 신규 아이템 등록)
- Core 모듈 (공통 인프라) 구축
- 기본 아이콘 (기존 게임 아이콘 재활용)

### Out of Scope
- 신규 3D 모델 (Tier 3, v2.0 이후)
- 신규 사운드
- 신규 애니메이션
- 어태치먼트 시스템 확장 (VanillaAttachmentsExpanded 영역)

## 5.4 기능 명세

### 5.4.1 무기 데이터 스키마

```json
{
  "id": "quackforge_weapon_vector_custom",
  "displayName": {
    "en": "Vector Custom",
    "ko": "벡터 커스텀"
  },
  "category": "smg",
  "tier": 3,
  "baseModel": "weapon_vector",
  "stats": {
    "damage": 32,
    "fireRate": 850,
    "accuracy": 0.78,
    "recoil": 0.35,
    "magazineSize": 30,
    "reloadTime": 2.4,
    "range": 65
  },
  "ammoType": "9mm",
  "weight": 2.8,
  "price": 12500,
  "dropRate": 0.02,
  "craftingRecipe": {
    "blueprintId": "quackforge_blueprint_vector_custom",
    "materials": [
      { "itemId": "scrap_metal", "count": 15 },
      { "itemId": "weapon_part_rare", "count": 3 },
      { "itemId": "gunpowder", "count": 8 }
    ],
    "workbenchTier": 2,
    "craftTime": 120
  }
}
```

### 5.4.2 방어구 데이터 스키마

```json
{
  "id": "quackforge_armor_stealth_vest",
  "displayName": {
    "en": "Stealth Vest",
    "ko": "은폐 조끼"
  },
  "slot": "chest",
  "tier": 3,
  "baseModel": "armor_tactical_vest",
  "stats": {
    "protection": 45,
    "penetrationResistance": 0.6,
    "weightPenalty": 1.2,
    "movementPenalty": 0.05,
    "staminaBonus": 0.1
  },
  "durability": 180,
  "repairCost": 850
}
```

**설계 결정**:
- 민첩 특화, 탱크 특화 등 **빌드 지향성**을 스탯 곡선으로 표현
- `movementPenalty` 음수 값 허용 (= 이동 보너스) — 민첩 장비 표현용

### 5.4.3 도면(Blueprint) 시스템

**개념**: 워크벤치 제작에 필요한 **전제 조건 아이템**. 도면 획득 없이는 제작 불가.

**획득 경로**:
1. **보스 드랍** (확률 기반)
2. **챌린지 모드 클리어 보상** (Phase 4 연동)
3. **특정 레벨 도달 시 자동 언락** (Phase 3 연동, 옵션)
4. **레이드 중 희귀 스폰** (작은 확률)

**데이터 스키마**:
```json
{
  "id": "quackforge_blueprint_vector_custom",
  "displayName": { "en": "Blueprint: Vector Custom", "ko": "도면: 벡터 커스텀" },
  "unlocksWeapon": "quackforge_weapon_vector_custom",
  "unlockConditions": {
    "minPlayerLevel": 10,
    "dropSources": [
      { "enemy": "boss_farm_king", "chance": 0.15 },
      { "challenge": "boss_rush_farm", "guaranteed": true }
    ]
  },
  "consumeOnUse": false
}
```

`consumeOnUse: false` = 한 번 획득하면 영구 해금 (Spring의 Role 할당과 유사).
`consumeOnUse: true` = 1회용 (제작 시 소모).

### 5.4.4 워크벤치 통합

기존 워크벤치 UI에 QuackForge 레시피가 자연스럽게 섞여 보여야 함.

**전략**: Harmony 패치로 `Workbench.GetAvailableRecipes()` 결과에 QuackForge 레시피를 **추가(append)** 하는 방식. 기존 레시피는 건드리지 않음.

```
(의사 코드)
[HarmonyPatch(typeof(Workbench), "GetAvailableRecipes")]
[HarmonyPostfix]
static void InjectQuackForgeRecipes(ref List<Recipe> __result)
{
    var playerBlueprints = BlueprintRegistry.GetUnlockedFor(currentPlayer);
    foreach (var bp in playerBlueprints)
    {
        __result.Add(RecipeBuilder.From(bp));
    }
}
```

## 5.5 기술 설계

### 5.5.1 Core 모듈 (선행)

Phase 1에서 처음 구현. 이후 모든 Phase가 의존.

**주요 컴포넌트**:

| 컴포넌트 | 역할 | 유사 개념 |
|---------|------|----------|
| `QfConfig` | 설정 로드/저장 | Spring @ConfigurationProperties |
| `QfLogger` | 모듈별 태그 로거 | SLF4J |
| `QfEventBus` | 내부 이벤트 pub/sub | Spring ApplicationEvent |
| `QfSaveContext` | 세이브 파일 IO | Spring JPA Repository 스타일 |

### 5.5.2 데이터 로드 파이프라인

```
[game boot]
  ↓
Data.WeaponRegistry.LoadAll()
  ↓
1. src/QuackForge.Data/Definitions/weapons/*.json 파일 탐색
  ↓
2. System.Text.Json 역직렬화 → WeaponDefinition 객체
  ↓
3. 검증 (필수 필드, ID 중복)
  ↓
4. WeaponRegistry._cache 에 등록
  ↓
5. 게임 내 ItemDatabase 에 주입 (Harmony 훅)
```

JSON 파일은 **DLL 내부 embedded resource**로 포함. 유저가 직접 수정하지 못하게 (향후 유저 설정 파일로 오버라이드 허용 고려).

### 5.5.3 ID 해시 충돌 방지

덕코프가 내부적으로 문자열 ID 대신 해시 ID를 쓴다면 (확인 필요):
- 해시 충돌 감지 로직 필수
- 충돌 발견 시 `quackforge_weapon_vector_custom_v2` 식으로 리네임

## 5.6 작업 분해

### T1.1 공식 Mod Guide 분석 (0.5일)
- Team Soda Steam 공지 전문 번역·분석
- 공식 Mod API 커버리지 확인
- `Duckov_Data/Mods` 폴더 규격 파악

### T1.2 Assembly-CSharp 역공학 (1~2일)
- Rider로 `Assembly-CSharp.dll` 열기
- `Weapon`, `Armor`, `Workbench`, `ItemDatabase`, `Recipe` 클래스 구조 문서화
- 스탯 필드 이름·타입 정확히 확인
- `docs/reverse-engineering/weapons.md` 작성

### T1.3 Core 모듈 구현 (1일)
- `QuackForge.Core` 프로젝트 추가
- Config, Logger, EventBus, SaveContext 구현
- `Plugin.Awake()` 에서 Core 초기화

### T1.4 Data 모듈 — 무기 (2일)
- `WeaponDefinition` 클래스, JSON 스키마
- `WeaponRegistry.LoadAll()` 구현
- 10종 무기 JSON 작성 (밸런스 초안)
- `ItemDatabase` 에 등록하는 Harmony 패치

### T1.5 Data 모듈 — 방어구 (1일)
- 무기와 같은 패턴
- 5종 방어구 JSON 작성

### T1.6 Blueprint 시스템 (1~2일)
- `Blueprint`, `BlueprintRegistry` 구현
- 도면 아이템 → 워크벤치 레시피 언락 Harmony 패치
- 드랍 테이블 확장 (보스 드랍 기반, 간단한 버전)

### T1.7 통합 테스트 (1일)
- Mac 덕코프에서 신규 아이템 콘솔 명령으로 스폰 확인
- 도면 획득 → 워크벤치 표시 → 제작 → 사용 전 플로우
- VanillaAttachmentsExpanded 동시 설치 후 충돌 확인

### T1.8 VM QA (0.5일)
- Windows 환경 동작 확인
- 성능 프로파일 (프레임 드랍 여부)

### T1.9 Phase 1 회고 & v0.1.0 태깅 (0.5일)
- `CHANGELOG.md` 작성
- Git 태그 `v0.1.0-alpha.1`
- Open Questions 정리 → Phase 2 PRD 업데이트

## 5.7 주요 기술 결정

### 결정 1-1: 데이터 형식 — JSON vs C# 코드

**결정**: JSON (embedded resource)

**근거**:
- 데이터/로직 분리 (SRP)
- 밸런스 튜닝 시 C# 재컴파일 불필요
- 향후 유저 오버라이드 설정 도입 용이

### 결정 1-2: 도면 소비 여부 — 1회용 vs 영구

**결정**: 영구 해금 (기본값), 1회용은 데이터별 옵트인

**근거**:
- 덕코프의 도면 시스템이 없으므로 유저 친화적 설계 우선
- 1회용 도면은 예외적인 고티어 아이템에만 적용

### 결정 1-3: 워크벤치 UI 확장 — 통합 vs 별도 탭

**결정**: 기존 레시피 목록에 append (통합)

**근거**:
- 유저가 "QuackForge 전용 탭"을 따로 열 필요 없이 자연스러운 워크플로우
- Harmony Postfix 한 방으로 구현 가능

### 결정 1-4: 초기 밸런스 원칙

- Tier 3 QuackForge 무기는 **바닐라 Tier 2~3 중간** 성능
- Tier 5 (최상위)는 **바닐라 최상위와 동등하거나 약간 우위**
- 획득 비용(도면 드랍률, 재료)이 성능 보정

## 5.8 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R1-1 | Assembly-CSharp 심하게 난독화 | 저 | 상 | DnSpy/ILSpy로 복수 툴 교차 확인 |
| R1-2 | 공식 Mod API가 무기 추가 미지원 | 중 | 중 | BepInEx 단독 경로로 피벗 |
| R1-3 | 게임 업데이트로 `Weapon` 클래스 변경 | 중 | 중 | 버전 잠금 + 자동 비활성화 |
| R1-4 | ID 해시 충돌 | 저 | 저 | 충돌 감지 로그 + 런타임 경고 |
| R1-5 | VAE와 레시피 경쟁 | 중 | 저 | 명시적 테스트 + 프리픽스 분리 |

## 5.9 Open Questions

- [ ] Q1-1: 공식 Mod API가 제공하는 정확한 커버리지
- [ ] Q1-2: `Weapon.baseModel` 필드가 실제 존재하는지 (모델 재활용 가능 여부)
- [ ] Q1-3: 도면 아이템을 어떤 아이템 카테고리로 분류할지 (문서? 퀘스트 아이템? 커스텀?)
- [ ] Q1-4: 다국어 지원 범위 (Phase 1: 영/한 / 중국어/일본어는 Phase 5 커뮤니티 번역)
- [ ] Q1-5: 초기 10종 무기 리스트 확정 (타입 분배: AR 3 / SMG 2 / 샷건 2 / 스나 1 / 권총 2)

## 5.10 산출물

| 산출물 | 형태 |
|--------|------|
| `QuackForge.Core.dll` | BepInEx 플러그인 |
| `QuackForge.Data.dll` | BepInEx 플러그인 |
| 15종 아이템 JSON | DLL 내 embedded |
| `docs/reverse-engineering/*.md` | 역공학 노트 |
| Git 태그 `v0.1.0-alpha.1` | 마일스톤 |

---

# Part VI. Phase 2 — 레벨링 시스템 MVP

**기간**: 2~3주
**전제**: Phase 1 완료, Core 모듈 기능 확장 가능
**난이도**: Tier 4 (새 게임 시스템 구축, 가장 어려움)

> ⚠️ **이 Phase가 프로젝트 전체에서 가장 복잡하다.** Spring 개발자 관점에서 "기존 모놀리스에 새 도메인(인증·권한)을 통째로 얹는" 수준의 작업량.

## 6.1 Phase 2 목표

"적을 처치하면 XP가 쌓이고, 일정 XP에 도달하면 레벨이 올라가며, 레벨업 시 HP가 자동 증가한다. 유저는 세이브 파일을 불러와도 레벨 정보가 유지된다."

**MVP 수준**: UI 없음, 자동 HP 증가만. 스탯 분배는 Phase 3.

## 6.2 Done 기준

- [ ] 적 처치 시 XP 획득 (적 티어별 차등)
- [ ] 퀘스트 클리어 시 XP 획득 (간단 버전)
- [ ] 레벨 1 → 2 → 3 ... 50까지 XP 테이블 적용
- [ ] 레벨업 시 자동으로 최대 HP +10 (하드코딩)
- [ ] `quackforge.json` 에 레벨·XP 저장
- [ ] 세이브 로드 시 레벨·XP 복원
- [ ] 레벨업 순간 콘솔 로그 + 게임 내 간단 토스트 알림
- [ ] BepInEx Configuration Manager로 디버그 모드 활성화 시 `AddXp(1000)` 콘솔 명령 사용 가능

## 6.3 Scope

### In Scope
- XP 수집 (적 처치, 퀘스트)
- 레벨 계산 및 XP 테이블
- 최대 HP 스탯 1종만 적용
- 세이브 확장 파일 IO
- 레벨업 알림 (로그 + 토스트)

### Out of Scope (Phase 3으로)
- 5종 스탯 전체
- 스탯 분배 UI
- 스탯 투자 선택
- 레벨업 시각 효과 (파티클 등)

## 6.4 기능 명세

### 6.4.1 XP 획득 소스

| 소스 | 기본 XP | 조건 |
|------|--------|------|
| 일반 적 처치 | 10 | Tier 1 적 |
| 강한 적 처치 | 25 | Tier 2~3 적 |
| 엘리트 적 처치 | 75 | Tier 4 적 |
| 보스 처치 | 500 | 지역 보스 |
| 메인 퀘스트 클리어 | 200 | 스토리 퀘스트 |
| 사이드 퀘스트 클리어 | 50 | 반복 퀘스트 |
| 레이드 탈출 성공 | +20% | 해당 레이드 누적 XP에 보너스 |
| 레이드 사망 | -50% | 해당 레이드 누적 XP에서 감산 (저장 전 계산) |

### 6.4.2 XP 테이블 (레벨 → 다음 레벨까지 XP)

```
Level N → N+1 필요 XP:
  base = 100
  growth = 1.12
  xpNeeded(n) = floor(base * growth^(n-1))
```

| 레벨 | 다음까지 XP | 누적 |
|------|-----------|------|
| 1 → 2 | 100 | 100 |
| 2 → 3 | 112 | 212 |
| 5 → 6 | 157 | 671 |
| 10 → 11 | 277 | 1,791 |
| 20 → 21 | 860 | 7,289 |
| 30 → 31 | 2,671 | 24,309 |
| 50 → 51 | 24,913 | (Max) |

### 6.4.3 레벨당 자동 증가 (MVP)

- 최대 HP +10
- 그 외 스탯은 Phase 3에서

## 6.5 기술 설계

### 6.5.1 XP 이벤트 훅 지점 파악 (핵심 과제)

Phase 2의 **가장 큰 리스크**. 게임 내 "적 처치" 이벤트가 어디서 발생하는지 확인해야 함.

**예상 훅 지점** (Rider 디컴파일 기반 추정):

```
후보 1: Enemy.Die()
후보 2: EnemyHealth.OnDeath event
후보 3: CombatManager.NotifyKill(attacker, target)
```

**작업 순서**:
1. Rider 디컴파일러로 `Assembly-CSharp.dll` 열기
2. `Die`, `OnDeath`, `Kill`, `Damage` 키워드로 검색
3. 실제 호출 지점에 Harmony 훅 테스트
4. 로그로 호출 빈도 확인 (과도한 호출 방지)

### 6.5.2 StatManager 설계

```
QuackForge.Progression.Stats.StatManager (싱글턴)
│
├── CurrentLevel: int
├── CurrentXp: long
├── UnspentPoints: int  (Phase 3부터)
├── Stats: Dictionary<StatType, int>  (Phase 3부터)
│
├── AddXp(amount: long): void
├── CalculateLevel(xp: long): int
├── OnLevelUp(newLevel: int): void
│
└── Events:
    - LevelUp(oldLevel, newLevel)
    - XpGained(amount, source)
```

**설계 원칙**:
- 글로벌 싱글턴 (현재 세이브 슬롯의 상태를 유일하게 보유)
- 세이브 로드 시 `LoadFrom(saveContext)`, 저장 시 `SaveTo(saveContext)`
- 스탯 적용은 Harmony 패치가 `StatManager.Stats[StatType.Vitality]` 를 조회

### 6.5.3 세이브 IO

**저장 트리거**:
```
[게임 내] SaveManager.Save()
  ↓ (Harmony Postfix)
[QuackForge] QuackForgeSaveWriter.Write(saveContext)
  ↓
{saveFolder}/quackforge.json 파일 생성/덮어쓰기
```

**로드 트리거**:
```
[게임 내] SaveManager.Load(slot)
  ↓ (Harmony Postfix)
[QuackForge] QuackForgeSaveReader.Read(saveContext)
  ↓
StatManager.LoadFrom(data)
```

**파일 없을 시**: 새 세이브 간주, StatManager 초기화 (Level 1, XP 0).

### 6.5.4 HP 스탯 적용 Harmony 패치

```
[HarmonyPatch(typeof(PlayerHealth), "get_MaxHp")]
[HarmonyPostfix]
static void AddQuackForgeBonus(ref int __result)
{
    var level = StatManager.Instance.CurrentLevel;
    __result += (level - 1) * 10;  // 레벨 1은 보너스 없음
}
```

**주의**: `MaxHp` 가 속성(property)인지 필드(field)인지에 따라 패치 방식 다름. `[HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.MaxHp), MethodType.Getter)]` 형태.

### 6.5.5 레벨업 알림

MVP 버전:
1. 콘솔 로그: `[Progression] Level Up! 5 → 6`
2. 게임 내 기존 토스트 시스템 재활용 (`ToastManager.Show("Level Up!")`) — 있으면 사용, 없으면 생략

Phase 3에서 전용 UI 추가.

## 6.6 작업 분해

### T2.1 역공학 집중 세션 (3~4일) ⚠️
- `Enemy.Die` / `OnDeath` / `Kill` 훅 지점 확인
- `PlayerHealth.MaxHp` 접근 방식 확인
- `SaveManager.Save/Load` 인터페이스 파악
- `docs/reverse-engineering/progression.md` 작성
- **체크포인트**: 3개 훅 지점 모두 확정

### T2.2 Progression 모듈 뼈대 (1일)
- 프로젝트 추가, csproj, 기본 클래스 파일

### T2.3 StatManager 구현 (1~2일)
- 싱글턴, XP/Level 계산, 이벤트
- 단위 테스트 (xUnit 선택적)

### T2.4 XpCollector 구현 (2~3일)
- Harmony 패치로 적 처치 이벤트 잡기
- 적 티어별 XP 차등 적용
- 로그 확인

### T2.5 세이브 IO (2~3일)
- QuackForgeSaveWriter / Reader
- JSON 직렬화
- 세이브 폴더 경로 확인 (Mac/Windows 차이 대응)
- 버전 필드 마이그레이션 뼈대

### T2.6 HP 스탯 패치 (1일)
- `MaxHp` 게터 Postfix 패치
- 레벨별 증가 확인

### T2.7 레벨업 알림 (0.5일)
- 게임 내 토스트 시스템 확인
- 있으면 연동, 없으면 로그만

### T2.8 디버그 명령 (0.5일)
- BepInEx 콘솔 명령 or Config 기반 `/addxp 1000`
- 테스트 효율을 위해 필수

### T2.9 통합 테스트 & VM QA (1~2일)
- 레이드 → 적 처치 → XP → 레벨업 → 세이브 → 재시작 → 복원 풀 플로우
- 여러 세이브 슬롯 전환 테스트
- VM Windows 동작 확인

### T2.10 Phase 2 회고 & v0.2.0 태깅 (0.5일)

## 6.7 주요 기술 결정

### 결정 2-1: XP 계산 시점 — 이벤트 발생 즉시 vs 레이드 종료 시

**결정**: 이벤트 즉시 누적 + 레이드 종료 시 보너스/페널티 적용

**근거**:
- 이벤트 즉시 누적: 적 처치 피드백 즉시성
- 종료 시 정산: 탈출 보너스, 사망 페널티는 레이드 단위로만 의미 있음

### 결정 2-2: 레벨 상한 — 50 vs 100

**결정**: 50 (MVP), Phase 3에서 재검토

**근거**:
- 후반 XP 요구량 급증으로 밸런스 난해
- 100레벨 도달에 필요한 플레이타임이 비합리적
- 50을 쉽게 확장 가능하게 (테이블은 100까지 미리 정의)

### 결정 2-3: 세이브 파일 위치

**결정**: 덕코프 세이브 폴더 내부에 병치 (`quackforge.json`)

**근거**:
- 세이브 슬롯과 1:1 매칭 자연스러움
- 게임 세이브 백업 시 자동 포함
- Steam Cloud와의 상호작용은 **Open Question** (Q2-3)

### 결정 2-4: JSON 라이브러리 — System.Text.Json vs Newtonsoft

**결정**: System.Text.Json

**근거**:
- .NET Standard 2.1 네이티브 지원
- 외부 의존성 없음 (BepInEx 부하 감소)
- 성능 우위

## 6.8 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R2-1 | 적 처치 이벤트 훅 지점 파악 실패 | 중 | 상 | 복수 후보 훅에 시험 Patch, 로그로 검증 |
| R2-2 | 적 타입 분류 방식이 runtime에서 불명확 | 중 | 중 | 태그/레이어 기반 추정, 데이터로 화이트리스트 |
| R2-3 | MaxHp 패치가 다른 계산과 충돌 | 중 | 중 | Priority 명시, 계산 순서 로깅 |
| R2-4 | 세이브 타이밍이 예측 불가 (자동 저장 빈도) | 저 | 중 | 세이브 Postfix로 매 저장 시 동기화 |
| R2-5 | 세이브 슬롯 ID를 내부적으로 추적 못할 수 있음 | 중 | 상 | 파일 경로 기반 식별로 폴백 |
| R2-6 | Steam Cloud가 quackforge.json을 동기화 안함 | 고 | 저 | 유저 가이드에 명시, 수동 백업 안내 |

## 6.9 Open Questions

- [ ] Q2-1: 적 내부 분류 체계 (티어? 태그? 프리팹 네임?)
- [ ] Q2-2: `PlayerHealth.MaxHp` 가 readonly 인지 setter 가능한지
- [ ] Q2-3: Steam Cloud가 세이브 폴더 전체를 동기화하는지, 특정 파일만 하는지
- [ ] Q2-4: 퀘스트 완료 이벤트 훅 지점
- [ ] Q2-5: 레이드 종료 이벤트와 탈출/사망 구분 방법
- [ ] Q2-6: 세이브 슬롯 고유 ID 접근 방법

## 6.10 산출물

| 산출물 | 형태 |
|--------|------|
| `QuackForge.Progression.dll` | BepInEx 플러그인 |
| `docs/reverse-engineering/progression.md` | 역공학 노트 |
| `docs/xp-balance.md` | XP 밸런스 문서 |
| Git 태그 `v0.2.0-alpha.1` | 마일스톤 |

---

# Part VII. Phase 3 — 레벨링 시스템 확장

**기간**: 2주
**전제**: Phase 2 완료, XP 수집·세이브 파이프라인 안정적
**난이도**: Tier 4 (UI 포함)

## 7.1 Phase 3 목표

"유저가 레벨업 시 포인트를 획득하고, 인벤토리 화면의 캐릭터 탭에서 5종 스탯 중 원하는 곳에 분배할 수 있으며, 분배된 스탯이 실제 게임플레이에 반영된다."

## 7.2 Done 기준

- [ ] 레벨업 시 **3포인트** 지급 (설정 변경 가능)
- [ ] 5종 스탯 (Vitality / Strength / Agility / Precision / Survival) 분배 UI
- [ ] 각 스탯별로 실제 게임 내 수치가 변경됨 (Harmony 패치)
- [ ] 스탯 수치가 `quackforge.json` 에 저장/복원
- [ ] 레벨업 시각 알림 (토스트 + 사운드)
- [ ] BepInEx Configuration으로 스탯 효과량 조정 가능
- [ ] 스탯 초기화(리스팩) 옵션 — 아이템 or 설정 옵션
- [ ] Alpha 테스트 (본인 + 지인 2~3명)

## 7.3 기능 명세

### 7.3.1 5종 스탯 정의

| 스탯 | 축약 | 효과 | 1포인트당 |
|------|------|------|----------|
| **Vitality** | VIT | 최대 HP | +10 HP |
| **Strength** | STR | 캐리 용량 + 근접 데미지 | +0.5kg, +2% |
| **Agility** | AGI | 이동 속도 + 스태미나 | +1% 이속, +3 스태미나 |
| **Precision** | PRE | 사격 정확도 + 반동 감소 | +1.5% 정확도, -1% 반동 |
| **Survival** | SUR | 체력 회복 + 허기/갈증 감속 | +5% 회복, -3% 감소 |

각 스탯 최대 50포인트 (하드캡, 설정 가능).

### 7.3.2 포인트 지급 & 분배

**지급**:
- 레벨 1 → 2 올라갈 때 3포인트
- 레벨별 누적 가능 (미분배 포인트 보관)

**분배**:
- 인벤토리 화면의 "캐릭터" 탭에서
- 각 스탯 옆 `+` 버튼 → 미분배 포인트 -1, 해당 스탯 +1
- `Confirm` 버튼으로 확정 (취소 가능성 때문)

**리스팩**:
- 옵션 1: 설정에서 `AllowFreeRespec = true` → 언제든 리셋
- 옵션 2: "리스팩 토큰" 아이템 획득 시 (챌린지 모드 보상 후보)
- 옵션 3: 포인트 분배 전까지만 취소 가능 (확정 후 불가)

**결정**: 옵션 1 기본값 true (하드코어 모드 유저는 false로 설정)

### 7.3.3 UI 명세

**진입점**: 인벤토리 화면 (게임 내 I 키)

**탭 구조** (기존 탭에 "Character" 추가):
```
┌─────────────────────────────────────────┐
│ [Inventory] [Crafting] [Quests] [Char]  │  ← QuackForge 추가
├─────────────────────────────────────────┤
│                                         │
│  Level: 12          XP: 4,250 / 8,921   │
│  ■■■■■■□□□□  47%                        │
│                                         │
│  Unspent Points: 2                      │
│                                         │
│  Vitality     3  [+]    (+30 HP)        │
│  Strength     1  [+]    (+0.5kg cap)    │
│  Agility      2  [+]    (+2% speed)     │
│  Precision    4  [+]    (+6% accuracy)  │
│  Survival     2  [+]    (+10% heal)     │
│                                         │
│                         [Confirm]        │
└─────────────────────────────────────────┘
```

### 7.3.4 레벨업 알림 UI

- 화면 중앙에 "LEVEL UP!" 오버레이 (1.5초)
- 사운드 효과 (게임 내 기존 UI 사운드 재활용)
- 우상단에 "+3 points to spend" 뱃지 (클릭 시 캐릭터 탭으로 이동)

## 7.4 기술 설계

### 7.4.1 UI — UGUI 기반

덕코프가 사용하는 UI 스택 확인 필요 (UGUI vs UI Toolkit). **UGUI 가정**:

- `Canvas` 추가 (기존 인벤토리 Canvas 의 자식으로)
- `RectTransform`, `Button`, `Text`, `Image` 컴포넌트
- 프리팹 로드 or 런타임 GameObject 생성
- 이벤트 바인딩: `button.onClick.AddListener(...)`

**디자인 전략**:
- Phase 3 MVP: **기능 우선, 미적 완성도는 최소** (텍스트 + 버튼만)
- Phase 5 폴리싱: 색상, 아이콘, 애니메이션

### 7.4.2 스탯 효과 Harmony 패치 지점

각 스탯별 패치 지점:

| 스탯 | 패치 대상 | 방식 |
|------|----------|------|
| Vitality | `PlayerHealth.MaxHp` getter | Postfix, 보너스 추가 |
| Strength - 용량 | `Inventory.MaxWeight` getter | Postfix |
| Strength - 근접 | `MeleeAttack.Damage` | Postfix |
| Agility - 이속 | `PlayerMovement.Speed` getter | Postfix |
| Agility - 스태미나 | `PlayerStamina.Max` getter | Postfix |
| Precision - 정확도 | `Weapon.Accuracy` getter | Postfix |
| Precision - 반동 | `Weapon.Recoil` getter | Postfix |
| Survival - 회복 | `PlayerHealth.RegenRate` | Postfix |
| Survival - 허기 | `Hunger.DecayRate` | Postfix |

**패치 우선순위**:
- `[HarmonyPriority(Priority.Normal)]` 기본
- 다른 모드와 충돌 감지 시 조정

### 7.4.3 UI 모듈 구조

```
QuackForge.Progression.UI/
├── StatScreen.cs           # 메인 화면 컨트롤러
├── StatRow.cs              # 스탯 1줄 (이름 + 값 + 버튼)
├── LevelUpNotification.cs  # 팝업 알림
└── Prefabs/
    └── (런타임 생성, 프리팹 파일 불필요)
```

### 7.4.4 설정 확장

`BepInEx/config/com.returntrue.quackforge.cfg`:

```ini
[QuackForge.Progression.XP]
## XP per kill (tier 1 enemy)
XpPerKillTier1 = 10

## XP per kill (boss)
XpPerKillBoss = 500

## Multiplier on raid exit (e.g. 1.2 = +20%)
ExitBonusMultiplier = 1.2

[QuackForge.Progression.Stats]
## Points per level
PointsPerLevel = 3

## Max points per stat
MaxPointsPerStat = 50

## HP per Vitality point
HpPerVitality = 10

## Allow free respec (true/false)
AllowFreeRespec = true
```

## 7.5 작업 분해

### T3.1 UI 리버스 엔지니어링 (2일)
- 인벤토리 화면 어떤 Unity 클래스가 관리하는지 확인
- Canvas 구조, 탭 시스템 동작 방식
- `docs/reverse-engineering/ui.md`

### T3.2 스탯 수식 & 밸런스 (1일)
- 5종 스탯 효과량 결정
- 밸런스 시트 (엑셀/Google Sheets)
- `docs/stat-balance.md`

### T3.3 스탯별 Harmony 패치 (3일)
- 9개 패치 지점 구현
- 각각 단위 검증

### T3.4 Character 탭 UI (3~4일)
- `StatScreen` 구현
- 기존 인벤토리 탭 시스템에 통합
- 포인트 분배 로직

### T3.5 레벨업 알림 UI (1일)
- `LevelUpNotification` 구현
- 애니메이션 (간단한 Fade In/Out)

### T3.6 설정 확장 & 리스팩 (1일)
- Config 항목 추가
- 리스팩 기능 (설정 on/off에 따라)

### T3.7 통합 테스트 (1~2일)
- 저레벨 → 고레벨 시뮬레이션 (디버그 명령)
- UI 열기/닫기 반복, 메모리 누수 체크
- 저장/로드 정합성

### T3.8 Alpha 테스터 배포 (1일)
- 지인 2~3명에게 전달
- 피드백 수집 Form

### T3.9 v0.3.0 태깅 & CHANGELOG

## 7.6 주요 기술 결정

### 결정 3-1: UI 위치 — 인벤토리 탭 vs 별도 화면

**결정**: 인벤토리 탭 추가

**근거**:
- 기존 UI 흐름에 자연스럽게 녹아듦
- 별도 화면은 유저가 잊기 쉬움
- 인벤토리 UI 확장이 Harmony 패치로 가능할 것으로 예상

### 결정 3-2: 스탯 효과량 — 선형 vs 수확체감

**결정**: 선형 (MVP), Phase 5 이후 수확체감 실험

**근거**:
- 선형이 디버깅·밸런싱 단순
- 수확체감은 데이터 누적 후 커브 조정 (Phase 5)

### 결정 3-3: 리스팩 — 자유 vs 제한

**결정**: 기본값 자유, 설정으로 제한 가능

**근거**:
- 하드코어 유저 / 캐주얼 유저 모두 수용
- 기본값은 "실수로 잘못 찍어도 복구 가능" 원칙

## 7.7 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R3-1 | 인벤토리 UI 확장 불가 (하드코딩 탭) | 중 | 상 | 별도 팝업 창으로 폴백 |
| R3-2 | 스탯 패치가 장비 스탯 계산과 중첩 | 중 | 중 | 계산 순서 로깅, Priority 조정 |
| R3-3 | UI 성능 이슈 (프레임 드랍) | 저 | 저 | OnEnable 시에만 계산 |
| R3-4 | 리스팩이 세이브 무결성 깨뜨림 | 저 | 중 | 명시적 확인 dialog |

## 7.8 Open Questions

- [ ] Q3-1: 인벤토리 탭 시스템이 확장 가능한 구조인지
- [ ] Q3-2: UGUI 프리팹 vs 런타임 생성 어느 쪽이 편한지
- [ ] Q3-3: 게임 내 기존 사운드 리소스 접근 방법
- [ ] Q3-4: 장비 스탯과 캐릭터 스탯 합산 로직 위치

## 7.9 Alpha 테스트 계획

### 테스터 모집
- 개발자 본인 (50시간+)
- 덕코프 플레이 이력 있는 지인 2명
- Beta 단계 진입 조건: 크리티컬 버그 0, 최소 피드백 5건

### 피드백 수집 항목
1. 레벨업이 "보상답게" 느껴지는가
2. 스탯 효과가 실제 플레이에 체감되는가
3. 1포인트당 효과량이 적절한가 (너무 작음/큼)
4. UI가 이해하기 쉬운가
5. 버그 리포트 (재현 스텝 포함)

## 7.10 산출물

| 산출물 | 형태 |
|--------|------|
| `QuackForge.Progression.dll` v0.3.0 | 업데이트된 플러그인 |
| Alpha 테스트 피드백 문서 | `docs/alpha-feedback.md` |
| Stat UI 스크린샷 | `docs/screenshots/` |
| Git 태그 `v0.3.0-alpha.1` | 마일스톤 |

---

# Part VIII. Phase 4 — 챌린지 모드

**기간**: 2~3주
**전제**: Phase 3 완료, 레벨링 시스템 안정
**난이도**: Tier 2 (기존 맵 재활용) + Tier 4 (신규 게임 모드)

## 8.1 Phase 4 목표

"유저가 메인 메뉴에서 '챌린지 모드'를 선택하면, 기존 작은 맵에 보스 3종이 배치된 상태로 시작한다. 제한 시간 내 모든 보스를 처치하면 전용 무기 도면과 리스팩 토큰을 보상으로 받는다."

## 8.2 Done 기준

- [ ] 메인 메뉴에 "Challenge Mode" 진입점 추가
- [ ] 보스 러시 모드 1종 구현 (맵: 농장마을 기반)
- [ ] 보스 3종 순차 스폰 (보스1 처치 → 보스2 스폰 → 보스3)
- [ ] 제한 시간 10분
- [ ] 부활 2회 허용
- [ ] 클리어 시 전용 보상 풀에서 드랍
  - QuackForge 전용 무기 도면 1종 (랜덤 or 확정)
  - 리스팩 토큰 1개
- [ ] 클리어 기록 `quackforge.json` 에 저장 (베스트 타임)
- [ ] 챌린지 종료 후 기존 게임 상태에 영향 없음

## 8.3 Scope

### In Scope
- 챌린지 모드 1종 (Boss Rush - Farm)
- 기존 맵 재활용 (농장마을)
- 보스 3종 선정 (게임 내 기존 보스 재활용)
- 전용 보상 풀
- 메인 메뉴 UI 통합
- 클리어 기록 저장

### Out of Scope (v1.1+ 확장)
- 추가 챌린지 맵 (Boss Rush - Lab, Boss Rush - City)
- 리더보드
- 타임 어택 전용 모드
- 신규 맵 제작 (Tier 3)
- 협동 모드

## 8.4 기능 명세

### 8.4.1 챌린지 모드 명세

**Boss Rush - Farm (v1.0)**:

| 속성 | 값 |
|------|-----|
| 베이스 맵 | 농장마을 |
| 제한 시간 | 10분 |
| 보스 로스터 | 농장 보스 → 농장 보스 변종 → 농장 킹 (Open Question에서 확정) |
| 스폰 방식 | 순차 (이전 보스 처치 후 다음 보스) |
| 부활 횟수 | 2회 |
| 일반 적 | 없음 (보스 전용) |
| 일반 드랍 | 비활성화 (챌린지 오염 방지) |
| 보상 | 클리어 시에만 지급 |
| 진입 조건 | 플레이어 레벨 15+ (너무 이른 시도 방지) |

### 8.4.2 보상 설계

**클리어 시 확정 지급**:
- 리스팩 토큰 × 1
- QuackForge 전용 무기 도면 × 1 (챌린지 전용 풀에서)

**클리어 타임 보너스** (선택):
- 5분 이내: 추가 도면 × 1
- 3분 이내: 레어 무기 도면

**실패 페널티**: 없음 (챌린지는 부담 없이 재도전 가능하게)

### 8.4.3 메인 메뉴 통합

**진입점 위치**:
```
[Main Menu]
  - Continue
  - New Raid
  - Challenge Mode  ← QuackForge 추가
  - Settings
  - Quit
```

**선택 시 화면**:
```
┌──────────────────────────────────────┐
│  CHALLENGE MODE                       │
│                                       │
│  [ Boss Rush - Farm ]                 │
│    Best Time: 4:21                    │
│    Completions: 3                     │
│    Unlock: Level 15+ ✓               │
│    [Start]                            │
│                                       │
│  [ Boss Rush - Lab ] 🔒              │
│    Coming in v1.1                    │
│                                       │
│                    [Back]             │
└──────────────────────────────────────┘
```

## 8.5 기술 설계

### 8.5.1 GameMode 시스템 파악

덕코프 내부에 "게임 모드" 개념이 추상화되어 있는지 확인 필요.

**3가지 가능성**:
1. **추상화 있음** → GameMode 인터페이스 상속, 쉬운 통합
2. **하드코딩** → 기존 코드를 Harmony로 훅 + 상태 플래그 주입
3. **중간 지대** → 부분 확장 가능

**작업**: Rider로 `GameMode`, `RaidManager`, `Map` 키워드 검색.

### 8.5.2 기존 맵 재활용 전략

**선택지**:

| 전략 | 장점 | 단점 |
|------|------|------|
| A. 원본 맵 그대로 로드 + 적 스폰만 오버라이드 | 간단 | 농장마을 건물·아이템 전부 그대로 (챌린지엔 불필요) |
| B. 원본 맵 로드 + 영역 좁혀서 벽 세우기 | 컴팩트한 챌린지 공간 | 구현 복잡 |
| C. 원본 맵 그대로 + 텔레포트 장애물 | 단순 | 플레이어가 맵 밖으로 이탈 가능 |

**결정**: A 채택. MVP 단순성 우선. B는 Phase 5 폴리싱.

### 8.5.3 BossSpawner

```
QuackForge.Challenge.Spawning.BossSpawner
│
├── QueueBosses(bossIds: List<string>): void
├── SpawnNext(location: Vector3): void
├── OnBossDeath(boss: Enemy): void
│     ↓
│   if (queue empty) → ChallengeCompleted()
│   else → SpawnNext()
│
└── events:
    - BossSpawned
    - BossDefeated
    - ChallengeCompleted
```

### 8.5.4 챌린지 상태 관리

`ChallengeSession` 클래스:
- `StartTime: DateTime`
- `ElapsedSeconds: float`
- `RemainingLives: int`
- `DefeatedBosses: int`
- `Status: enum { Running, Completed, Failed }`

레이드 종료 시 (클리어/실패/이탈) `ChallengeSession.End()` → 세이브 + 메인 메뉴 복귀.

### 8.5.5 상태 격리 (중요)

챌린지 모드 시작 시 **원본 캐릭터 상태 스냅샷 저장**, 종료 시 복원.

**왜**: 챌린지 중 획득한 아이템이 원본 인벤토리에 섞이지 않아야 함. 사망도 원본 세이브에 영향 없어야.

**구현**:
```
ChallengeSession.Start()
  ↓
SnapshotManager.Save(currentPlayer) → tempState
  ↓
ChallengeLoadout.Apply(tempLoadout) — 챌린지 전용 장비
  ↓
... 챌린지 진행 ...
  ↓
ChallengeSession.End()
  ↓
SnapshotManager.Restore(tempState)
  ↓
if (completed) → Rewards.Grant() → 원본 인벤토리에 추가
```

## 8.6 작업 분해

### T4.1 GameMode 시스템 분석 (2일)
- 덕코프 내부 구조 확인
- 재활용/확장 경로 결정
- `docs/reverse-engineering/gamemode.md`

### T4.2 맵 로드 메커니즘 파악 (1일)
- `Map`, `SceneLoader`, `RaidManager` 분석

### T4.3 챌린지 모드 뼈대 (2일)
- `QuackForge.Challenge` 프로젝트 생성
- `ChallengeSession`, `BossSpawner` 기본 구조

### T4.4 상태 격리 시스템 (2~3일)
- `SnapshotManager` 구현
- 인벤토리/캐릭터 스냅샷
- 복원 로직 + 테스트

### T4.5 보스 스폰 로직 (2일)
- 순차 스폰 규칙
- 보스 데스 이벤트 훅
- 진행 상황 추적

### T4.6 메인 메뉴 UI (2일)
- 진입점 버튼 추가 (Harmony 패치)
- 챌린지 선택 화면

### T4.7 보상 시스템 (1~2일)
- 클리어 감지
- 보상 풀 정의 + 드랍
- 리스팩 토큰 아이템 신규 추가

### T4.8 통합 테스트 (2일)
- 풀 플로우 (메뉴 → 챌린지 → 클리어 → 보상 → 복귀)
- 중도 이탈 / 사망 시나리오
- 원본 세이브 오염 여부 확인

### T4.9 Alpha 테스터 확장 피드백 (1일)

### T4.10 v0.4.0 태깅

## 8.7 주요 기술 결정

### 결정 4-1: 맵 선정 — 농장마을 단일 vs 여러 개

**결정**: 농장마을 1종만 (v1.0)

**근거**:
- MVP 단순성
- 다른 맵 챌린지는 Phase 5 이후 확장
- 1종을 완성도 있게 > 3종을 적당히

### 결정 4-2: 보상 — 확정 vs 확률

**결정**: 확정 지급 + 타임 보너스 확률

**근거**:
- 도전 성공에 대한 확실한 보상 동기
- 타임 보너스로 리플레이 유도

### 결정 4-3: 부활 횟수 — 무제한 vs 제한 vs 영구사망

**결정**: 2회 (기본값), 설정으로 조정

**근거**:
- 무제한: 도전감 상실
- 영구사망: 진입 장벽 과도
- 2회: 실수 회복 가능하되 긴장감 유지

## 8.8 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R4-1 | GameMode 시스템이 하드코딩되어 확장 어려움 | 고 | 상 | Harmony 훅으로 상태 플래그 강제 주입 |
| R4-2 | 원본 세이브 오염 (상태 격리 실패) | 중 | 상 | 백업 자동 생성, 복원 보증 테스트 |
| R4-3 | 보스 AI가 맵 구조에 의존 (예상 밖 동작) | 중 | 중 | 보스별 스폰 위치 튜닝 |
| R4-4 | 10분 타이머가 UI 렌더링 부담 | 저 | 저 | 1초 단위 업데이트 |
| R4-5 | 메인 메뉴 UI 확장 불가 | 중 | 중 | 별도 진입 버튼 (인벤토리 탭 등)로 폴백 |

## 8.9 Open Questions

- [ ] Q4-1: 덕코프 GameMode / Scene 전환 메커니즘
- [ ] Q4-2: 챌린지 모드 전용 보스 AI 필요 여부
- [ ] Q4-3: 보스 로스터 확정 (기존 3종 선택)
- [ ] Q4-4: 리스팩 토큰을 어떤 아이템 카테고리로 등록할지
- [ ] Q4-5: 클리어 타임 정밀도 (초? 밀리초?)

## 8.10 산출물

| 산출물 | 형태 |
|--------|------|
| `QuackForge.Challenge.dll` | 신규 플러그인 |
| Challenge Mode 트레일러 영상 (옵션) | `docs/trailer/` |
| Git 태그 `v0.4.0-beta.1` | Beta 진입 |

---

# Part IX. Phase 5 — 배포 & 안정화

**기간**: 1~2주
**전제**: Phase 4 완료, 모든 주요 기능 구현
**난이도**: Tier 2 (CI/CD, 문서화 중심)

## 9.1 Phase 5 목표

"QuackForge를 Nexus Mods에 공개 배포하고, 초기 100명 다운로드 + 크리티컬 버그 0의 안정적인 상태로 v1.0을 출시한다."

## 9.2 Done 기준

- [ ] GitHub Actions CI로 Windows 빌드 자동화
- [ ] SemVer 기반 버전 관리
- [ ] 영문/한글 README 완성 (스크린샷 + GIF 포함)
- [ ] Nexus Mods 페이지 게시 (Description, Installation, FAQ)
- [ ] 릴리즈 번들 구조 (ZIP으로 바로 설치 가능)
- [ ] 출시 후 1주일 동안 Nexus 댓글 모니터링
- [ ] 크리티컬 버그 0건 유지
- [ ] (선택) Steam Workshop 버전도 준비

## 9.3 Scope

### In Scope
- CI/CD 셋업
- 문서 완성도 업
- 배포 패키지 제작
- Nexus Mods 업로드
- 커뮤니티 응대 (1주)

### Out of Scope
- Steam Workshop (별도 Phase 5.1)
- 추가 챌린지 맵
- 에셋 신규 제작
- 비영어/비한국어 번역

## 9.4 기능 명세

### 9.4.1 릴리즈 패키지 구조

```
QuackForge-v1.0.0.zip
├── BepInEx/
│   └── plugins/
│       └── QuackForge/
│           ├── QuackForge.Loader.dll
│           ├── QuackForge.Core.dll
│           ├── QuackForge.Data.dll
│           ├── QuackForge.Progression.dll
│           └── QuackForge.Challenge.dll
├── README.md
├── CHANGELOG.md
└── LICENSE
```

**유저 설치 방법**:
1. 덕코프에 BepInEx 5 설치
2. 위 ZIP을 덕코프 루트에 압축 해제
3. 게임 실행

### 9.4.2 버전 관리

**SemVer 규칙**:
- `MAJOR.MINOR.PATCH` + 선택적 `-prerelease`
- `1.0.0` — 첫 정식 출시
- `1.0.1` — 버그 수정
- `1.1.0` — 기능 추가 (호환 유지)
- `2.0.0` — 호환 깨지는 변경 (세이브 마이그레이션 필요)

**태그 규칙**:
```
v1.0.0          ← 정식
v1.0.0-rc.1     ← 릴리즈 후보
v1.0.0-beta.3   ← 베타
v1.0.0-alpha.1  ← 알파
```

### 9.4.3 Nexus Mods 페이지

**필수 섹션**:
- **Description** (게임 이름, 주요 기능 불릿, 스크린샷)
- **Features**
  - Level-based stat progression
  - Boss Rush Challenge Mode
  - 10+ new weapons and 5+ new armors
- **Installation** (BepInEx 설치법 링크 + 본 모드 설치)
- **FAQ** (자주 묻는 질문)
- **Known Issues**
- **Changelog**
- **Credits**

**시각 자료**:
- 메인 썸네일 이미지
- 스크린샷 5장+ (스탯 창, 레벨업, 챌린지 모드, 신규 무기 등)
- 게임플레이 GIF 2~3개 (선택)

### 9.4.4 CI/CD (GitHub Actions)

```
.github/workflows/build-release.yml
```

**트리거**: `v*` 태그 push 시

**단계**:
1. Windows runner에서 `dotnet build -c Release`
2. 산출 DLL들을 릴리즈 패키지 구조로 재배치
3. ZIP 압축
4. GitHub Release로 자동 업로드
5. (선택) Nexus 업로드 — API가 있으면

## 9.5 작업 분해

### T5.1 GitHub Actions 작성 (1일)
- `.github/workflows/build-release.yml`
- 태그 기반 트리거
- 로컬에서 수동 트리거도 지원 (`workflow_dispatch`)

### T5.2 릴리즈 스크립트 (0.5일)
- `scripts/build-release.sh` — 로컬 빌드 + ZIP 패키징
- 버전 자동 추출 (csproj의 Version → 파일명)

### T5.3 README 완성 (1~2일)
- 영문/한글 버전
- 스크린샷 5장 촬영
- GIF 2개 제작 (LICEcap/Kap)

### T5.4 문서 정리 (1일)
- `CHANGELOG.md`
- `LICENSE` (MIT 예정)
- `CONTRIBUTING.md` (기여자용)

### T5.5 Nexus Mods 페이지 작성 (1일)
- 계정 생성 / 로그인
- 모드 페이지 생성
- 모든 메타데이터 입력
- v0.5.0-rc.1 업로드 (Hidden)

### T5.6 내부 RC 테스트 (1일)
- Alpha 테스터로 최종 확인
- 설치 플로우 전체 재현
- 발견 이슈 즉시 수정

### T5.7 v1.0.0 공개 (당일)
- Git 태그 `v1.0.0`
- Nexus 모드 Visibility → Public
- Reddit /r/EscapeFromDuckov 및 디스코드에 공지

### T5.8 1주 모니터링 (7일)
- Nexus 댓글/버그 리포트 확인 (매일)
- 크리티컬 이슈 발생 시 1.0.1 패치
- 피드백 수집 → v1.1 로드맵

## 9.6 주요 기술 결정

### 결정 5-1: 라이선스

**결정**: MIT License

**근거**:
- 가장 허용적 (다른 모더가 참고·포크 가능)
- 비상업적 제한 없이 기여 친화적
- 프로젝트문 같은 게임 스튜디오 소스공개 문화 참조

### 결정 5-2: 배포 우선순위 — Nexus vs Workshop

**결정**: Nexus 먼저, Workshop은 1주 안정화 후

**근거**:
- Nexus: 버전 관리·업데이트 자유도 높음
- Workshop: 유저 도달 범위 최대, 단 규정 엄격 (유료 금지 등 모더 카르텔 이슈 참조)
- 먼저 Nexus에서 버그 거르고 Workshop 배포

### 결정 5-3: 지원 정책

**결정**: 게임 업데이트 후 7일 이내 호환성 패치 목표, 나머지는 Best Effort

**근거**:
- Solo 프로젝트 지속 가능성
- 유저 기대치 명시적 관리

## 9.7 리스크

| ID | 리스크 | 확률 | 영향 | 대응 |
|----|--------|------|------|------|
| R5-1 | 출시 직후 크리티컬 버그 | 중 | 상 | RC 단계 철저한 테스트, 빠른 패치 준비 |
| R5-2 | 모더 카르텔 정치 (나무위키 언급) | 저 | 중 | 완전 무료, 오픈소스, 커뮤니티 친화적 입장 유지 |
| R5-3 | 덕코프 업데이트로 즉시 호환 깨짐 | 중 | 상 | 게임 버전 감지 + 자동 비활성화 |
| R5-4 | 초기 사용자 낮음 (인지도 문제) | 고 | 저 | Reddit/Discord 홍보, 트레일러 GIF |
| R5-5 | 유저 지원 부하 과다 | 중 | 중 | FAQ 충실화, 이슈 템플릿 제공 |

## 9.8 Open Questions

- [ ] Q5-1: Nexus API로 자동 업로드 가능한지
- [ ] Q5-2: BepInEx 팩 동시 배포 전략 (Vortex Extension 활용)
- [ ] Q5-3: 한국 커뮤니티 홍보 채널 (인벤? 디시?)
- [ ] Q5-4: 기여자 정책 (PR 수락 기준)

## 9.9 산출물

| 산출물 | 형태 |
|--------|------|
| `QuackForge-v1.0.0.zip` | 릴리즈 패키지 |
| GitHub Release | `v1.0.0` 태그 |
| Nexus Mods 페이지 | Public |
| CHANGELOG.md | 전 버전 히스토리 |

## 9.10 Post-Launch (v1.0 이후)

**v1.1 예상 항목** (1~2개월 내):
- Boss Rush - Lab 챌린지 맵 추가
- 스탯 수확체감 커브 도입 (데이터 기반)
- 다국어 지원 (중국어/일본어, 커뮤니티 번역)
- Steam Workshop 배포

**v2.0 원대한 목표** (장기):
- Unity Editor 기반 AssetBundle 제작 능력 확보
- 신규 3D 모델 무기/방어구
- 신규 맵 제작
- 협동 챌린지 모드 (싱글 전용 게임 한계 내에서)

---

# Part X. 리스크 레지스터

전 Phase에 걸친 리스크를 한 곳에 모아 관리. 각 리스크는 Phase별 상세 PRD에서 이미 정의됨.

## 10.1 Top 10 프로젝트 리스크 (우선순위순)

| 순위 | ID | 리스크 | Phase | 확률 | 영향 | 상태 |
|------|-----|--------|-------|------|------|------|
| 1 | R0-1 | VM에서 덕코프 실행 불가 | 0 | 중 | 상 | 🔴 Unresolved |
| 2 | R2-1 | 적 처치 이벤트 훅 지점 파악 실패 | 2 | 중 | 상 | 🔴 Unresolved |
| 3 | R4-1 | GameMode 시스템 하드코딩 | 4 | 고 | 상 | 🔴 Unresolved |
| 4 | R2-5 | 세이브 슬롯 ID 추적 불가 | 2 | 중 | 상 | 🔴 Unresolved |
| 5 | R5-3 | 덕코프 업데이트로 호환 깨짐 | 5+ | 중 | 상 | 🟡 상시 리스크 |
| 6 | R1-1 | Assembly-CSharp 난독화 | 1 | 저 | 상 | 🟡 검증 필요 |
| 7 | R1-2 | 공식 Mod API 무기 추가 미지원 | 1 | 중 | 중 | 🟡 검증 필요 |
| 8 | R3-1 | 인벤토리 UI 확장 불가 | 3 | 중 | 상 | 🟡 검증 필요 |
| 9 | R4-2 | 원본 세이브 오염 | 4 | 중 | 상 | 🟡 설계로 완화 |
| 10 | R5-1 | 출시 직후 크리티컬 버그 | 5 | 중 | 상 | ⚪ 미도달 |

## 10.2 리스크 카테고리별 분류

### 카테고리 A: 게임 내부 구조 불투명성
- R1-1, R1-2, R2-1, R2-5, R3-1, R4-1, R4-2
- **공통 대응**: Rider 디컴파일러 적극 활용 + 복수 후보 지점에 실험적 패치

### 카테고리 B: 환경 / 인프라
- R0-1, R0-2, R0-3
- **공통 대응**: 폴백 경로 사전 정의 (VMware → Parallels → 실기 PC)

### 카테고리 C: 게임 업데이트 호환성
- R5-3 (상시)
- **공통 대응**: 버전 감지 + 자동 비활성화 + 7일 내 패치 정책

### 카테고리 D: 생태계 / 커뮤니티
- R5-2 (모더 정치), R5-4 (인지도), R1-5 (타 모드 충돌)
- **공통 대응**: 무료·오픈소스·네임스페이스 엄격 준수

## 10.3 리스크 모니터링 주기

- **매주 회고**: 리스크 상태 업데이트 (🔴→🟡→🟢)
- **Phase 종료 시점**: 해당 Phase 리스크 클로즈, 다음 Phase 리스크 활성화
- **크리티컬 리스크(🔴)**: 주 2회 체크

---

# Part XI. 부록

## Appendix A. 통합 Open Questions

Phase별 Open Questions 전체 목록 (총 22개). 상단이 우선순위 높음.

### A.1 긴급 (Phase 0~1 중 해소 필수)

- [ ] Q0-1: UTM VirtFS가 실제로 VM에서 안정적인가? — **Phase 0 T0.9**
- [ ] Q0-2: Mac 덕코프가 Apple Silicon 네이티브인지 x86 Rosetta인지?
- [ ] Q1-1: 공식 Mod API가 제공하는 정확한 커버리지
- [ ] Q1-2: `Weapon.baseModel` 필드가 실제 존재하는지
- [ ] Q2-1: 적 내부 분류 체계 (티어? 태그? 프리팹 네임?)
- [ ] Q2-2: `PlayerHealth.MaxHp` 가 readonly 인지 setter 가능한지
- [ ] Q2-6: 세이브 슬롯 고유 ID 접근 방법

### A.2 중요 (Phase 2~3 진입 전)

- [ ] Q0-3: BepInEx Mac 빌드와 덕코프 Mac 클라이언트 호환성 검증됨?
- [ ] Q1-3: 도면 아이템을 어떤 아이템 카테고리로 분류할지
- [ ] Q1-5: 초기 10종 무기 리스트 확정
- [ ] Q2-3: Steam Cloud가 세이브 폴더 전체를 동기화하는지
- [ ] Q2-4: 퀘스트 완료 이벤트 훅 지점
- [ ] Q2-5: 레이드 종료 이벤트와 탈출/사망 구분 방법
- [ ] Q3-1: 인벤토리 탭 시스템이 확장 가능한 구조인지
- [ ] Q3-4: 장비 스탯과 캐릭터 스탯 합산 로직 위치

### A.3 일반 (필요 시 해소)

- [ ] Q0-4: Rider 라이선스 경로
- [ ] Q1-4: 다국어 지원 범위
- [ ] Q3-2: UGUI 프리팹 vs 런타임 생성
- [ ] Q3-3: 게임 내 기존 사운드 리소스 접근 방법
- [ ] Q4-2: 챌린지 모드 전용 보스 AI 필요 여부
- [ ] Q4-3: 보스 로스터 확정
- [ ] Q4-4: 리스팩 토큰 아이템 카테고리
- [ ] Q4-5: 클리어 타임 정밀도
- [ ] Q5-1: Nexus API로 자동 업로드 가능한지
- [ ] Q5-2: BepInEx 팩 동시 배포 전략
- [ ] Q5-3: 한국 커뮤니티 홍보 채널
- [ ] Q5-4: 기여자 정책

## Appendix B. 용어집

| 용어 | 정의 |
|------|------|
| **Addressables** | Unity의 비동기 에셋 로딩 시스템. 덕코프가 사용 중 |
| **AssetBundle** | Unity 에셋을 외부 파일로 분리해 런타임 로드 |
| **BepInEx** | Unity 게임용 커뮤니티 모드 로더 |
| **Blueprint** | QuackForge 용어. 제작을 해금하는 도면 아이템 |
| **Boss Rush** | 여러 보스를 순차 처치하는 챌린지 모드 |
| **dnSpy** | .NET 디컴파일러 (Windows 전용). Rider가 Mac 대체 |
| **Extraction Shooter** | 탐험 → 파밍 → 탈출 게임 장르. 덕코프/타르코프가 속함 |
| **Harmony / HarmonyX** | .NET 런타임 메서드 패치 라이브러리 |
| **Mod API** | 게임사가 공식 제공하는 모드 개발 규격. 덕코프는 Workshop 통합 |
| **QuackForge** | 본 프로젝트 코드네임 |
| **Respec** | 이미 분배한 스탯 포인트를 초기화하고 재분배 |
| **SemVer** | Semantic Versioning. MAJOR.MINOR.PATCH |
| **Tier** | 난이도/복잡도 등급. 1(쉬움) ~ 4(어려움) |
| **VirtFS** | UTM의 공유 폴더 프로토콜 |
| **Workbench** | 덕코프 게임 내 제작 시설 |
| **XP** | Experience Points. 레벨업 위한 경험치 |

## Appendix C. 레퍼런스 링크

### 필수 참고

| 분류 | 자료 | URL |
|------|------|-----|
| 모드 | VanillaAttachmentsExpanded | nexusmods.com/escapefromduckov/mods/51 |
| 모드 | devopsdinosaur/duckov-mods | github.com/devopsdinosaur/duckov-mods |
| 도구 | BepInEx 공식 | docs.bepinex.dev |
| 도구 | HarmonyX Wiki | github.com/BepInEx/HarmonyX/wiki |
| 도구 | Vortex Extension | nexusmods.com/site/mods/1517 |
| 공식 | Team Soda Mod Guide | steamcommunity.com/games/3167020/announcements |
| 레퍼런스 | Duckov Nexus (전체 모드) | nexusmods.com/games/escapefromduckov |
| 레퍼런스 | DuckovHub | duckovhub.com |

### 학습 자료

| 주제 | 자료 |
|------|------|
| BepInEx 플러그인 튜토리얼 | `docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html` |
| Harmony 사용법 | Harmony wiki의 Patching 섹션 |
| Unity UGUI | Unity 공식 Manual |
| C# for Java devs | Microsoft Learn "C# for Java devs" |

## Appendix D. 작업 시간 누계 예상

| Phase | 기간 (주) | 예상 시간 | 누계 |
|-------|----------|----------|------|
| 0 | 0.5 | 15h | 15h |
| 1 | 1.5 | 60h | 75h |
| 2 | 2.5 | 100h | 175h |
| 3 | 2 | 80h | 255h |
| 4 | 2.5 | 100h | 355h |
| 5 | 1.5 | 45h | 400h |
| **Total** | **10.5주** | **400h** | — |

**가정**: 주말 10h + 평일 저녁 5h = 주당 평균 35~40h. Solo 작업 기준.

**실제로는 역공학 작업이 30~40% 비중**. Phase 1 T1.2, Phase 2 T2.1, Phase 3 T3.1, Phase 4 T4.1 이 "알고 있는 C# 개발" 영역 밖이라 예측이 어려움. 20% 버퍼 포함한 현실적 추정: **480~500h**.

## Appendix E. Claude Code 워크플로우 제안

프로젝트 특성상 Claude Code와 `@personas` 조합이 효율적:

| 작업 | 주 페르소나 | 보조 페르소나 |
|------|------------|--------------|
| Phase 0 셋업 | @devops | @task |
| Phase 1 데이터 설계 | @senior | @pm |
| Phase 2 역공학 | @learner | @mentor |
| Phase 2~3 레벨링 구현 | @senior | @review |
| Phase 3 UI 설계 | @senior | @qa |
| Phase 4 게임 모드 | @lead | @security |
| Phase 5 배포 | @devops | @pm |

**Claude Code 슬래시 명령 활용 제안**:
- `/plan [Phase]` — 해당 Phase 상세 태스크 브레이크다운
- `/review src/QuackForge.Progression/` — 코드 리뷰 요청
- `/decompile Weapon` — 역공학 결과 문서화
- `/test-plan [Phase]` — 테스트 시나리오 생성

## Appendix F. 문서 버전 이력

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| v1.0 | 2026-04-17 | 초안 작성 (단일 PRD) | N |
| v2.0 | 2026-04-17 | 전 Phase 상세 PRD 통합 | N |

## Appendix G. Phase 0 즉시 실행 체크리스트

이 문서를 덮는 순간 실행할 항목:

1. [ ] `brew install --cask dotnet-sdk`
2. [ ] Rider 설치 및 라이선스 활성화
3. [ ] GitHub 비공개 레포 `quackforge` 생성
4. [ ] 덕코프 Mac 경로 확인
5. [ ] BepInEx Mac 빌드 다운로드 → 설치 → 깡통 실행 확인
6. [ ] UTM 설치 → Windows 11 ARM 평가판 VM 생성
7. [ ] VM에 Steam + 덕코프 설치 → **실행 성공 확인 (최대 리스크)**
8. [ ] 본 PRD에 첨부된 `src/QuackForge.Loader/` 파일들 배치
9. [ ] `dotnet restore && dotnet build -c Release`
10. [ ] `./scripts/deploy-mac.sh` 실행 → 덕코프 실행 → 로그 확인
11. [ ] Git `v0.0.1-phase0` 태그 + push

위 11단계 완료 시 **Phase 1 킥오프 가능 상태**.

---

## Closing Notes

이 문서는 **Solo 개발자가 Mac 환경에서 10~13주 동안 덕코프 모드를 설계·개발·배포하는 로드맵**이다. 핵심 철학은:

1. **Spring 백엔드 개발 경험을 C#/Unity에 그대로 투영** — 네이밍, 모듈 분리, AOP, 버전 관리 원칙 모두 기존 지식 재활용
2. **Tier 3(3D 에셋)은 v1.0에서 배제** — 혼자서 감당 불가능한 영역 차단
3. **역공학이 가장 큰 미지수** — Phase 1~4 각각에 역공학 전담 태스크 배치
4. **배포는 Nexus 우선, Workshop 후순위** — 버그 감지와 모더 정치 분리
5. **Mac 메인 + VM QA + 실기 PC 릴리즈** — 3단 파이프라인으로 품질 보증

문서가 길지만, 각 Phase는 **독립적으로 읽고 실행 가능**하도록 구성. Phase 0 완료 후 Phase 1로 넘어갈 때 §Part V만 다시 펼쳐도 충분하다.

행운을 빕니다. 🦆

— End of PRD v2.0 —




