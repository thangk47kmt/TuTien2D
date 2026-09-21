using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public readonly record struct StrikeResult(int Damage, bool Crit, bool Miss, bool Glance, string Tag);

public static class CombatEngine
{
    public const decimal MaxMul = 1.80m;
    public const decimal MinMul = 0.50m;
    public const double MaxArmorReduction = 0.72;

    public static StrikeResult PlayerStrike(
        int attack, int enemyDefense, int agility,
        Random rng, GameRule rule, bool skill, bool burst,
        decimal counterMul, int skillExtra, int eliteBonusPct,
        int enemySpeed = 5)
    {
        if (IsMiss(agility, enemySpeed, rng))
            return new StrikeResult(0, false, true, false, "miss");

        var power = Math.Max(0, attack + Math.Max(0, skillExtra));
        var mitigated = Mitigate(power, enemyDefense, rule.DefDivPlayer, rule.DefAbsorb);
        var glance = IsGlance(rng);
        var roll = Variance(mitigated, rule.HitVariance, rng);
        if (glance) roll *= 0.70;

        var crit = !glance && rng.Next(100) < CritChance(agility);
        var mul = 1m;
        if (skill) mul *= Positive(rule.SkillMul, 1.55m);
        if (burst) mul *= Positive(rule.BurstMul, 1.60m);
        if (crit) mul *= 1.35m;
        mul *= ClampMul(counterMul);
        if (eliteBonusPct > 0) mul *= 1 + Math.Clamp(eliteBonusPct, 0, 80) / 100m;

        var dmg = Math.Max(1, (int)Math.Round(roll * (double)mul));
        var tag = glance ? "glance" : crit ? "crit" : skill ? "skill" : "hit";
        return new StrikeResult(dmg, crit, false, glance, tag);
    }

    public static StrikeResult MonsterStrike(
        int attack, int playerDefense, int playerAgility,
        Random rng, GameRule rule, bool defending, decimal defendOverride,
        int monsterSpeed = 5)
    {
        if (IsDodge(playerAgility, monsterSpeed, rng))
            return new StrikeResult(0, false, true, false, "dodge");

        var mitigated = Mitigate(Math.Max(0, attack), playerDefense, rule.DefDivMonster, rule.DefAbsorb);
        var roll = Variance(mitigated, rule.HitVariance, rng);
        var defMul = defending
            ? (double)Positive(defendOverride > 0 ? defendOverride : rule.DefendMul, 0.45m)
            : 1.0;
        var dmg = Math.Max(1, (int)Math.Round(roll * defMul));
        return new StrikeResult(dmg, false, false, false, defending ? "block" : "hit");
    }

    public static StrikeResult MonsterStrike(
        int attack, int playerDefense, Random rng, GameRule rule, bool defending, decimal defendOverride)
        => MonsterStrike(attack, playerDefense, 8, rng, rule, defending, defendOverride, 5);

    public static int Mitigate(int attack, int defense, int k, decimal absorb = 0.33m)
    {
        if (attack <= 0) return 0;
        k = Math.Max(1, k);
        defense = Math.Max(0, defense);
        var denom = attack + defense * k;
        var hyperbolic = attack * (double)attack / denom;
        var linear = Math.Max(0, attack - defense / (double)k);
        var mix = Math.Clamp((double)absorb, 0, 0.85);
        var blended = hyperbolic * (1 - mix) + linear * mix;
        var floor = attack * (1 - MaxArmorReduction);
        return Math.Max(1, (int)Math.Round(Math.Max(floor, blended)));
    }

    public static int Mitigate(int attack, int defense, int k) => Mitigate(attack, defense, k, 0.33m);

    public static double Variance(int baseDmg, int hitVariance, Random rng)
    {
        if (baseDmg <= 0) return 0;
        var span = Math.Clamp(hitVariance, 0, 25);
        if (span == 0) return baseDmg;
        var pct = rng.Next(-span, span + 1) / 100.0;
        return Math.Max(1, baseDmg * (1 + pct));
    }

    public static int CritChance(int agility) => Math.Clamp(4 + Math.Max(0, agility) / 8, 4, 28);

    public static bool IsMiss(int agility, int enemySpeed, Random rng)
    {
        if (enemySpeed <= agility) return false;
        var chance = Math.Clamp((enemySpeed - agility) / 4, 1, 16);
        return rng.Next(100) < chance;
    }

    public static bool IsDodge(int agility, int monsterSpeed, Random rng)
    {
        if (agility <= monsterSpeed) return false;
        var chance = Math.Clamp((agility - monsterSpeed) / 5, 1, 16);
        return rng.Next(100) < chance;
    }

    public static bool IsGlance(Random rng) => rng.Next(100) < 12;

    public static bool WithdrawSuccess(int agility, int monsterSpeed, Random rng)
    {
        var chance = Math.Clamp(45 + agility / 3 - monsterSpeed / 4, 22, 82);
        return rng.Next(100) < chance;
    }

    public static decimal ClampMul(decimal mul) => Math.Clamp(mul, MinMul, MaxMul);
    public static decimal Positive(decimal value, decimal fallback) => value > 0 ? value : fallback;
    public static int MpCost(ProfessionSkill? skill, GameRule? rule = null) =>
        skill is { MpCost: > 0 } ? skill.MpCost : 10;

    public static int ResolveEliteBonus(MonsterKind kind, ProfessionSkill? skill)
    {
        if (skill is null || skill.EliteBossBonusPct <= 0) return 0;
        return kind is MonsterKind.Elite or MonsterKind.Rare or MonsterKind.Boss
            ? Math.Clamp(skill.EliteBossBonusPct, 0, 80) : 0;
    }

    public static string DefenderProxy(MonsterKind kind) => kind switch
    {
        MonsterKind.Boss => "security",
        MonsterKind.Elite => "security",
        MonsterKind.Rare => "law",
        _ => ""
    };
}
