using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using QuackForge.Core.Logging;

namespace QuackForge.Data.Weapons
{
    public sealed class WeaponRegistry
    {
        public const string IdPrefix = "quackforge_weapon_";
        private const string ResourceNamespace = "QuackForge.Data.Weapons.Definitions.";

        private readonly Dictionary<string, WeaponDefinition> _cache = new(StringComparer.Ordinal);
        private readonly IQfLog _log = QfLogger.For("Data.Weapons");

        public IReadOnlyCollection<WeaponDefinition> All => _cache.Values;
        public int Count => _cache.Count;

        public bool TryGet(string id, out WeaponDefinition definition)
        {
            if (_cache.TryGetValue(id, out var found))
            {
                definition = found;
                return true;
            }
            definition = default!;
            return false;
        }

        public int LoadAll()
        {
            _cache.Clear();
            var assembly = typeof(WeaponRegistry).Assembly;
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
                        _log.Error($"duplicate weapon id '{def.Id}' (resource: {resource}) — skipped");
                        continue;
                    }
                    _cache[def.Id] = def;
                    _log.Debug($"loaded weapon '{def.Id}' ({def.Category} tier {def.Tier})");
                }
                catch (Exception ex)
                {
                    _log.Error($"failed to load weapon resource {resource}", ex);
                }
            }
            _log.Info($"weapon registry ready — {_cache.Count} entries from {resourceNames.Length} resources.");
            return _cache.Count;
        }

        private static WeaponDefinition LoadDefinition(Assembly assembly, string resource, JsonSerializerOptions options)
        {
            using var stream = assembly.GetManifestResourceStream(resource)
                               ?? throw new InvalidOperationException($"resource stream null for {resource}");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var def = JsonSerializer.Deserialize<WeaponDefinition>(json, options)
                      ?? throw new InvalidDataException($"deserialized to null: {resource}");
            return def;
        }

        private static void Validate(WeaponDefinition def, string resource)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                throw new InvalidDataException($"{resource}: id missing");
            if (!def.Id.StartsWith(IdPrefix, StringComparison.Ordinal))
                throw new InvalidDataException($"{resource}: id '{def.Id}' must start with '{IdPrefix}'");
            if (string.IsNullOrWhiteSpace(def.Category))
                throw new InvalidDataException($"{resource}: category missing for '{def.Id}'");
            if (def.Stats == null)
                throw new InvalidDataException($"{resource}: stats missing for '{def.Id}'");
        }
    }
}
