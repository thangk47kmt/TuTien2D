using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;
namespace TuTien.Domain.Tests;
public class StatAndRuleTests
{
    [Fact] public void Profession_bonus_applies_once()
    {
        var s = new CharacterStatCalculator().Calculate(new Player { Level = 1 }, new Profession { AttackBonus = 2, SpiritBonus = 4 }, [], null);
        Assert.Equal(12, s.Attack);
    }
    [Fact] public void Aura_is_clamped()
    {
        var high = new AuraCalculator().Calculate(new AuraFactors { Base = 9, Location = 9, Time = 9, Event = 9, Density = 9, Group = 9, Technique = 9, Personal = 9 });
        Assert.True(high <= AuraCalculator.MaxTotal);
    }
    [Fact] public void Pity_resets_on_rare()
    {
        var table = new LootTable { PityThreshold = 3, PityResetOnRare = 0, Entries = [new LootTableEntry { Weight = 1 }] };
        Assert.Equal(0, new LootService().NextPity(4, true, table));
    }
    [Fact] public void Travel_distance_table()
    {
        Assert.Equal(1.10m, TravelRules.FromDistance(10).Multiplier);
    }
    [Fact] public void Name_validator()
    {
        Assert.NotNull(NameValidator.NormalizeOrError(" "));
        Assert.Null(NameValidator.NormalizeOrError("Dao Huu"));
    }
}
