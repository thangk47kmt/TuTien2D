using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuTien.Application.Services;
using TuTien.Domain.Entities;

namespace TuTien.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public class AdminCatalogController : ControllerBase
{
    private readonly AdminCatalogService _catalog;
    private readonly BalanceService _balance;
    public AdminCatalogController(AdminCatalogService catalog, BalanceService balance)
    {
        _catalog = catalog; _balance = balance;
    }
    Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("catalog")] public Task<object> Catalog(CancellationToken ct) => _catalog.CatalogAsync(ct);
    [HttpPut("rules")] public async Task<IActionResult> Rule([FromBody] GameRule body, CancellationToken ct) { await _catalog.SaveRuleAsync(body, UserId(), ct); return NoContent(); }
    [HttpPut("professions")] public async Task<IActionResult> Profession([FromBody] Profession body, CancellationToken ct) { await _catalog.SaveProfessionAsync(body, UserId(), ct); return NoContent(); }
    [HttpPut("monsters")] public async Task<IActionResult> Monster([FromBody] MonsterDefinition body, CancellationToken ct) { await _catalog.SaveMonsterAsync(body, UserId(), ct); return NoContent(); }
    [HttpPut("items")] public async Task<IActionResult> Item([FromBody] ItemDefinition body, CancellationToken ct) { await _catalog.SaveItemAsync(body, UserId(), ct); return NoContent(); }
    [HttpPut("skills")] public async Task<IActionResult> Skill([FromBody] ProfessionSkill body, CancellationToken ct) { await _catalog.SaveSkillAsync(body, UserId(), ct); return NoContent(); }
    [HttpPut("counters")] public async Task<IActionResult> Counter([FromBody] ProfessionCounter body, CancellationToken ct) { await _catalog.SaveCounterAsync(body, UserId(), ct); return NoContent(); }
    [HttpGet("balance")] public Task<object> Balance([FromQuery] int level = 5, CancellationToken ct = default) => _balance.EvaluateAsync(level, ct);
    [HttpPost("balance/suggest")] public Task<object> Suggest([FromQuery] int level = 5, CancellationToken ct = default) => _balance.SuggestAsync(level, ct);
    [HttpPost("balance/apply")] public async Task<IActionResult> Apply([FromQuery] int level = 5, CancellationToken ct = default) { await _balance.ApplyAsync(level, UserId(), ct); return NoContent(); }
}
