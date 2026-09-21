using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Application.Services;

public static class CombatRulesHelper
{
    public static async Task<(GameRule Rule, ProfessionSkill? Skill, decimal Counter)> LoadAsync(IAppDbContext db, Player player, string monsterName, CancellationToken ct)
    {
        var rule = await db.GameRules.FirstOrDefaultAsync(ct) ?? new GameRule();
        var skill = player.Profession is null ? null
            : await db.ProfessionSkills.FirstOrDefaultAsync(s => s.ProfessionCode == player.Profession.Code, ct);
        decimal counter = 1m;
        if (skill is { EliteBossBonusPct: > 0 } && (monsterName.Contains("Boss") || monsterName.Contains("Tinh anh") || monsterName.Contains("Hiem")))
            counter *= 1 + skill.EliteBossBonusPct / 100m;
        return (rule, skill, counter);
    }

    public static int SkillExtra(StatBreakdown b, ProfessionSkill? skill) =>
        skill is null ? 0 : (int)Math.Round((double)(b.Attack * skill.AtkCoeff + b.Spirit * skill.SpiCoeff));

    public static void ApplySelfAndHeal(Player player, ProfessionSkill? skill)
    {
        if (skill is null) return;
        if (skill.SelfDamageFlat > 0) player.Hp = Math.Max(1, player.Hp - skill.SelfDamageFlat);
        if (skill.HealFlat > 0 || skill.HealCoeff > 0)
            player.Hp = Math.Min(player.MaxHp, player.Hp + skill.HealFlat + (int)Math.Round(player.MaxHp * 0 + (double)skill.HealCoeff * 10));
    }
}
