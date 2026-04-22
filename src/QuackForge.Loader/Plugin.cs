using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using QuackForge.Core;
using QuackForge.Data.Armors;
using QuackForge.Data.Blueprints;
using QuackForge.Data.Weapons;
using QuackForge.Loader.UI;
using QuackForge.Progression;
using QuackForge.Progression.Debug;
using UnityEngine;

namespace QuackForge.Loader
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.returntrue.quackforge";
        public const string PluginName = "QuackForge";
        // BepInPlugin attribute 는 numeric System.Version 만 허용하므로 alpha suffix 는
        // 생략. 실 릴리즈 태그는 v0.1.0-alpha.1 (CHANGELOG 및 git tag 참조).
        public const string PluginVersion = "0.1.0";

        public static ManualLogSource Log { get; private set; } = null!;
        public static WeaponRegistry Weapons { get; private set; } = null!;
        public static ArmorRegistry Armors { get; private set; } = null!;
        public static BlueprintRegistry Blueprints { get; private set; } = null!;
        public static QfProgression Progression { get; private set; } = null!;

        private Harmony _harmony = null!;
        private ConfigEntry<bool> _enableMod = null!;
        private ConfigEntry<bool> _debugOverlayEnabled = null!;
        private ConfigEntry<KeyboardShortcut> _debugAddXpKey = null!;
        private ConfigEntry<int> _debugAddXpAmount = null!;
        private ConfigEntry<float> _saveFlushIntervalSec = null!;

        private float _nextFlushAt;

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

            _debugOverlayEnabled = Config.Bind(
                "Debug",
                "OverlayEnabled",
                true,
                "Show the IMGUI overlay (level-up toast + stat summary). Disable for release-like playtest.");

            _debugAddXpKey = Config.Bind(
                "Debug",
                "AddXpKey",
                new KeyboardShortcut(KeyCode.F9),
                "Press to grant debug XP to the player (goes through game's EXPManager.AddExp).");

            _debugAddXpAmount = Config.Bind(
                "Debug",
                "AddXpAmount",
                1000,
                "XP amount to grant per AddXp hotkey press.");

            _saveFlushIntervalSec = Config.Bind(
                "General",
                "SaveFlushIntervalSec",
                15f,
                "Interval between sidecar quackforge.json flushes (seconds).");

            var savePath = ResolveSaveFilePath();
            QfCore.Initialize(Log, Config, savePath);

            Weapons = new WeaponRegistry();
            Weapons.LoadAll();

            Armors = new ArmorRegistry();
            Armors.LoadAll();

            var core = QfCore.Instance!;
            Blueprints = new BlueprintRegistry(core.Events, core.Save);
            Blueprints.LoadAll();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);
            _harmony.PatchAll(typeof(QfProgression).Assembly);

            // Harmony 패치가 Progression.Patches.* 를 등록한 뒤 Progression 초기화 (순서 의존 없음).
            Progression = QfProgression.Initialize(pointsPerLevel: 1, autoAllocateVit: true);

            if (_debugOverlayEnabled.Value) DebugOverlay.Attach(this, Progression, core.Events);

            _nextFlushAt = Time.realtimeSinceStartup + _saveFlushIntervalSec.Value;

            Log.LogInfo($"\ud83e\udd86 {PluginName} is awake. Forging begins. (v{PluginVersion})");
        }

        private void Update()
        {
            if (_enableMod == null || !_enableMod.Value) return;

            // Debug: AddXp 키바인드
            if (_debugAddXpKey != null && _debugAddXpKey.Value.IsDown())
            {
                DebugCommands.AddXp(_debugAddXpAmount.Value);
            }

            // 주기적 사이드카 flush
            if (Time.realtimeSinceStartup >= _nextFlushAt)
            {
                QfCore.Instance?.Save.FlushIfDirty();
                _nextFlushAt = Time.realtimeSinceStartup + _saveFlushIntervalSec.Value;
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            QfCore.Instance?.Save.FlushIfDirty();
            Log?.LogInfo($"{PluginName} unloaded.");
        }

        private static string ResolveSaveFilePath()
        {
            // BepInEx.Paths.ConfigPath 는 BepInEx/config/. 같은 계층에 quackforge.json 둬서
            // 모드 사이드카임을 드러냄. 게임 자체 세이브와 분리 (PRD 결정 2-1).
            var bepinexRoot = Path.GetDirectoryName(Paths.ConfigPath) ?? Paths.BepInExRootPath;
            return Path.Combine(bepinexRoot, "quackforge.json");
        }
    }
}
