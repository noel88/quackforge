using System.Collections.Generic;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Progression;
using QuackForge.Progression.Stats;
using UnityEngine;

namespace QuackForge.Loader.UI
{
    // IMGUI 기반 개발용 오버레이.
    // - 우상단 스탯 요약 (현재 VIT / unspent points)
    // - 레벨업 / 포인트 적립 이벤트 시 토스트 (3초 노출)
    //
    // Phase 3 의 진짜 UI 가 준비되면 ConfigEntry "OverlayEnabled" = false 로 비활성화.
    // Unity GameObject 로 싱글턴 MonoBehaviour 부착.
    public sealed class DebugOverlay : MonoBehaviour
    {
        private const float ToastDurationSec = 3.0f;
        private const int MaxToasts = 4;

        private static DebugOverlay? _instance;

        private QfProgression? _progression;
        private readonly List<Toast> _toasts = new();
        private readonly IQfLog _log = QfLogger.For("UI.Overlay");

        private GUIStyle? _headerStyle;
        private GUIStyle? _lineStyle;
        private GUIStyle? _toastStyle;

        public static void Attach(MonoBehaviour host, QfProgression progression, QfEventBus bus)
        {
            if (_instance != null) return;

            var go = new GameObject("QuackForgeDebugOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DebugOverlay>();
            _instance._progression = progression;

            bus.Subscribe<StatPointsGrantedEvent>(_instance.OnPointsGranted);
            bus.Subscribe<StatAllocatedEvent>(_instance.OnStatAllocated);
            _instance._log.Info("DebugOverlay attached");
        }

        private void OnPointsGranted(StatPointsGrantedEvent evt)
        {
            Push($"+{evt.Amount} pts ({evt.Source})");
        }

        private void OnStatAllocated(StatAllocatedEvent evt)
        {
            Push($"{evt.Stat} +{evt.Amount} → {evt.AllocatedAfter}");
        }

        private void Push(string message)
        {
            _toasts.Add(new Toast { Message = message, ExpireAt = Time.realtimeSinceStartup + ToastDurationSec });
            if (_toasts.Count > MaxToasts) _toasts.RemoveAt(0);
        }

        private void OnGUI()
        {
            _headerStyle ??= BuildStyle(fontSize: 14, color: new Color(1f, 0.85f, 0.35f, 1f), bold: true);
            _lineStyle   ??= BuildStyle(fontSize: 13, color: new Color(0.9f, 0.9f, 0.9f, 0.95f), bold: false);
            _toastStyle  ??= BuildStyle(fontSize: 15, color: new Color(0.6f, 1f, 0.6f, 1f), bold: true);

            DrawStatSummary();
            DrawToasts();
        }

        private void DrawStatSummary()
        {
            if (_progression == null) return;
            var stats = _progression.Stats;

            var w = 260f;
            var h = 92f;
            var x = Screen.width - w - 16f;
            var y = 16f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUILayout.BeginArea(new Rect(x + 8, y + 6, w - 16, h - 12));
            GUILayout.Label("QuackForge", _headerStyle);
            GUILayout.Label($"Unspent: {stats.UnspentPoints}", _lineStyle);
            GUILayout.Label($"VIT: {stats.GetAllocated(StatType.VIT)}  STR: {stats.GetAllocated(StatType.STR)}  AGI: {stats.GetAllocated(StatType.AGI)}", _lineStyle);
            GUILayout.Label($"PRE: {stats.GetAllocated(StatType.PRE)}  SUR: {stats.GetAllocated(StatType.SUR)}", _lineStyle);
            GUILayout.EndArea();
        }

        private void DrawToasts()
        {
            var now = Time.realtimeSinceStartup;
            _toasts.RemoveAll(t => t.ExpireAt <= now);
            if (_toasts.Count == 0) return;

            var w = 260f;
            var h = 24f;
            var x = Screen.width - w - 16f;
            var y = 120f;

            for (int i = 0; i < _toasts.Count; i++)
            {
                var toast = _toasts[i];
                var remaining = toast.ExpireAt - now;
                var alpha = Mathf.Clamp01(remaining / ToastDurationSec);
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Box(new Rect(x, y + i * (h + 4), w, h), GUIContent.none);
                GUI.Label(new Rect(x + 8, y + i * (h + 4) + 4, w - 16, h), toast.Message, _toastStyle);
                GUI.color = prev;
            }
        }

        private static GUIStyle BuildStyle(int fontSize, Color color, bool bold)
        {
            return new GUIStyle
            {
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                normal = new GUIStyleState { textColor = color },
            };
        }

        private struct Toast
        {
            public string Message;
            public float ExpireAt;
        }
    }
}
