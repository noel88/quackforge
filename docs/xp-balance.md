# XP / Level / Stat 밸런스 (v0.2.0-alpha.1 시점)

Phase 2 의 Hybrid leveling 전략 (PRD §6.0) 에 대한 실제 동작 + 밸런스 결정 기록.

## 전략 요약

XP 누적 / 레벨 계산 / 레벨업 알림은 **게임 (`Duckov.EXPManager`) 에 위임**. QuackForge 는 `EXPManager.onLevelChanged` 이벤트만 구독해서:

1. 레벨업당 스탯 포인트 적립 (`pointsPerLevel = 1`)
2. (MVP) `autoAllocateVit = true` → 새 포인트는 자동으로 VIT 에 투입
3. VIT 변동 → Harmony Postfix → `Health.MaxHealth` 가산

## 게임의 레벨 곡선 (실측)

`EXPManager.levelExpDefinition` 은 Unity Inspector 에 직렬화된 `List<long>` 으로 디컴파일에선 값이 안 보이지만, F9 디버그 키 (`AddXp(1000)`) 반복으로 임계값 관찰:

| Level | 누적 EXP 임계 | 레벨업까지 추가 EXP |
|---:|---:|---:|
| 1 → 2  | 2,000  | 2,000 |
| 2 → 3  | 5,000  | 3,000 |
| 3 → 4  | 9,000  | 4,000 |
| 4 → 5  | 15,000 | 6,000 |
| 5 → 6  | 21,000 | 6,000 |
| 6 → 7  | 28,000 | 7,000 |
| 7 → 8  | 36,000 | 8,000 |
| 8 → 9  | 45,000 | 9,000 |
| 9 → 10 | 55,000 | 10,000 |
| 10 → 11 | 66,000 | 11,000 |
| 11 → 12 | 78,000 | 12,000 |

대략 **선형 + α** 곡선 (level n → n+1 이 약 n×1000 EXP). 추후 levelExpDefinition 전체 값을 게임 세이브 / Inspector dump 로 확인 가능.

## QuackForge 의 스탯 정책 (현재)

- **`pointsPerLevel = 1`**: 레벨업 1회 = 스탯 포인트 1점.
- **`autoAllocateVit = true`**: 적립된 포인트는 즉시 VIT 에 투입. 분배 UI (#32) 가 나오면 false 로 전환 예정.
- **VIT 1당 최대 HP +10** (Harmony `Health.MaxHealth` Postfix). 기본값. 추후 보스/서바이벌 빌드 차별화 시 조정 후보.
- **STR / AGI / PRE / SUR** 정의는 enum 만 존재. 실 효과 패치는 Phase 3 #31 (스탯별 Harmony 패치 9종) 에서.

## 관찰된 인게임 동작 (Round 8/9 QA 로그 기준)

- F9 11회 (EXP 0 → 13000) 동안 레벨 4까지 도달. VIT=4 누적, HP +40 가산 시각 컨펌.
- F9 더 누적 시 레벨 12까지 도달, VIT=11 + HP +110.
- `quackforge.json` 사이드카에 `StatSnapshot { Unspent: 0, Allocated: { VIT: N } }` 영속.
- 게임 종료/재시작 시 정확히 복원 (`[Progression.Stats] restored — unspent=0, VIT=N`).

## 결정 메모

- Hybrid 전략 = 게임 자체 레벨 곡선 (`levelExpDefinition`) 을 그대로 받아 모드의 밸런스 부담 최소화. 게임 패치 시 자동 동기화.
- pointsPerLevel/autoAllocateVit 는 모두 `QfProgression.Initialize` 인자 — 후속 PR 에서 ConfigEntry 화 가능.
- 레벨 다운(EXP 하향) 은 Phase 2 범위 밖. `XpSubscriber.OnLevelChanged` 에서 `next <= prev` 가드.
- Stat 분배 UI (#32) 가 들어오면 autoAllocate 끄고 사용자 선택 권한 부여.

## 후속 추적

- `#31` 스탯별 Harmony 패치 9종 (STR → ItemWeight, AGI → MoveSpeed 등)
- `#32` Character 탭 UI (수동 분배)
- `#33` 레벨업 알림 UI (DebugOverlay → 정식 UnityUI 교체, #77 의 cursor 충돌도 같이 해결)
- `#34` 설정 확장 (pointsPerLevel / autoAllocate / VIT→HP 비율 ConfigEntry 화)
