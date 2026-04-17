using System;
using System.Collections.Generic;

namespace QuackForge.Core.Save
{
    // Phase 1 stub: in-memory store. Phase 2 (T2.5) 에서 quackforge.json 사이드카로 확장.
    // 게임의 SavesSystem 직접 사용이 아닌 사이드카 전략 (PRD 결정 2-1).
    public sealed class QfSaveContext
    {
        private readonly Dictionary<string, object?> _store = new Dictionary<string, object?>(StringComparer.Ordinal);
        private readonly object _lock = new object();

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            lock (_lock) _store[key] = value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            lock (_lock)
            {
                if (_store.TryGetValue(key, out var raw) && raw is T typed)
                {
                    value = typed;
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            lock (_lock) return _store.Remove(key);
        }

        public void Clear()
        {
            lock (_lock) _store.Clear();
        }
    }
}
