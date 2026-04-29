# Duckov UI 시스템 리버스 엔지니어링 (T3.1 / #29)

대상 어셈블리: `TeamSoda.Duckov.Core.dll` (게임 Managed/), 디컴파일 도구: `ilspycmd 10.0.0`.

## TL;DR

- **UI 프레임워크**: 기본은 **UGUI** (`UnityEngine.UI`, `Canvas`, `RectTransform`, TMPro). UI Toolkit (`UIDocument`, `VisualElement`) 은 1군데(소품성) 만 사용 — 사실상 UGUI 단일 스택.
- **탭 시스템 확장 가능 (Q3-1 ✅)** — 단, 코드만으로는 부족. 새 `View` 서브클래스 + Harmony 패치로 `GameplayUIManager.views` 에 인젝트 + Unity prefab (또는 동적 GameObject) 로 실제 패널 비주얼 작성 필요.
- **위험**: ViewTabs 시각 통합은 Inspector 직렬화 prefab 의존이 강함. 동적 인젝트 시 부모 transform 계층 (`transform.parent.parent` 가 `ViewTabs`) 에 정확히 attach 해야 자동 발견됨. 안 되면 **R3-1 폴백** (별도 Canvas 자체 띄움) 으로 우회 가능.

## 핵심 클래스 계층

```
ManagedUIElement (abstract MonoBehaviour)         TeamSoda.Duckov.Core.dll:103348
├── open: bool / parent: ManagedUIElement
├── Open(parent) / Close()
├── static event onOpen / onClose
└── 부모 닫히면 자식도 자동 Close

View : ManagedUIElement (abstract)                 :107536
├── static View ActiveView   ← 한 번에 하나만 활성
├── static event OnActiveViewChanged
├── viewTabs: ViewTabs        ← Awake 에서 transform.parent.parent.GetComponent<ViewTabs>()
├── exitButton: UnityEngine.UI.Button
├── sfx_Open / sfx_Close
└── OnOpen 시 다른 ActiveView 강제 Close

GameplayUIManager (singleton MonoBehaviour)        :103425+
├── List<View> views   [SerializeField]            ← Inspector 등록
├── Dictionary<Type, View> viewDic                 ← lazy cache
├── PrefabPool<ItemDisplay/SlotDisplay/InventoryEntry>
├── static GameplayUIManager.Instance
└── static T GetViewInstance<T>() where T : View
        → viewDic 캐시 hit → return
        → 없으면 views.Find(e => e is T) → cache → return
```

구체 View 약 25 종 (인벤토리 / 워크벤치 / 퀘스트 / 마스터키 / ATM / 비트코인 마이너 등):
`InventoryView` / `CraftView` / `BlackMarketView` / `QuestView` / `MasterKeysView` / `FormulasIndexView` / `ItemRepairView` / `ItemDecomposeView` / ...

## 인벤토리 화면 구조

`InventoryView : View` (`:104621`):

```csharp
public class InventoryView : View {
    [SerializeField] FadeGroup fadeGroup;
    [SerializeField] ItemSlotCollectionDisplay slotDisplay;     // 캐릭터 장비 슬롯
    [SerializeField] InventoryDisplay inventoryDisplay;         // 가방 그리드
    [SerializeField] ItemDetailsDisplay detailsDisplay;         // 우측 디테일
    [SerializeField] FadeGroup itemDetailsFadeGroup;

    static InventoryView Instance => View.GetViewInstance<InventoryView>();

    Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

    OnOpen() {
        slotDisplay.Setup(characterItem);
        inventoryDisplay.Setup(characterItem.Inventory);
    }
}
```

`InventoryDisplay : MonoBehaviour, IPoolable` (`:98826`) — 그리드 자체:
- `entryPrefab: InventoryEntry`, `entriesParent: Transform`
- `GridLayoutGroup contentLayout`
- `usePages` / `itemsEachPage = 30` — 페이지네이션
- `PrefabPool<InventoryEntry>` — 풀링
- 정렬 버튼, 필터 (`InventoryFilterDisplay`)

→ 인벤토리는 단일 grid 패널, **탭 자체는 InventoryView 외부 (`ViewTabs`) 가 담당**.

## ViewTabs (탭 시스템)

```csharp
public class ViewTabs : MonoBehaviour {                         // :45254
    [SerializeField] FadeGroup fadeGroup;
    public void Show()/Hide();
    void Update() { if (View.ActiveView == null) Hide(); }      // active view 사라지면 자동 숨김
}

public class ViewTabDisplayEntry : MonoBehaviour {              // :45277
    [SerializeField] string viewTypeName;                       // ★ 핵심: 타입명을 string 으로 매칭
    [SerializeField] GameObject indicator;                      // 활성 표시
    [SerializeField] PunchReceiver punch;                       // 활성 시 시각 효과

    void Awake() {
        ManagedUIElement.onOpen  += OnViewOpen;
        ManagedUIElement.onClose += OnViewClose;
    }
    void OnViewOpen(ManagedUIElement el) {
        if (el.GetType().Name == viewTypeName) ShowIndicator();
    }
}
```

