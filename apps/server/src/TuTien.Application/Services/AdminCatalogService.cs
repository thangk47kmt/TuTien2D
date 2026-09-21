using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Application.Common;
using TuTien.Domain.Entities;

namespace TuTien.Application.Services;

public class AdminCatalogService
{
    private readonly IAppDbContext _db;
    public AdminCatalogService(IAppDbContext db) => _db = db;

    public async Task<object> CatalogAsync(CancellationToken ct) => new
    {
        rule = await _db.GameRules.FirstOrDefaultAsync(ct),
        realms = await _db.RealmThresholds.OrderBy(x => x.MinLevel).ToListAsync(ct),
        professions = await _db.Professions.OrderBy(x => x.Name).ToListAsync(ct),
        skills = await _db.ProfessionSkills.OrderBy(x => x.ProfessionCode).ToListAsync(ct),
        counters = await _db.ProfessionCounters.ToListAsync(ct),
        monsters = await _db.MonsterDefinitions.OrderBy(x => x.Name).ToListAsync(ct),
        items = await _db.ItemDefinitions.OrderBy(x => x.Name).ToListAsync(ct),
        chests = await _db.ChestDefinitions.ToListAsync(ct),
        loot = await _db.LootTableEntries.Include(e => e.LootTable).Include(e => e.Item).ToListAsync(ct)
    };

    public async Task SaveRuleAsync(GameRule incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.GameRules.FirstOrDefaultAsync(x => x.Id == incoming.Id || x.Code == incoming.Code, ct) ?? throw new AppException("missing", "Khong co rule.", 404);
        row.XpBase = incoming.XpBase; row.XpPerLevel = incoming.XpPerLevel; row.XpPerMinute = incoming.XpPerMinute;
        row.AtkBase = incoming.AtkBase; row.AtkPerLevel = incoming.AtkPerLevel;
        row.DefBase = incoming.DefBase; row.DefPerLevel = incoming.DefPerLevel;
        row.SpiBase = incoming.SpiBase; row.SpiPerLevel = incoming.SpiPerLevel;
        row.AgiBase = incoming.AgiBase; row.AgiPerLevel = incoming.AgiPerLevel;
        row.HpBase = incoming.HpBase; row.HpPerLevel = incoming.HpPerLevel;
        row.MpBase = incoming.MpBase; row.MpPerSpirit = incoming.MpPerSpirit;
        row.SkillMul = incoming.SkillMul; row.BurstMul = incoming.BurstMul; row.DefendMul = incoming.DefendMul;
        row.TargetMonsters = incoming.TargetMonsters; row.TargetChests = incoming.TargetChests;
        row.BalanceBand = incoming.BalanceBand;
        row.WAtk = incoming.WAtk; row.WDef = incoming.WDef; row.WSpi = incoming.WSpi;
        Audit(actor, "GameRule", "update", incoming.Code);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveProfessionAsync(Profession incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.Professions.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct) ?? throw new AppException("missing", "Khong co nghe.", 404);
        row.AttackBonus = incoming.AttackBonus; row.DefenseBonus = incoming.DefenseBonus; row.SpiritBonus = incoming.SpiritBonus;
        row.FortuneBonus = incoming.FortuneBonus; row.AgilityBonus = incoming.AgilityBonus;
        row.StoneRewardPercent = incoming.StoneRewardPercent; row.CultivationPercent = incoming.CultivationPercent;
        row.TechniqueDiscountPercent = incoming.TechniqueDiscountPercent; row.DetectionBonus = incoming.DetectionBonus;
        row.IsActive = incoming.IsActive;
        Audit(actor, "Profession", "update", row.Code);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveMonsterAsync(MonsterDefinition incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.MonsterDefinitions.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct) ?? throw new AppException("missing", "Khong co quai.", 404);
        row.Hp = incoming.Hp; row.Attack = incoming.Attack; row.Defense = incoming.Defense;
        row.SpawnWeight = incoming.SpawnWeight; row.CultivationXp = incoming.CultivationXp; row.SpiritStones = incoming.SpiritStones;
        Audit(actor, "Monster", "update", row.Code);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveItemAsync(ItemDefinition incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.ItemDefinitions.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct) ?? throw new AppException("missing", "Khong co do.", 404);
        row.Attack = incoming.Attack; row.Defense = incoming.Defense; row.Spirit = incoming.Spirit;
        row.Agility = incoming.Agility; row.MaxHp = incoming.MaxHp; row.HealAmount = incoming.HealAmount;
        row.ProfessionBonusPercent = incoming.ProfessionBonusPercent; row.ProfessionLocked = incoming.ProfessionLocked;
        Audit(actor, "Item", "update", row.Code);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveSkillAsync(ProfessionSkill incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.ProfessionSkills.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct) ?? throw new AppException("missing", "Khong co ky nang.", 404);
        row.AtkCoeff = incoming.AtkCoeff; row.SpiCoeff = incoming.SpiCoeff; row.HealFlat = incoming.HealFlat;
        row.HealCoeff = incoming.HealCoeff; row.SelfDamageFlat = incoming.SelfDamageFlat; row.MpCost = incoming.MpCost;
        row.DetectBonus = incoming.DetectBonus; row.StoneRewardBonusPct = incoming.StoneRewardBonusPct;
        row.CultivationBonusPct = incoming.CultivationBonusPct; row.DefendMulOverride = incoming.DefendMulOverride;
        row.EliteBossBonusPct = incoming.EliteBossBonusPct;
        Audit(actor, "ProfessionSkill", "update", row.Code);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveCounterAsync(ProfessionCounter incoming, Guid actor, CancellationToken ct)
    {
        var row = await _db.ProfessionCounters.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct) ?? throw new AppException("missing", "Khong co counter.", 404);
        row.DamageMul = incoming.DamageMul; row.DefenseMul = incoming.DefenseMul; row.Note = incoming.Note ?? "";
        Audit(actor, "Counter", "update", $"{row.AttackerCode}->{row.DefenderCode}");
        await _db.SaveChangesAsync(ct);
    }

    void Audit(Guid actor, string entity, string action, string value) =>
        _db.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), ActorUserId = actor, Entity = entity, Action = action, NewValue = value, CreatedAtUtc = DateTime.UtcNow });
}
