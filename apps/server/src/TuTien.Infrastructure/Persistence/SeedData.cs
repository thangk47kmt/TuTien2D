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
        var admin = new AppUser { Id = Guid.NewGuid(), UserName = "admin", Email = "admin@tutien.local", Role = "Admin", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin#123");
        var player = new AppUser { Id = Guid.NewGuid(), UserName = "daoist", Email = "daoist@tutien.local", Role = "Player", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        player.PasswordHash = hasher.HashPassword(player, "Play#123");
        db.Users.AddRange(admin, player);

        db.Professions.AddRange(
            Prof("technology", "Cong nghe", 2, 0, 4),
            Prof("medical", "Y te", 0, 2, 3),
            Prof("education", "Giao duc", 0, 1, 4),
            Prof("finance", "Tai chinh", 1, 0, 2),
            Prof("law", "Phap ly", 1, 3, 1),
            Prof("arts", "Nghe thuat", 0, 0, 3),
            Prof("agriculture", "Nong nghiep", 1, 2, 1),
            Prof("service", "Dich vu", 0, 1, 2),
            Prof("engineering", "Ky thuat", 3, 2, 1),
            Prof("security", "An ninh", 3, 3, 0)
        );

        var breath = new TechniqueDefinition { Id = Guid.NewGuid(), Code = "dan_khi", Name = "Dan Khi Quyet", Description = "Cong phap co ban", Kind = TechniqueKind.Cultivation, LearnCost = 0, CultivationPercent = 10 };
        db.TechniqueDefinitions.Add(breath);

        var map = new WorldMap { Id = Guid.NewGuid(), Code = "qingyun", Name = "Thanh Van", Width = 24, Height = 16 };
        db.WorldMaps.Add(map);
        db.WorldZones.Add(new WorldZone { Id = Guid.NewGuid(), MapId = map.Id, Code = "safe", Name = "Tran", Kind = ZoneKind.Safe, MinX = 0, MinY = 0, MaxX = 5, MaxY = 5, AuraLevel = AuraLevel.Normal, AuraMultiplier = 1 });

        var wolfLoot = new LootTable { Id = Guid.NewGuid(), Code = "wolf", Name = "Wolf", PityThreshold = 8 };
        db.LootTables.Add(wolfLoot);
        var wolf = new MonsterDefinition { Id = Guid.NewGuid(), Code = "yeu_lang", Name = "Yeu Lang", Kind = MonsterKind.Common, Hp = 40, Attack = 8, Defense = 2, SpawnWeight = 20, CultivationXp = 18, SpiritStones = 6, LootTableId = wolfLoot.Id, ZoneCode = "meadow" };
        db.MonsterDefinitions.Add(wolf);

        var chest = new ChestDefinition { Id = Guid.NewGuid(), Code = "phan", Name = "Ruong Pham", Quality = ItemQuality.Mortal, SpawnWeight = 8, LootTableId = wolfLoot.Id };
        db.ChestDefinitions.Add(chest);

        db.TravelLocations.Add(new TravelLocation { Id = Guid.NewGuid(), Code = "home", Name = "Nha", DistanceKm = 2 });
        db.Events.Add(new GameEvent { Id = Guid.NewGuid(), Code = "full_moon", Name = "Trang tron", IsEnabled = false, AuraMultiplier = 1.2m });
        await db.SaveChangesAsync();
    }

    static Profession Prof(string code, string name, int atk, int def, int spi) => new()
    {
        Id = Guid.NewGuid(), Code = code, Name = name, Description = name, IsActive = true,
        AttackBonus = atk, DefenseBonus = def, SpiritBonus = spi, PrimaryBonusText = $"+{atk} cong", SecondaryBonusText = $"+{spi} than"
    };
}
