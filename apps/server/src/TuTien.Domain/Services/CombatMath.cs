namespace TuTien.Domain.Services;

public static class CombatMath
{
    public static int PlayerDamage(int attack, int enemyDefense, Random rng, bool skill, bool burstTechnique)
    {
        var raw = attack + rng.Next(0, 7) - enemyDefense / 3;
        if (skill) raw = (int)Math.Round(raw * 1.55);
        if (burstTechnique) raw = (int)Math.Round(raw * 1.60);
        return Math.Max(1, raw);
    }

    public static int MonsterDamage(int attack, int playerDefense, Random rng, bool defending)
    {
        var raw = attack - playerDefense / 2 + rng.Next(0, 5);
        if (defending) raw = (int)Math.Round(raw * 0.45);
        return Math.Max(1, raw);
    }
}
