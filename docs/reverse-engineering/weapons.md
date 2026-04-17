# Assembly Reverse Engineering — Weapons, Armors, Recipes, Health, EXP

**작성일**: 2026-04-17
**대상**: Duckov v2.2.0, Unity 2022.3.62f2
**도구**: `ilspycmd 10.0.0.8330` (ICSharpCode.Decompiler)
**소스**: `Duckov.app/Contents/Resources/Data/Managed/`

> 이 문서는 **Phase 1 구현(Data 모듈·Blueprint) 에 필요한 최소 정보**. Phase 2 (스탯/레벨) 대상 클래스(EXPManager, Health, Stats)는 §6 에 요약만 기록하고 Phase 2 착수 시 보강.

## 1. 어셈블리 맵

| DLL | 역할 | 클래스 수 |
|---|---|---|
| `Assembly-CSharp.dll` | Unity 기본 에셈블리. 에디터/데모 스크립트 위주 | 157 |
| **`TeamSoda.Duckov.Core.dll`** | **메인 게임 로직, 모딩 API, 아이템 설정** | **1274** |
| `TeamSoda.Duckov.Utilities.dll` | 유틸리티 |
| **`ItemStatsSystem.dll`** | **아이템·인벤토리·스탯 파운데이션** | 66 |
| `TeamSoda.MiniLocalizor.dll` | 로컬라이제이션 |

역공학 대상은 **Core + ItemStatsSystem** 두 DLL 이 대부분.

## 2. 핵심 모델 — `ItemStatsSystem.Item`

모든 아이템(무기, 방어구, 탄약, 포뮬러 아이템 등)은 `Item : MonoBehaviour` 를 베이스로 구성. 실제 동작(발사/공격/방어)은 `ItemSettingBase` 파생 **컴포넌트** 가 같은 GameObject 에 붙음.

```csharp
public class Item : MonoBehaviour, ISelfValidator
{
    [SerializeField] private int typeID;                    // 고유 ID
    [SerializeField] private int order;
    [LocalizationKey("Items")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int maxStackCount = 1;
    [SerializeField] private int value;                     // 판매가/교환가
    [SerializeField] private int quality;
    [SerializeField] private DisplayQuality displayQuality;
    [SerializeField] private float weight;
    [SerializeField] private TagCollection tags;
    [SerializeField] private ItemGraphicInfo itemGraphic;
    [SerializeField] private StatCollection stats;          // 핵심 수치 저장
    [SerializeField] private SlotCollection slots;          // 부착부품 슬롯
    [SerializeField] private ModifierDescriptionCollection modifiers;
    [SerializeField] private CustomDataCollection variables;
    [SerializeField] private CustomDataCollection constants;
    [SerializeField] private Inventory inventory;
    [SerializeField] private List<Effect> effects;
}
```

- 스탯은 문자열 키 → `GetHashCode()` 해시 기반. 예: `"MaxHealth".GetHashCode()`
- 값 조회 패턴: `item.GetStatValue(hash)` (float 반환)
- `[ItemTypeID]` attribute 로 인스펙터에서 ID 선택 가능 (`ItemStatsSystem.ItemTypeIDAttribute`)

## 3. 아이템 Setting 컴포넌트 계보

```
ItemSettingBase : MonoBehaviour (abstract)
├── ItemSetting_Gun           # 원거리 무기
├── ItemSetting_MeleeWeapon   # 근접 무기
├── ItemSetting_Bullet        # 탄약
├── ItemSetting_Accessory     # 액세서리 (백팩, 벨트 등 추정)
├── ItemSetting_Formula       # 크래프팅 레시피 아이템
├── ItemSetting_GPU           # GPU (특수 모듈?)
├── ItemSetting_NightVision   # 야시경
└── ItemSetting_Skill         # 스킬 아이템
```

