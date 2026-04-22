using Duckov;
using QuackForge.Core.Logging;

namespace QuackForge.Progression.Debug
{
    // Phase 2 개발/테스트용 디버그 커맨드. 게임 EXPManager 래퍼.
    //
    // BepInEx Configuration Manager (혹은 향후 UI) 에서 호출할 수 있도록 static.
    // 실제 노출은 Plugin.Awake 에서 커맨드 시스템에 등록 (추후).
    public static class DebugCommands
    {
        private static readonly IQfLog Log = QfLogger.For("Progression.Debug");

        public static bool AddXp(int amount)
        {
            if (amount <= 0)
            {
                Log.Warn($"AddXp({amount}) rejected — must be positive");
                return false;
            }

            if (EXPManager.Instance == null)
            {
                Log.Error("AddXp called but EXPManager.Instance is null (game not fully booted?)");
                return false;
            }

            var before = EXPManager.EXP;
            var ok = EXPManager.AddExp(amount);
            if (!ok)
            {
                Log.Error("EXPManager.AddExp returned false");
                return false;
            }

            Log.Info($"AddXp({amount}) — EXP {before} → {EXPManager.EXP}, Level={EXPManager.Level}");
            return true;
        }
    }
}
