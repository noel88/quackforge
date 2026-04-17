# VM Setup Guide (Parallels Desktop)

Phase 0 인프라 이슈 중 VM 내부에서 수동으로 해야 하는 단계를 모은 체크리스트. UTM 대신 Parallels Desktop을 사용합니다 (R0-1 리스크 완화).

관련 이슈: #4 (VM 덕코프), #5 (VM BepInEx), #9 (Mac↔VM 파일 공유)

## 전제

- [x] Parallels Desktop 설치 (macOS 호스트)
- [x] Windows 11 ARM VM 생성 (4 vCPU / 8 GB / 동적 디스크)
- [x] Parallels Tools 설치 (현재 v26.2.2)
- [x] Host/Guest Shared Folders 기능 활성화

호스트에서 `prlctl list --info "Windows 11"` 로 재확인 가능.

---

## T0.4 — VM 덕코프 설치 (R0-1 최대 리스크)

### 수동 단계 (VM 안)

1. VM 부팅: `prlctl start "Windows 11"` 또는 Parallels Desktop에서 재개
2. Windows Update 1회 적용
3. Steam 설치 → 로그인
4. Escape from Duckov 설치
5. 게임 메인 메뉴 진입 테스트
6. 1회 레이드 입장 + FPS 계측 (Steam 오버레이 F12 또는 PresentMon)

### Done

- [ ] 메인 메뉴 진입 성공
- [ ] 레이드 입장 성공
- [ ] 평균 **30 FPS 이상** 유지

### 폴백 (실패 시)

1. Parallels 비디오 설정 조정 (3D 가속 highest 확인, 비디오 메모리 수동 지정)
2. Hypervisor 타입 `apple ↔ parallels` 교체 시도
3. 실기 PC로 QA 대체

---

## T0.5 — VM BepInEx 설치

### 사전 조건

- T0.4 완료 (덕코프 VM 설치 완료)

### 수동 단계 (VM 안)

1. `BepInEx_win_x64_5.4.21.0.zip` 다운로드
   - URL: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21
2. 덕코프 설치 폴더에 압축 해제
   - 기본 경로: `C:\Program Files (x86)\Steam\steamapps\common\Escape From Duckov\`
3. 덕코프 1회 실행 → 메인 메뉴 진입 후 종료
4. `BepInEx\LogOutput.log` 확인

### Done

- [ ] LogOutput.log 첫 줄에 `BepInEx 5.4.21.0 - Duckov` 메시지 확인

---

## T0.9 — Mac↔VM 파일 공유

### 목표

Mac에서 빌드한 `QuackForge.Loader.dll` 이 VM 덕코프 `BepInEx/plugins/` 에 자동 반영되어야 함.

### 기본 옵션 — Parallels Shared Folders

1. **호스트 (Mac) 준비**
   - Parallels Desktop → VM 설정 → `옵션 → 공유 → 공유 폴더`
   - "사용자 폴더 매핑 → Home" 또는 특정 폴더만 공유 설정
   - 권장: 빌드 출력 폴더 `~/Desktop/Project/side-project/quackforge/src/QuackForge.Loader/bin/Release` 만 공유
2. **VM (Windows) 확인**
   - 탐색기에서 `\\Mac\Home\Desktop\Project\side-project\quackforge\...` 접근 가능해야 함
3. **심볼릭 링크 (관리자 PowerShell)**
   ```powershell
   cd "C:\Program Files (x86)\Steam\steamapps\common\Escape From Duckov\BepInEx\plugins"
   New-Item -ItemType SymbolicLink `
     -Path "QuackForge.Loader.dll" `
     -Target "\\Mac\Home\Desktop\Project\side-project\quackforge\src\QuackForge.Loader\bin\Release\QuackForge.Loader.dll"
   ```
   - UNC 경로 심볼릭 링크 실패 시, 공유 폴더를 먼저 드라이브 문자로 매핑 (`net use Z: \\Mac\Home ...`) 후 해당 경로로 심링크.
4. **검증**
   - Mac: `dotnet build -c Release`
   - VM: 덕코프 실행 → `BepInEx\LogOutput.log` 에 `🦆 QuackForge is awake...` 로그 확인

### 폴백 옵션 B — HTTP Serve (PRD §4.7 R0-5)

심볼릭 링크/공유 폴더가 불안정하면 `scripts/serve-mac.sh` 경로로 전환.

```bash
# Mac 측
cd ~/Desktop/Project/side-project/quackforge
./scripts/serve-mac.sh   # 추후 이슈 #8 에서 작성
```

VM 측은 PowerShell 스크립트로 `Invoke-WebRequest` 하여 DLL 갱신.

### Done

- [ ] Mac에서 빌드한 DLL이 VM 덕코프에서 로드됨
- [ ] LogOutput.log 에 QuackForge Awake 로그 출력

---

## 검증 순서 요약

```
T0.4 (VM 덕코프)
    ↓
T0.5 (VM BepInEx)
    ↓
T0.9 (Mac↔VM 공유)
    ↓
T0.8 (Mac 배포 스크립트) → T0.7 (빌드 & 로컬 검증)
```

Mac 덕코프 설치 (T0.2) 는 T0.5 와 병렬 진행 가능.