`ItemAgent_*` 시리즈(Gun / Kazoo / MeleeWeapon)는 **런타임 행동** (애니메이션, 사운드 트리거). 데이터 추가시 Setting 쪽이 우선.

## 4. `ItemSetting_Gun` — 원거리 무기

```csharp
public class ItemSetting_Gun : ItemSettingBase
{
    public enum TriggerModes { auto, semi, bolt }
    public enum ReloadModes  { fullMag, singleBullet }

    private int targetBulletID = -1;

    public ADSAimMarker adsAimMarker;        // 조준 마커
    public GameObject muzzleFxPfb;           // 총구 화염 프리팹
    public Projectile bulletPfb;             // 탄 프리팹 (기본)
    public string shootKey = "Default";      // 사격 사운드 키
    public string reloadKey = "Default";     // 재장전 사운드 키
    public TriggerModes triggerMode;
    public ReloadModes reloadMode;
    public bool CanControlMind;
    public bool autoReload;
    public ElementTypes element;             // 속성 (물리/화염/얼음/...)
    public Buff buff;                        // 명중 시 부여 버프

    // 스탯 해시 (런타임)
    // "BulletCount", "Capacity", "Caliber", "OverrideTriggerMode"

    public int Capacity => Mathf.RoundToInt(base.Item.GetStatValue(CapacityHash));
    public int BulletCount { ... }           // stat 기반
    public TriggerModes currentTriggerMode { ... } // OverrideTriggerMode 스탯으로 변경 가능
}
```

### 주요 런타임 메서드
- `SetTargetBulletType(Item | int)` — 탄종 변경
- `UniTaskVoid LoadBulletsFromInventory(Inventory)` — 재장전 (비동기)
- `bool LoadSpecificBullet(Item)` / `bool IsValidBullet(Item)`
- `Dictionary<int, BulletTypeInfo> GetBulletTypesInInventory(Inventory)`
- `void UseABullet()`, `void TakeOutAllBullets()`

### Phase 1 무기 10종 설계 시 주의

- 무기 고유 스탯 (탄창, 발사속도, 정확도 등) 은 `StatCollection` 에 직렬화. **신규 무기 = 기존 무기 변형**이므로 **AssetBundle 베이스 프리팹 레퍼런스 필요**.
- `element`, `buff`, `triggerMode` 는 인스펙터 직접 지정 (직렬화된 필드).
- `targetBulletID` 는 private, runtime 설정. 초기값 공식 지원 매니페스트로 지정 가능한지 T1.3/T1.4 확인 필요.

## 5. `ItemSetting_MeleeWeapon` — 근접 무기

```csharp
public class ItemSetting_MeleeWeapon : ItemSettingBase
{
    public bool dealExplosionDamage;
    public ElementTypes element;
    public float buffChance;
    public Buff buff;

    public override void SetMarkerParam(Item selfItem) { ... }
}
```

간결. 데미지 자체는 `Item.stats` 의 "Damage" 계열 스탯을 통해. Gun 과 달리 자체 필드 적음 → **확장이 쉬움** (무기 10종 중 근접 비율 판단 시 참고).

## 6. 방어구 — `ItemSetting_Accessory`

게임에는 `ItemSetting_Helmet` / `ItemSetting_Armor` 같은 명시적 클래스가 **없음**. 대신:

- Body/Head 방어력은 **Item 의 스탯** (`"BodyArmor"`, `"HeadArmor"`) 으로 표현 → 어떤 Item 이라도 스탯을 실으면 "방어구" 역할.
- `ItemSetting_Accessory` 는 공통 컴포넌트 (추정: 등에 메는 백팩, 고글, 벨트 등).
- **속성 저항**도 Item 스탯: `ElementFactor_Physics/Fire/Poison/Electricity/Space/Ghost/Ice` (아래 Health 참조).

### Phase 1 방어구 5종 설계 시 주의

