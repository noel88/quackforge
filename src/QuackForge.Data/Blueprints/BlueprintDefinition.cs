using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuackForge.Data.Blueprints
{
    public sealed class BlueprintDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("displayName")]
        public Dictionary<string, string> DisplayName { get; set; } = new();

        [JsonPropertyName("unlocksWeapon")]
        public string? UnlocksWeapon { get; set; }

        [JsonPropertyName("unlocksArmor")]
        public string? UnlocksArmor { get; set; }

        [JsonPropertyName("unlockConditions")]
        public UnlockConditions Conditions { get; set; } = new();

        // true = 1회용 (제작 시 소모), false = 영구 해금
        [JsonPropertyName("consumeOnUse")]
        public bool ConsumeOnUse { get; set; }
    }

    public sealed class UnlockConditions
    {
        [JsonPropertyName("minPlayerLevel")] public int MinPlayerLevel { get; set; }
        [JsonPropertyName("dropSources")] public List<DropSource> DropSources { get; set; } = new();
    }

    public sealed class DropSource
    {
        [JsonPropertyName("enemy")] public string? Enemy { get; set; }
        [JsonPropertyName("challenge")] public string? Challenge { get; set; }
        [JsonPropertyName("chance")] public float Chance { get; set; }
        [JsonPropertyName("guaranteed")] public bool Guaranteed { get; set; }
    }
}
