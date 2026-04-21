namespace QuackForge.Data.Blueprints
{
    public readonly struct BlueprintUnlockedEvent
    {
        public string BlueprintId { get; }
        public BlueprintUnlockedEvent(string blueprintId) => BlueprintId = blueprintId;
    }

    public readonly struct BlueprintConsumedEvent
    {
        public string BlueprintId { get; }
        public BlueprintConsumedEvent(string blueprintId) => BlueprintId = blueprintId;
    }
}
