using HarmonyLib;

namespace QuackForge.Progression.Patches
{
    // Approach C+D 마이그레이션 (#35):
    //   - 보너스 가산은 더 이상 여기서 하지 않음. StatModifierBinder 가 Item stat
    //     시스템에 정식 Modifier 로 등록 → MaxHealth getter 도 자동으로 반영됨.
    //   - 단, Duckov.Effects.DamageAction.OnTriggeredPositive 의 percent-damage
    //     계산이 MainControl.Health.MaxHealth 를 사용하는데 우리 VIT 보너스가
    //     포함된 값이라 디버프(출혈 등) 효과 비례 증폭 부작용 발생.
    //   - DamageAction 실행 스코프 동안만 우리 보너스를 빼서 반환.
    [HarmonyPatch(typeof(global::Health), nameof(global::Health.MaxHealth), MethodType.Getter)]
    public static class HealthMaxHealthPatch
    {
        [HarmonyPostfix]
        public static void Postfix(global::Health __instance, ref float __result)
        {
            if (!DamageActionPatch.ExcludeBonus) return;
            if (!__instance.IsMainCharacterHealth) return;
            __result -= QfStatBonusBoard.VitMaxHpBonus;
            if (__result < 1f) __result = 1f; // safety: percent of 0 = 0 damage; 1 floor
        }
    }
}
