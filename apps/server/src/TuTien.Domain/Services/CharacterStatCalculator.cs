using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public sealed record StatBreakdown(
    int Attack, int Defense, int Spirit, int Agility, int MaxHp, int MaxMp,
    int Fortune, int DetectionRadius, int CultivationPercent, int StoneRewardPercent, int TechniqueDiscountPercent);

public sealed class CharacterStatCalculator
{
    public StatBreakdown Calculate(Player player, Profession profession, IReadOnlyList<(ItemDefinition Item, bool ProfessionMatch)> equipped, TechniqueDefinition? activeTechnique)
    {
        var atk = 8 + player.Level * 2 + profession.AttackBonus;
        var def = 3 + player.Level + profession.DefenseBonus;
        var spirit = 8 + player.Level + profession.SpiritBonus;
        var agi = 5 + player.Level + profession.AgilityBonus;
        var fortune = 3 + player.Fortune + profession.FortuneBonus;
        var maxHp = 100 + player.Level * 8;
        var cultPct = profession.CultivationPercent;
        var stonePct = profession.StoneRewardPercent;
        var disc = profession.TechniqueDiscountPercent;
        var detect = 2 + profession.DetectionBonus / 10;
        foreach (var (item, match) in equipped)
        {
            var bonusMul = match && item.ProfessionBonusPercent > 0 ? 1 + item.ProfessionBonusPercent / 100.0 : 1.0;
            atk += (int)Math.Round(item.Attack * bonusMul);
            def += (int)Math.Round(item.Defense * bonusMul);
            spirit += (int)Math.Round(item.Spirit * bonusMul);
            agi += (int)Math.Round(item.Agility * bonusMul);
            maxHp += item.MaxHp;
        }
        if (activeTechnique is not null)
        {
            if (activeTechnique.Kind == TechniqueKind.Profession)
            {
                atk += profession.AttackBonus; def += profession.DefenseBonus; spirit += profession.SpiritBonus;
            }
            if (activeTechnique.Kind == TechniqueKind.RealmLock)
            {
                atk = (int)Math.Round(atk * 1.28 + player.ExtremePoints * 0.4);
                def = (int)Math.Round(def * 1.20 + player.ExtremePoints * 0.3);
                spirit = (int)Math.Round(spirit * 1.15);
            }
            cultPct += activeTechnique.CultivationPercent;
            atk += (int)Math.Round(atk * (activeTechnique.AttackPercent / 100.0));
            spirit += activeTechnique.SpiritBonus;
            detect += activeTechnique.DetectionRadiusBonus;
        }
        if (player.Realm == RealmKind.QiRefining) { atk += 4; def += 3; spirit += 4; maxHp += 20; }
        else if (player.Realm == RealmKind.Foundation) { atk += 10; def += 8; spirit += 10; maxHp += 50; }
        return new StatBreakdown(atk, def, spirit, agi, maxHp, 60 + spirit * 2, fortune, Math.Clamp(detect, 1, 8), cultPct, stonePct, disc);
    }
    public int XpRequired(Player player) => 60 + player.Level * 35;
}
