# 스탯 수식 & 밸런스 (T3.2 / #30)

PRD §7.3.1 의 5종 스탯 정의를 게임 실제 stat 시스템에 매핑한 결과 + 1포인트당 효과량 + 캡 + 패치 전략.

## 게임 stat 시스템 (RE 결과)

`Duckov.CharacterMainControl` 의 모든 character stat 는 **`Item.GetStatValue(int hash)`** 패턴. hash 는 `"<StatName>".GetHashCode()` 로 생성. 게임 자체 stat 풀:

| 영역 | hash 키 | 경로 |
|---|---|---|
| HP | `"MaxHealth"` | `Health.MaxHealth` (이미 `Health` 의 직접 getter) |
| 이동 | `"WalkSpeed"`, `"RunSpeed"`, `"Moveability"`, `"DashSpeed"` | `CharacterMainControl.CharacterWalkSpeed` etc. |
| 스태미나 | `"MaxStamina"`, `"StaminaDrainRate"`, `"StaminaRecoverRate"` | `CharacterMainControl.MaxStamina` etc. |
| 무게 | `"MaxWeight"` | `CharacterMainControl.MaxWeight` |
| 회복 | `"HealGain"` | `CharacterMainControl.HealGain` (1+x multiplier) |
| 음식 | `"FoodGain"`, `"EnergyCost"`, `"WaterCost"` | `CharacterMainControl.FoodGain` etc. |
| 사격 | `"GunDamageMultiplier"`, `"GunScatterMultiplier"`, `"RecoilControl"`, `"GunCritRateGain"`, `"GunCritDamageGain"`, `"BulletSpeedMultiplier"`, `"GunShootSpeedMultiplier"`, `"ReloadSpeedGain"` | static, base.Item 으로 접근 |
| 근접 | `"MeleeDamageMultiplier"`, `"MeleeCritRateGain"`, `"MeleeCritDamageGain"` | static |
| 인벤토리 | `"InventoryCapacity"` | static |
| 시각/청각 | `"NightVisionAbility"`, `"HearingAbility"`, `"ViewAngle"`, `"ViewDistance"`, `"SenseRange"` | static |

→ **장비/소모품 stat 은 `Item.GetStatValue` 결과에 직접 가산**되거나 **`*` multiplier** 형. 우리 모드는 각 게터 Postfix 로 보너스 추가가 가장 깔끔 (Health 패턴 그대로).

## 1포인트당 효과량 (PRD §7.3.1 그대로 채택)

