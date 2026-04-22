namespace QuackForge.Progression.Stats
{
    // PRD §6.3 5-스탯 시스템. Phase 2 MVP 는 VIT 만 실제 효과 반영, 나머지는 enum 선반영.
    public enum StatType
    {
        // Vitality — MaxHP 증가
        VIT = 0,

        // Strength — 근접 데미지, 권총 데미지 (Phase 3)
        STR = 1,

        // Agility — 이동 속도, 민첩성 (Phase 3)
        AGI = 2,

        // Precision — 명중률, 헤드샷 배수 (Phase 3)
        PRE = 3,

        // Survival — 스태미나, 방어 계수 (Phase 3)
        SUR = 4,
    }
}
