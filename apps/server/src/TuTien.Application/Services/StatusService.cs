using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Application.Common;
using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Application.Services;

public class StatusService
{
    private readonly IAppDbContext _db;
    public StatusService(IAppDbContext db) => _db = db;

    public async Task ExpireAsync(Guid playerId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var dead = await _db.PlayerStatuses
            .Where(s => s.PlayerId == playerId &&
                        ((s.ExpiresAtUtc != null && s.ExpiresAtUtc <= now) ||
                         (s.RemainingTurns <= 0 && s.ExpiresAtUtc == null)))
            .ToListAsync(ct);
        if (dead.Count == 0) return;
        _db.PlayerStatuses.RemoveRange(dead);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<PlayerStatus>> ActiveAsync(Guid playerId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _db.PlayerStatuses.Include(s => s.Definition)
            .Where(s => s.PlayerId == playerId &&
                        (s.ExpiresAtUtc == null || s.ExpiresAtUtc > now) &&
                        (s.RemainingTurns > 0 || s.ExpiresAtUtc != null))
            .ToListAsync(ct);
    }

    public async Task<StatusAggregate> AggregateAsync(Guid playerId, CancellationToken ct)
    {
        var rows = await ActiveAsync(playerId, ct);
        return StatusMath.Sum(rows.Where(r => r.Definition is not null).Select(r => (r.Definition!, r.Stacks)));
    }

    public async Task<PlayerStatus> ApplyAsync(Guid playerId, string code, string source, CancellationToken ct)
    {
        var def = await _db.StatusDefinitions.FirstOrDefaultAsync(s => s.Code == code && s.IsActive, ct)
                  ?? throw new AppException("status_missing", "Khong co trang thai.", 404);
        var now = DateTime.UtcNow;
        var existing = await _db.PlayerStatuses.FirstOrDefaultAsync(
            s => s.PlayerId == playerId && s.StatusDefinitionId == def.Id &&
                 (s.ExpiresAtUtc == null || s.ExpiresAtUtc > now), ct);
        DateTime? exp = def.DurationSeconds > 0 ? now.AddSeconds(def.DurationSeconds) : null;
        var turns = def.DurationTurns;
        if (existing is null)
        {
            existing = new PlayerStatus
            {
                Id = Guid.NewGuid(), PlayerId = playerId, StatusDefinitionId = def.Id,
                Stacks = 1, RemainingTurns = turns, ExpiresAtUtc = exp, Source = source, CreatedAtUtc = now
            };
            _db.PlayerStatuses.Add(existing);
        }
        else if (def.StackMode == StatusStackMode.Ignore) return existing;
        else if (def.StackMode == StatusStackMode.Stack)
        {
            existing.Stacks = Math.Min(def.MaxStacks, existing.Stacks + 1);
            existing.RemainingTurns = Math.Max(existing.RemainingTurns, turns);
            if (exp is not null) existing.ExpiresAtUtc = existing.ExpiresAtUtc is { } old && old > exp ? old : exp;
            existing.Source = source;
        }
        else
        {
            existing.Stacks = 1; existing.RemainingTurns = turns; existing.ExpiresAtUtc = exp; existing.Source = source;
        }
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task TickTurnsAsync(Guid playerId, CancellationToken ct)
    {
        var rows = await _db.PlayerStatuses.Where(s => s.PlayerId == playerId && s.RemainingTurns > 0).ToListAsync(ct);
        foreach (var s in rows)
        {
            s.RemainingTurns--;
            if (s.RemainingTurns <= 0 && s.ExpiresAtUtc is null) _db.PlayerStatuses.Remove(s);
        }
        await _db.SaveChangesAsync(ct);
    }
}
