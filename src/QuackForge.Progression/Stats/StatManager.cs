using System;
using System.Collections.Generic;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Core.Save;

namespace QuackForge.Progression.Stats
{
    // 스탯 포인트 적립 + 스탯별 투입량 관리.
    // XP/레벨 계산은 게임 EXPManager 에 위임 (Hybrid 전략).
    public sealed class StatManager
    {
        private const string SaveKey = "QuackForge.Progression.Stats";

        private readonly Dictionary<StatType, int> _allocated = new();
        private readonly IQfLog _log = QfLogger.For("Progression.Stats");
        private readonly QfEventBus _bus;
        private readonly QfSaveContext _save;

        private int _unspent;

        // ConfigEntry 기반 정책 (Phase 3 #34). 기본값은 PRD §7.3.1.
        public int MaxPointsPerStat { get; set; } = 50;
        public bool AllowFreeRespec { get; set; } = true;

        public StatManager(QfEventBus bus, QfSaveContext save)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _save = save ?? throw new ArgumentNullException(nameof(save));
            foreach (StatType s in Enum.GetValues(typeof(StatType)))
                _allocated[s] = 0;
        }

        public int UnspentPoints => _unspent;

        public int GetAllocated(StatType stat) => _allocated.TryGetValue(stat, out var v) ? v : 0;

        public int TotalPoints
        {
            get
            {
                int sum = _unspent;
                foreach (var v in _allocated.Values) sum += v;
                return sum;
            }
        }

        public void GrantPoints(int amount, string source)
        {
            if (amount <= 0) return;
            _unspent += amount;
            _log.Info($"+{amount} stat points (source={source}, unspent={_unspent})");
            _bus.Publish(new StatPointsGrantedEvent(amount, _unspent, source));
            Persist();
        }

        public bool Allocate(StatType stat, int amount)
        {
            if (amount <= 0) return false;
            if (_unspent < amount)
            {
                _log.Warn($"Allocate({stat}, {amount}) rejected — unspent={_unspent}");
                return false;
            }
            var current = GetAllocated(stat);
            if (current + amount > MaxPointsPerStat)
            {
                _log.Warn($"Allocate({stat}, {amount}) rejected — would exceed cap {MaxPointsPerStat} (current={current})");
                return false;
            }
            _unspent -= amount;
            _allocated[stat] = current + amount;
            _log.Info($"allocated +{amount} to {stat} (now={_allocated[stat]}, unspent={_unspent})");
            _bus.Publish(new StatAllocatedEvent(stat, amount, _allocated[stat]));
            Persist();
            return true;
        }

        public bool Deallocate(StatType stat, int amount)
        {
            if (amount <= 0) return false;
            if (!AllowFreeRespec)
            {
                _log.Warn($"Deallocate({stat}, {amount}) rejected — AllowFreeRespec=false");
                return false;
            }
            var current = GetAllocated(stat);
            if (current < amount)
            {
                _log.Warn($"Deallocate({stat}, {amount}) rejected — allocated={current}");
                return false;
            }
            _allocated[stat] = current - amount;
            _unspent += amount;
            _log.Info($"deallocated {amount} from {stat} (now={_allocated[stat]}, unspent={_unspent})");
            _bus.Publish(new StatDeallocatedEvent(stat, amount, _allocated[stat], _unspent));
            Persist();
            return true;
        }

        // 모든 스탯 할당 초기화 → 전부 unspent 로 환원. AllowFreeRespec 가드.
        public bool ResetAllocation()
        {
            if (!AllowFreeRespec)
            {
                _log.Warn("ResetAllocation rejected — AllowFreeRespec=false");
                return false;
            }
            int returned = 0;
            foreach (StatType s in Enum.GetValues(typeof(StatType)))
            {
                returned += _allocated[s];
                _allocated[s] = 0;
            }
            _unspent += returned;
            _log.Info($"reset allocation — returned {returned} points (unspent={_unspent})");
            foreach (StatType s in Enum.GetValues(typeof(StatType)))
                _bus.Publish(new StatDeallocatedEvent(s, 0, 0, _unspent));
            Persist();
            return true;
        }

        // Phase 2 MVP: 누적 포인트 전부 VIT 로 자동 투입.
        public void AutoAllocateVit()
        {
            if (_unspent <= 0) return;
            var amount = _unspent;
            Allocate(StatType.VIT, amount);
        }

        public void Reset()
        {
            _unspent = 0;
            foreach (StatType s in Enum.GetValues(typeof(StatType))) _allocated[s] = 0;
            Persist();
        }

        public void Restore()
        {
            if (!_save.TryGet<StatSnapshot>(SaveKey, out var snap)) return;
            _unspent = snap.Unspent;
            foreach (StatType s in Enum.GetValues(typeof(StatType)))
                _allocated[s] = snap.Allocated != null && snap.Allocated.TryGetValue(s, out var v) ? v : 0;
            _log.Info($"restored — unspent={_unspent}, VIT={_allocated[StatType.VIT]}");
        }

        private void Persist()
        {
            _save.Set(SaveKey, new StatSnapshot
            {
                Unspent = _unspent,
                Allocated = new Dictionary<StatType, int>(_allocated),
            });
        }

        // Save 레이어에 들어갈 직렬화용 스냅샷.
        public sealed class StatSnapshot
        {
            public int Unspent { get; set; }
            public Dictionary<StatType, int> Allocated { get; set; } = new();
        }
    }

    public readonly struct StatPointsGrantedEvent
    {
        public int Amount { get; }
        public int UnspentAfter { get; }
        public string Source { get; }
        public StatPointsGrantedEvent(int amount, int unspentAfter, string source)
        {
            Amount = amount;
            UnspentAfter = unspentAfter;
            Source = source;
        }
    }

    public readonly struct StatAllocatedEvent
    {
        public StatType Stat { get; }
        public int Amount { get; }
        public int AllocatedAfter { get; }
        public StatAllocatedEvent(StatType stat, int amount, int allocatedAfter)
        {
            Stat = stat;
            Amount = amount;
            AllocatedAfter = allocatedAfter;
        }
    }

    public readonly struct StatDeallocatedEvent
    {
        public StatType Stat { get; }
        public int Amount { get; }
        public int AllocatedAfter { get; }
        public int UnspentAfter { get; }
        public StatDeallocatedEvent(StatType stat, int amount, int allocatedAfter, int unspentAfter)
        {
            Stat = stat;
            Amount = amount;
            AllocatedAfter = allocatedAfter;
            UnspentAfter = unspentAfter;
        }
    }
}
