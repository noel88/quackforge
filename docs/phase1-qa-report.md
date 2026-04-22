# Phase 1 QA Report — v0.1.0-alpha.1

**작성일**: 2026-04-22
**대상 빌드**: QuackForge 0.1.0-alpha.1
**관련 이슈**: #16 (Mac 통합 테스트), #17 (VM QA)

## 1. 환경

| 플랫폼 | 버전 |
|---|---|
| macOS Host | Darwin 25.4.0 (arm64, Apple M1 Max) |
| .NET SDK | 10.0.201 (netstandard2.1 타겟) |
| BepInEx (Mac) | 5.4.23.5 macos_universal |
| BepInEx (Win) | 5.4.23.5 win_x64 |
| Duckov | v2.2.0 (Unity 2022.3.62f2) |
| VM | Parallels Desktop + Windows 11 ARM |

## 2. Mac 통합 테스트 (#16)

### 검증 경로

| 단계 | 결과 |
|---|---|
| `dotnet build -c Release` | ✅ 0 warn, 0 error |
| `scripts/deploy-mac.sh` | ✅ 24개 DLL 배포 (QuackForge.* + 추이 deps) |
| `scripts/run-duckov-mac.sh` | ✅ Rosetta x86_64 + DYLD 재주입 + BepInEx 로드 |
| BepInEx 초기화 | ✅ `BepInEx 5.4.23.5 - Duckov` |
| Unity 감지 | ✅ `Detected Unity version: v2022.3.62f2` |
| 플러그인 로드 | ✅ `Loading [QuackForge 0.1.0]` (BepInPlugin attribute 는 numeric Version 만 허용, alpha suffix 는 git tag + CHANGELOG 에서만) |
| QfCore 초기화 | ✅ `[Core] QfCore initialized.` |
| 무기 레지스트리 | ✅ `weapon registry ready — 10 entries from 10 resources.` |
| 방어구 레지스트리 | ✅ `armor registry ready — 5 entries from 5 resources.` |
| 블루프린트 레지스트리 | ✅ `blueprint registry ready — 10 entries, 0 unlocked.` |
| 메인 진입 | ✅ `🦆 QuackForge is awake. Forging begins.` |
| 종료 | ✅ `QuackForge unloaded.` |

### 미검증 (Part B 데이터-only 범위 밖)

- ❌ 콘솔 스폰 → Item Prefab 필요, AssetBundle 경로 필수 (#65 후속)
- ❌ 워크벤치 제작 → `CraftingFormulaCollection` SO 병합 필요 (#65)
- ❌ VanillaAttachmentsExpanded 공존 (R1-5) → 모드 대상 AssetBundle 진입 후 재평가

**결론**: **데이터 파이프라인은 완결**. 게임 내 실제 스폰/제작은 AssetBundle 경로 확립 후 #65 후속 이슈에서 검증.

## 3. VM QA (#17)

### 검증 경로

| 단계 | 결과 |
|---|---|
| `install-bepinex-vm.ps1` | ✅ `BepInEx_win_x64_5.4.23.5` 자동 다운로드/설치 |
| `link-plugin-vm.ps1` | ✅ `LinkType: SymbolicLink`, Target UNC Mac 경로 |
| `run-duckov-vm.ps1` (Steam 경유) | ✅ 게임 실행 성공 |
| BepInEx 초기화 | ✅ `BepInEx 5.4.23.5 - Duckov` + `System platform: Bits64, Windows` |
| Unity 감지 | ✅ `Detected Unity version: v2022.3.62f2` |
| Mac 빌드 DLL 즉시 반영 | ✅ Mac `dotnet build` → VM 덕코프 재시작만으로 업데이트 |
| QfCore + 3 registries | ✅ Mac 와 동일 (10/5/10) |
| FPS (x64 에뮬레이션) | ✅ **평균 70-80 FPS** (R0-1 기준 30 이상 넉넉히 통과) |

### 프레임 드랍 리포트
- 메인 메뉴·인벤토리·레이드 진입 구간 모두 60-80 FPS 구간 유지.
- Parallels x64 에뮬이 Unity mono 와 BepInEx winhttp proxy DLL 을 무리 없이 소화.
- 대규모 전투나 많은 엔티티 상황은 Phase 2 이후 추가 계측 필요.

### 회귀 이슈
- 게임 종료 시 `[Info : Console] Error: Index was out of range. startIndex` 발생 — `QuackForge unloaded` 이후라 우리 코드 아닌 게임 내부 셧다운 이슈. 모니터링 대상.

## 4. Open Questions (Phase 2 이월)

| ID | 질문 | 영향 |
|---|---|---|
| Q-PRD-UNITY | Unity 2022.3.5f1 → 2022.3.62f2 (PRD 초판 시점 대비 게임 업데이트) | 마이너, 호환 확인됨 |
| Q-EXP-REUSE | `Duckov.EXPManager` 가 이미 존재 — 별도 궤도 vs 훅 확장 결정 | Phase 2 설계에 직접 영향 |
| Q-ITEM-AB | AssetBundle 플랫폼 분리 빌드 필요 여부 (Mac arm64/x64, Win x64) | Phase 1 AssetBundle 후속 (#65) |
| Q-MANIFEST | 공식 Mod 매니페스트 포맷 (info.ini vs mod.json vs 번들 메타) | #65 |
| Q-FORMULA-MERGE | `CraftingFormulaCollection` 신규 formula 병합 공식 경로 | #65 |

## 5. Phase 1 완료 기준 재평가

PRD 의 Phase 1 Done:
- [x] 무기 10 / 방어구 5 / 도면 10 JSON 정의 + embedded resource 로더 + 검증
- [x] Core 인프라 (Logger / Config / EventBus / SaveContext) 실장
- [x] Mac + VM 양쪽 환경에서 BepInEx + 플러그인 로드 end-to-end 검증
- [x] 해금 상태 persist + 이벤트 발행 인프라 (Phase 2/4 대기)
- [ ] **게임 내 실제 아이템 스폰/장착/제작** → **#65 (AssetBundle)** 로 이관

**판정**: Phase 1 데이터·인프라 영역 **alpha 준비 완료**. 시각적 실증은 AssetBundle 확립 후 v0.1.0 (non-alpha) 혹은 v0.2 에 포함.
