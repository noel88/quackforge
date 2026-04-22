# AssetBundle Workflow — #65 가이드

**대상**: 솔로 개발자, Unity 경험 적거나 없음
**목표**: Duckov 가 로드할 수 있는 Mac/Windows AssetBundle 을 QuackForge 저장소에서 반복 가능한 명령으로 빌드
**선행 조건**: #65 결정 확정 후 (Phase 2 와 병행 진행)

## 1. Unity 에디터 설치 (사용자 수동)

### 버전
덕코프 런타임 `2022.3.62f2` 와 정확히 일치하는 editor 는 **2022.3.62f1** (f2 에디터 빌드 미공개). LTS 내에서 근접하면 AssetBundle 호환 OK.

### Unity Hub 설치 옵션
```
Editor        : Apple silicon (M-series 네이티브)
Build Support : Mac Build Support (Mono)       [필수]
                Windows Build Support (Mono)   [권장 — VM 테스트용]
기타          : 전부 해제
```

### 라이선스
Personal (무료) — 연매출/외부 펀딩 10만 USD 미만 조건 자동 충족.

## 2. 저장소 Unity 프로젝트 레이아웃

```
quackforge/
├── unity-project/                    # Unity 프로젝트 루트 (#65 에서 추가)
│   ├── Assets/
│   │   ├── Mods/
│   │   │   └── QuackForge/          # 모드 단위 AssetBundle 태그
│   │   │       ├── Weapons/         # weapon prefab 들
│   │   │       ├── Armors/
│   │   │       └── Icons/
│   │   └── Editor/
│   │       └── BuildAssetBundles.cs # 빌드 스크립트
│   ├── Packages/
│   │   └── manifest.json
│   └── ProjectSettings/
├── mod/                              # 빌드 산출물 배포 레이아웃 (Part A)
│   └── quackforge/
│       ├── info.ini                 # 매니페스트 (포맷 미확정, #65 실증 중)
│       ├── preview.png
│       ├── mac/
│       │   └── quackforge_mac.bundle
│       └── windows/
│           └── quackforge_win.bundle
└── scripts/
    └── build-mod.sh                 # unity-project → mod/ 빌드 파이프라인
```

**.gitignore 추가 항목 (`unity-project/` 하위)**
```
Library/
Temp/
Logs/
UserSettings/
Build/
obj/
*.csproj
*.sln
*.user
.vs/
```

## 3. 테스트 무기 1개 (minimal reproducible example)

**목표**: `quackforge_weapon_pistol_sidekick` 아이템 ID 를 콘솔에서 스폰하면 인벤토리에 실제 아이템이 들어온다.

### 단계
1. Unity 프로젝트 생성 (Unity Hub → New project → 3D → **Unity 2022.3.62f1**)
2. 저장 위치: `quackforge/unity-project/` 로 지정
3. `Window > Package Manager` 에서:
   - `com.unity.editorcoroutines` (AssetBundle 빌드에 유용)
