namespace TuTien.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProfessionId { get; set; }
    public int Level { get; set; } = 1;
    public RealmKind Realm { get; set; } = RealmKind.Mortal;
    public int RealmStage { get; set; } = 1;
    public int CultivationXp { get; set; }
    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public int Mp { get; set; } = 60;
    public int MaxMp { get; set; } = 60;
    public int SpiritStones { get; set; } = 30;
    public int Fortune { get; set; } = 3;
    public int Stability { get; set; } = 10;
    public int ProfessionPoints { get; set; }
    public int ExtremePoints { get; set; }
    public bool RealmLocked { get; set; }
    public int MapX { get; set; } = 3;
    public int MapY { get; set; } = 3;
    public Guid? ActiveTechniqueId { get; set; }
    public int PityScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public uint Version { get; set; }
    public AppUser? User { get; set; }
    public Profession? Profession { get; set; }
    public TechniqueDefinition? ActiveTechnique { get; set; }
    public List<PlayerItem> Items { get; set; } = [];
    public List<PlayerEquipment> Equipment { get; set; } = [];
    public List<PlayerTechnique> Techniques { get; set; } = [];
    public List<PlayerDiscovery> Discoveries { get; set; } = [];
}

public class Profession
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int SpiritBonus { get; set; }
    public int FortuneBonus { get; set; }
    public int AgilityBonus { get; set; }
    public int StoneRewardPercent { get; set; }
    public int CultivationPercent { get; set; }
    public int TechniqueDiscountPercent { get; set; }
    public int DetectionBonus { get; set; }
    public string PrimaryBonusText { get; set; } = string.Empty;
    public string SecondaryBonusText { get; set; } = string.Empty;
}

public class PlayerItem
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid ItemDefinitionId { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsLocked { get; set; }
    public bool IsEquipped { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Player? Player { get; set; }
    public ItemDefinition? Definition { get; set; }
}

public class PlayerEquipment
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public EquipmentSlotKind Slot { get; set; }
    public Guid PlayerItemId { get; set; }
    public Player? Player { get; set; }
    public PlayerItem? Item { get; set; }
}

public class PlayerTechnique
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TechniqueDefinitionId { get; set; }
    public int Layer { get; set; } = 1;
    public bool IsActive { get; set; }
    public DateTime LearnedAtUtc { get; set; }
    public Player? Player { get; set; }
    public TechniqueDefinition? Technique { get; set; }
}

public class PlayerDiscovery
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime DiscoveredAtUtc { get; set; }
}

public class ItemDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "◆";
    public ItemType Type { get; set; }
    public ItemQuality Quality { get; set; }
    public int RequiredLevel { get; set; } = 1;
    public RealmKind RequiredRealm { get; set; } = RealmKind.Mortal;
    public string? PreferredProfessionCode { get; set; }
    public bool ProfessionLocked { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Spirit { get; set; }
    public int Agility { get; set; }
    public int MaxHp { get; set; }
    public int HealAmount { get; set; }
    public int ProfessionBonusPercent { get; set; }
    public bool Stackable { get; set; }
    public int StackLimit { get; set; } = 1;
    public bool Usable { get; set; }
}

public class TechniqueDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemQuality Quality { get; set; }
    public TechniqueKind Kind { get; set; }
    public string? RequiredProfessionCode { get; set; }
    public int RequiredLevel { get; set; } = 1;
    public RealmKind RequiredRealm { get; set; } = RealmKind.Mortal;
    public int Layers { get; set; } = 1;
    public int LearnCost { get; set; }
    public int UpgradeCost { get; set; }
    public int CultivationPercent { get; set; }
    public int AttackPercent { get; set; }
    public int SpiritBonus { get; set; }
    public int DetectionRadiusBonus { get; set; }
    public int SelfDamageOnSkill { get; set; }
    public bool LocksRealm { get; set; }
    public string? ConflictsWithCode { get; set; }
    public int ConfigVersion { get; set; } = 1;
}