**키 발견**: `viewTypeName` 이 **타입의 `Name` (FullName 아님!)** 으로 비교됨. 즉 우리 모드 namespace 가 달라도 `class QuackForgeCharacterView : View` 정의하고 entry 의 viewTypeName="QuackForgeCharacterView" 로 두면 indicator 동작.

## 모드가 새 View / 탭 추가하는 방법

### A. 정식 통합 경로 (코드 + Unity prefab)

1. **코드** (모드 어셈블리):
   ```csharp
   public class QuackForgeCharacterView : View {
       protected override void OnOpen()  { base.OnOpen(); /* 우리 UI 활성 */ }
       protected override void OnClose() { base.OnClose(); /* 우리 UI 비활성 */ }
   }
   ```
2. **Unity prefab** (AssetBundle 또는 `unity-project/`):
   - 게임의 ViewTabs 자식 계층과 같은 형태로 패널 prefab 작성
   - prefab 안에 `QuackForgeCharacterView` 컴포넌트 부착, 자식들로 우리 UI elements (TMPro 텍스트, 버튼 등) 배치
   - viewTabs 자동 발견을 위해 **`view.transform.parent.parent` 가 `ViewTabs` 이도록 계층 맞춰야 함** (`view`'s parent = "Page" 슬롯, parent.parent = ViewTabs 컨테이너)
3. **런타임 인젝트** (Harmony Postfix on `GameplayUIManager.Awake`):
   ```csharp
   var go = Object.Instantiate(myViewPrefab, viewTabsTransform, worldPositionStays: false);
   var view = go.GetComponent<QuackForgeCharacterView>();
   __instance.views.Add(view);
   __instance.viewDic[typeof(QuackForgeCharacterView)] = view;
   // ViewTabs 자식 row 에 ViewTabDisplayEntry prefab 도 같이 Instantiate
   ```

### B. 탭 통합 안 하고 별도 Canvas 폴백 (R3-1)

ViewTabs 시각 통합이 prefab 호환성 이슈로 안 통하면:
- 우리만의 `Canvas`/`GameObject` 동적 생성
- `class QuackForgeCharacterView : View` (still inherit `View`) 로 두되 viewTabs 는 null 두고 자체 키바인드 (예: Tab+C) 로 토글
- `ManagedUIElement.onOpen/onClose` 이벤트는 그대로 발화되므로 게임 라이프사이클 (다른 View 열리면 자동 Close) 은 따라감
- 탭 UI 통합만 빠짐 — 사용자는 단축키로 진입

폴백 시 IMGUI (현재 `DebugOverlay` 같은) 로도 가능하지만 cursor 충돌 (#77) 때문에 권장하지 않음.

## 키 입력 / 네비게이션

`UIInputManager.OnNavigate / OnConfirm / OnCancel` 정적 이벤트 — `View.Awake` 에서 자동 구독. 새 View 도 동일하게 받음. 우리 View 가 활성일 때만 처리하려면 `View.ActiveView == this` 가드 필요 (이미 base 가 처리).

## 결론

| 질문 | 답 |
|---|---|
| Q3-1 (탭 시스템 확장 가능?) | **✅ 가능**. 코드 + Unity prefab + Harmony 인젝트 조합 |
| 폴백 R3-1 필요? | 가능성 있음. ViewTabs prefab 계층 호환성이 prefab 작업 시 결정됨 |
| UGUI vs UI Toolkit? | **UGUI 단일** (UI Toolkit 거의 미사용) |
| 인벤토리 = 단일 View? | ✅ `InventoryView` 단일. 탭은 외부 `ViewTabs` 가 담당 |

## 후속 (#30, #31, #32)

- **#30 스탯 수식**: 이번엔 UI 가 아니라 stat → 게임 효과 매핑. `Health.MaxHealth` (#24 이미 done) 같은 식으로 STR/AGI/PRE/SUR 의 패치 지점 RE 필요.
- **#31 Harmony 패치 9종**: 각 스탯이 어디에 영향 주는지 게임 어셈블리 grep + 패치 작성.
- **#32 Character 탭 UI**: 본 문서 기반으로 `QuackForgeCharacterView` 구현. Phase 3 sprint 의 메인 작업. AssetBundle 워크플로 (`unity-project/`) 에서 prefab 빌드 필요할 가능성 큼.
- **#33 레벨업 알림 UI**: 같은 캔버스 (또는 별도 toast Canvas) 로 정식 UnityUI 토스트. #77 (DebugOverlay cursor 충돌) 자연 해소.
