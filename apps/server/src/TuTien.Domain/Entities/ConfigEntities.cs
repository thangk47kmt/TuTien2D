namespace TuTien.Domain.Entities;

public class GameRule
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "default";
    public int XpBase { get; set; } = 60;
    public int XpPerLevel { get; set; } = 35;
    public int XpPerMinute { get; set; } = 3;
    public int AtkBase { get; set; } = 8;
    public int AtkPerLevel { get; set; } = 2;
    public int DefBase { get; set; } = 3;
    public int DefPerLevel { get; set; } = 1;
    public int SpiBase { get; set; } = 8;
    public int SpiPerLevel { get; set; } = 1;
    public int AgiBase { get; set; } = 5;
    public int AgiPerLevel { get; set; } = 1;
    public int HpBase { get; set; } = 100;
    public int HpPerLevel { get; set; } = 8;
    public int MpBase { get; set; } = 60;
    public int MpPerSpirit { get; set; } = 2;
    public decimal SkillMul { get; set; } = 1.55m;
    public decimal BurstMul { get; set; } = 1.60m;
    public decimal DefendMul { get; set; } = 0.45m;
    public int HitVariance { get; set; } = 6;
    public int DefDivPlayer { get; set; } = 3;
    public int DefDivMonster { get; set; } = 2;
    public decimal DefAbsorb { get; set; } = 0.33m;
    public int FortuneLootPct { get; set; } = 2;
    public decimal PityBoost { get; set; } = 1.5m;
    public int WAtk { get; set; } = 4;
    public int WDef { get; set; } = 3;
    public int WSpi { get; set; } = 3;
    public int WAgi { get; set; } = 2;
    public int WHp { get; set; } = 1;
    public int WMp { get; set; } = 1;
    public int WSkill { get; set; } = 2;
    public decimal BalanceBand { get; set; } = 0.05m;
    public int MapWidth { get; set; } = 24;
    public int MapHeight { get; set; } = 16;
    public int TargetMonsters { get; set; } = 10;
    public int TargetChests { get; set; } = 4;
}

public class RealmThreshold
{
    public Guid Id { get; set; }
    public RealmKind Realm { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinLevel { get; set; }
    public int MinLifetimeXp { get; set; }
    public int AtkBonus { get; set; }
    public int DefBonus { get; set; }
    public int SpiBonus { get; set; }
    public int AgiBonus { get; set; }
    public int HpBonus { get; set; }
    public int XpExtraPerLevel { get; set; }
    public string? RequiredTechniqueCode { get; set; }
}

public class ProfessionSkill
{
    public Guid Id { get; set; }
    public string ProfessionCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SkillTag Tag { get; set; }
    public decimal AtkCoeff { get; set; }
    public decimal SpiCoeff { get; set; }
    public int HealFlat { get; set; }
    public decimal HealCoeff { get; set; }
    public int SelfDamageFlat { get; set; }
    public int SelfDamagePct { get; set; }
    public int MpCost { get; set; } = 10;
    public int DetectBonus { get; set; }
    public int StoneRewardBonusPct { get; set; }
    public int CultivationBonusPct { get; set; }
    public decimal DefendMulOverride { get; set; }
    public int EliteBossBonusPct { get; set; }
}

public class ProfessionCounter
{
    public Guid Id { get; set; }
    public string AttackerCode { get; set; } = string.Empty;
    public string DefenderCode { get; set; } = string.Empty;
    public decimal DamageMul { get; set; } = 1m;
    public decimal DefenseMul { get; set; } = 1m;
    public string Note { get; set; } = string.Empty;
}
