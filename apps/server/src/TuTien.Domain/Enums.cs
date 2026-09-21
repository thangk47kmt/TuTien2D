namespace TuTien.Domain;

public enum RealmKind { Mortal = 0, QiRefining = 1, Foundation = 2 }
public enum MonsterKind { Common = 0, Elite = 1, Rare = 2, Boss = 3 }
public enum ItemType { Weapon = 0, Robe = 1, Boots = 2, Artifact = 3, Pill = 4, Material = 5, TechniqueFragment = 6, Key = 7, Quest = 8 }
public enum ItemQuality { Mortal = 0, Spirit = 1, Mystery = 2, Earth = 3, Heaven = 4 }
public enum EquipmentSlotKind { Weapon = 0, Robe = 1, Boots = 2, Artifact = 3 }
public enum CombatActionType { Attack = 0, Skill = 1, Defend = 2, UseItem = 3, Withdraw = 4 }
public enum CombatStatus { Active = 0, Won = 1, Lost = 2, Withdrawn = 3, Expired = 4 }
public enum CultivationStatus { Running = 0, Settled = 1, Cancelled = 2 }
public enum ChestStatus { Spawned = 0, Opened = 1, Expired = 2 }
public enum RewardSourceType { Combat = 0, Chest = 1, Cultivation = 2, Travel = 3, SecretRealm = 4, Admin = 5 }
public enum RewardType { CultivationXp = 0, SpiritStone = 1, Item = 2, ProfessionPoint = 3, ExtremePoint = 4 }
public enum TechniqueKind { Cultivation = 0, Sense = 1, CombatBurst = 2, RealmLock = 3, Profession = 4 }
public enum ZoneKind { Safe = 0, Meadow = 1, Forest = 2, Mountain = 3, Water = 4, CultivationSpot = 5, CommonHunt = 6, EliteHunt = 7, RareHunt = 8, ChestField = 9, SecretGate = 10 }
public enum AuraLevel { Low = 0, Normal = 1, High = 2, Turbulent = 3 }
public enum RumorKind { RareMonster = 0, AuraShift = 1, Boss = 2, ChestEstimate = 3, ZoneEvent = 4 }
public enum SkillTag { Burst = 0, Guard = 1, Control = 2, Sense = 3, Craft = 4, Fortune = 5, Utility = 6 }
