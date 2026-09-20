namespace TuTien.Domain.Entities;

public class WorldMap
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "qingyun";
    public string Name { get; set; } = "Thanh Van Tran";
    public int Width { get; set; } = 24;
    public int Height { get; set; } = 16;
}

public class WorldZone
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ZoneKind Kind { get; set; }
    public int MinX { get; set; }
    public int MinY { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }
    public AuraLevel AuraLevel { get; set; }
    public decimal AuraMultiplier { get; set; } = 1.0m;
    public WorldMap? Map { get; set; }
}

public class MonsterDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MonsterKind Kind { get; set; }
    public int Level { get; set; } = 1;
    public RealmKind Realm { get; set; } = RealmKind.Mortal;
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public string SkillsJson { get; set; } = "[]";
    public string ZoneCode { get; set; } = "meadow";
    public int SpawnWeight { get; set; } = 10;
    public int LifetimeSeconds { get; set; }
    public int RespawnSeconds { get; set; } = 20;
    public Guid? LootTableId { get; set; }
    public int CultivationXp { get; set; }
    public int SpiritStones { get; set; }
    public LootTable? LootTable { get; set; }
}

public class MonsterSpawn
{
    public Guid Id { get; set; }
    public Guid MonsterDefinitionId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int CurrentHp { get; set; }
    public DateTime SpawnedAtUtc { get; set; }
    public DateTime? DespawnAtUtc { get; set; }
    public DateTime? DefeatedAtUtc { get; set; }
    public bool IsAlive { get; set; } = true;
    public MonsterDefinition? Definition { get; set; }
}

public class ChestDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ItemQuality Quality { get; set; }
    public int SpawnWeight { get; set; } = 10;
    public int LifetimeSeconds { get; set; } = 600;
    public int DetectionRadius { get; set; } = 3;
    public int OpenCharges { get; set; } = 1;
    public Guid? LootTableId { get; set; }
    public bool RequiresKey { get; set; }
    public LootTable? LootTable { get; set; }
}

public class ChestSpawn
{
    public Guid Id { get; set; }
    public Guid ChestDefinitionId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public ChestStatus Status { get; set; } = ChestStatus.Spawned;
    public DateTime SpawnedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public Guid? OpenedByPlayerId { get; set; }
    public DateTime? OpenedAtUtc { get; set; }
    public uint Version { get; set; }
    public ChestDefinition? Definition { get; set; }
}

public class LootTable
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PityThreshold { get; set; } = 8;
    public int PityResetOnRare { get; set; }
    public int ConfigVersion { get; set; } = 1;
    public List<LootTableEntry> Entries { get; set; } = [];
}

public class LootTableEntry
{
    public Guid Id { get; set; }
    public Guid LootTableId { get; set; }
    public Guid? ItemDefinitionId { get; set; }
    public RewardType RewardType { get; set; }
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    public int Weight { get; set; } = 10;
    public ItemQuality MinimumQualityForPity { get; set; } = ItemQuality.Mystery;
    public LootTable? LootTable { get; set; }
    public ItemDefinition? Item { get; set; }
}

public class Rumor
{
    public Guid Id { get; set; }
    public RumorKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string ApproximateZone { get; set; } = string.Empty;
    public int Reliability { get; set; } = 50;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GameEvent
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public decimal AuraMultiplier { get; set; } = 1.0m;
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
}

public class SecretRealmDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public RealmKind RequiredRealm { get; set; } = RealmKind.QiRefining;
    public int DurationSeconds { get; set; } = 600;
    public int CooldownSeconds { get; set; } = 1800;
}

public class SecretRealmSession
{
    public Guid Id { get; set; }
    public Guid DefinitionId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettledAtUtc { get; set; }
}

public class TravelLocation
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DistanceKm { get; set; }
}

public class TravelCheckIn
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime CheckedInAtUtc { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class TravelBuff
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public decimal Multiplier { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string SourceLocation { get; set; } = string.Empty;
}

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ConfigurationVersion
{
    public Guid Id { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int Version { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public Guid? PublishedByUserId { get; set; }
    public DateTime EffectiveAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
