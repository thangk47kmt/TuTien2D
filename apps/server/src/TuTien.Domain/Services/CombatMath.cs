using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public static class CombatMath
{
    public static int PlayerDamage(int attack, int enemyDefense, Random rng, bool skill, bool burstTechnique, GameRule? rule = null, decimal counterMul = 1m)
        => CombatEngine.PlayerStrike(attack, enemyDefense, 10, rng, rule ?? new GameRule(), skill, burstTechnique, counterMul, 0, 0).Damage;

    public static int MonsterDamage(int attack, int playerDefense, Random rng, bool defending, GameRule? rule = null)
        => CombatEngine.MonsterStrike(attack, playerDefense, rng, rule ?? new GameRule(), defending, 0).Damage;
}
