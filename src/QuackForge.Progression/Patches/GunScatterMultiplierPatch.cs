using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;
using UnityEngine;

namespace QuackForge.Progression.Patches
{
    // PRE 1pt 당 사격 정확도 +1.5% (산발 -1.5%).
    //
    // 대상: CharacterMainControl.GunScatterMultiplier getter
    //   (default 1f, 작은 값일수록 정확. 곱연산 형: __result *= (1 - 0.015×PRE))
    //   극단값 방지를 위해 floor 가드 (기본 0.1).
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.GunScatterMultiplier), MethodType.Getter)]
    public static class GunScatterMultiplierPatch
    {
        public static float ScatterReducePerPrePct { get; set; } = 0.015f;
        public static float ScatterFloor { get; set; } = 0.1f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;

        public static void BindConfig(float scatterReducePerPrePct, float scatterFloor)
        {
            ScatterReducePerPrePct = scatterReducePerPrePct;
            ScatterFloor = scatterFloor;
        }

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var pre = _stats.GetAllocated(StatType.PRE);
            if (pre <= 0) return;

            var multiplier = 1f - pre * ScatterReducePerPrePct;
            __result *= Mathf.Max(multiplier, ScatterFloor);
        }
    }
}
