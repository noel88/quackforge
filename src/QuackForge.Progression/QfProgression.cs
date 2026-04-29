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
        public ProgressionSettings Settings { get; }

        private readonly IQfLog _log = QfLogger.For("Progression");

        private QfProgression(StatManager stats, XpSubscriber xp, ProgressionSettings settings)
        {
            Stats = stats;
            XpSubscriber = xp;
            Settings = settings;
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

            // 모든 패치에 stat 바인딩 + 효과량 cfg 바인딩.
            HealthMaxHealthPatch.BindStats(stats);
            HealthMaxHealthPatch.BindConfig(settings.HpPerVit);
            MaxWeightPatch.BindStats(stats);
            MaxWeightPatch.BindConfig(settings.WeightPerStr);
            MaxStaminaPatch.BindStats(stats);
            MaxStaminaPatch.BindConfig(settings.StaminaPerAgi);
            CharacterMoveabilityPatch.BindStats(stats);
            CharacterMoveabilityPatch.BindConfig(settings.MoveabilityPerAgiPct);
            RecoilControlPatch.BindStats(stats);
            RecoilControlPatch.BindConfig(settings.RecoilControlPerPre);
            HealGainPatch.BindStats(stats);
            HealGainPatch.BindConfig(settings.HealGainPerSurPct);
            // 잔여 4종 (#31 PR B)
            MeleeDamageMultiplierPatch.BindStats(stats);
            MeleeDamageMultiplierPatch.BindConfig(settings.MeleeDamagePerStrPct);
            GunScatterMultiplierPatch.BindStats(stats);
            GunScatterMultiplierPatch.BindConfig(settings.ScatterReducePerPrePct, settings.ScatterFloor);
            EnergyCostPerMinPatch.BindStats(stats);
            EnergyCostPerMinPatch.BindConfig(settings.CostReducePerSurPct, settings.CostFloor);
            WaterCostPerMinPatch.BindStats(stats);
            WaterCostPerMinPatch.BindConfig(settings.CostReducePerSurPct, settings.CostFloor);

            if (settings.AutoAllocateVit)
            {
                // Phase 2 MVP: 이전 세션에서 분배 안 된 포인트가 있으면 전부 VIT 로 자동 투입.
                stats.AutoAllocateVit();

                // 이후 적립되는 포인트도 즉시 VIT 로 투입되도록 이벤트 구독.
                core.Events.Subscribe<StatPointsGrantedEvent>(_ => stats.AutoAllocateVit());
            }

            Instance = new QfProgression(stats, xp, settings);
            Instance._log.Info($"QfProgression initialized (pointsPerLevel={settings.PointsPerLevel}, autoAllocateVit={settings.AutoAllocateVit}, maxPointsPerStat={settings.MaxPointsPerStat}, allowFreeRespec={settings.AllowFreeRespec})");
            return Instance;
        }
    }
}
