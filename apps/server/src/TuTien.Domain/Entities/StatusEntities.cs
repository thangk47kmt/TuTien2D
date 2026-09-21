namespace TuTien.Domain.Entities;

public class StatusDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "✦";
    public StatusPolarity Polarity { get; set; }
    public StatusStackMode StackMode { get; set; }
    public int MaxStacks { get; set; } = 1;
    public int DurationSeconds { get; set; }
    public int DurationTurns { get; set; }
    public int AtkPct { get; set; }
    public int DefPct { get; set; }
    public int SpiPct { get; set; }
    public int AgiPct { get; set; }
    public int MaxHpPct { get; set; }
    public int CultivationPct { get; set; }
    public int StonePct { get; set; }
    public int CritBonus { get; set; }
    public decimal OutgoingMul { get; set; } = 1m;
    public decimal IncomingMul { get; set; } = 1m;
    public decimal AuraMul { get; set; } = 1m;
    public int RegenHp { get; set; }
    public int RegenMp { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PlayerStatus
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid StatusDefinitionId { get; set; }
    public int Stacks { get; set; } = 1;
    public int RemainingTurns { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public Player? Player { get; set; }
    public StatusDefinition? Definition { get; set; }
}
