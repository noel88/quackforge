using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using QuackForge.Core;
using QuackForge.Data.Armors;
using QuackForge.Data.Blueprints;
using QuackForge.Data.Weapons;

namespace QuackForge.Loader
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.returntrue.quackforge";
        public const string PluginName = "QuackForge";
        public const string PluginVersion = "0.0.1";

        public static ManualLogSource Log { get; private set; } = null!;
        public static WeaponRegistry Weapons { get; private set; } = null!;
        public static ArmorRegistry Armors { get; private set; } = null!;
        public static BlueprintRegistry Blueprints { get; private set; } = null!;

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

            QfCore.Initialize(Log, Config);

            Weapons = new WeaponRegistry();
            Weapons.LoadAll();

            Armors = new ArmorRegistry();
            Armors.LoadAll();

            var core = QfCore.Instance!;
            Blueprints = new BlueprintRegistry(core.Events, core.Save);
            Blueprints.LoadAll();

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
