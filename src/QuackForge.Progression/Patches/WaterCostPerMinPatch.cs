using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;
using UnityEngine;

namespace QuackForge.Progression.Patches
{
    // SUR 1pt 당 갈증(물) 소비 -3%.
    //
    // 대상: CharacterMainControl.WaterCostPerMin getter
    //   (분당 water 소비. 작을수록 물 오래감)
    //   곱연산 형: __result *= (1 - 0.03×SUR), floor 0.1.
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.WaterCostPerMin), MethodType.Getter)]
    public static class WaterCostPerMinPatch
    {
        public static float CostReducePerSurPct { get; set; } = 0.03f;
        public static float CostFloor { get; set; } = 0.1f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;

        public static void BindConfig(float costReducePerSurPct, float costFloor)
        {
            CostReducePerSurPct = costReducePerSurPct;
            CostFloor = costFloor;
        }

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var sur = _stats.GetAllocated(StatType.SUR);
            if (sur <= 0) return;

            var multiplier = 1f - sur * CostReducePerSurPct;
            __result *= Mathf.Max(multiplier, CostFloor);
        }
    }
}
