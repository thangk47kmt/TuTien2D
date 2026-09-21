using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Domain.Tests;

public class CombatEngineTests
{
    static GameRule Rule() => new();

    [Fact]
    public void Mitigate_is_monotonic_in_defense()
    {
        var low = CombatEngine.Mitigate(30, 4, 3, 0.33m);
        var high = CombatEngine.Mitigate(30, 40, 3, 0.33m);
        Assert.True(high < low);
        Assert.True(high >= 1);
    }

    [Fact]
    public void Mitigate_never_fully_negates_attack()
    {
        var dmg = CombatEngine.Mitigate(20, 999, 3, 0.33m);
        Assert.True(dmg >= 1);
        Assert.True(dmg <= 20);
    }

    [Fact]
    public void Burst_and_skill_outscale_basic_hit()
    {
        var basic = CombatEngine.PlayerStrike(24, 6, 12, new Random(7), Rule(), false, false, 1m, 0, 0, 4);
        var skill = CombatEngine.PlayerStrike(24, 6, 12, new Random(7), Rule(), true, false, 1m, 8, 0, 4);
        var burst = CombatEngine.PlayerStrike(24, 6, 12, new Random(7), Rule(), true, true, 1m, 8, 0, 4);
        Assert.True(skill.Damage >= basic.Damage);
        Assert.True(burst.Damage >= skill.Damage);
    }

    [Fact]
    public void Counter_multiplier_is_clamped()
    {
        Assert.Equal(0.50m, CombatEngine.ClampMul(0.1m));
        Assert.Equal(1.80m, CombatEngine.ClampMul(9m));
        Assert.Equal(1.20m, CombatEngine.ClampMul(1.20m));
    }

    [Fact]
    public void Defend_reduces_incoming()
    {
        var open = CombatEngine.MonsterStrike(18, 8, 10, new Random(3), Rule(), false, 0, 5);
        var block = CombatEngine.MonsterStrike(18, 8, 10, new Random(3), Rule(), true, 0, 5);
        Assert.True(block.Damage <= open.Damage);
        Assert.Equal("block", block.Tag);
    }

    [Fact]
    public void Elite_bonus_only_on_elites()
    {
        var skill = new ProfessionSkill { EliteBossBonusPct = 25 };
        Assert.Equal(0, CombatEngine.ResolveEliteBonus(MonsterKind.Common, skill));
        Assert.Equal(25, CombatEngine.ResolveEliteBonus(MonsterKind.Boss, skill));
        Assert.Equal("security", CombatEngine.DefenderProxy(MonsterKind.Boss));
        Assert.Equal("", CombatEngine.DefenderProxy(MonsterKind.Common));
    }

    [Fact]
    public void Withdraw_is_not_guaranteed()
    {
        var hits = 0;
        var rng = new Random(11);
        for (var i = 0; i < 40; i++)
            if (CombatEngine.WithdrawSuccess(8, 20, rng)) hits++;
        Assert.InRange(hits, 1, 39);
    }

    [Fact]
    public void Compat_wrapper_burst_exceeds_normal()
    {
        var a = CombatMath.PlayerDamage(20, 4, new Random(1), false, false);
        var b = CombatMath.PlayerDamage(20, 4, new Random(1), true, true);
        Assert.True(b > a);
    }
}
