using System.Collections.Generic;
using Duckov;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using QuackForge.Core.Logging;
using UnityEngine;

namespace QuackForge.Progression.Stats
{
    // Approach C — 게임의 Modifier 시스템 (ItemStatsSystem.dll) 에 우리 보너스를
    // 정식 등록. Health.MaxHealth getter 등 모든 진입점이 stat.Value 를 통해
    // 통일된 값을 받음 (Postfix 패치 제거).
    //
    // MainCharacter 가 바뀌면 (raid 진입/탈출, 캐릭터 교체 등) 자동으로 unbind/rebind.
    // Modifier 인스턴스는 유지 + Modifier.Value 만 갱신 (재생성 비용 없음).
    public sealed class StatModifierBinder : MonoBehaviour
    {
        private static StatModifierBinder? _instance;

        private readonly IQfLog _log = QfLogger.For("Stats.Binder");
        private StatManager? _stats;
        private ProgressionSettings? _settings;
        private CharacterMainControl? _bound;
        private readonly Dictionary<int, Modifier> _mods = new();

        // (게임 stat key, 사용할 ModifierType)
        // PRD §7.4.2 매핑 + game RE 결과로 결정.
        private static readonly (string Key, ModifierType Type)[] StatMap =
        {
            ("MaxHealth",              ModifierType.Add),
            ("MaxWeight",              ModifierType.Add),
            ("MaxStamina",             ModifierType.Add),
            ("Moveability",            ModifierType.PercentageMultiply),
            ("RecoilControl",          ModifierType.Add),
            ("HealGain",               ModifierType.Add),
            ("MeleeDamageMultiplier",  ModifierType.Add),
            ("GunScatterMultiplier",   ModifierType.PercentageMultiply),
            ("EnergyCost",             ModifierType.PercentageMultiply),
            ("WaterCost",              ModifierType.PercentageMultiply),
        };

        public static StatModifierBinder Attach(StatManager stats, ProgressionSettings settings)
        {
            if (_instance != null) return _instance;
            var go = new GameObject("QuackForgeStatModifierBinder");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<StatModifierBinder>();
            _instance._stats = stats;
            _instance._settings = settings;
            return _instance;
        }

        private void Update()
        {
            // raid 전환 / 캐릭터 재생성 등으로 MainCharacter 가 바뀌면 자동 rebind.
            var current = LevelManager.Instance?.MainCharacter;
            if (current != _bound)
            {
                UnbindCurrent();
                BindTo(current);
            }
            UpdateModifierValues();
        }

        private void BindTo(CharacterMainControl? character)
        {
            _bound = character;
            if (character == null) return;
            var item = character.CharacterItem;
            if (item == null) return;

            foreach (var (key, type) in StatMap)
            {
                int hash = key.GetHashCode();
                var stat = item.GetStat(hash);
                if (stat == null) continue;

                var mod = new Modifier(type, 0f, this);
                stat.AddModifier(mod);
                _mods[hash] = mod;
            }
            _log.Info($"bound to MainCharacter ({_mods.Count}/{StatMap.Length} stats)");
        }

        private void UnbindCurrent()
        {
            if (_mods.Count == 0)
            {
                _bound = null;
                return;
            }
            foreach (var kv in _mods)
                kv.Value.RemoveFromTarget();
            _mods.Clear();
            _bound = null;
            QfStatBonusBoard.VitMaxHpBonus = 0f;
            _log.Debug("unbound modifiers");
        }

        private void UpdateModifierValues()
        {
            if (_stats == null || _settings == null || _mods.Count == 0) return;

            var s = _settings;
            var st = _stats;
            int vit = st.GetAllocated(StatType.VIT);
            int str = st.GetAllocated(StatType.STR);
            int agi = st.GetAllocated(StatType.AGI);
            int pre = st.GetAllocated(StatType.PRE);
            int sur = st.GetAllocated(StatType.SUR);

            float vitBonus = vit * s.HpPerVit;

            SetMod("MaxHealth",             vitBonus);
            SetMod("MaxWeight",             str * s.WeightPerStr);
            SetMod("MaxStamina",            agi * s.StaminaPerAgi);
            // PercentageMultiply 의 Value 는 (1+x) 의 x 부분. AGI 1pt 당 +1% 이속 → +0.01.
            SetMod("Moveability",           agi * s.MoveabilityPerAgiPct);
            SetMod("RecoilControl",         pre * s.RecoilControlPerPre);
            SetMod("HealGain",              sur * s.HealGainPerSurPct);
            SetMod("MeleeDamageMultiplier", str * s.MeleeDamagePerStrPct);
            // 정확도/소비 = 곱연산 감소. floor 적용 (PercentageMultiply 자체엔 floor 없음).
            SetMod("GunScatterMultiplier",  Mathf.Max(-pre * s.ScatterReducePerPrePct, s.ScatterFloor - 1f));
            SetMod("EnergyCost",            Mathf.Max(-sur * s.CostReducePerSurPct,  s.CostFloor - 1f));
            SetMod("WaterCost",             Mathf.Max(-sur * s.CostReducePerSurPct,  s.CostFloor - 1f));

            QfStatBonusBoard.VitMaxHpBonus = vitBonus;
        }

        private void SetMod(string key, float value)
        {
            int hash = key.GetHashCode();
            if (_mods.TryGetValue(hash, out var mod) && mod.Value != value)
                mod.Value = value;
        }

        private void OnDestroy()
        {
            UnbindCurrent();
            if (_instance == this) _instance = null;
        }
    }
}
