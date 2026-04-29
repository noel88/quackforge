using Duckov;
using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // PRE 1pt 당 RecoilControl +0.01 (multiplier 가산).
    //
    // 대상: CharacterMainControl.RecoilControl getter (default 1f, item stat 으로 보정)
    //
    // 게임 내 사용 (Round 7 RE):
    //   recoilV = ... * (1f / gun.CharacterRecoilControl) * recoilMultiplier;
    // → RecoilControl 값이 클수록 1/x 가 작아져 반동 감소.
    [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.RecoilControl), MethodType.Getter)]
    public static class RecoilControlPatch
    {
        public static float RecoilControlPerPre { get; set; } = 0.01f;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;
        public static void BindConfig(float recoilControlPerPre) => RecoilControlPerPre = recoilControlPerPre;

        [HarmonyPostfix]
        public static void Postfix(CharacterMainControl __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacter) return;

            var pre = _stats.GetAllocated(StatType.PRE);
            if (pre <= 0) return;

            __result += pre * RecoilControlPerPre;
        }
    }
}
