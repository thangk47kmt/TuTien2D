using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuTien.Application.Dtos;
using TuTien.Application.Services;
namespace TuTien.Api.Controllers;
[ApiController]
[Authorize]
[Route("api/v1")]
public class GameControllers : ControllerBase
{
    private readonly GameplayService _game;
    public GameControllers(GameplayService game) => _game = game;
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [AllowAnonymous] [HttpGet("professions")] public Task<List<ProfessionDto>> Professions(CancellationToken ct) => _game.ListProfessionsAsync(ct);
    [HttpPost("players")] public Task<PlayerProfileDto> Create(CreatePlayerRequest req, CancellationToken ct) => _game.CreatePlayerAsync(UserId(), req, ct);
    [HttpGet("players/me")] public Task<PlayerProfileDto> Me(CancellationToken ct) => _game.GetProfileAsync(UserId(), ct);
    [HttpGet("world")] public Task<WorldSnapshotDto> World(CancellationToken ct) => _game.GetWorldAsync(UserId(), ct);
    [HttpPost("world/move")] public Task<WorldSnapshotDto> Move(MoveRequest req, CancellationToken ct) => _game.MoveAsync(UserId(), req, ct);
    [HttpPost("world/sense")] public Task<WorldSnapshotDto> Sense(CancellationToken ct) => _game.UseSenseAsync(UserId(), ct);
    [HttpPost("combat/start")] public Task<CombatStateDto> StartCombat(StartCombatRequest req, CancellationToken ct) => _game.StartCombatAsync(UserId(), req, ct);
    [HttpPost("combat/{id:guid}/action")] public Task<CombatStateDto> Act(Guid id, CombatActionRequest req, CancellationToken ct) => _game.ActAsync(UserId(), id, req, ct);
    [HttpGet("combat/active")] public Task<CombatStateDto?> ActiveCombat(CancellationToken ct) => _game.ActiveCombatAsync(UserId(), ct);
    [HttpGet("inventory")] public Task<List<InventoryItemDto>> Inventory(CancellationToken ct) => _game.InventoryAsync(UserId(), ct);
    [HttpPost("inventory/equip")] public Task<List<InventoryItemDto>> Equip(EquipRequest req, CancellationToken ct) => _game.EquipAsync(UserId(), req, true, ct);
    [HttpPost("inventory/unequip")] public Task<List<InventoryItemDto>> Unequip(EquipRequest req, CancellationToken ct) => _game.EquipAsync(UserId(), req, false, ct);
    [HttpPost("inventory/{id:guid}/use")] public async Task<IActionResult> Use(Guid id, CancellationToken ct) { await _game.UseItemAsync(UserId(), id, ct); return NoContent(); }
    [HttpGet("techniques")] public Task<List<TechniqueDto>> Techniques(CancellationToken ct) => _game.TechniquesAsync(UserId(), ct);
    [HttpPost("techniques/learn")] public async Task<IActionResult> Learn(LearnTechniqueRequest req, CancellationToken ct) { await _game.LearnOrActivateAsync(UserId(), req, ct); return NoContent(); }
    [HttpPost("chests/open")] public Task<List<RewardLineDto>> OpenChest(OpenChestRequest req, CancellationToken ct) => _game.OpenChestAsync(UserId(), req, ct);
    [HttpPost("cultivation/start")] public Task<CultivationStatusDto> StartCultivation(StartCultivationRequest req, CancellationToken ct) => _game.StartCultivationAsync(UserId(), req, ct);
    [HttpGet("cultivation")] public Task<CultivationStatusDto> Cultivation(CancellationToken ct) => _game.CultivationStatusAsync(UserId(), ct);
    [HttpPost("cultivation/settle")] public Task<List<RewardLineDto>> Settle([FromBody] EquipRequest? body, CancellationToken ct) => _game.SettleCultivationAsync(UserId(), body?.IdempotencyKey, ct);
    [HttpGet("rumors")] public Task<List<RumorDto>> Rumors(CancellationToken ct) => _game.RumorsAsync(ct);
    [HttpGet("travel/locations")] public Task<List<TravelLocationDto>> Locations(CancellationToken ct) => _game.TravelLocationsAsync(ct);
    [HttpPost("travel/check-in")] public async Task<IActionResult> CheckIn(CheckInRequest req, CancellationToken ct) { await _game.CheckInAsync(UserId(), req, ct); return NoContent(); }
    [HttpGet("travel/buff")] public Task<object?> Buff(CancellationToken ct) => _game.TravelBuffAsync(UserId(), ct);
    [HttpPost("secret-realms/enter")] public Task<object> EnterRealm(CancellationToken ct) => _game.EnterSecretRealmAsync(UserId(), ct);
}
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly GameplayService _game;
    public AdminController(GameplayService game) => _game = game;
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("overview")] public Task<object> Overview(CancellationToken ct) => _game.AdminOverviewAsync(ct);
    [HttpPut("monsters/{id:guid}/weight")] public async Task<IActionResult> Weight(Guid id, UpdateSpawnWeightRequest req, CancellationToken ct) { await _game.UpdateMonsterWeightAsync(id, req.SpawnWeight, UserId(), ct); return NoContent(); }
    [HttpPut("zones/{id:guid}/aura")] public async Task<IActionResult> Aura(Guid id, UpdateAuraRequest req, CancellationToken ct) { await _game.UpdateZoneAuraAsync(id, req.Multiplier, UserId(), ct); return NoContent(); }
    [HttpPut("events/{id:guid}")] public async Task<IActionResult> Toggle(Guid id, ToggleEventRequest req, CancellationToken ct) { await _game.ToggleEventAsync(id, req.Enabled, UserId(), ct); return NoContent(); }
    [HttpGet("rewards")] public Task<List<TuTien.Domain.Entities.RewardTransaction>> Rewards([FromQuery] int take = 50, CancellationToken ct = default) => _game.RewardsAsync(take, ct);
    [HttpGet("combats")] public Task<List<TuTien.Domain.Entities.CombatSession>> Combats([FromQuery] int take = 50, CancellationToken ct = default) => _game.CombatsAsync(take, ct);
}
