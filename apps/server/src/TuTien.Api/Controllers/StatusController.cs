using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Application.Dtos;
using TuTien.Application.Services;

namespace TuTien.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public class StatusController : ControllerBase
{
    private readonly IAppDbContext _db;
    private readonly StatusService _status;
    public StatusController(IAppDbContext db, StatusService status) { _db = db; _status = status; }
    Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("statuses")]
    public async Task<ActionResult<List<StatusDto>>> List(CancellationToken ct)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.UserId == UserId(), ct);
        if (player is null) return Ok(new List<StatusDto>());
        await _status.ExpireAsync(player.Id, ct);
        var rows = await _status.ActiveAsync(player.Id, ct);
        return rows.Where(r => r.Definition is not null).Select(r =>
            new StatusDto(r.Id, r.Definition!.Code, r.Definition.Name, r.Definition.Icon,
                r.Definition.Polarity.ToString(), r.Stacks, r.RemainingTurns, r.ExpiresAtUtc, r.Source)).ToList();
    }
}
