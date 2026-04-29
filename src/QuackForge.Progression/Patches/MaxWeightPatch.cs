using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // STR 1pt 당 캐리 용량 +0.5 (게임 stat "MaxWeight" 단위 그대로).
    //
    // 대상: CharacterMainControl.MaxWeight getter
    //   public float MaxWeight {
    //       get { return characterItem ? characterItem.GetStatValue(maxWeightHash) : 0f; }
    //   }
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.MaxWeight), MethodType.Getter)]
    public static class MaxWeightPatch
    {
        public const float WeightPerStr = 0.5f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var str = _stats.GetAllocated(StatType.STR);
            if (str <= 0) return;

            __result += str * WeightPerStr;
        }
    }
}
