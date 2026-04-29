using HarmonyLib;
using QuackForge.Progression.Stats;

namespace QuackForge.Progression.Patches
{
    // Health.MaxHealth 의 getter 에 VIT × 10 을 가산.
    //
    // 대상 시그니처 (역공학 #11):
    //   public float MaxHealth {
    //       get { float num = !item ? defaultMaxHealth : item.GetStatValue(maxHealthHash); ... return num; }
    //   }
    //
    // Phase 2 MVP 는 MainCharacterHealth 에만 적용하고자 하지만 성능상 단순히 Health 전체 포스트픽스 하되,
    // IsMainCharacterHealth 체크로 가드. 적이나 NPC Health 에는 영향 주지 않음.
    [HarmonyPatch(typeof(global::Health), nameof(global::Health.MaxHealth), MethodType.Getter)]
    public static class HealthMaxHealthPatch
    {
        public static int HpPerVit { get; set; } = 10;

        private static StatManager? _stats;

        public static void BindStats(StatManager stats) => _stats = stats;
        public static void BindConfig(int hpPerVit) => HpPerVit = hpPerVit;

        [HarmonyPostfix]
        public static void Postfix(global::Health __instance, ref float __result)
        {
            if (_stats == null) return;
            if (!__instance.IsMainCharacterHealth) return;

            var vit = _stats.GetAllocated(StatType.VIT);
            if (vit <= 0) return;

            __result += vit * HpPerVit;
        }
    }
}
