using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public sealed record StatusAggregate(
    int AtkPct, int DefPct, int SpiPct, int AgiPct, int MaxHpPct,
    int CultivationPct, int StonePct, int CritBonus,
    decimal OutgoingMul, decimal IncomingMul, decimal AuraMul,
    int RegenHp, int RegenMp);

public static class StatusMath
{
    public static StatusAggregate Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 1m, 1m, 1m, 0, 0);

    public static StatusAggregate Sum(IEnumerable<(StatusDefinition Def, int Stacks)> rows)
    {
        var atk = 0; var def = 0; var spi = 0; var agi = 0; var hp = 0;
        var cult = 0; var stone = 0; var crit = 0; var regenHp = 0; var regenMp = 0;
        decimal outgoing = 1m, incoming = 1m, aura = 1m;
        foreach (var (d, raw) in rows)
        {
            var s = Math.Clamp(raw, 1, Math.Max(1, d.MaxStacks));
            atk += d.AtkPct * s; def += d.DefPct * s; spi += d.SpiPct * s; agi += d.AgiPct * s;
            hp += d.MaxHpPct * s; cult += d.CultivationPct * s; stone += d.StonePct * s; crit += d.CritBonus * s;
            regenHp += d.RegenHp * s; regenMp += d.RegenMp * s;
            outgoing *= PowMul(d.OutgoingMul, s);
            incoming *= PowMul(d.IncomingMul, s);
            aura *= PowMul(d.AuraMul, s);
        }
        return new StatusAggregate(
            Math.Clamp(atk, -70, 120), Math.Clamp(def, -70, 120), Math.Clamp(spi, -70, 120), Math.Clamp(agi, -70, 120),
            Math.Clamp(hp, -50, 80), Math.Clamp(cult, -50, 80), Math.Clamp(stone, -50, 80), Math.Clamp(crit, -20, 40),
            CombatEngine.ClampMul(outgoing), CombatEngine.ClampMul(incoming), Math.Clamp(aura, 0.50m, 2.50m), regenHp, regenMp);
    }

    public static StatBreakdown Apply(StatBreakdown b, StatusAggregate a)
    {
        int Scale(int value, int pct) => Math.Max(1, (int)Math.Round(value * (1 + pct / 100.0)));
        return b with
        {
            Attack = Scale(b.Attack, a.AtkPct),
            Defense = Scale(b.Defense, a.DefPct),
            Spirit = Scale(b.Spirit, a.SpiPct),
            Agility = Scale(b.Agility, a.AgiPct),
            MaxHp = Scale(b.MaxHp, a.MaxHpPct),
            CultivationPercent = b.CultivationPercent + a.CultivationPct,
            StoneRewardPercent = b.StoneRewardPercent + a.StonePct
        };
    }

    static decimal PowMul(decimal mul, int stacks)
    {
        if (mul <= 0) mul = 1m;
        var v = 1m;
        for (var i = 0; i < stacks; i++) v *= mul;
        return v;
    }
}