| 스탯 | 약어 | PRD 효과 | 1포인트당 | 단위/유형 | 게임 데이터 검증 |
|---|---|---|---|---|---|
| Vitality | VIT | 최대 HP | **+10 HP** | float, 가산 | ✅ 실증 (PR #78 v0.2.0) |
| Strength | STR | 캐리 용량 + 근접 데미지 | **+0.5 kg** & **+2%** | float, 가산 / 백분율 | ⏳ MaxWeight 단위가 kg 인지 unit 인지 확인 필요 |
| Agility | AGI | 이동 속도 + 스태미나 | **+1% 이속** & **+3 스태미나** | 백분율 / float 가산 | ⏳ Moveability 가 곱연산이라 백분율이 자연스러움 |
| Precision | PRE | 사격 정확도 + 반동 감소 | **+1.5% 정확도** & **−1% 반동** | 백분율 (둘 다) | ⏳ GunScatterMultiplier 작은게 정확, RecoilControl 큰게 안정 |
| Survival | SUR | 체력 회복 + 허기/갈증 감속 | **+5% 회복** & **−3% 감소** | 백분율 (HealGain 가산, EnergyCost/WaterCost 곱) | ⏳ HealGain 이 이미 1+x multiplier 형이라 가산 OK |

### 캡

- **각 스탯 최대 50 포인트** (PRD 결정, ConfigEntry 화 가능 — `MaxPointsPerStat = 50`)
- 따라서 풀 50 투입 시 단일 스탯 최대 보너스:

| 스탯 | 50pt 풀 효과 |
|---|---|
| VIT | +500 HP |
| STR | +25 kg 캐리 / +100% 근접 데미지 |
| AGI | +50% 이속 / +150 스태미나 |
| PRE | +75% 정확도 / −50% 반동 |
| SUR | +250% 회복 / −150% 허기/갈증 (= 사실상 0 소비) |

→ STR/AGI/PRE/SUR 50pt 보너스가 다소 과해 보임. **Phase 3 알파 테스터 피드백으로 효과량 재조정 후보**.

## 패치 전략 — Harmony Postfix (#31 입력)

각 스탯의 게터 (또는 가장 가까운 단일 진입점) 에 Postfix 로 보너스 가산. `MainCharacter` 가드 + `_stats` null check 패턴은 `HealthMaxHealthPatch` 그대로.

| Stat | 1차 패치 대상 | 시그니처 | 비고 |
|---|---|---|---|
| VIT — HP | `Health.MaxHealth` | float getter | ✅ 이미 구현 (#24) |
| STR — 캐리 | `CharacterMainControl.MaxWeight` | float getter | Postfix `__result += STR * 0.5f` |
| STR — 근접 | `CharacterMainControl.MeleeDamageMultiplier` | float getter | Postfix `__result += STR * 0.02f` (multiplier 인지 가산식인지 추가 확인) |
| AGI — 이속 | `CharacterMainControl.CharacterMoveability` | float getter | walk/run/turn 모두 곱연산. Postfix `__result *= (1 + AGI * 0.01f)` |
| AGI — 스태미나 | `CharacterMainControl.MaxStamina` | float getter | Postfix `__result += AGI * 3f` |
| PRE — 정확도 | `Gun.GunScatterMultiplier` (또는 character-side) | float getter | Postfix 분산 감소 (`__result *= 1 - PRE * 0.015f`). 하한 0.1 가드 |
| PRE — 반동 | `CharacterMainControl.CharacterRecoilControl` | float getter | Postfix `__result += PRE * 0.01f` (큰 값일수록 반동 감소) |
| SUR — 회복 | `CharacterMainControl.HealGain` | float getter | 이미 `1+x` multiplier 형. Postfix `__result += SUR * 0.05f` |
| SUR — 허기/갈증 | `EnergyCost`/`WaterCost` (어디서 적용되는지 추가 RE) | float | Postfix `__result *= 1 - SUR * 0.03f`, 하한 0 |

**총 9개 패치** (PRD §7.4.2 와 일치).

### MainCharacter 가드

`CharacterMainControl.IsMainCharacter` 또는 `LevelManager.Instance?.MainCharacter == __instance` 비교. 적/NPC 의 stat 에는 영향 안 줘야 함 (Health 패치도 `IsMainCharacterHealth` 가드 사용).

### 패치 우선순위

`[HarmonyPriority(Priority.Normal)]` 기본. 다른 모드와 충돌 감지 시 LogWarning + priority 조정.

## 누적 / 분배 정책 (#32 Character UI 입력)

- **누적**: 미분배 포인트 보관 (`StatManager.UnspentPoints`). 이미 v0.2.0 구현.
- **분배**: 인벤토리 → Character 탭 → `+` 버튼 → 즉시 반영 (Confirm 없이) OR Confirm 모드 (PRD 옵션). MVP 는 즉시 반영, 토스트만.
- **리스팩**: PRD 옵션 1 (`AllowFreeRespec = true`) 기본. ConfigEntry 화.

## 결정 요약

| 결정 | 값 |
|---|---|
| 1포인트당 효과량 | PRD §7.3.1 그대로 (위 표) |
| 스탯 캡 | 50 (ConfigEntry: `MaxPointsPerStat`) |
| MainCharacter 가드 | 모든 패치에 적용 |
| 리스팩 정책 | `AllowFreeRespec = true` (기본) |
| autoAllocateVit | Character UI 등장 (#32) 시점에 false 로 전환 |

## 후속

- **#31 Harmony 패치 9종**: 위 매핑 그대로 구현. 패치 1개씩 in-game F9 + stat 수동 set (debug API) 으로 효과 검증.
- **#32 Character UI**: docs/reverse-engineering/ui.md 의 ViewTabs 인젝트 패턴.
- **알파 피드백 후 효과량 재조정 후보**: AGI 50pt 시 +50% 이속이 과한지 등.

## 미해결 (#31 작업 시 추가 RE 필요)

- `MeleeDamageMultiplier` 가 multiplier 인지 가산식인지 (이름은 multiplier 지만 호출부 확인 필요)
- `EnergyCost` / `WaterCost` 가 어디서 어떻게 적용되는지 (decay rate 자체에 곱? 또는 매 tick 차감량?)
- `GunScatterMultiplier` 가 character-side 인지 weapon-side 인지 (둘 다 있을 수 있음)
- 50pt 캡이 게임 밸런스에 적절한지 (알파 테스트로 검증)
