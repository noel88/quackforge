using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // AGI 1pt 당 이동 속도 +1% (multiplier).
    //
    // 대상: CharacterMainControl.CharacterMoveability getter
    //   public float CharacterMoveability => GetFloatStatValue(moveabilityHash);
    //
    // 게임은 walk/run/turn 모두 이 값을 곱연산하므로 한 군데 패치로 모두 영향:
    //   CharacterWalkSpeed = walkSpeed * CharacterMoveability * (gun.MoveSpeedMultiplier ...)
    //   CharacterRunSpeed  = runSpeed  * CharacterMoveability * ...
    //   CharacterTurnSpeed = turnSpeed * CharacterMoveability
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.CharacterMoveability), MethodType.Getter)]
    public static class CharacterMoveabilityPatch
    {
        public const float MoveabilityPerAgiPct = 0.01f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var agi = _stats.GetAllocated(StatType.AGI);
            if (agi <= 0) return;

            __result *= 1f + agi * MoveabilityPerAgiPct;
        }
    }
}
