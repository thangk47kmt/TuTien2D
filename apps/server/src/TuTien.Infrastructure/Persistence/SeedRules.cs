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

        if (!await db.StatusDefinitions.AnyAsync())
        {
            StatusDefinition St(string code, string name, string desc, StatusPolarity pol, int sec, int turns,
                int atk = 0, int def = 0, int spi = 0, int cult = 0, decimal outgoing = 1m, decimal incoming = 1m, decimal aura = 1m, int regenMp = 0) => new()
            {
                Id = Guid.NewGuid(), Code = code, Name = name, Description = desc, Polarity = pol,
                DurationSeconds = sec, DurationTurns = turns, AtkPct = atk, DefPct = def, SpiPct = spi,
                CultivationPct = cult, OutgoingMul = outgoing, IncomingMul = incoming, AuraMul = aura, RegenMp = regenMp
            };
            db.StatusDefinitions.AddRange(
                St("linh_chien", "Linh Chien", "+20% ATK 10 phut", StatusPolarity.Buff, 600, 0, atk: 20),
                St("linh_khien", "Linh Khien", "+20% DEF, incoming x0.9, 3 hiep", StatusPolarity.Buff, 0, 3, def: 20, incoming: 0.90m),
                St("hoi_khi", "Hoi Khi", "Regen MP 4 hiep", StatusPolarity.Buff, 0, 4, regenMp: 8),
                St("travel_linh_khi", "Du Hanh Linh Khi", "Check-in aura + tu vi", StatusPolarity.Buff, 3600, 0, cult: 10, aura: 1.15m),
                St("khiem_the", "Khiem The", "Giam sat vao", StatusPolarity.Buff, 0, 3, incoming: 0.80m),
                St("me_loan", "Me Loan", "+sat ra 2 hiep", StatusPolarity.Buff, 0, 2, outgoing: 1.15m, spi: 10),
                St("pha_the", "Pha The", "+ATK ngan", StatusPolarity.Buff, 0, 2, atk: 15, outgoing: 1.10m));
        }
        await db.SaveChangesAsync();
    }
}