4. `Assets/` 아래 폴더 구조 생성 (위 참조)
5. Duckov 의 기존 pistol prefab 을 Asset Ripper 등으로 추출 → 복제본을 `Assets/Mods/QuackForge/Weapons/` 에 배치 (원본 그대로 올리지 말 것 — 재배포 금지 에셋)
6. 복제 prefab 의 `Item.typeID` 를 **모드 전용 범위** (예: `200001`) 로 변경
7. `Stats` 컴포넌트 수치를 우리 JSON (`quackforge_weapon_pistol_sidekick`) 과 일치하게 설정
8. **AssetBundle 태그**: Inspector 하단 → `AssetBundle` 드롭다운 → `quackforge_mac` (또는 `quackforge_win`) 로 지정
9. `Assets/Editor/BuildAssetBundles.cs` 실행 (메뉴 항목 `QuackForge > Build AssetBundles`)
10. 빌드 산출물을 `mod/quackforge/mac|windows/` 로 복사
11. 덕코프 `Duckov_Data/Mods/quackforge/` 에 배포 → 게임 실행 → `Mods` 메뉴에서 활성화
12. 인게임 콘솔에서 `/spawn quackforge_weapon_pistol_sidekick` (실제 명령은 #65 역공학에서 확정)

### 남은 불확실성
- **매니페스트 포맷**: `info.ini` 인지 `mod.json` 인지 Steam 공지 본문 확인 필요
- **typeID 범위**: 모드 전용 예약 범위가 공식 문서에 있는지 확인
- **AssetBundle 타겟 플랫폼**: Mac arm64 / x64 / Windows x64 각각 따로 빌드 필요한지 단일 번들 가능한지
- **Base prefab 참조 방식**: 완전 복제 vs 부모 prefab 링크 (게임 업데이트 내성 차이)

이 4개는 테스트 무기 1개 end-to-end 검증 과정에서 실증 → `docs/reverse-engineering/mod-assetbundle.md` 로 정리 예정.

## 4. 빌드 스크립트 템플릿

### 4.1 Unity Editor 쪽 (`Assets/Editor/BuildAssetBundles.cs`)

```csharp
using UnityEditor;
using System.IO;

public static class BuildAssetBundles
{
    [MenuItem("QuackForge/Build AssetBundles (Mac)")]
    public static void BuildMac()
    {
        Build("../mod/quackforge/mac", BuildTarget.StandaloneOSX);
    }

    [MenuItem("QuackForge/Build AssetBundles (Windows)")]
    public static void BuildWindows()
    {
        Build("../mod/quackforge/windows", BuildTarget.StandaloneWindows64);
    }

    private static void Build(string outputPath, BuildTarget target)
    {
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            target);
        AssetDatabase.Refresh();
    }
}
```

### 4.2 셸 쪽 (`scripts/build-mod.sh`)

```bash
#!/usr/bin/env bash
# Headless Unity batchmode AssetBundle 빌드.
# CI / 일괄 릴리즈 빌드용. 평소 개발은 Unity Editor 메뉴로 충분.
set -euo pipefail

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity}"
PROJECT="$(cd "$(dirname "$0")/.." && pwd)/unity-project"

"$UNITY_BIN" -batchmode -nographics -quit \
    -projectPath "$PROJECT" \
    -executeMethod BuildAssetBundles.BuildMac \
    -logFile -
```

## 5. 배포 통합

데이터 JSON (이미 embedded 로 있음) + AssetBundle 번들 + info.ini 를 하나의 `.zip` 으로 묶어 **Nexus** 와 **Steam Workshop** 동시 배포. 기존 `scripts/build-release.sh` 에 번들 포함 로직 추가 (#65 후반).

## 6. 검증 체크리스트

- [ ] Unity Editor 2022.3.62f1 설치
- [ ] `unity-project/` 생성 + .gitignore 추가
- [ ] 테스트 무기 prefab 1개 (typeID 200001) AssetBundle 빌드
- [ ] 덕코프 `Duckov_Data/Mods/` 에 배포
- [ ] 게임 인게임 `Mods` 메뉴에서 QuackForge 토글 가능
- [ ] 콘솔 스폰으로 아이템 인벤토리 진입
- [ ] 스탯 값이 JSON 대로 반영 (damage/fireRate 등)
- [ ] QuackForge.Data.WeaponRegistry 가 AssetBundle 의 typeID 와 매칭 확인 (브릿지 Harmony 패치 후속)

## 7. 이후 단계

- **#65 클로즈**: 테스트 무기 1개 end-to-end 검증 완료
- **후속 이슈**: 나머지 9 무기 + 5 방어구 prefab AssetBundle 일괄 빌드 + `BlueprintRegistry.Unlock` → `CraftingManager.UnlockFormula` 브릿지
- **Phase 4 (챌린지 모드)** 와 병렬 진행 가능 — AssetBundle 경로는 challenge 모드의 보상 아이템에도 그대로 재활용
