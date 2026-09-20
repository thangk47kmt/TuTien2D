namespace TuTien.Domain.Entities;

public class CombatSession
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid? MonsterSpawnId { get; set; }
    public Guid MonsterDefinitionId { get; set; }
    public string MonsterName { get; set; } = string.Empty;
    public int MonsterHp { get; set; }
    public int MonsterMaxHp { get; set; }
    public int MonsterAttack { get; set; }
    public int MonsterDefense { get; set; }
    public int PlayerHpSnapshot { get; set; }
    public int PlayerAttackSnapshot { get; set; }
    public int PlayerDefenseSnapshot { get; set; }
    public CombatStatus Status { get; set; } = CombatStatus.Active;
    public string? IdempotencyKey { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public DateTime LastActionAtUtc { get; set; }
    public uint Version { get; set; }
    public List<CombatAction> Actions { get; set; } = [];
}

public class CombatAction
{
    public Guid Id { get; set; }
    public Guid CombatSessionId { get; set; }
    public CombatActionType Type { get; set; }
    public Guid? ItemId { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public string Note { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public CombatSession? Session { get; set; }
}

public class RewardTransaction
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public RewardSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public RewardType RewardType { get; set; }
    public Guid? DefinitionId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CorrelationId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public int LootTableVersion { get; set; } = 1;
}

public class CultivationSession
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string LocationCode { get; set; } = "qingyun";
    public decimal AuraSnapshot { get; set; } = 1.0m;
    public Guid? TechniqueSnapshotId { get; set; }
    public decimal Coefficient { get; set; } = 1.0m;
    public int ExpectedXp { get; set; }
    public CultivationStatus Status { get; set; } = CultivationStatus.Running;
    public bool IsSettled { get; set; }
    public DateTime? SettledAtUtc { get; set; }
    public Guid? ResultId { get; set; }
    public string? IdempotencyKey { get; set; }
    public uint Version { get; set; }
}

public class SongCultivationSession
{
    public Guid Id { get; set; }
    public Guid HostPlayerId { get; set; }
    public bool RequiresOnline { get; set; }
    public decimal Multiplier { get; set; } = 1.10m;
    public CultivationStatus Status { get; set; } = CultivationStatus.Running;
    public DateTime WindowStartUtc { get; set; }
    public DateTime WindowEndUtc { get; set; }
    public bool IsSettled { get; set; }
    public bool IsSimulatedCompanion { get; set; } = true;
}

public class SongCultivationMember
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PlayerId { get; set; }
    public bool PartCompleted { get; set; }
    public DateTime? LastHeartbeatUtc { get; set; }
}

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
