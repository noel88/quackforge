using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace QuackForge.Loader
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.returntrue.quackforge";
        public const string PluginName = "QuackForge";
        public const string PluginVersion = "0.0.1";

        public static ManualLogSource Log { get; private set; } = null!;

        private Harmony _harmony = null!;
        private ConfigEntry<bool> _enableMod = null!;

        private void Awake()
        {
            Log = Logger;

            _enableMod = Config.Bind(
                section: "General",
                key: "EnableMod",
                defaultValue: true,
                description: "Master switch for QuackForge. Disable to temporarily deactivate without uninstalling.");

            if (!_enableMod.Value)
            {
                Log.LogWarning($"{PluginName} is disabled via config (General.EnableMod = false).");
                return;
            }

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            Log.LogInfo($"\ud83e\udd86 {PluginName} is awake. Forging begins. (v{PluginVersion})");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Log?.LogInfo($"{PluginName} unloaded.");
        }
    }
}
