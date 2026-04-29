using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // STR 1pt 당 근접 데미지 +2% (가산).
    //
    // 대상: CharacterMainControl.MeleeDamageMultiplier getter
    //   (default 0f. 게임이 damage * (1 + multiplier) 식으로 호출하므로 +0.02×STR 가산
    //    = STR 1pt 당 +2% 근접 데미지)
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.MeleeDamageMultiplier), MethodType.Getter)]
    public static class MeleeDamageMultiplierPatch
    {
        public static float MeleeDamagePerStrPct { get; set; } = 0.02f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;
        public static void BindConfig(float meleeDamagePerStrPct) => MeleeDamagePerStrPct = meleeDamagePerStrPct;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var str = _stats.GetAllocated(StatType.STR);
            if (str <= 0) return;

            __result += str * MeleeDamagePerStrPct;
        }
    }
}
