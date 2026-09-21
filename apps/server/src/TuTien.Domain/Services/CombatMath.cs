using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public static class CombatMath
{
    public static int PlayerDamage(int attack, int enemyDefense, Random rng, bool skill, bool burstTechnique, GameRule? rule = null, decimal counterMul = 1m)
    {
        var r = rule ?? new GameRule();
        var raw = attack + rng.Next(0, Math.Max(1, r.HitVariance + 1)) - enemyDefense / Math.Max(1, r.DefDivPlayer);
        if (skill) raw = (int)Math.Round(raw * (double)r.SkillMul);
        if (burstTechnique) raw = (int)Math.Round(raw * (double)r.BurstMul);
        raw = (int)Math.Round(raw * (double)counterMul);
        return Math.Max(1, raw);
    }

    public static int MonsterDamage(int attack, int playerDefense, Random rng, bool defending, GameRule? rule = null)
    {
        var r = rule ?? new GameRule();
        var raw = attack - playerDefense / Math.Max(1, r.DefDivMonster) + rng.Next(0, Math.Max(1, r.HitVariance));
        var defMul = defending ? (double)r.DefendMul : 1;
        if (defending && r.DefendMul > 0) defMul = (double)r.DefendMul;
        return Math.Max(1, (int)Math.Round(raw * defMul));
    }
}
