using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using QuackForge.Core.Logging;

namespace QuackForge.Data.Armors
{
    public sealed class ArmorRegistry
    {
        public const string IdPrefix = "quackforge_armor_";
        private const string ResourceNamespace = "QuackForge.Data.Armors.Definitions.";

        private readonly Dictionary<string, ArmorDefinition> _cache = new(StringComparer.Ordinal);
        private readonly IQfLog _log = QfLogger.For("Data.Armors");

        public IReadOnlyCollection<ArmorDefinition> All => _cache.Values;
        public int Count => _cache.Count;

        public bool TryGet(string id, out ArmorDefinition definition)
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
            var assembly = typeof(ArmorRegistry).Assembly;
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
                        _log.Error($"duplicate armor id '{def.Id}' (resource: {resource}) — skipped");
                        continue;
                    }
                    _cache[def.Id] = def;
                    _log.Debug($"loaded armor '{def.Id}' ({def.Slot} tier {def.Tier})");
                }
                catch (Exception ex)
                {
                    _log.Error($"failed to load armor resource {resource}", ex);
                }
            }
            _log.Info($"armor registry ready — {_cache.Count} entries from {resourceNames.Length} resources.");
            return _cache.Count;
        }

        private static ArmorDefinition LoadDefinition(Assembly assembly, string resource, JsonSerializerOptions options)
        {
            using var stream = assembly.GetManifestResourceStream(resource)
                               ?? throw new InvalidOperationException($"resource stream null for {resource}");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var def = JsonSerializer.Deserialize<ArmorDefinition>(json, options)
                      ?? throw new InvalidDataException($"deserialized to null: {resource}");
            return def;
        }

        private static void Validate(ArmorDefinition def, string resource)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                throw new InvalidDataException($"{resource}: id missing");
            if (!def.Id.StartsWith(IdPrefix, StringComparison.Ordinal))
                throw new InvalidDataException($"{resource}: id '{def.Id}' must start with '{IdPrefix}'");
            if (string.IsNullOrWhiteSpace(def.Slot))
                throw new InvalidDataException($"{resource}: slot missing for '{def.Id}'");
            if (def.Stats == null)
                throw new InvalidDataException($"{resource}: stats missing for '{def.Id}'");
        }
    }
}
