using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Application.Services;

public readonly record struct CombatContext(GameRule Rule, ProfessionSkill? Skill, decimal Counter, int ElitePct, int MonsterSpeed, MonsterKind Kind);

public static class CombatRulesHelper
{
    public static async Task<CombatContext> LoadAsync(IAppDbContext db, Player player, Guid monsterDefinitionId, CancellationToken ct)
    {
        var rule = await db.GameRules.FirstOrDefaultAsync(ct) ?? new GameRule();
        var skill = player.Profession is null
            ? null
            : await db.ProfessionSkills.FirstOrDefaultAsync(s => s.ProfessionCode == player.Profession.Code, ct);
        var monster = await db.MonsterDefinitions.FirstOrDefaultAsync(m => m.Id == monsterDefinitionId, ct);
        var kind = monster?.Kind ?? MonsterKind.Common;
        var speed = monster?.Speed ?? 5;
        decimal counter = 1m;
        var proxy = CombatEngine.DefenderProxy(kind);
        if (!string.IsNullOrEmpty(proxy) && player.Profession is not null)
        {
            var row = await db.ProfessionCounters.FirstOrDefaultAsync(
                c => c.AttackerCode == player.Profession.Code && c.DefenderCode == proxy, ct);
            if (row is not null) counter = row.DamageMul;
        }
        var elite = CombatEngine.ResolveEliteBonus(kind, skill);
        return new CombatContext(rule, skill, CombatEngine.ClampMul(counter), elite, speed, kind);
    }

    public static async Task<(GameRule Rule, ProfessionSkill? Skill, decimal Counter)> LoadAsync(
        IAppDbContext db, Player player, string monsterName, CancellationToken ct)
    {
        var ctx = await LoadByNameAsync(db, player, monsterName, ct);
        return (ctx.Rule, ctx.Skill, ctx.Counter);
    }

    static async Task<CombatContext> LoadByNameAsync(IAppDbContext db, Player player, string monsterName, CancellationToken ct)
    {
        var kind = monsterName.Contains("Boss", StringComparison.OrdinalIgnoreCase) ? MonsterKind.Boss
            : monsterName.Contains("Tinh", StringComparison.OrdinalIgnoreCase) ? MonsterKind.Elite
            : monsterName.Contains("Hiem", StringComparison.OrdinalIgnoreCase) ? MonsterKind.Rare
            : MonsterKind.Common;
        var ctx = await LoadAsync(db, player, Guid.Empty, ct);
        return ctx with { Kind = kind, ElitePct = CombatEngine.ResolveEliteBonus(kind, ctx.Skill) };
    }

    public static int SkillExtra(StatBreakdown b, ProfessionSkill? skill)
    {
        if (skill is null) return 0;
        return Math.Max(0, (int)Math.Round(b.Attack * (double)skill.AtkCoeff + b.Spirit * (double)skill.SpiCoeff));
    }

    public static void ApplySelfAndHeal(Player player, ProfessionSkill? skill)
    {
        if (skill is null) return;
        if (skill.SelfDamageFlat > 0) player.Hp = Math.Max(1, player.Hp - skill.SelfDamageFlat);
        if (skill.SelfDamagePct > 0) player.Hp = Math.Max(1, player.Hp - player.MaxHp * skill.SelfDamagePct / 100);
        if (skill.HealFlat > 0 || skill.HealCoeff > 0)
        {
            var heal = skill.HealFlat + (int)Math.Round(player.Spirit * (double)skill.HealCoeff);
            player.Hp = Math.Min(player.MaxHp, player.Hp + Math.Max(0, heal));
        }
    }
}
