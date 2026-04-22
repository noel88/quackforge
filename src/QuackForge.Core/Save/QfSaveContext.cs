using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using QuackForge.Core.Logging;

namespace QuackForge.Core.Save
{
    // 모드 사이드카 세이브. 게임의 SavesSystem 과 독립된 quackforge.json 단일 파일.
    //
    // 설계 원칙 (PRD 결정 2-1):
    //   - 게임 세이브와 분리 → 게임 업데이트로 세이브 포맷 변경돼도 안전
    //   - 사용자가 모드 제거해도 게임 세이브 그대로
    //   - JSON text 포맷 → 사용자가 필요 시 수동 편집/복구 가능
    //
    // 파일 위치:
    //   기본: ~/Library/Application Support/Steam/steamapps/common/Escape From Duckov/BepInEx/quackforge.json (Mac)
    //   구성: QfSaveContext(Path) 생성자로 오버라이드 가능
    //
    // 값은 타입 이름 + JSON 문자열로 저장. 재구동 시 같은 타입으로 역직렬화.
    // 복잡한 객체 (Dictionary<Enum, int> 등) 도 동작하도록 JsonSerializer 에 위임.
    public sealed class QfSaveContext
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        private readonly string? _filePath;
        private readonly Dictionary<string, Entry> _store = new(StringComparer.Ordinal);
        private readonly object _lock = new();
        private readonly IQfLog _log = QfLogger.For("Core.Save");

        private bool _dirty;

        public QfSaveContext()
            : this(filePath: null) { }

        public QfSaveContext(string? filePath)
        {
            _filePath = filePath;
            if (!string.IsNullOrEmpty(_filePath)) Load();
        }

        public string? FilePath => _filePath;

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            var json = JsonSerializer.Serialize(value, typeof(T), JsonOpts);
            lock (_lock)
            {
                _store[key] = new Entry { TypeName = SimpleTypeName(typeof(T)), Json = json };
                _dirty = true;
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            lock (_lock)
            {
                if (_store.TryGetValue(key, out var entry) && entry != null)
                {
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<T>(entry.Json, JsonOpts);
                        if (parsed != null)
                        {
                            value = parsed;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"TryGet<{typeof(T).Name}>('{key}') deserialize failed", ex);
                    }
                }
            }
            value = default!;
            return false;
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key required", nameof(key));
            lock (_lock)
            {
                if (_store.Remove(key))
                {
                    _dirty = true;
                    return true;
                }
                return false;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _store.Clear();
                _dirty = true;
            }
        }

        // 명시적으로 호출. Plugin 쪽에서 주기적으로 or OnDestroy 에서 트리거 예정.
        public bool FlushIfDirty()
        {
            if (string.IsNullOrEmpty(_filePath)) return false;
            Dictionary<string, Entry> snapshot;
            lock (_lock)
            {
                if (!_dirty) return false;
                snapshot = new Dictionary<string, Entry>(_store, StringComparer.Ordinal);
                _dirty = false;
            }

            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var tmp = _filePath + ".tmp";
                var payload = JsonSerializer.Serialize(new FileFormat { Version = 1, Entries = snapshot }, JsonOpts);
                File.WriteAllText(tmp, payload);
                if (File.Exists(_filePath)) File.Delete(_filePath);
                File.Move(tmp, _filePath);
                _log.Debug($"flushed {snapshot.Count} entries → {_filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"flush failed: {_filePath}", ex);
                // 다음 주기에 재시도
                lock (_lock) _dirty = true;
                return false;
            }
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath)) return;
            try
            {
                var payload = File.ReadAllText(_filePath);
                var parsed = JsonSerializer.Deserialize<FileFormat>(payload, JsonOpts);
                if (parsed?.Entries == null) return;

                lock (_lock)
                {
                    _store.Clear();
                    foreach (var kv in parsed.Entries) _store[kv.Key] = kv.Value;
                    _dirty = false;
                }
                _log.Info($"loaded {parsed.Entries.Count} entries from {_filePath}");
            }
            catch (Exception ex)
            {
                _log.Error($"load failed: {_filePath} — starting empty", ex);
            }
        }

        private static string SimpleTypeName(Type t) => t.FullName ?? t.Name;

        // On-disk 포맷 (버전 1). 향후 스키마 마이그레이션 지점.
        private sealed class FileFormat
        {
            public int Version { get; set; } = 1;
            public Dictionary<string, Entry> Entries { get; set; } = new();
        }

        private sealed class Entry
        {
            public string TypeName { get; set; } = "";
            public string Json { get; set; } = "";
        }
    }
}