- 방어구 고유 필드 없음 → **Item 스탯 + Sprite 구성으로 충분**. T1.4 (방어구) 는 Gun 보다 쉬움.
- 슬롯 시스템 (`SlotCollection`, 아마 머리/몸/백팩 파츠) 이 장착 위치를 결정.

## 7. 크래프팅 / Blueprint 시스템

### 7.1 `CraftingFormula` (struct)

```csharp
[Serializable]
public struct CraftingFormula
{
    [Serializable]
    public struct ItemEntry { [ItemTypeID] public int id; public int amount; }

    public string id;              // 레시피 ID
    public ItemEntry result;       // 결과 아이템 + 수량
    public string[] tags;
    [SerializeField] public Cost cost;   // 재료 + 화폐
    public bool unlockByDefault;   // 기본 해금
    public bool lockInDemo;
    public string requirePerk;     // 특정 퍽 필요
    public bool hideInIndex;

    public bool IDValid => !string.IsNullOrEmpty(id);
}
```

### 7.2 `Duckov.Economy.Cost` (struct)

```csharp
[Serializable]
public struct Cost
{
    [Serializable]
    public struct ItemEntry { [ItemTypeID] public int id; public long amount; }

    public long money;
    public ItemEntry[] items;

    public bool Enough   { get; }   // EconomyManager.IsEnough
    public bool IsFree   { get; }
    public bool Pay(bool accountAvaliable = true, bool cashAvaliable = true);
    public UniTask Return(...);
}
```

### 7.3 `CraftingManager : MonoBehaviour`

```csharp
public class CraftingManager : MonoBehaviour
{
    public static Action<CraftingFormula, Item> OnItemCrafted;
    public static Action<string> OnFormulaUnlocked;

    public static CraftingManager Instance { get; private set; }
    public static IEnumerable<string> UnlockedFormulaIDs { get; }

    public static void UnlockFormula(string formulaID);
    public async UniTask<List<Item>> Craft(string id);

    // 세이브 키: "Crafting/UnlockedFormulaIDs"
}
```

### 7.4 `CraftingFormulaCollection : ScriptableObject`

```csharp
[CreateAssetMenu]
public class CraftingFormulaCollection : ScriptableObject
{
    [SerializeField] private List<CraftingFormula> list;
    public static CraftingFormulaCollection Instance => GameplayDataSettings.CraftingFormulas;
    public ReadOnlyCollection<CraftingFormula> Entries { get; }
    public static bool TryGetFormula(string id, out CraftingFormula formula);
}
```

### 7.5 Phase 1 Blueprint 시스템 설계 시사점

- **포뮬러 DB 는 `CraftingFormulaCollection` SO 1개**. 공식 Mod API 는 추가 formula 를 이 리스트에 넣는 메커니즘을 제공할 가능성 높음(Part A).
- `requirePerk` 필드 존재 → 퍽 시스템과 크래프팅이 연결되어 있음. QuackForge 의 Blueprint "레벨에 따라 해금" 요구사항은 `requirePerk` 대신 `unlockByDefault=false` + `CraftingManager.UnlockFormula(id)` 를 레벨업 이벤트에서 호출하는 방식으로 구현 가능. (Part B, Harmony 불필요)
- `Cost` 의 `money + ItemEntry[]` 조합으로 골드+재료 혼합 레시피 지원됨.

## 8. `Workbench` — 크래프팅 스테이션

```csharp
public class Workbench : InteractableBase
{
    protected override void OnInteractFinished()
    {
        ItemCustomizeSelectionView.Show();
    }
}
```

매우 얇음. 실 UI 는 `ItemCustomizeSelectionView` (별도 클래스). Workbench 자체 확장 필요성 낮음.

## 9. 데이터 시트 생성 플로우 (Phase 1 실제 빌드)

공식 Mod API 로 아이템을 추가하려면(추정):

