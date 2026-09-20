using TuTien.Application.Services;

namespace TuTien.Api;

public class WorldHeartbeatService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public WorldHeartbeatService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var game = scope.ServiceProvider.GetRequiredService<GameplayService>();
                await game.EnsureWorldPopulationAsync(stoppingToken);
            }
            catch { }
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }
}
