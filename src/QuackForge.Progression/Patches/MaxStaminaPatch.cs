using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // AGI 1pt 당 최대 스태미나 +3 (게임 stat "MaxStamina" 단위 그대로).
    //
    // 대상: CharacterMainControl.MaxStamina getter
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.MaxStamina), MethodType.Getter)]
    public static class MaxStaminaPatch
    {
        public static float StaminaPerAgi { get; set; } = 3f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;
        public static void BindConfig(float staminaPerAgi) => StaminaPerAgi = staminaPerAgi;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var agi = _stats.GetAllocated(StatType.AGI);
            if (agi <= 0) return;

            __result += agi * StaminaPerAgi;
        }
    }
}
