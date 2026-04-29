using BepInEx.Configuration;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Progression.Stats;
using UnityEngine;
using UnityEngine.UI;

namespace QuackForge.Loader.UI
{
    // 레벨업 알림 UnityUI 오버레이.
    //   - 화면 중앙 "LEVEL UP!" 라벨 1.5초 표시 + 페이드 아웃
    //   - 같은 캔버스 우상단에 "+N pts" 뱃지 (페이드 아웃 동안)
    //   - StatPointsGrantedEvent 구독 (game's onLevelChanged 와 1:1)
    //
    // host GameObject 는 별도 (HideAndDontSave + DontDestroyOnLoad). 게임 cursor lock
    // 깨지 않도록 GraphicRaycaster 만 두고 PointerEvent 자체엔 무관 (raycastTarget=false).
    //
    // Sound (Q3-3) — 게임 내 UI 사운드 reflection 으로 호출하는 건 후속. 이번엔 미구현.
    public sealed class LevelUpNotification : MonoBehaviour
    {
        private const float DisplaySec = 1.5f;
        private const float FadeOutSec = 1.0f;

        private static LevelUpNotification? _instance;

        private readonly IQfLog _log = QfLogger.For("UI.LevelUp");

        private GameObject? _root;
        private CanvasGroup? _group;
        private Text? _headerText;
        private Text? _badgeText;

        private float _hideAtRealtime = -1f;
        private float _fadeStartRealtime = -1f;

        public static void Attach(MonoBehaviour host, QfEventBus bus, ConfigEntry<bool>? enabled)
        {
            if (_instance != null) return;
            if (enabled != null && !enabled.Value) return;

            var go = new GameObject("QuackForgeLevelUpNotification");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LevelUpNotification>();

            bus.Subscribe<StatPointsGrantedEvent>(_instance.OnPointsGranted);
            _instance._log.Info("LevelUpNotification attached");
        }

        private void OnPointsGranted(StatPointsGrantedEvent evt)
        {
            if (_root == null) BuildUI();
            if (_headerText != null) _headerText.text = "LEVEL UP!";
            if (_badgeText != null) _badgeText.text = $"+{evt.Amount} pts";
            _root!.SetActive(true);
            if (_group != null) _group.alpha = 1f;
            _hideAtRealtime = Time.realtimeSinceStartup + DisplaySec;
            _fadeStartRealtime = _hideAtRealtime;
        }

        private void Update()
        {
            if (_root == null || !_root.activeSelf || _group == null) return;

            var now = Time.realtimeSinceStartup;
            if (now < _fadeStartRealtime) return;

            var elapsed = now - _fadeStartRealtime;
            if (elapsed >= FadeOutSec)
            {
                _group.alpha = 0f;
                _root.SetActive(false);
                return;
            }
            _group.alpha = 1f - elapsed / FadeOutSec;
        }

        private void BuildUI()
        {
            _root = new GameObject("LevelUpRoot");
            _root.hideFlags = HideFlags.HideAndDontSave;
            _root.transform.SetParent(transform);

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1100; // CharacterPanel 위
            _root.AddComponent<CanvasScaler>();
            // GraphicRaycaster 추가 안 함 — PointerEvent 미수신 (cursor lock 비간섭).
            _group = _root.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            // 중앙 헤더
            _headerText = CreateText("HeaderText", "LEVEL UP!", 36, FontStyle.Bold, new Color(1f, 0.85f, 0.35f, 1f), TextAnchor.MiddleCenter);
            var headerRect = _headerText.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerRect.pivot = new Vector2(0.5f, 0.5f);
            headerRect.anchoredPosition = new Vector2(0f, 60f);
            headerRect.sizeDelta = new Vector2(420f, 80f);

            // 우상단 뱃지
            _badgeText = CreateText("BadgeText", "+0 pts", 18, FontStyle.Bold, new Color(0.7f, 1f, 0.7f, 1f), TextAnchor.MiddleCenter);
            var badgeRect = _badgeText.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(-24f, -24f);
            badgeRect.sizeDelta = new Vector2(140f, 40f);

            _root.SetActive(false);
            _log.Info("LevelUpNotification UI built");
        }

        private Text CreateText(string name, string content, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root!.transform, false);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false; // cursor lock 비간섭
            return text;
        }

        private static Font? _cachedFont;
        private static Font GetFont()
        {
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 36);
            return _cachedFont;
        }
    }
}
