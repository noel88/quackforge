using System;
using Duckov.UI;
using QuackForge.Core.Logging;
using UnityEngine;
using UnityEngine.UI;
// ViewTabs / ViewTabDisplayEntry 는 global namespace (Duckov 네임스페이스 외부).

namespace QuackForge.Loader.UI
{
    // #32 PR B — 게임 ViewTabs 에 "Char" 탭 인젝트.
    //
    // 전략 (낮은 위험):
    //   1. ManagedUIElement.onOpen 구독 → 어떤 View 든 처음 열릴 때 ViewTabs scene 활성 보장
    //   2. ViewTabs 자식의 ViewTabDisplayEntry prefab 1개 클론 (시각/스타일 보존)
    //   3. 클론에서 ViewTabDisplayEntry 컴포넌트 제거 (원본 viewTypeName 추적 단절)
    //   4. Button.onClick 리스너 교체 → CharacterPanel.Toggle()
    //   5. 라벨 텍스트 "Char" 로 변경 (UGUI Text + TMPro reflection 양쪽)
    //
    // 인디케이터 (탭 활성 표시) 동기화는 우리 CharacterPanel 이 ManagedUIElement 가
    // 아니라서 자동 동작 X. 후속 PR 에서 fake View 로 sync 가능.
    public static class CharacterTabInjector
    {
        private static readonly IQfLog Log = QfLogger.For("UI.CharacterTab");

        private static GameObject? _injected;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            ManagedUIElement.onOpen += OnAnyOpen;
            Log.Info("CharacterTabInjector listening for first View open");
        }

        private static void OnAnyOpen(ManagedUIElement el)
        {
            if (_injected != null) return;
            try
            {
                var tabs = UnityEngine.Object.FindObjectOfType<ViewTabs>(includeInactive: true);
                if (tabs == null)
                {
                    Log.Debug($"OnOpen({el?.GetType().Name}) — no ViewTabs in scene yet");
                    return;
                }
                Inject(tabs);
            }
            catch (Exception e)
            {
                Log.Error($"injector threw — disabling future attempts: {e}");
                _injected = new GameObject("QuackForge.CharacterTab.failed"); // sentinel
            }
        }

        private static void Inject(ViewTabs tabs)
        {
            var entries = tabs.GetComponentsInChildren<ViewTabDisplayEntry>(includeInactive: true);
            if (entries == null || entries.Length == 0)
            {
                Log.Warn("ViewTabs found but no ViewTabDisplayEntry children — abort inject");
                return;
            }

            var template = entries[0];
            var container = template.transform.parent;
            if (container == null)
            {
                Log.Warn("template entry has no parent — abort inject");
                return;
            }

            var clone = UnityEngine.Object.Instantiate(template.gameObject, container);
            clone.name = "QuackForge.CharacterTab";

            // 클론된 ViewTabDisplayEntry 제거 — 원본 viewTypeName 추적 단절.
            var entryComp = clone.GetComponent<ViewTabDisplayEntry>();
            if (entryComp != null) UnityEngine.Object.Destroy(entryComp);

            // Button click 리스너 교체. 게임 prefab 이 IPointerClickHandler 별도
            // 컴포넌트를 쓰는 경우(round IT1 발견)도 있어서, Button 이 없으면 강제로
            // 클론 root 에 Button 추가해 click 보장.
            var btn = clone.GetComponentInChildren<Button>(includeInactive: true);
            if (btn == null)
            {
                // raycast target 보장용 투명 Image (이미 있으면 재사용).
                var img = clone.GetComponent<Image>();
                if (img == null)
                {
                    img = clone.AddComponent<Image>();
                    img.color = new Color(1f, 1f, 1f, 0.01f);
                }
                img.raycastTarget = true;
                btn = clone.AddComponent<Button>();
                Log.Info("clone had no Button — injected Button + Image on root");
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => CharacterPanel.Instance?.Toggle());

            // 라벨 텍스트 변경. UGUI Text + TMPro 양쪽 시도.
            UpdateLabels(clone, "Char");

            _injected = clone;
            Log.Info($"injected Character tab into ViewTabs (under {container.name})");
        }

        private static void UpdateLabels(GameObject root, string newText)
        {
            // UGUI Text
            foreach (var t in root.GetComponentsInChildren<Text>(includeInactive: true))
                t.text = newText;

            // TMPro (reflection — Loader 가 TMPro 직접 참조 안 하도록).
            foreach (var c in root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
            {
                if (c == null) continue;
                var type = c.GetType();
                if (type.Name != "TextMeshProUGUI" && type.Name != "TextMeshPro") continue;
                var prop = type.GetProperty("text");
                prop?.SetValue(c, newText, null);
            }
        }
    }
}
