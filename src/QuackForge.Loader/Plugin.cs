using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using QuackForge.Core;
using QuackForge.Data.Armors;
using QuackForge.Data.Blueprints;
using QuackForge.Data.Weapons;
using QuackForge.Loader.Runtime;
using QuackForge.Loader.UI;
using QuackForge.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Harmony 인스턴스는 static. Plugin GameObject 가 죽어도 패치 자체는
        // AppDomain 어셈블리에 살아남으므로 UnpatchSelf 를 호출하지 않는다.
        private static Harmony? _harmony;
        private static bool _runtimeAttached;

        private ConfigEntry<bool> _enableMod = null!;
        private ConfigEntry<bool> _debugOverlayEnabled = null!;
        private ConfigEntry<KeyboardShortcut> _debugAddXpKey = null!;
        private ConfigEntry<int> _debugAddXpAmount = null!;
        private ConfigEntry<float> _saveFlushIntervalSec = null!;

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

            // Phase 2 QA 발견:
            //   Plugin GameObject 는 Duckov 부팅 막바지에 destroy 될 수 있다.
            //   Plugin.Awake 시점엔 활성 scene 이 없어 DontDestroyOnLoad 도 무효.
            //   매 프레임 로직(F9 키바인드, save flush) + DebugOverlay 는
            //   첫 frame 이 돈 후에 별도 persistent host 로 attach.
            //
            //   Duckov 가 자체 mod 로더로 scene 을 갈아끼우면서 SceneManager.sceneLoaded
            //   이벤트를 안 발화하는 케이스 (Round 3 확인) 가 있어 Update() fallback 도 둠.
            //   둘 중 먼저 발화하는 쪽이 attach.
            // Round 5 진단 발견:
            //   Plugin GameObject 가 Awake → OnEnable → 즉시 OnDisable (active=False) 패턴.
            //   Duckov 부팅 단계에서 BepInEx host GO 에 SetActive(false) 가 호출됨.
            //   destroy 는 안 되지만 Update/Coroutine 도 안 돌고 sceneLoaded 도 못 받음.
            //
            //   해결: Awake 끝에서 즉시 별도 host GO 생성 + HideFlags.HideAndDontSave.
            //   HideAndDontSave 는 Unity 의 unused asset cleanup / scene sweep 에서
            //   GameObject 를 제외하므로 boot sweep 을 회피한다.
            //   (BepInEx 5.4.21+ chainloader 가 동일 이유로 사용하는 패턴)
            AttachRuntime("Awake");

            // sceneLoaded fallback 은 유지 (host 도 어떻게든 죽으면 다음 scene 에서 재시도).
            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo($"🦆 {PluginName} bootstrapped (v{PluginVersion}).");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_runtimeAttached) return;
            AttachRuntime($"sceneLoaded:{scene.name}");
        }

        private void AttachRuntime(string trigger)
        {
            if (_runtimeAttached) return;
            _runtimeAttached = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            var host = new GameObject("QuackForge.Runtime");
            // HideAndDontSave 가 boot sweep 회피의 핵심.
            host.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(host);

            var runtime = host.AddComponent<QfRuntime>();
            runtime.Init(_debugAddXpKey, _debugAddXpAmount, _saveFlushIntervalSec);

            if (_debugOverlayEnabled.Value)
            {
                DebugOverlay.Attach(runtime, Progression, QfCore.Instance!.Events);
            }

            Log.LogInfo($"🦆 {PluginName} runtime attached (trigger={trigger}). Forging begins.");
        }

        private void OnDestroy()
        {
            // Plugin GameObject 는 Duckov 부팅 막바지에 destroy 되지만,
            // 실제 런타임은 QfRuntime / DebugOverlay 별도 host 에 살아있으므로
            // Harmony.UnpatchSelf 호출하지 않음 (패치를 살려둬야 함).
            // sceneLoaded 미연결 상태면 cleanup.
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
