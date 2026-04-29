using System;
using Duckov;
using QuackForge.Core.Logging;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Xp
{
    // 게임의 EXPManager.onLevelChanged(int prev, int next) 구독 → 스탯 포인트 적립.
    // Hybrid 전략: XP/레벨 계산은 게임에 위임, 우리는 레벨업 이벤트만 사용.
    public sealed class XpSubscriber : IDisposable
    {
        private readonly StatManager _stats;
        private readonly IQfLog _log = QfLogger.For("Progression.Xp");
        private readonly int _pointsPerLevel;
        private bool _subscribed;

        public XpSubscriber(StatManager stats, int pointsPerLevel = 1)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _pointsPerLevel = Math.Max(1, pointsPerLevel);
        }

        public void Subscribe()
        {
            if (_subscribed) return;
            EXPManager.onLevelChanged += OnLevelChanged;
            _subscribed = true;
            _log.Info($"subscribed to EXPManager.onLevelChanged (points/level = {_pointsPerLevel})");
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            EXPManager.onLevelChanged -= OnLevelChanged;
            _subscribed = false;
        }

        public void Dispose() => Unsubscribe();

        private void OnLevelChanged(int prev, int next)
        {
            // try/catch: 콜백에서 예외가 나면 게임의 EXPManager 가 다른 핸들러까지
            // 끊어버릴 수 있으므로 invocation list 보호 차원에서 swallow + log.
            try
            {
                if (next <= prev) return; // 하향 조정은 포인트 반환 안 함 (Phase 2 범위 밖)
                var gained = (next - prev) * _pointsPerLevel;
                _log.Info($"level {prev} → {next} — granting {gained} stat points");
                _stats.GrantPoints(gained, $"level {prev}→{next}");
            }
            catch (Exception e)
            {
                _log.Error($"OnLevelChanged threw: {e}");
            }
        }
    }
}
