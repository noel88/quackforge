using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // SUR 1pt 당 회복량 +5% (HealGain 가산).
    //
    // 대상: CharacterMainControl.HealGain getter
    //
    // 게임 내 사용 (Round 7 RE, line 7974):
    //   health.AddHealth(healthValue * (1f + HealGain));
    // → HealGain 이 +0.05×SUR 가산되면 회복량이 (1 + 기본 + 0.05×SUR) 배.
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.HealGain), MethodType.Getter)]
    public static class HealGainPatch
    {
        public const float HealGainPerSurPct = 0.05f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var sur = _stats.GetAllocated(StatType.SUR);
            if (sur <= 0) return;

            __result += sur * HealGainPerSurPct;
        }
    }
}
