using BepInEx.Configuration;
using QuackForge.Core;
using QuackForge.Core.Logging;
using QuackForge.Progression.Debug;
using UnityEngine;

namespace QuackForge.Loader.Runtime
{
    // Phase 2 QA 발견:
    //   Duckov 부팅 막바지에 BepInEx Chainloader 가 배치한 Plugin host GameObject 가
    //   Resources.UnloadUnusedAssets / 첫 scene 로드 흐름에 휩쓸려 Destroy 된다.
    //   Plugin.Awake 시점엔 활성 scene 이 아직 없어 DontDestroyOnLoad 도 무효.
    //
    // 해결:
    //   매 프레임 로직(키바인드, 주기적 사이드카 flush)을 별도 GameObject 로 분리하고
    //   첫 scene 이 로드된 후에 생성/등록해서 DontDestroyOnLoad 가 실효를 갖게 한다.
    public sealed class QfRuntime : MonoBehaviour
    {
        private readonly IQfLog _log = QfLogger.For("Runtime");

        private ConfigEntry<KeyboardShortcut>? _addXpKey;
        private ConfigEntry<int>? _addXpAmount;
        private ConfigEntry<float>? _flushIntervalSec;
        private float _nextFlushAt;

        public void Init(
            ConfigEntry<KeyboardShortcut> addXpKey,
            ConfigEntry<int> addXpAmount,
            ConfigEntry<float> flushIntervalSec)
        {
            _addXpKey = addXpKey;
            _addXpAmount = addXpAmount;
            _flushIntervalSec = flushIntervalSec;
            _nextFlushAt = Time.realtimeSinceStartup + flushIntervalSec.Value;
            _log.Info($"runtime ready (addXpKey={addXpKey.Value}, flushInterval={flushIntervalSec.Value}s)");
        }

        private void Update()
        {
            if (_addXpKey != null && _addXpKey.Value.IsDown())
            {
                DebugCommands.AddXp(_addXpAmount!.Value);
            }

            if (_flushIntervalSec != null && Time.realtimeSinceStartup >= _nextFlushAt)
            {
                QfCore.Instance?.Save.FlushIfDirty();
                _nextFlushAt = Time.realtimeSinceStartup + _flushIntervalSec.Value;
            }
        }

        private void OnApplicationQuit()
        {
            QfCore.Instance?.Save.FlushIfDirty();
        }

        private void OnDestroy()
        {
            // Persistent GameObject 라 정상 게임 종료 외엔 호출되지 않아야 함.
            QfCore.Instance?.Save.FlushIfDirty();
        }
    }
}
