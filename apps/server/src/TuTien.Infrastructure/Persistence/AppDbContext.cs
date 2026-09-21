using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Domain.Entities;

namespace TuTien.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<ItemDefinition> ItemDefinitions => Set<ItemDefinition>();
    public DbSet<PlayerItem> PlayerItems => Set<PlayerItem>();
    public DbSet<PlayerEquipment> PlayerEquipment => Set<PlayerEquipment>();
    public DbSet<TechniqueDefinition> TechniqueDefinitions => Set<TechniqueDefinition>();
    public DbSet<PlayerTechnique> PlayerTechniques => Set<PlayerTechnique>();
    public DbSet<PlayerDiscovery> PlayerDiscoveries => Set<PlayerDiscovery>();
    public DbSet<WorldMap> WorldMaps => Set<WorldMap>();
    public DbSet<WorldZone> WorldZones => Set<WorldZone>();
    public DbSet<MonsterDefinition> MonsterDefinitions => Set<MonsterDefinition>();
    public DbSet<MonsterSpawn> MonsterSpawns => Set<MonsterSpawn>();
    public DbSet<ChestDefinition> ChestDefinitions => Set<ChestDefinition>();
    public DbSet<ChestSpawn> ChestSpawns => Set<ChestSpawn>();
    public DbSet<LootTable> LootTables => Set<LootTable>();
    public DbSet<LootTableEntry> LootTableEntries => Set<LootTableEntry>();
    public DbSet<CombatSession> CombatSessions => Set<CombatSession>();
    public DbSet<CombatAction> CombatActions => Set<CombatAction>();
    public DbSet<RewardTransaction> RewardTransactions => Set<RewardTransaction>();
    public DbSet<CultivationSession> CultivationSessions => Set<CultivationSession>();
    public DbSet<SongCultivationSession> SongCultivationSessions => Set<SongCultivationSession>();
    public DbSet<SongCultivationMember> SongCultivationMembers => Set<SongCultivationMember>();
    public DbSet<Rumor> Rumors => Set<Rumor>();
    public DbSet<GameEvent> Events => Set<GameEvent>();
    public DbSet<SecretRealmDefinition> SecretRealms => Set<SecretRealmDefinition>();
    public DbSet<SecretRealmSession> SecretRealmSessions => Set<SecretRealmSession>();
    public DbSet<TravelLocation> TravelLocations => Set<TravelLocation>();
    public DbSet<TravelCheckIn> TravelCheckIns => Set<TravelCheckIn>();
    public DbSet<TravelBuff> TravelBuffs => Set<TravelBuff>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ConfigurationVersion> ConfigurationVersions => Set<ConfigurationVersion>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<GameRule> GameRules => Set<GameRule>();
    public DbSet<RealmThreshold> RealmThresholds => Set<RealmThreshold>();
    public DbSet<ProfessionSkill> ProfessionSkills => Set<ProfessionSkill>();
    public DbSet<ProfessionCounter> ProfessionCounters => Set<ProfessionCounter>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.UserName).HasMaxLength(64);
            e.HasOne(x => x.Player).WithOne(x => x.User).HasForeignKey<Player>(x => x.UserId);
        });
        b.Entity<RefreshToken>(e => e.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId));
        b.Entity<Player>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.Name).HasMaxLength(18);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId);
            e.HasOne(x => x.ActiveTechnique).WithMany().HasForeignKey(x => x.ActiveTechniqueId);
        });
        b.Entity<Profession>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<ItemDefinition>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<PlayerItem>(e =>
        {
            e.HasOne(x => x.Player).WithMany(x => x.Items).HasForeignKey(x => x.PlayerId);
            e.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.ItemDefinitionId);
        });
        b.Entity<TechniqueDefinition>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<PlayerTechnique>(e =>
        {
            e.HasIndex(x => new { x.PlayerId, x.TechniqueDefinitionId }).IsUnique();
            e.HasOne(x => x.Player).WithMany(x => x.Techniques).HasForeignKey(x => x.PlayerId);
            e.HasOne(x => x.Technique).WithMany().HasForeignKey(x => x.TechniqueDefinitionId);
        });
        b.Entity<PlayerDiscovery>(e => e.HasIndex(x => new { x.PlayerId, x.X, x.Y }).IsUnique());
        b.Entity<MonsterDefinition>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<ChestDefinition>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<LootTable>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<LootTableEntry>(e =>
        {
            e.HasOne(x => x.LootTable).WithMany(x => x.Entries).HasForeignKey(x => x.LootTableId);
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemDefinitionId);
        });
        b.Entity<MonsterSpawn>(e => e.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.MonsterDefinitionId));
        b.Entity<ChestSpawn>(e =>
        {
            e.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.ChestDefinitionId);
            e.Property(x => x.Version).IsConcurrencyToken();
        });
        b.Entity<CombatSession>(e =>
        {
            e.HasIndex(x => x.IdempotencyKey);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasMany(x => x.Actions).WithOne(x => x.Session).HasForeignKey(x => x.CombatSessionId);
        });
        b.Entity<RewardTransaction>(e => e.HasIndex(x => x.IdempotencyKey).IsUnique());
        b.Entity<CultivationSession>(e =>
        {
            e.HasIndex(x => new { x.PlayerId, x.IsSettled });
            e.Property(x => x.Version).IsConcurrencyToken();
        });
        b.Entity<TravelCheckIn>(e => e.HasIndex(x => x.IdempotencyKey).IsUnique());
        b.Entity<IdempotencyRecord>(e => e.HasIndex(x => new { x.UserId, x.Key }).IsUnique());
        b.Entity<GameEvent>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<WorldZone>(e => e.HasOne(x => x.Map).WithMany().HasForeignKey(x => x.MapId));
        b.Entity<GameRule>(e => e.HasIndex(x => x.Code).IsUnique());
        b.Entity<RealmThreshold>(e => e.HasIndex(x => x.Realm).IsUnique());
        b.Entity<ProfessionSkill>(e => { e.HasIndex(x => x.Code).IsUnique(); e.HasIndex(x => x.ProfessionCode); });
        b.Entity<ProfessionCounter>(e => e.HasIndex(x => new { x.AttackerCode, x.DefenderCode }).IsUnique());
    }
}
