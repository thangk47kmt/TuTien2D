using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TuTien.Domain;
using TuTien.Domain.Entities;

namespace TuTien.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task EnsureAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Professions.AnyAsync()) return;

        var hasher = new PasswordHasher<AppUser>();
        AppUser U(string name, string role, string pass)
        {
            var u = new AppUser { Id = Guid.NewGuid(), UserName = name, Email = name + "@tutien.local", Role = role, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
            u.PasswordHash = hasher.HashPassword(u, pass);
            return u;
        }
        db.Users.AddRange(U("admin", "Admin", "Admin#123"), U("daoist", "Player", "Play#123"));

        Profession P(string code, string name, int atk, int def, int spi, int fortune, int agi, int stone, int cult, int disc, int detect) => new()
        {
            Id = Guid.NewGuid(), Code = code, Name = name, Description = name, IsActive = true,
            AttackBonus = atk, DefenseBonus = def, SpiritBonus = spi, FortuneBonus = fortune, AgilityBonus = agi,
            StoneRewardPercent = stone, CultivationPercent = cult, TechniqueDiscountPercent = disc, DetectionBonus = detect,
            PrimaryBonusText = $"+{atk} cong / +{spi} than", SecondaryBonusText = $"+{cult}% tu luyen"
        };
        db.Professions.AddRange(
            P("technology", "Cong nghe", 2, 0, 4, 1, 1, 0, 5, 10, 20),
            P("medical", "Y te", 0, 2, 3, 0, 0, 0, 8, 0, 10),
            P("education", "Giao duc", 0, 1, 4, 0, 0, 0, 12, 15, 5),
            P("finance", "Tai chinh", 1, 0, 2, 4, 0, 20, 0, 0, 0),
            P("law", "Phap ly", 1, 3, 1, 0, 0, 0, 0, 0, 0),
            P("arts", "Nghe thuat", 0, 0, 3, 2, 2, 0, 6, 0, 15),
            P("agriculture", "Nong nghiep", 1, 2, 1, 1, 0, 5, 10, 0, 5),
            P("service", "Dich vu", 0, 1, 2, 1, 2, 5, 4, 5, 10),
            P("engineering", "Ky thuat", 3, 2, 1, 0, 0, 0, 0, 0, 0),
            P("security", "An ninh", 3, 3, 0, 0, 1, 0, 0, 0, 5)
        );

        ItemDefinition Item(string code, string name, ItemType type, ItemQuality q, int atk, int def, int spi, string? prof = null, int bonus = 0, int heal = 0) => new()
        {
            Id = Guid.NewGuid(), Code = code, Name = name, Description = name, Icon = type == ItemType.Pill ? "丹" : "◆",
            Type = type, Quality = q, Attack = atk, Defense = def, Spirit = spi, HealAmount = heal,
            PreferredProfessionCode = prof, ProfessionBonusPercent = bonus, ProfessionLocked = prof is not null && bonus >= 40,
            Usable = heal > 0, Stackable = heal > 0, StackLimit = heal > 0 ? 20 : 1
        };
        var sword = Item("wood_sword", "Moc Kiem", ItemType.Weapon, ItemQuality.Mortal, 6, 0, 0);
        var robe = Item("cloth_robe", "Bo Y", ItemType.Robe, ItemQuality.Mortal, 0, 4, 1);
        var boots = Item("cloud_boots", "Van Ly", ItemType.Boots, ItemQuality.Spirit, 0, 1, 0);
        var art = Item("jade_mirror", "Ngoc Kinh", ItemType.Artifact, ItemQuality.Mystery, 2, 0, 6, "education", 20);
        var pill = Item("heal_pill", "Hoi Linh Dan", ItemType.Pill, ItemQuality.Mortal, 0, 0, 0, heal: 35);
        var techSword = Item("circuit_blade", "Mach Kiem", ItemType.Weapon, ItemQuality.Spirit, 10, 0, 2, "technology", 25);
        db.ItemDefinitions.AddRange(sword, robe, boots, art, pill, techSword);

        LootTable T(string code, string name, int pity) => new() { Id = Guid.NewGuid(), Code = code, Name = name, PityThreshold = pity };
        var common = T("common", "Thuong", 8);
        var rareT = T("rare", "Hiem", 5);
        var chestT = T("chest", "Ruong", 6);
        db.LootTables.AddRange(common, rareT, chestT);
        void E(LootTable table, RewardType type, Guid? item, int w, int min = 1, int max = 1) =>
            db.LootTableEntries.Add(new LootTableEntry { Id = Guid.NewGuid(), LootTableId = table.Id, RewardType = type, ItemDefinitionId = item, Weight = w, MinQuantity = min, MaxQuantity = max });
        E(common, RewardType.SpiritStone, null, 40, 4, 10);
        E(common, RewardType.Item, pill.Id, 20);
        E(common, RewardType.Item, sword.Id, 8);
        E(rareT, RewardType.Item, art.Id, 10);
        E(rareT, RewardType.SpiritStone, null, 20, 12, 20);
        E(chestT, RewardType.Item, robe.Id, 20);
        E(chestT, RewardType.Item, boots.Id, 10);
        E(chestT, RewardType.Item, techSword.Id, 6);
        E(chestT, RewardType.SpiritStone, null, 30, 8, 16);

        db.TechniqueDefinitions.AddRange(
            new TechniqueDefinition { Id = Guid.NewGuid(), Code = "dan_khi", Name = "Dan Khi Quyet", Description = "Cong phap co ban +10% tu luyen", Kind = TechniqueKind.Cultivation, LearnCost = 0, CultivationPercent = 10 },
            new TechniqueDefinition { Id = Guid.NewGuid(), Code = "tam_linh", Name = "Tam Linh Bi Thuat", Description = "Tang ban kinh cam ung", Kind = TechniqueKind.Sense, LearnCost = 40, DetectionRadiusBonus = 2 },
            new TechniqueDefinition { Id = Guid.NewGuid(), Code = "nghich_mach", Name = "Nghich Mach Tam Kich", Description = "Ky nang manh, phan phe 8 HP", Kind = TechniqueKind.CombatBurst, LearnCost = 80, AttackPercent = 20, SelfDamageOnSkill = 8 },
            new TechniqueDefinition { Id = Guid.NewGuid(), Code = "cuc_canh", Name = "Cuc Canh Luyen Khi", Description = "Khoa canh gioi, tang chi so", Kind = TechniqueKind.RealmLock, LearnCost = 120, LocksRealm = true, ConflictsWithCode = "dan_khi" },
            new TechniqueDefinition { Id = Guid.NewGuid(), Code = "bach_nghe", Name = "Bach Nghe Tam Kinh", Description = "Nhan doi bonus nghe", Kind = TechniqueKind.Profession, LearnCost = 70 }
        );

        var map = new WorldMap { Id = Guid.NewGuid(), Code = "qingyun", Name = "Thanh Van Tran", Width = 24, Height = 16 };
        db.WorldMaps.Add(map);
        WorldZone Z(string code, string name, ZoneKind kind, int x1, int y1, int x2, int y2, AuraLevel aura) => new()
        {
            Id = Guid.NewGuid(), MapId = map.Id, Code = code, Name = name, Kind = kind, MinX = x1, MinY = y1, MaxX = x2, MaxY = y2, AuraLevel = aura, AuraMultiplier = AuraCalculator.FromAuraLevel(aura)
        };
        db.WorldZones.AddRange(
            Z("safe", "Tran", ZoneKind.Safe, 0, 0, 5, 5, AuraLevel.Normal),
            Z("meadow", "Dong", ZoneKind.Meadow, 6, 0, 14, 7, AuraLevel.Normal),
            Z("forest", "Rung", ZoneKind.Forest, 15, 0, 23, 7, AuraLevel.High),
            Z("hunt", "San", ZoneKind.EliteHunt, 6, 8, 16, 15, AuraLevel.High),
            Z("boss", "Coc", ZoneKind.RareHunt, 17, 8, 23, 15, AuraLevel.Turbulent)
        );

        MonsterDefinition M(string code, string name, MonsterKind kind, int hp, int atk, int def, int w, int xp, int stone, Guid loot) => new()
        {
            Id = Guid.NewGuid(), Code = code, Name = name, Kind = kind, Hp = hp, Attack = atk, Defense = def, SpawnWeight = w,
            CultivationXp = xp, SpiritStones = stone, LootTableId = loot, RespawnSeconds = kind == MonsterKind.Boss ? 90 : 20
        };
        db.MonsterDefinitions.AddRange(
            M("yeu_lang", "Yeu Lang", MonsterKind.Common, 40, 8, 2, 30, 18, 6, common.Id),
            M("truc_xa", "Thanh Truc Xa", MonsterKind.Common, 55, 10, 3, 22, 24, 8, common.Id),
            M("thiet_hung", "Thiet Bi Hung", MonsterKind.Elite, 90, 14, 8, 10, 40, 14, rareT.Id),
            M("linh_ho", "Tu Dien Linh Ho", MonsterKind.Rare, 120, 18, 6, 4, 70, 22, rareT.Id),
            M("xich_giao", "Xich Viem Giao", MonsterKind.Boss, 220, 24, 10, 1, 160, 50, rareT.Id)
        );

        db.ChestDefinitions.AddRange(
            new ChestDefinition { Id = Guid.NewGuid(), Code = "phan", Name = "Ruong Pham", Quality = ItemQuality.Mortal, SpawnWeight = 12, LootTableId = chestT.Id },
            new ChestDefinition { Id = Guid.NewGuid(), Code = "linh", Name = "Ruong Linh", Quality = ItemQuality.Spirit, SpawnWeight = 5, LootTableId = chestT.Id },
            new ChestDefinition { Id = Guid.NewGuid(), Code = "huyen", Name = "Ruong Huyen", Quality = ItemQuality.Mystery, SpawnWeight = 2, LootTableId = rareT.Id }
        );

        db.TravelLocations.AddRange(
            new TravelLocation { Id = Guid.NewGuid(), Code = "home", Name = "Nha", DistanceKm = 2 },
            new TravelLocation { Id = Guid.NewGuid(), Code = "market", Name = "Cho", DistanceKm = 12 },
            new TravelLocation { Id = Guid.NewGuid(), Code = "mountain", Name = "Nui", DistanceKm = 80 },
            new TravelLocation { Id = Guid.NewGuid(), Code = "city", Name = "Thanh", DistanceKm = 320 },
            new TravelLocation { Id = Guid.NewGuid(), Code = "sea", Name = "Bien", DistanceKm = 1600 }
        );

        db.Events.Add(new GameEvent { Id = Guid.NewGuid(), Code = "full_moon", Name = "Trang tron", IsEnabled = false, AuraMultiplier = 1.2m });
        db.SecretRealms.Add(new SecretRealmDefinition { Id = Guid.NewGuid(), Code = "thanh_moc", Name = "Thanh Moc Bi Canh", RequiredRealm = RealmKind.QiRefining, DurationSeconds = 600 });
        db.Rumors.AddRange(
            new Rumor { Id = Guid.NewGuid(), Kind = RumorKind.RareMonster, Title = "Ho lua xuat hien", Body = "Huong Dong, do tin cay trung binh.", ApproximateZone = "Dong", Reliability = 60, ExpiresAtUtc = DateTime.UtcNow.AddDays(2), IsActive = true },
            new Rumor { Id = Guid.NewGuid(), Kind = RumorKind.Boss, Title = "Giao do o Coc", Body = "Boss Xich Viem, can di nhom.", ApproximateZone = "Coc", Reliability = 40, ExpiresAtUtc = DateTime.UtcNow.AddDays(1), IsActive = true },
            new Rumor { Id = Guid.NewGuid(), Kind = RumorKind.ChestEstimate, Title = "Ruong Huyen", Body = "Mot ruong huyen sap sinh.", ApproximateZone = "San", Reliability = 35, ExpiresAtUtc = DateTime.UtcNow.AddHours(18), IsActive = true }
        );
        await db.SaveChangesAsync();
    }
}
