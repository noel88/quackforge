using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Progression;
using QuackForge.Progression.Stats;
using UnityEngine;
using UnityEngine.UI;

namespace QuackForge.Loader.UI
{
    // 코드로 빌드한 UGUI Character 패널.
    //   - 게임 ViewTabs 에는 미통합 (별도 PR 에서 Harmony 인젝트). 자체 토글 키.
    //   - 5종 스탯 +/- 분배 + Unspent 표시.
    //   - StatPointsGranted / StatAllocated / StatDeallocated 이벤트 구독해 자동 갱신.
    //
    // host GameObject 는 별도 (HideAndDontSave + DontDestroyOnLoad). 내부에 Canvas
    // 자식으로 패널 빌드.
    public sealed class CharacterPanel : MonoBehaviour
    {
        private static CharacterPanel? _instance;

        private readonly IQfLog _log = QfLogger.For("UI.CharacterPanel");

        private QfProgression? _progression;
        private ConfigEntry<KeyboardShortcut>? _toggleKey;

        private GameObject? _root;
        private Text? _unspentText;
        private readonly Dictionary<StatType, Text> _statValueTexts = new();

        public static void Attach(
            MonoBehaviour host,
            QfProgression progression,
            QfEventBus bus,
            ConfigEntry<KeyboardShortcut> toggleKey)
        {
            if (_instance != null) return;

            var go = new GameObject("QuackForgeCharacterPanel");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<CharacterPanel>();
            _instance._progression = progression;
            _instance._toggleKey = toggleKey;

            bus.Subscribe<StatPointsGrantedEvent>(_ => _instance.RefreshUI());
            bus.Subscribe<StatAllocatedEvent>(_ => _instance.RefreshUI());
            bus.Subscribe<StatDeallocatedEvent>(_ => _instance.RefreshUI());
            _instance._log.Info($"CharacterPanel attached (toggleKey={toggleKey.Value})");
        }

        private void Update()
        {
            if (_toggleKey == null) return;
            if (_toggleKey.Value.IsDown()) Toggle();
        }

        public void Toggle()
        {
            if (_root == null) BuildUI();
            var newState = !_root!.activeSelf;
            _root.SetActive(newState);
            if (newState) RefreshUI();
        }

        private void BuildUI()
        {
            _root = new GameObject("CharacterPanelRoot");
            _root.hideFlags = HideFlags.HideAndDontSave;
            _root.transform.SetParent(transform);

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            _root.AddComponent<CanvasScaler>();
            _root.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(_root.transform, false);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.88f);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(380f, 380f);

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 8;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            AddText(panel.transform, "QuackForge — Character", 18, bold: true, color: new Color(1f, 0.85f, 0.35f));
            _unspentText = AddText(panel.transform, "Unspent: 0", 14, bold: false, color: Color.white);

            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                var valText = AddStatRow(panel.transform, stat);
                _statValueTexts[stat] = valText;
            }

            var hintLabel = _toggleKey != null ? $"Press [{_toggleKey.Value}] to toggle" : "";
            AddText(panel.transform, hintLabel, 11, bold: false, color: new Color(0.7f, 0.7f, 0.7f));

            _log.Info("CharacterPanel UI built");
        }

        private Text AddText(Transform parent, string content, int size, bool bold, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = size + 8;
            return text;
        }

        private Text AddStatRow(Transform parent, StatType stat)
        {
            var row = new GameObject($"Row_{stat}");
            row.transform.SetParent(parent, false);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            var rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 32;

            var nameText = AddText(row.transform, stat.ToString(), 14, bold: false, Color.white);
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.GetComponent<LayoutElement>().preferredWidth = 100f;

            var valueText = AddText(row.transform, "0", 14, bold: true, new Color(0.9f, 1f, 0.7f));
            valueText.GetComponent<LayoutElement>().preferredWidth = 50f;

            AddButton(row.transform, "−", () => _progression?.Stats.Deallocate(stat, 1));
            AddButton(row.transform, "+", () => _progression?.Stats.Allocate(stat, 1));

            return valueText;
        }

        private void AddButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.42f, 0.18f, 1f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.18f, 0.42f, 0.18f, 1f);
            colors.highlightedColor = new Color(0.28f, 0.62f, 0.28f, 1f);
            colors.pressedColor = new Color(0.10f, 0.30f, 0.10f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick());

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 36f;
            le.minHeight = 30f;

            var labelText = AddText(go.transform, label, 16, bold: true, Color.white);
            // 버튼 자식 Text 가 LayoutGroup 의 preferred 추정에 들어가지 않도록 ignore.
            var labelLe = labelText.GetComponent<LayoutElement>();
            labelLe.ignoreLayout = true;
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private void RefreshUI()
        {
            if (_unspentText == null || _progression == null) return;
            var stats = _progression.Stats;
            _unspentText.text = $"Unspent: {stats.UnspentPoints}";
            foreach (var kv in _statValueTexts)
                kv.Value.text = stats.GetAllocated(kv.Key).ToString();
        }

        private static Font? _cachedFont;
        private static Font GetFont()
        {
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            return _cachedFont;
        }
    }
}
