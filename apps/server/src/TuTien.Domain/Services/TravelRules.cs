namespace TuTien.Domain.Services;

public static class TravelRules
{
    public static (decimal Multiplier, int DurationHours) FromDistance(int km) => km switch
    {
        < 5 => (1.00m, 0),
        < 20 => (1.10m, 6),
        < 100 => (1.30m, 12),
        < 500 => (1.60m, 24),
        < 1500 => (2.00m, 36),
        _ => (2.30m, 48)
    };
}
