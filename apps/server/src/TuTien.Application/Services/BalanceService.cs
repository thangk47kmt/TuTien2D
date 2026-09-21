using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Application.Services;

public class BalanceService
{
    private readonly IAppDbContext _db;
    private readonly CharacterStatCalculator _stats = new();
    public BalanceService(IAppDbContext db) => _db = db;

    public async Task<object> EvaluateAsync(int level, CancellationToken ct)
    {
        var rule = await _db.GameRules.FirstOrDefaultAsync(ct) ?? new GameRule();
        var realm = await _db.RealmThresholds.Where(x => x.MinLevel <= level).OrderByDescending(x => x.MinLevel).FirstOrDefaultAsync(ct);
        var professions = await _db.Professions.Where(p => p.IsActive).ToListAsync(ct);
        var skills = await _db.ProfessionSkills.ToListAsync(ct);
        var rows = new List<object>();
        var scores = new List<int>();
        foreach (var p in professions)
        {
            var dummy = new Player { Level = level, Realm = realm?.Realm ?? 0, Fortune = 3 };
            var b = _stats.Calculate(dummy, p, [], null, rule, realm);
            var unique = Unique(p, skills.FirstOrDefault(s => s.ProfessionCode == p.Code));
            var raw = _stats.CombatPower(b, rule, unique);
            scores.Add(raw);
            rows.Add(new { p.Code, p.Name, b.Attack, b.Defense, b.Spirit, unique, raw });
        }
        var target = scores.Count == 0 ? 0 : scores.Sum() / scores.Count;
        var band = (int)(target * rule.BalanceBand);
        var flags = professions.Zip(scores, (p, s) => new { p.Code, delta = s - target, ok = Math.Abs(s - target) <= Math.Max(1, band) }).ToList();
        var counters = await _db.ProfessionCounters.ToListAsync(ct);
        return new { level, target, band, rows, flags, counters = counters.Select(c => new { c.AttackerCode, c.DefenderCode, c.DamageMul }) };
    }

    public async Task<object> SuggestAsync(int level, CancellationToken ct)
    {
        var eval = await EvaluateAsync(level, ct);
        return new { eval, note = "Bam apply de scale bonus nghe ve target." };
    }

    public async Task ApplyAsync(int level, Guid actor, CancellationToken ct)
    {
        var rule = await _db.GameRules.FirstOrDefaultAsync(ct) ?? new GameRule();
        var realm = await _db.RealmThresholds.Where(x => x.MinLevel <= level).OrderByDescending(x => x.MinLevel).FirstOrDefaultAsync(ct);
        var professions = await _db.Professions.Where(p => p.IsActive).ToListAsync(ct);
        var skills = await _db.ProfessionSkills.ToListAsync(ct);
        var scored = new List<(Profession p, int raw)>();
        foreach (var p in professions)
        {
            var dummy = new Player { Level = level, Fortune = 3 };
            var b = _stats.Calculate(dummy, p, [], null, rule, realm);
            scored.Add((p, _stats.CombatPower(b, rule, Unique(p, skills.FirstOrDefault(s => s.ProfessionCode == p.Code)))));
        }
        var target = scored.Count == 0 ? 1d : scored.Average(s => s.raw);
        foreach (var (p, raw) in scored)
        {
            var scale = target / Math.Max(1, raw);
            p.AttackBonus = (int)Math.Round(p.AttackBonus * scale);
            p.DefenseBonus = (int)Math.Round(p.DefenseBonus * scale);
            p.SpiritBonus = (int)Math.Round(p.SpiritBonus * scale);
        }
        _db.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), ActorUserId = actor, Entity = "Profession", Action = "balance_apply", NewValue = $"level:{level}", CreatedAtUtc = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
    }

    static int Unique(Profession p, ProfessionSkill? s)
    {
        var v = p.StoneRewardPercent / 5 + p.CultivationPercent / 4 + p.TechniqueDiscountPercent / 5 + p.DetectionBonus / 3;
        if (s is null) return v;
        return v + (int)(s.AtkCoeff * 8 + s.HealCoeff * 6 + s.DetectBonus + s.StoneRewardBonusPct / 3 + s.EliteBossBonusPct / 4);
    }
}