1. Unity Editor 에서 신규 Item Prefab 구성 (Item + ItemSetting_Gun/Melee/Accessory)
2. `typeID` 를 모드 전용 범위로 할당 (충돌 회피)
3. `StatCollection` 에 무기 성능 수치 입력
4. `ItemGraphicInfo` 에 아이콘/3D/뷰 설정
5. AssetBundle 빌드 → `Duckov_Data/Mods/<mod_name>/`
6. 같은 모드에서 `CraftingFormulaCollection` 증분 추가
7. 매니페스트 (포맷은 T1.1 후속) 에 엔트리 등록

> T1.3 Core 모듈 후, T1.4 (무기 10종) 에서 **Unity Editor 빌드 체인 설정**이 필요. 이게 Mac에서 원활한지 초기 실증 단계(예: 테스트 무기 1개) 가 권장됨.

## 10. Phase 2 예고 — EXP/Health 스냅샷 (요약)

### `Duckov.EXPManager`
- **게임은 이미 레벨 시스템이 있음** (이전 PRD 가정과 다름).
- `Instance.point` (long) = 총 EXP
- `levelExpDefinition: List<long>` = 레벨별 threshold
- `AddExp(int)`, `LevelFromExp(long)`, `GetLevelExpRange(int)`
- 이벤트: `onExpChanged(long)`, `onLevelChanged(int prev, int next)`
- 세이브키: 내부 관리 (ISaveDataProvider)

**함의**: Phase 2 레벨링 모듈은 `EXPManager` 와 **나란히 존재**(별도 "QuackForge XP") 혹은 **Harmony 로 onLevelChanged 훅**하여 스탯 포인트 분배.

### `Health`
- team (Teams enum), MaxHealth/CurrentHealth, Invincible, IsDead
- 스탯 키 (해시): `"MaxHealth"`, `"BodyArmor"`, `"HeadArmor"`, `"ElementFactor_{Physics|Fire|Poison|Electricity|Space|Ghost|Ice}"`
- 이벤트: `OnHealthChange`, `OnMaxHealthChange`, `OnDeadEvent`, `OnHurtEvent` (UnityEvent<Health>/<DamageInfo>)
- Element 저항은 곱셈 방식으로 보임 (추가 역공학 필요, Phase 2).

## 11. Phase 1 진입 전 Open Questions

- Q1-A: `ItemSetting_Gun.targetBulletID` 는 공식 매니페스트로 선설정 가능한가?
- Q1-B: AssetBundle 빌드 시 Mac arm64/x64 플랫폼 분리 번들이 필요한가? Universal 번들 지원?
- Q1-C: `[ItemTypeID]` 의 모드 전용 ID 범위(예약) 가 있는가?
- Q1-D: `CraftingFormulaCollection.Instance` 는 ScriptableObject 싱글턴 — 모드 formula 를 인게임 배열에 병합하는 공식 경로가 있는지?

이 4개는 **T1.3 직전** 공식 모드 예제 1개 (무기 혹은 레시피) 로 실증.

## 12. 결론 및 Phase 1 설계 권고

1. **무기 데이터 모델은 이미 확립됨** (`Item + ItemSetting_Gun/Melee`). 모드 측에서 정의할 것은 JSON 매니페스트 + AssetBundle.
2. **방어구는 스탯으로 표현** (`BodyArmor/HeadArmor` + `ElementFactor_*`). 전용 컴포넌트 불필요, Item 만 구성.
3. **Blueprint** = `CraftingFormula` 레코드 추가 + `CraftingManager.UnlockFormula` 호출. Harmony 패치 없이도 가능 → Part A 로 전부 편입 가능.
4. **Workbench** 는 확장 불필요. 레시피만 꽂으면 기존 UI 에서 자동 노출 추정.
5. **Phase 2 EXP 설계 재검토 필수** — 게임 자체 레벨 시스템 존재. QuackForge 의 XP/레벨을 별도 궤도로 둘지, 기존 시스템을 확장할지 Phase 2 킥오프 전 결정.
