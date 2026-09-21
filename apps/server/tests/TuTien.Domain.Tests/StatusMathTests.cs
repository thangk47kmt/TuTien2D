using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Domain.Tests;

public class StatusMathTests
{
    [Fact]
    public void Stacks_scale_percent_and_clamp()
    {
        var def = new StatusDefinition { AtkPct = 20, MaxStacks = 3, OutgoingMul = 1.10m };
        var sum = StatusMath.Sum([(def, 3)]);
        Assert.Equal(60, sum.AtkPct);
        Assert.True(sum.OutgoingMul > 1m);
        Assert.True(sum.OutgoingMul <= CombatEngine.MaxMul);
    }

    [Fact]
    public void Apply_increases_attack()
    {
        var raw = new StatBreakdown(10, 10, 10, 10, 100, 60, 3, 2, 0, 0, 0);
        var bag = new StatusAggregate(20, 0, 0, 0, 0, 5, 0, 0, 1m, 1m, 1m, 0, 0);
        var next = StatusMath.Apply(raw, bag);
        Assert.Equal(12, next.Attack);
        Assert.Equal(5, next.CultivationPercent);
    }

    [Fact]
    public void Empty_does_not_change_stats()
    {
        var raw = new StatBreakdown(8, 3, 8, 5, 100, 60, 3, 2, 10, 5, 0);
        var next = StatusMath.Apply(raw, StatusMath.Empty);
        Assert.Equal(raw.Attack, next.Attack);
        Assert.Equal(raw.Defense, next.Defense);
    }
}
