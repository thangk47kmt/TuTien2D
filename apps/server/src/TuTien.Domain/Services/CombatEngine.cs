using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public readonly record struct StrikeResult(int Damage, bool Crit, string Tag);

public static class CombatEngine
{
    public static StrikeResult PlayerStrike(
        int attack, int enemyDefense, int agility,
        Random rng, GameRule rule, bool skill, bool burst,
        decimal counterMul, int skillExtra, int eliteBonusPct)
    {
        var mitigated = Mitigate(attack + skillExtra, enemyDefense, rule.DefDivPlayer);
        var roll = Variance(mitigated, rule.HitVariance, rng);
        var crit = rng.Next(100) < CritChance(agility);
        var mul = 1m;
        if (skill) mul *= rule.SkillMul;
        if (burst) mul *= rule.BurstMul;
        if (crit) mul *= 1.35m;
        mul *= ClampMul(counterMul);
        if (eliteBonusPct > 0) mul *= 1 + eliteBonusPct / 100m;
        var dmg = Math.Max(1, (int)Math.Round(roll * (double)mul));
        return new StrikeResult(dmg, crit, crit ? "crit" : skill ? "skill" : "hit");
    }

    public static StrikeResult MonsterStrike(
        int attack, int playerDefense, Random rng, GameRule rule, bool defending, decimal defendOverride)
    {
        var mitigated = Mitigate(attack, playerDefense, rule.DefDivMonster);
        var roll = Variance(mitigated, rule.HitVariance, rng);
        var defMul = defending
            ? (defendOverride > 0 ? (double)defendOverride : (double)rule.DefendMul)
            : 1.0;
        var dmg = Math.Max(1, (int)Math.Round(roll * defMul));
        return new StrikeResult(dmg, false, defending ? "block" : "hit");
    }

    public static int Mitigate(int attack, int defense, int k)
    {
        k = Math.Max(1, k);
        var denom = attack + defense * k;
        if (denom <= 0) return Math.Max(1, attack);
        return Math.Max(1, (int)Math.Round(attack * (double)attack / denom));
    }

    public static double Variance(int baseDmg, int hitVariance, Random rng)
    {
        var span = Math.Clamp(hitVariance, 0, 25);
        if (span == 0) return baseDmg;
        var pct = rng.Next(-span, span + 1) / 100.0;
        return Math.Max(1, baseDmg * (1 + pct));
    }

    public static int CritChance(int agility) => Math.Clamp(4 + agility / 8, 4, 28);
    public static decimal ClampMul(decimal mul) => Math.Clamp(mul, 0.50m, 1.80m);
    public static int MpCost(ProfessionSkill? skill, GameRule rule) => skill is { MpCost: > 0 } ? skill.MpCost : 10;
}
