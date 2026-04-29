using HarmonyLib;
using QuackForge.Core.Logging;

namespace QuackForge.Progression.Patches
{
    // Approach D — Duckov.Effects.DamageAction.OnTriggeredPositive 가
    // percent damage 계산에 MainControl.Health.MaxHealth 를 사용하는데, 우리
    // VIT 보너스가 그 MaxHealth 에 합산되어 있어 percent damage debuff 효과까지
    // 비례 증폭되는 부작용 발생 (#35 IT2 발견).
    //
    // 해결: DamageAction 실행 스코프 동안 thread-static 플래그를 켜고, 그 동안엔
    // Health.MaxHealth getter 가 우리 보너스를 빼서 반환 (HealthMaxHealthDamageScopePatch).
    // Prefix 에서 set, Finalizer 에서 clear (예외 시에도 안전).
    [HarmonyPatch(typeof(Duckov.Effects.DamageAction), "OnTriggeredPositive")]
    public static class DamageActionPatch
    {
        [System.ThreadStatic]
        private static bool _excludeBonus;

        public static bool ExcludeBonus => _excludeBonus;

        private static readonly IQfLog Log = QfLogger.For("Patches.DamageAction");

        [HarmonyPrefix]
        public static void Prefix() => _excludeBonus = true;

        // Finalizer 는 정상 + 예외 종료 모두에서 실행됨. flag leak 방지.
        [HarmonyFinalizer]
        public static System.Exception? Finalizer(System.Exception __exception)
        {
            _excludeBonus = false;
            if (__exception != null)
                Log.Warn($"DamageAction.OnTriggeredPositive threw (rethrow): {__exception.GetType().Name}");
            return __exception;
        }
    }
}
