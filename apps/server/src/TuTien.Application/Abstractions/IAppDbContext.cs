using Microsoft.EntityFrameworkCore;
using TuTien.Domain.Entities;

namespace TuTien.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Player> Players { get; }
    DbSet<Profession> Professions { get; }
    DbSet<ItemDefinition> ItemDefinitions { get; }
    DbSet<PlayerItem> PlayerItems { get; }
    DbSet<PlayerEquipment> PlayerEquipment { get; }
    DbSet<TechniqueDefinition> TechniqueDefinitions { get; }
    DbSet<PlayerTechnique> PlayerTechniques { get; }
    DbSet<PlayerDiscovery> PlayerDiscoveries { get; }
    DbSet<WorldMap> WorldMaps { get; }
    DbSet<WorldZone> WorldZones { get; }
    DbSet<MonsterDefinition> MonsterDefinitions { get; }
    DbSet<MonsterSpawn> MonsterSpawns { get; }
    DbSet<ChestDefinition> ChestDefinitions { get; }
    DbSet<ChestSpawn> ChestSpawns { get; }
    DbSet<LootTable> LootTables { get; }
    DbSet<LootTableEntry> LootTableEntries { get; }
    DbSet<CombatSession> CombatSessions { get; }
    DbSet<CombatAction> CombatActions { get; }
    DbSet<RewardTransaction> RewardTransactions { get; }
    DbSet<CultivationSession> CultivationSessions { get; }
    DbSet<SongCultivationSession> SongCultivationSessions { get; }
    DbSet<SongCultivationMember> SongCultivationMembers { get; }
    DbSet<Rumor> Rumors { get; }
    DbSet<GameEvent> Events { get; }
    DbSet<SecretRealmDefinition> SecretRealms { get; }
    DbSet<SecretRealmSession> SecretRealmSessions { get; }
    DbSet<TravelLocation> TravelLocations { get; }
    DbSet<TravelCheckIn> TravelCheckIns { get; }
    DbSet<TravelBuff> TravelBuffs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ConfigurationVersion> ConfigurationVersions { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }
    DbSet<GameRule> GameRules { get; }
    DbSet<RealmThreshold> RealmThresholds { get; }
    DbSet<ProfessionSkill> ProfessionSkills { get; }
    DbSet<ProfessionCounter> ProfessionCounters { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
