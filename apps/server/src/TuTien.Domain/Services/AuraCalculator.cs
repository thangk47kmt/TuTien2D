namespace TuTien.Domain.Services;

public sealed class AuraFactors
{
    public decimal Base { get; init; } = 1.0m;
    public decimal Location { get; init; } = 1.0m;
    public decimal Time { get; init; } = 1.0m;
    public decimal Event { get; init; } = 1.0m;
    public decimal Density { get; init; } = 1.0m;
    public decimal Group { get; init; } = 1.0m;
    public decimal Technique { get; init; } = 1.0m;
    public decimal Personal { get; init; } = 1.0m;
}

public sealed class AuraCalculator
{
    public const decimal MinFactor = 0.25m;
    public const decimal MaxFactor = 3.00m;
    public const decimal MaxTotal = 8.00m;
    public decimal Calculate(AuraFactors factors)
    {
        decimal Clamp(decimal v) => Math.Clamp(v, MinFactor, MaxFactor);
        var total = Clamp(factors.Base) * Clamp(factors.Location) * Clamp(factors.Time) * Clamp(factors.Event)
                    * Clamp(factors.Density) * Clamp(factors.Group) * Clamp(factors.Technique) * Clamp(factors.Personal);
        if (total < 0) total = MinFactor;
        return Math.Min(total, MaxTotal);
    }
    public static decimal FromAuraLevel(AuraLevel level) => level switch
    {
        AuraLevel.Low => 0.75m,
        AuraLevel.Normal => 1.00m,
        AuraLevel.High => 1.35m,
        AuraLevel.Turbulent => 1.70m,
        _ => 1.00m
    };
}
