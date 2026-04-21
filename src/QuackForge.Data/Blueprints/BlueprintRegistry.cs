using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Core.Save;

namespace QuackForge.Data.Blueprints
{
    public sealed class BlueprintRegistry
    {
        public const string IdPrefix = "quackforge_blueprint_";
        private const string ResourceNamespace = "QuackForge.Data.Blueprints.Definitions.";
        private const string SaveKey = "QuackForge.Blueprints.Unlocked";

        private readonly Dictionary<string, BlueprintDefinition> _cache = new(StringComparer.Ordinal);
        private readonly HashSet<string> _unlocked = new(StringComparer.Ordinal);
        private readonly IQfLog _log = QfLogger.For("Data.Blueprints");
        private readonly QfEventBus _bus;
        private readonly QfSaveContext _save;

        public BlueprintRegistry(QfEventBus bus, QfSaveContext save)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public IReadOnlyCollection<BlueprintDefinition> All => _cache.Values;
        public IReadOnlyCollection<string> UnlockedIds => _unlocked;
        public int Count => _cache.Count;

        public bool TryGet(string id, out BlueprintDefinition definition)
        {
            if (_cache.TryGetValue(id, out var found))
            {
                definition = found;
                return true;
            }
            definition = default!;
            return false;
        }

        public bool IsUnlocked(string id) => _unlocked.Contains(id);

        public bool Unlock(string id)
        {
            if (!_cache.ContainsKey(id))
            {
                _log.Warn($"Unlock called with unknown blueprint '{id}'");
                return false;
            }
            if (!_unlocked.Add(id)) return false;
            Persist();
            _log.Info($"blueprint unlocked: {id}");
            _bus.Publish(new BlueprintUnlockedEvent(id));
            return true;
        }

        // 1회용 블루프린트 제작 시 소모
        public bool Consume(string id)
        {
            if (!_cache.TryGetValue(id, out var def))
            {
                _log.Warn($"Consume called with unknown blueprint '{id}'");
                return false;
            }
            if (!def.ConsumeOnUse)
            {
                _log.Debug($"Consume skipped (permanent blueprint): {id}");
                return false;
            }
            if (!_unlocked.Remove(id)) return false;
            Persist();
            _log.Info($"blueprint consumed: {id}");
            _bus.Publish(new BlueprintConsumedEvent(id));
            return true;
        }

        public int LoadAll()
        {
            _cache.Clear();
            var assembly = typeof(BlueprintRegistry).Assembly;
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(ResourceNamespace, StringComparison.Ordinal) && n.EndsWith(".json", StringComparison.Ordinal))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            foreach (var resource in resourceNames)
            {
                try
                {
                    var def = LoadDefinition(assembly, resource, options);
                    Validate(def, resource);
                    if (_cache.ContainsKey(def.Id))
                    {
                        _log.Error($"duplicate blueprint id '{def.Id}' (resource: {resource}) — skipped");
                        continue;
                    }
                    _cache[def.Id] = def;
                    _log.Debug($"loaded blueprint '{def.Id}' → weapon={def.UnlocksWeapon ?? "-"} armor={def.UnlocksArmor ?? "-"}");
                }
                catch (Exception ex)
                {
                    _log.Error($"failed to load blueprint resource {resource}", ex);
                }
            }

            RestoreUnlocked();
            _log.Info($"blueprint registry ready — {_cache.Count} entries, {_unlocked.Count} unlocked.");
            return _cache.Count;
        }

        private static BlueprintDefinition LoadDefinition(Assembly assembly, string resource, JsonSerializerOptions options)
        {
            using var stream = assembly.GetManifestResourceStream(resource)
                               ?? throw new InvalidOperationException($"resource stream null for {resource}");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<BlueprintDefinition>(json, options)
                   ?? throw new InvalidDataException($"deserialized to null: {resource}");
        }

        private static void Validate(BlueprintDefinition def, string resource)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                throw new InvalidDataException($"{resource}: id missing");
            if (!def.Id.StartsWith(IdPrefix, StringComparison.Ordinal))
                throw new InvalidDataException($"{resource}: id '{def.Id}' must start with '{IdPrefix}'");
            if (string.IsNullOrWhiteSpace(def.UnlocksWeapon) && string.IsNullOrWhiteSpace(def.UnlocksArmor))
                throw new InvalidDataException($"{resource}: must unlock weapon or armor ({def.Id})");
        }

        private void RestoreUnlocked()
        {
            _unlocked.Clear();
            if (_save.TryGet<HashSet<string>>(SaveKey, out var saved))
            {
                foreach (var id in saved)
                {
                    if (_cache.ContainsKey(id)) _unlocked.Add(id);
                }
            }
        }

        private void Persist()
        {
            _save.Set(SaveKey, new HashSet<string>(_unlocked, StringComparer.Ordinal));
        }
    }
}
