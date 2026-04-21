using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuackForge.Data.Armors
{
    public sealed class ArmorDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("displayName")]
        public Dictionary<string, string> DisplayName { get; set; } = new();

        [JsonPropertyName("slot")]
        public string Slot { get; set; } = "";

        [JsonPropertyName("tier")]
        public int Tier { get; set; }

        [JsonPropertyName("baseModel")]
        public string BaseModel { get; set; } = "";

        [JsonPropertyName("stats")]
        public ArmorStats Stats { get; set; } = new();

        [JsonPropertyName("durability")]
        public int Durability { get; set; }

        [JsonPropertyName("repairCost")]
        public int RepairCost { get; set; }
    }

    public sealed class ArmorStats
    {
        [JsonPropertyName("protection")] public float Protection { get; set; }
        [JsonPropertyName("penetrationResistance")] public float PenetrationResistance { get; set; }
        [JsonPropertyName("weightPenalty")] public float WeightPenalty { get; set; }

        // 음수 허용 (= 이동 보너스) — PRD §5.4.2
        [JsonPropertyName("movementPenalty")] public float MovementPenalty { get; set; }

        [JsonPropertyName("staminaBonus")] public float StaminaBonus { get; set; }
    }
}
