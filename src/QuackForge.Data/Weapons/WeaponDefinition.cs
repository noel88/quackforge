using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuackForge.Data.Weapons
{
    public sealed class WeaponDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("displayName")]
        public Dictionary<string, string> DisplayName { get; set; } = new();

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("tier")]
        public int Tier { get; set; }

        [JsonPropertyName("baseModel")]
        public string BaseModel { get; set; } = "";

        [JsonPropertyName("stats")]
        public WeaponStats Stats { get; set; } = new();

        [JsonPropertyName("ammoType")]
        public string AmmoType { get; set; } = "";

        [JsonPropertyName("weight")]
        public float Weight { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("dropRate")]
        public float DropRate { get; set; }

        [JsonPropertyName("craftingRecipe")]
        public CraftingRecipe? CraftingRecipe { get; set; }
    }

    public sealed class WeaponStats
    {
        [JsonPropertyName("damage")] public float Damage { get; set; }
        [JsonPropertyName("fireRate")] public float FireRate { get; set; }
        [JsonPropertyName("accuracy")] public float Accuracy { get; set; }
        [JsonPropertyName("recoil")] public float Recoil { get; set; }
        [JsonPropertyName("magazineSize")] public int MagazineSize { get; set; }
        [JsonPropertyName("reloadTime")] public float ReloadTime { get; set; }
        [JsonPropertyName("range")] public float Range { get; set; }
    }

    public sealed class CraftingRecipe
    {
        [JsonPropertyName("blueprintId")] public string BlueprintId { get; set; } = "";
        [JsonPropertyName("materials")] public List<MaterialEntry> Materials { get; set; } = new();
        [JsonPropertyName("workbenchTier")] public int WorkbenchTier { get; set; }
        [JsonPropertyName("craftTime")] public float CraftTime { get; set; }
    }

    public sealed class MaterialEntry
    {
        [JsonPropertyName("itemId")] public string ItemId { get; set; } = "";
        [JsonPropertyName("count")] public int Count { get; set; }
    }
}
