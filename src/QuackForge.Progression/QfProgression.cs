using System;
using QuackForge.Core;
using QuackForge.Core.Logging;
using QuackForge.Progression.Stats;
using QuackForge.Progression.Xp;

namespace QuackForge.Progression
{
    public sealed class QfProgression
    {
        public static QfProgression? Instance { get; private set; }

        public StatManager Stats { get; }
        public XpSubscriber XpSubscriber { get; }
        public ProgressionSettings Settings { get; }
        public StatModifierBinder Binder { get; }

        private readonly IQfLog _log = QfLogger.For("Progression");

        private QfProgression(StatManager stats, XpSubscriber xp, ProgressionSettings settings, StatModifierBinder binder)
        {
            Stats = stats;
            XpSubscriber = xp;
            Settings = settings;
            Binder = binder;
        }

        public static QfProgression Initialize(ProgressionSettings? settings = null)
        {
            if (Instance != null) throw new InvalidOperationException("QfProgression already initialized.");
            var core = QfCore.Instance ?? throw new InvalidOperationException("QfCore not initialized — call QfCore.Initialize first.");
            settings ??= new ProgressionSettings();

            var stats = new StatManager(core.Events, core.Save)
            {
                MaxPointsPerStat = settings.MaxPointsPerStat,
                AllowFreeRespec = settings.AllowFreeRespec,
            };
            stats.Restore();

            var xp = new XpSubscriber(stats, settings.PointsPerLevel);
            xp.Subscribe();

            // 보너스 적용 = StatModifierBinder 가 game stat 시스템에 정식 Modifier 등록 (#35).
            // 개별 stat 별 Harmony getter 패치는 폐기 (Health.MaxHealth 의 percent-damage
            // 우회만 별도 패치).
            var binder = StatModifierBinder.Attach(stats, settings);

            if (settings.AutoAllocateVit)
            {
                stats.AutoAllocateVit();
                core.Events.Subscribe<StatPointsGrantedEvent>(_ => stats.AutoAllocateVit());
            }

            Instance = new QfProgression(stats, xp, settings, binder);
            Instance._log.Info($"QfProgression initialized (pointsPerLevel={settings.PointsPerLevel}, autoAllocateVit={settings.AutoAllocateVit}, maxPointsPerStat={settings.MaxPointsPerStat}, allowFreeRespec={settings.AllowFreeRespec})");
            return Instance;
        }
    }
}
