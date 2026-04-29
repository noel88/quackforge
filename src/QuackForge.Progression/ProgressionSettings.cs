namespace QuackForge.Progression
{
    // QuackForge.Progression 의 모든 튜닝 가능한 값을 한 객체로 묶음.
    // BepInEx ConfigEntry 와 1:1 매핑 (Plugin.Awake 에서 cfg → 인스턴스 빌드).
    //
    // 기본값은 PRD §7.3.1 / §7.4.4 + Phase 2 MVP 결정 그대로.
    public sealed class ProgressionSettings
    {
        // [General]
        public int PointsPerLevel { get; set; } = 1;
        public bool AutoAllocateVit { get; set; } = true;
        public int MaxPointsPerStat { get; set; } = 50;
        public bool AllowFreeRespec { get; set; } = true;

        // [Effects] — 1포인트당 효과량
        public int HpPerVit { get; set; } = 10;
        public float WeightPerStr { get; set; } = 0.5f;
        public float MeleeDamagePerStrPct { get; set; } = 0.02f;
        public float StaminaPerAgi { get; set; } = 3f;
        public float MoveabilityPerAgiPct { get; set; } = 0.01f;
        public float RecoilControlPerPre { get; set; } = 0.01f;
        public float ScatterReducePerPrePct { get; set; } = 0.015f;
        public float ScatterFloor { get; set; } = 0.1f;
        public float HealGainPerSurPct { get; set; } = 0.05f;
        public float CostReducePerSurPct { get; set; } = 0.03f;
        public float CostFloor { get; set; } = 0.1f;
    }
}
