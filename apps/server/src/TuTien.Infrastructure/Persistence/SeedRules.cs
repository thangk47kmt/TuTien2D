using Microsoft.EntityFrameworkCore;
using TuTien.Domain;
using TuTien.Domain.Entities;

namespace TuTien.Infrastructure.Persistence;

public static class SeedRules
{
    public static async Task EnsureAsync(AppDbContext db)
    {
        if (!await db.GameRules.AnyAsync())
            db.GameRules.Add(new GameRule { Id = Guid.NewGuid(), Code = "default" });

        if (!await db.RealmThresholds.AnyAsync())
        {
            db.RealmThresholds.AddRange(
                new RealmThreshold { Id = Guid.NewGuid(), Realm = RealmKind.Mortal, Name = "Pham Nhan", MinLevel = 1 },
                new RealmThreshold { Id = Guid.NewGuid(), Realm = RealmKind.QiRefining, Name = "Luyen Khi", MinLevel = 5, AtkBonus = 4, DefBonus = 3, SpiBonus = 4, HpBonus = 20 },
                new RealmThreshold { Id = Guid.NewGuid(), Realm = RealmKind.Foundation, Name = "Truc Co", MinLevel = 12, AtkBonus = 10, DefBonus = 8, SpiBonus = 10, HpBonus = 50, XpExtraPerLevel = 10 });
        }

        if (!await db.ProfessionSkills.AnyAsync())
        {
            ProfessionSkill S(string p, string code, string name, SkillTag tag, decimal atk = 0, decimal spi = 0, int heal = 0, decimal healC = 0, int self = 0, int detect = 0, int stone = 0, int cult = 0, decimal defO = 0, int elite = 0) =>
                new() { Id = Guid.NewGuid(), ProfessionCode = p, Code = code, Name = name, Description = name, Tag = tag, AtkCoeff = atk, SpiCoeff = spi, HealFlat = heal, HealCoeff = healC, SelfDamageFlat = self, DetectBonus = detect, StoneRewardBonusPct = stone, CultivationBonusPct = cult, DefendMulOverride = defO, EliteBossBonusPct = elite, MpCost = 10 };
            db.ProfessionSkills.AddRange(
                S("technology", "quet_mach", "Quet Mach", SkillTag.Sense, detect: 2),
                S("medical", "hoi_linh", "Hoi Linh", SkillTag.Guard, heal: 20, healC: 0.4m),
                S("education", "tam_truyen", "Tam Truyen", SkillTag.Craft),
                S("finance", "tinh_van", "Tinh Van", SkillTag.Fortune, stone: 15),
                S("law", "khiem_chuong", "Khiem Chuong", SkillTag.Guard, defO: 0.35m),
                S("arts", "me_loan", "Me Loan", SkillTag.Control, spi: 0.3m),
                S("agriculture", "trach_linh", "Trach Linh", SkillTag.Fortune, cult: 15),
                S("service", "thong_hanh", "Thong Hanh", SkillTag.Utility, detect: 1),
                S("engineering", "pha_giac", "Pha Giac", SkillTag.Burst, atk: 0.35m, elite: 20),
                S("security", "trap_kich", "Trap Kich", SkillTag.Burst, atk: 0.45m, self: 6));
        }

        if (!await db.ProfessionCounters.AnyAsync())
        {
            void C(string a, string d, decimal mul, string note) =>
                db.ProfessionCounters.Add(new ProfessionCounter { Id = Guid.NewGuid(), AttackerCode = a, DefenderCode = d, DamageMul = mul, DefenseMul = 2m - mul, Note = note });
            C("security", "law", 0.85m, "Sat vs Thu");
            C("engineering", "law", 0.85m, "Sat vs Thu");
            C("security", "medical", 0.90m, "Sat vs hoi");
            C("law", "security", 1.15m, "Khien an Sat");
            C("law", "engineering", 1.15m, "Khien an Sat");
            C("medical", "security", 1.10m, "Hoi vs Sat");
            C("arts", "law", 1.15m, "Hoa neo Thu");
            C("technology", "medical", 1.10m, "Cam neo Thu");
            C("finance", "security", 0.88m, "Van yeu Sat");
            C("agriculture", "engineering", 0.88m, "Van yeu Sat");
            C("education", "security", 0.90m, "Cam yeu Sat");
            C("service", "engineering", 0.90m, "Hoa yeu Sat");
        }
        await db.SaveChangesAsync();
    }
}
