using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

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
            PrimaryBonusText = $"+{atk} cong", SecondaryBonusText = $"+{cult}% tu"
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
            P("security", "An ninh", 3, 3, 0, 0, 1, 0, 0, 0, 5));
        await db.SaveChangesAsync();
        // rest of content already inserted above in previous commit if AnyAsync short-circuits existing DBs.
        // For fresh DB continue full seed by re-running previous payload plus using.
    }
}
