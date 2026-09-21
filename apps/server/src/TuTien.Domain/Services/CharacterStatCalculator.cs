using TuTien.Domain.Entities;

namespace TuTien.Domain.Services;

public sealed record StatBreakdown(
    int Attack, int Defense, int Spirit, int Agility, int MaxHp, int MaxMp,
    int Fortune, int DetectionRadius, int CultivationPercent, int StoneRewardPercent, int TechniqueDiscountPercent);

public sealed class CharacterStatCalculator
{
    public StatBreakdown Calculate(Player player, Profession profession, IReadOnlyList<(ItemDefinition Item, bool ProfessionMatch)> equipped, TechniqueDefinition? activeTechnique, GameRule? rule = null, RealmThreshold? realm = null)
    {
        var r = rule ?? new GameRule();
        var atk = r.AtkBase + player.Level * r.AtkPerLevel + profession.AttackBonus + (realm?.AtkBonus ?? 0);
        var def = r.DefBase + player.Level * r.DefPerLevel + profession.DefenseBonus + (realm?.DefBonus ?? 0);
        var spirit = r.SpiBase + player.Level * r.SpiPerLevel + profession.SpiritBonus + (realm?.SpiBonus ?? 0);
        var agi = r.AgiBase + player.Level * r.AgiPerLevel + profession.AgilityBonus + (realm?.AgiBonus ?? 0);
        var fortune = 3 + player.Fortune + profession.FortuneBonus;
        var maxHp = r.HpBase + player.Level * r.HpPerLevel + (realm?.HpBonus ?? 0);
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
            { atk += profession.AttackBonus; def += profession.DefenseBonus; spirit += profession.SpiritBonus; }
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
        if (realm is null)
        {
            if (player.Realm == RealmKind.QiRefining) { atk += 4; def += 3; spirit += 4; maxHp += 20; }
            else if (player.Realm == RealmKind.Foundation) { atk += 10; def += 8; spirit += 10; maxHp += 50; }
        }
        return new StatBreakdown(atk, def, spirit, agi, maxHp, r.MpBase + spirit * r.MpPerSpirit, fortune, Math.Clamp(detect, 1, 8), cultPct, stonePct, disc);
    }

    public int XpRequired(Player player, GameRule? rule = null, RealmThreshold? realm = null)
    {
        var r = rule ?? new GameRule();
        return r.XpBase + player.Level * r.XpPerLevel + (realm?.XpExtraPerLevel ?? 0);
    }

    public int CombatPower(StatBreakdown b, GameRule? rule = null, int skillBudget = 0)
    {
        var r = rule ?? new GameRule();
        return r.WAtk * b.Attack + r.WDef * b.Defense + r.WSpi * b.Spirit + r.WAgi * b.Agility
             + r.WHp * b.MaxHp / 10 + r.WMp * b.MaxMp / 10 + r.WSkill * skillBudget;
    }
}
