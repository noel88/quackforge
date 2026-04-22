using System;
using QuackForge.Core;
using QuackForge.Core.Logging;
using QuackForge.Progression.Patches;
using QuackForge.Progression.Stats;
using QuackForge.Progression.Xp;

namespace QuackForge.Progression
{
    public sealed class QfProgression
    {
        public static QfProgression? Instance { get; private set; }

        public StatManager Stats { get; }
        public XpSubscriber XpSubscriber { get; }

        private readonly IQfLog _log = QfLogger.For("Progression");

        private QfProgression(StatManager stats, XpSubscriber xp)
        {
            Stats = stats;
            XpSubscriber = xp;
        }

        public static QfProgression Initialize(int pointsPerLevel = 1, bool autoAllocateVit = true)
        {
            if (Instance != null) throw new InvalidOperationException("QfProgression already initialized.");
            var core = QfCore.Instance ?? throw new InvalidOperationException("QfCore not initialized — call QfCore.Initialize first.");

            var stats = new StatManager(core.Events, core.Save);
            stats.Restore();

            var xp = new XpSubscriber(stats, pointsPerLevel);
            xp.Subscribe();

            HealthMaxHealthPatch.BindStats(stats);

            if (autoAllocateVit)
            {
                // Phase 2 MVP: 이전 세션에서 분배 안 된 포인트가 있으면 전부 VIT 로 자동 투입.
                stats.AutoAllocateVit();

                // 이후 적립되는 포인트도 즉시 VIT 로 투입되도록 이벤트 구독.
                core.Events.Subscribe<StatPointsGrantedEvent>(_ => stats.AutoAllocateVit());
            }

            Instance = new QfProgression(stats, xp);
            Instance._log.Info($"QfProgression initialized (pointsPerLevel={pointsPerLevel}, autoAllocateVit={autoAllocateVit})");
            return Instance;
        }
    }
}
