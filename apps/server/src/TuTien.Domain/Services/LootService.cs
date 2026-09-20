using TuTien.Domain.Entities;
namespace TuTien.Domain.Services;
public sealed record LootRoll(RewardType Type, Guid? ItemDefinitionId, int Quantity, bool RareHit);
public sealed class LootService
{
    public LootRoll Roll(LootTable table, int pityScore, Random rng)
    {
        if (table.Entries.Count == 0) return new LootRoll(RewardType.SpiritStone, null, 5, false);
        var forceRare = pityScore >= table.PityThreshold;
        IEnumerable<LootTableEntry> pool = table.Entries;
        if (forceRare)
        {
            var rares = table.Entries.Where(e => e.Item is { Quality: >= ItemQuality.Mystery } || e.MinimumQualityForPity >= ItemQuality.Mystery).ToList();
            if (rares.Count > 0) pool = rares;
        }
        var list = pool.ToList();
        var total = list.Sum(e => Math.Max(1, e.Weight));
        var roll = rng.Next(total);
        LootTableEntry? chosen = null;
        var acc = 0;
        foreach (var e in list)
        {
            acc += Math.Max(1, e.Weight);
            if (roll < acc) { chosen = e; break; }
        }
        chosen ??= list[0];
        var qty = rng.Next(chosen.MinQuantity, chosen.MaxQuantity + 1);
        var rare = chosen.Item is { Quality: >= ItemQuality.Mystery } || forceRare;
        return new LootRoll(chosen.RewardType, chosen.ItemDefinitionId, Math.Max(1, qty), rare);
    }
    public int NextPity(int current, bool rareHit, LootTable table)
        => rareHit ? table.PityResetOnRare : Math.Min(current + 1, table.PityThreshold + 5);
}
