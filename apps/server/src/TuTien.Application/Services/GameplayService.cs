using Microsoft.EntityFrameworkCore;
using TuTien.Application.Abstractions;
using TuTien.Application.Common;
using TuTien.Application.Dtos;
using TuTien.Domain;
using TuTien.Domain.Entities;
using TuTien.Domain.Services;

namespace TuTien.Application.Services;

public class GameplayService
{
    public const int MapWidth = 24; public const int MapHeight = 16;
    private readonly IAppDbContext _db;
    private readonly CharacterStatCalculator _stats = new();
    private readonly LootService _loot = new();
    public GameplayService(IAppDbContext db) => _db = db;

    public Task<List<ProfessionDto>> ListProfessionsAsync(CancellationToken ct) =>
        _db.Professions.Where(p => p.IsActive).OrderBy(p => p.Name)
            .Select(p => new ProfessionDto(p.Id, p.Code, p.Name, p.Description, p.PrimaryBonusText, p.SecondaryBonusText, p.AttackBonus, p.DefenseBonus, p.SpiritBonus)).ToListAsync(ct);

    public async Task<PlayerProfileDto> CreatePlayerAsync(Guid userId, CreatePlayerRequest req, CancellationToken ct)
    {
        if (await _db.Players.AnyAsync(p => p.UserId == userId, ct)) throw new AppException("player_exists", "Tai khoan da co nhan vat.");
        var err = NameValidator.NormalizeOrError(req.Name); if (err is not null) throw new AppException("invalid_name", err);
        var profession = await _db.Professions.FirstOrDefaultAsync(p => p.Code == req.ProfessionCode && p.IsActive, ct) ?? throw new AppException("invalid_profession", "Nghe khong hop le.");
        var breath = await _db.TechniqueDefinitions.FirstAsync(t => t.Code == "dan_khi", ct);
        var now = DateTime.UtcNow;
        var player = new Player { Id = Guid.NewGuid(), UserId = userId, Name = NameValidator.Normalize(req.Name), ProfessionId = profession.Id, Level = 1, Realm = RealmKind.Mortal, Hp = 100, MaxHp = 100, Mp = 60, MaxMp = 60, SpiritStones = 40, MapX = 3, MapY = 3, ActiveTechniqueId = breath.Id, CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.Players.Add(player);
        _db.PlayerTechniques.Add(new PlayerTechnique { Id = Guid.NewGuid(), PlayerId = player.Id, TechniqueDefinitionId = breath.Id, IsActive = true, LearnedAtUtc = now });
        Discover(player, 3, 3); await _db.SaveChangesAsync(ct); await EnsureWorldPopulationAsync(ct);
        return await GetProfileAsync(userId, ct);
    }

    public async Task<PlayerProfileDto> GetProfileAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); var b = Breakdown(player); player.MaxHp = b.MaxHp; player.MaxMp = b.MaxMp; player.Hp = Math.Min(player.Hp, player.MaxHp); return ToProfile(player, b); }

    public async Task<WorldSnapshotDto> GetWorldAsync(Guid userId, CancellationToken ct)
    {
        var player = await Load(userId, ct); await EnsureWorldPopulationAsync(ct);
        var cells = await _db.PlayerDiscoveries.Where(d => d.PlayerId == player.Id).Select(d => new[] { d.X, d.Y }).ToListAsync(ct);
        var zone = await ZoneAt(player.MapX, player.MapY, ct);
        return new WorldSnapshotDto(MapWidth, MapHeight, player.MapX, player.MapY, cells, await Visible(player, ct), zone?.Name ?? "Hoang da", zone?.AuraMultiplier ?? 1m);
    }

    public async Task<WorldSnapshotDto> MoveAsync(Guid userId, MoveRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        if (req.X < 0 || req.Y < 0 || req.X >= MapWidth || req.Y >= MapHeight) throw new AppException("out_of_bounds", "Ra ngoai ban do.");
        if (Math.Abs(req.X - player.MapX) + Math.Abs(req.Y - player.MapY) > 1) throw new AppException("too_far", "Chi buoc 1 o.");
        player.MapX = req.X; player.MapY = req.Y; Discover(player, req.X, req.Y); await _db.SaveChangesAsync(ct);
        return await GetWorldAsync(userId, ct);
    }

    public async Task<WorldSnapshotDto> UseSenseAsync(Guid userId, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        if (player.Mp < 8) throw new AppException("no_mp", "Thieu linh luc.");
        player.Mp -= 8; Discover(player, player.MapX, player.MapY); await _db.SaveChangesAsync(ct);
        return await GetWorldAsync(userId, ct);
    }

    public async Task<CombatStateDto> StartCombatAsync(Guid userId, StartCombatRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var active = await _db.CombatSessions.Include(s => s.Actions).FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.Status == CombatStatus.Active, ct);
        if (active is not null) return ToCombat(active, player);
        var spawn = await _db.MonsterSpawns.Include(s => s.Definition).FirstOrDefaultAsync(s => s.Id == req.SpawnId && s.IsAlive, ct) ?? throw new AppException("spawn_gone", "Quai da bien.");
        var b = Breakdown(player);
        var tag = spawn.Definition!.Kind == MonsterKind.Boss ? " (Boss)" : spawn.Definition.Kind == MonsterKind.Rare ? " (Hiem)" : spawn.Definition.Kind == MonsterKind.Elite ? " (Tinh anh)" : "";
        var session = new CombatSession { Id = Guid.NewGuid(), PlayerId = player.Id, MonsterSpawnId = spawn.Id, MonsterDefinitionId = spawn.MonsterDefinitionId, MonsterName = spawn.Definition.Name + tag, MonsterHp = spawn.CurrentHp, MonsterMaxHp = spawn.Definition.Hp, MonsterAttack = spawn.Definition.Attack, MonsterDefense = spawn.Definition.Defense, PlayerHpSnapshot = player.Hp, PlayerAttackSnapshot = b.Attack, PlayerDefenseSnapshot = b.Defense, Status = CombatStatus.Active, StartedAtUtc = DateTime.UtcNow, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(8), LastActionAtUtc = DateTime.UtcNow };
        _db.CombatSessions.Add(session); await _db.SaveChangesAsync(ct); return ToCombat(session, player);
    }

    public async Task<CombatStateDto> ActAsync(Guid userId, Guid sessionId, CombatActionRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var session = await _db.CombatSessions.Include(s => s.Actions).FirstOrDefaultAsync(s => s.Id == sessionId && s.PlayerId == player.Id, ct) ?? throw new AppException("combat_missing", "Khong co tran.", 404);
        if (session.Status != CombatStatus.Active) return ToCombat(session, player);
        var rng = Random.Shared; var b = Breakdown(player); var tech = player.ActiveTechnique; var dealt = 0; var taken = 0; var note = "";
        if (req.Action == CombatActionType.Withdraw) { session.Status = CombatStatus.Withdrawn; session.EndedAtUtc = DateTime.UtcNow; note = "Rut lui."; }
        else
        {
            var skill = req.Action == CombatActionType.Skill;
            if (skill) { if (player.Mp < 10) throw new AppException("no_mp", "Thieu linh luc."); player.Mp -= 10; }
            var burst = skill && tech is { Kind: TechniqueKind.CombatBurst };
            dealt = CombatMath.PlayerDamage(b.Attack, session.MonsterDefense, rng, skill, burst);
            session.MonsterHp -= dealt;
            if (burst && tech is not null) { player.Hp = Math.Max(1, player.Hp - tech.SelfDamageOnSkill); note = $"Nghich mach {dealt}, phan phe {tech.SelfDamageOnSkill}."; }
            else note = skill ? $"Van cong {dealt}." : $"Danh {dealt}.";
            if (session.MonsterHp > 0) { taken = CombatMath.MonsterDamage(session.MonsterAttack, b.Defense, rng, req.Action == CombatActionType.Defend); player.Hp = Math.Max(0, player.Hp - taken); }
        }
        session.Actions.Add(new CombatAction { Id = Guid.NewGuid(), CombatSessionId = session.Id, Type = req.Action, DamageDealt = dealt, DamageTaken = taken, Note = note, CreatedAtUtc = DateTime.UtcNow });
        if (session.MonsterHp <= 0 && session.Status == CombatStatus.Active) await Win(player, session, ct);
        else if (player.Hp <= 0) { session.Status = CombatStatus.Lost; session.EndedAtUtc = DateTime.UtcNow; player.Hp = 1; player.SpiritStones = Math.Max(0, player.SpiritStones - 8); player.MapX = 3; player.MapY = 3; }
        await _db.SaveChangesAsync(ct); return ToCombat(session, player);
    }

    public async Task<CombatStateDto?> ActiveCombatAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); var s = await _db.CombatSessions.Include(x => x.Actions).FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Status == CombatStatus.Active, ct); return s is null ? null : ToCombat(s, player); }

    public async Task<List<InventoryItemDto>> InventoryAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); return player.Items.Select(i => ToItem(i, player.Profession!.Code)).ToList(); }

    public async Task<List<InventoryItemDto>> EquipAsync(Guid userId, EquipRequest req, bool equip, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var item = player.Items.FirstOrDefault(i => i.Id == req.ItemId) ?? throw new AppException("item_missing", "Khong co vat pham.");
        var def = item.Definition ?? throw new AppException("item_missing", "Thieu dinh nghia.");
        if (equip && def.ProfessionLocked && def.PreferredProfessionCode != player.Profession!.Code) throw new AppException("profession_locked", "Trang bi khoa nghe khac.");
        item.IsEquipped = equip; await _db.SaveChangesAsync(ct); return await InventoryAsync(userId, ct);
    }

    public async Task UseItemAsync(Guid userId, Guid itemId, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var item = player.Items.FirstOrDefault(i => i.Id == itemId) ?? throw new AppException("item_missing", "Khong co vat pham.");
        if (item.Definition is { HealAmount: > 0 }) { player.Hp = Math.Min(player.MaxHp, player.Hp + item.Definition.HealAmount); item.Quantity--; if (item.Quantity <= 0) _db.PlayerItems.Remove(item); await _db.SaveChangesAsync(ct); return; }
        throw new AppException("not_usable", "Khong dung duoc.");
    }
    public async Task LockItemAsync(Guid userId, Guid itemId, bool locked, CancellationToken ct)
    { var player = await Load(userId, ct); var item = player.Items.FirstOrDefault(i => i.Id == itemId) ?? throw new AppException("item_missing", "Khong co vat pham."); item.IsLocked = locked; await _db.SaveChangesAsync(ct); }

    public async Task<List<TechniqueDto>> TechniquesAsync(Guid userId, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var all = await _db.TechniqueDefinitions.ToListAsync(ct);
        return all.Select(t => { var cost = t.LearnCost; if (player.Profession!.TechniqueDiscountPercent > 0) cost = (int)Math.Round(cost * (100 - player.Profession.TechniqueDiscountPercent) / 100.0); var warn = t.Kind == TechniqueKind.CombatBurst ? "Phan phe khi van cong." : t.LocksRealm ? "Khoa canh gioi." : null; return new TechniqueDto(t.Id, t.Code, t.Name, t.Description, t.Kind.ToString(), cost, player.Techniques.Any(x => x.TechniqueDefinitionId == t.Id), player.ActiveTechniqueId == t.Id, t.LocksRealm, warn); }).ToList();
    }

    public async Task LearnOrActivateAsync(Guid userId, LearnTechniqueRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var tech = await _db.TechniqueDefinitions.FirstOrDefaultAsync(t => t.Code == req.Code, ct) ?? throw new AppException("tech_missing", "Khong co cong phap.");
        if (!player.Techniques.Any(t => t.TechniqueDefinitionId == tech.Id))
        {
            var cost = tech.LearnCost; if (player.Profession!.TechniqueDiscountPercent > 0) cost = (int)Math.Round(cost * (100 - player.Profession.TechniqueDiscountPercent) / 100.0);
            if (player.SpiritStones < cost) throw new AppException("no_stone", "Thieu linh thach.");
            player.SpiritStones -= cost;
            _db.PlayerTechniques.Add(new PlayerTechnique { Id = Guid.NewGuid(), PlayerId = player.Id, TechniqueDefinitionId = tech.Id, LearnedAtUtc = DateTime.UtcNow });
        }
        player.ActiveTechniqueId = tech.Id; player.RealmLocked = tech.LocksRealm; await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RewardLineDto>> OpenChestAsync(Guid userId, OpenChestRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var chest = await _db.ChestSpawns.Include(c => c.Definition)!.ThenInclude(d => d!.LootTable)!.ThenInclude(t => t!.Entries).ThenInclude(e => e.Item).FirstOrDefaultAsync(c => c.Id == req.ChestId, ct) ?? throw new AppException("chest_missing", "Khong co ruong.");
        var key = $"chest:{chest.Id}";
        if (await _db.RewardTransactions.AnyAsync(r => r.IdempotencyKey == key, ct)) return [new RewardLineDto("SpiritStone", "Da nhan", 0)];
        if (chest.Status != ChestStatus.Spawned) throw new AppException("chest_opened", "Ruong da mo.");
        chest.Status = ChestStatus.Opened; chest.OpenedByPlayerId = player.Id; chest.OpenedAtUtc = DateTime.UtcNow;
        var lines = new List<RewardLineDto>();
        if (chest.Definition?.LootTable is { } table) { var roll = _loot.Roll(table, player.PityScore, Random.Shared); player.PityScore = _loot.NextPity(player.PityScore, roll.RareHit, table); lines.Add(await Grant(player, roll, RewardSourceType.Chest, chest.Id, key, ct)); }
        else { player.SpiritStones += 8; lines.Add(new RewardLineDto("SpiritStone", "+8 linh thach", 8)); }
        await _db.SaveChangesAsync(ct); return lines;
    }

    public async Task<CultivationStatusDto> StartCultivationAsync(Guid userId, StartCultivationRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        if (await _db.CultivationSessions.AnyAsync(s => s.PlayerId == player.Id && !s.IsSettled, ct)) throw new AppException("busy", "Dang tu luyen.");
        var minutes = Math.Clamp(req.Minutes <= 0 ? 5 : req.Minutes, 1, 240);
        var zone = await ZoneAt(player.MapX, player.MapY, ct);
        var xp = (int)Math.Round(minutes * 3 * (zone?.AuraMultiplier ?? 1m) * (1 + Breakdown(player).CultivationPercent / 100m));
        var session = new CultivationSession { Id = Guid.NewGuid(), PlayerId = player.Id, StartUtc = DateTime.UtcNow, EndUtc = DateTime.UtcNow.AddMinutes(minutes), ExpectedXp = xp, AuraSnapshot = zone?.AuraMultiplier ?? 1m, Status = CultivationStatus.Running };
        _db.CultivationSessions.Add(session); await _db.SaveChangesAsync(ct);
        return new CultivationStatusDto(session.Id, session.Status.ToString(), session.EndUtc, session.ExpectedXp, false);
    }
    public async Task<CultivationStatusDto> CultivationStatusAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); var s = await _db.CultivationSessions.Where(x => x.PlayerId == player.Id && !x.IsSettled).OrderByDescending(x => x.StartUtc).FirstOrDefaultAsync(ct); return s is null ? new CultivationStatusDto(null, "None", null, null, false) : new CultivationStatusDto(s.Id, s.Status.ToString(), s.EndUtc, s.ExpectedXp, DateTime.UtcNow >= s.EndUtc); }
    public async Task<List<RewardLineDto>> SettleCultivationAsync(Guid userId, string? key, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var s = await _db.CultivationSessions.FirstOrDefaultAsync(x => x.PlayerId == player.Id && !x.IsSettled, ct) ?? throw new AppException("no_session", "Khong co phien tu luyen.");
        if (DateTime.UtcNow < s.EndUtc) throw new AppException("too_soon", "Chua het thoi gian.");
        var idem = key ?? $"cult:{s.Id}";
        if (await _db.RewardTransactions.AnyAsync(r => r.IdempotencyKey == idem, ct)) return [new RewardLineDto("CultivationXp", "Da ket toan", 0)];
        player.CultivationXp += s.ExpectedXp; LevelRealm(player); s.IsSettled = true; s.Status = CultivationStatus.Settled; s.SettledAtUtc = DateTime.UtcNow;
        _db.RewardTransactions.Add(new RewardTransaction { Id = Guid.NewGuid(), PlayerId = player.Id, SourceType = RewardSourceType.Cultivation, SourceId = s.Id, RewardType = RewardType.CultivationXp, Quantity = s.ExpectedXp, CreatedAtUtc = DateTime.UtcNow, CorrelationId = s.Id, IdempotencyKey = idem });
        await _db.SaveChangesAsync(ct); return [new RewardLineDto("CultivationXp", $"+{s.ExpectedXp} tu vi", s.ExpectedXp)];
    }

    public Task<List<RumorDto>> RumorsAsync(CancellationToken ct) => _db.Rumors.Where(r => r.IsActive).Select(r => new RumorDto(r.Id, r.Kind.ToString(), r.Title, r.Body, r.ApproximateZone, r.Reliability, r.ExpiresAtUtc)).ToListAsync(ct);
    public Task<List<TravelLocationDto>> TravelLocationsAsync(CancellationToken ct) => _db.TravelLocations.Select(l => new TravelLocationDto(l.Id, l.Code, l.Name, l.DistanceKm)).ToListAsync(ct);
    public async Task CheckInAsync(Guid userId, CheckInRequest req, CancellationToken ct)
    {
        var player = await Load(userId, ct);
        var loc = await _db.TravelLocations.FirstOrDefaultAsync(l => l.Id == req.LocationId, ct) ?? throw new AppException("loc_missing", "Khong co dia diem.");
        var key = req.IdempotencyKey ?? $"travel:{player.Id}:{loc.Id}:{DateTime.UtcNow:yyyyMMdd}";
        if (await _db.TravelCheckIns.AnyAsync(c => c.IdempotencyKey == key, ct)) return;
        var rules = TravelRules.FromDistance(loc.DistanceKm);
        _db.TravelCheckIns.Add(new TravelCheckIn { Id = Guid.NewGuid(), PlayerId = player.Id, LocationId = loc.Id, CheckedInAtUtc = DateTime.UtcNow, IdempotencyKey = key });
        if (rules.DurationHours > 0) _db.TravelBuffs.Add(new TravelBuff { Id = Guid.NewGuid(), PlayerId = player.Id, Multiplier = rules.Multiplier, ExpiresAtUtc = DateTime.UtcNow.AddHours(rules.DurationHours), SourceLocation = loc.Name });
        await _db.SaveChangesAsync(ct);
    }
    public async Task<object?> TravelBuffAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); var buff = await _db.TravelBuffs.Where(b => b.PlayerId == player.Id && b.ExpiresAtUtc > DateTime.UtcNow).OrderByDescending(b => b.ExpiresAtUtc).FirstOrDefaultAsync(ct); return buff is null ? null : new { buff.Multiplier, buff.ExpiresAtUtc, buff.SourceLocation }; }
    public async Task<object> EnterSecretRealmAsync(Guid userId, CancellationToken ct)
    { var player = await Load(userId, ct); if (player.Realm < RealmKind.QiRefining) throw new AppException("realm_low", "Can Luyen Khi."); return new { name = "Thanh Moc", endUtc = DateTime.UtcNow.AddMinutes(10), message = "Bi canh prototype." }; }
    public async Task<object> AdminOverviewAsync(CancellationToken ct) => new { players = await _db.Players.CountAsync(ct), users = await _db.Users.CountAsync(ct), combats = await _db.CombatSessions.CountAsync(ct), rewards = await _db.RewardTransactions.CountAsync(ct), monsters = await _db.MonsterDefinitions.Select(m => new { m.Id, m.Name, m.Kind, m.SpawnWeight }).ToListAsync(ct) };
    public async Task UpdateMonsterWeightAsync(Guid id, int weight, Guid actor, CancellationToken ct) { var m = await _db.MonsterDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("missing", "Khong co quai.", 404); m.SpawnWeight = weight; await _db.SaveChangesAsync(ct); }
    public async Task UpdateZoneAuraAsync(Guid id, decimal mul, Guid actor, CancellationToken ct) { var z = await _db.WorldZones.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("missing", "Khong co vung.", 404); z.AuraMultiplier = mul; await _db.SaveChangesAsync(ct); }
    public async Task ToggleEventAsync(Guid id, bool enabled, Guid actor, CancellationToken ct) { var ev = await _db.Events.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("missing", "Khong co event.", 404); ev.IsEnabled = enabled; await _db.SaveChangesAsync(ct); }
    public Task<List<RewardTransaction>> RewardsAsync(int take, CancellationToken ct) => _db.RewardTransactions.OrderByDescending(r => r.CreatedAtUtc).Take(take).ToListAsync(ct);
    public Task<List<CombatSession>> CombatsAsync(int take, CancellationToken ct) => _db.CombatSessions.OrderByDescending(r => r.StartedAtUtc).Take(take).ToListAsync(ct);
    public Task<List<CultivationSession>> CultivationErrorsAsync(CancellationToken ct) => _db.CultivationSessions.Where(s => !s.IsSettled && s.EndUtc < DateTime.UtcNow.AddHours(-2)).ToListAsync(ct);

    public async Task EnsureWorldPopulationAsync(CancellationToken ct)
    {
        var defs = await _db.MonsterDefinitions.ToListAsync(ct); if (defs.Count == 0) return;
        var alive = await _db.MonsterSpawns.CountAsync(s => s.IsAlive, ct); var rng = Random.Shared; var total = Math.Max(1, defs.Sum(d => Math.Max(1, d.SpawnWeight)));
        while (alive < 10)
        {
            var roll = rng.Next(total); var acc = 0; MonsterDefinition? chosen = null;
            foreach (var d in defs) { acc += Math.Max(1, d.SpawnWeight); if (roll < acc) { chosen = d; break; } }
            chosen ??= defs[0];
            _db.MonsterSpawns.Add(new MonsterSpawn { Id = Guid.NewGuid(), MonsterDefinitionId = chosen.Id, X = rng.Next(2, MapWidth - 2), Y = rng.Next(2, MapHeight - 2), CurrentHp = chosen.Hp, SpawnedAtUtc = DateTime.UtcNow, IsAlive = true });
            alive++;
        }
        if (await _db.ChestSpawns.CountAsync(c => c.Status == ChestStatus.Spawned, ct) < 4)
        {
            var chests = await _db.ChestDefinitions.ToListAsync(ct);
            if (chests.Count > 0)
            {
                var w = Math.Max(1, chests.Sum(c => Math.Max(1, c.SpawnWeight))); var roll = rng.Next(w); var acc = 0; var pick = chests[0];
                foreach (var c in chests) { acc += Math.Max(1, c.SpawnWeight); if (roll < acc) { pick = c; break; } }
                _db.ChestSpawns.Add(new ChestSpawn { Id = Guid.NewGuid(), ChestDefinitionId = pick.Id, X = rng.Next(2, MapWidth - 2), Y = rng.Next(2, MapHeight - 2), Status = ChestStatus.Spawned, SpawnedAtUtc = DateTime.UtcNow, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30) });
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    async Task Win(Player player, CombatSession session, CancellationToken ct)
    {
        session.Status = CombatStatus.Won; session.EndedAtUtc = DateTime.UtcNow;
        var def = await _db.MonsterDefinitions.Include(m => m.LootTable)!.ThenInclude(t => t!.Entries).ThenInclude(e => e.Item).FirstOrDefaultAsync(m => m.Id == session.MonsterDefinitionId, ct);
        var xp = def?.CultivationXp ?? 18; var stones = def?.SpiritStones ?? 6; stones += stones * Breakdown(player).StoneRewardPercent / 100;
        player.CultivationXp += xp; player.SpiritStones += stones; LevelRealm(player);
        if (session.MonsterSpawnId is Guid sid) { var spawn = await _db.MonsterSpawns.FirstOrDefaultAsync(s => s.Id == sid, ct); if (spawn is not null) { spawn.IsAlive = false; spawn.DefeatedAtUtc = DateTime.UtcNow; } }
        var key = $"combat:{session.Id}:win";
        if (!await _db.RewardTransactions.AnyAsync(r => r.IdempotencyKey == key, ct))
            _db.RewardTransactions.Add(new RewardTransaction { Id = Guid.NewGuid(), PlayerId = player.Id, SourceType = RewardSourceType.Combat, SourceId = session.Id, RewardType = RewardType.SpiritStone, Quantity = stones, CreatedAtUtc = DateTime.UtcNow, CorrelationId = session.Id, IdempotencyKey = key });
        if (def?.LootTable is { } table) { var roll = _loot.Roll(table, player.PityScore, Random.Shared); player.PityScore = _loot.NextPity(player.PityScore, roll.RareHit, table); await Grant(player, roll, RewardSourceType.Combat, session.Id, key + ":loot", ct); }
    }

    async Task<RewardLineDto> Grant(Player player, LootRoll roll, RewardSourceType src, Guid srcId, string key, CancellationToken ct)
    {
        if (await _db.RewardTransactions.AnyAsync(r => r.IdempotencyKey == key, ct)) return new RewardLineDto(roll.Type.ToString(), "Da nhan", 0);
        if (roll.Type == RewardType.Item && roll.ItemDefinitionId is Guid itemId)
        {
            var existing = player.Items.FirstOrDefault(i => i.ItemDefinitionId == itemId && i.Definition is { Stackable: true });
            if (existing is not null) existing.Quantity += roll.Quantity;
            else { var it = new PlayerItem { Id = Guid.NewGuid(), PlayerId = player.Id, ItemDefinitionId = itemId, Quantity = roll.Quantity, CreatedAtUtc = DateTime.UtcNow }; player.Items.Add(it); _db.PlayerItems.Add(it); }
        }
        else player.SpiritStones += roll.Quantity;
        _db.RewardTransactions.Add(new RewardTransaction { Id = Guid.NewGuid(), PlayerId = player.Id, SourceType = src, SourceId = srcId, RewardType = roll.Type, DefinitionId = roll.ItemDefinitionId, Quantity = roll.Quantity, CreatedAtUtc = DateTime.UtcNow, CorrelationId = srcId, IdempotencyKey = key });
        var name = roll.ItemDefinitionId is Guid id ? (await _db.ItemDefinitions.FindAsync([id], ct))?.Name ?? "do" : "linh thach";
        return new RewardLineDto(roll.Type.ToString(), $"+{roll.Quantity} {name}", roll.Quantity);
    }

    void LevelRealm(Player player)
    {
        while (player.CultivationXp >= _stats.XpRequired(player)) { player.CultivationXp -= _stats.XpRequired(player); player.Level++; }
        if (player.RealmLocked) return;
        if (player.Level >= 12 && player.Realm < RealmKind.Foundation) { player.Realm = RealmKind.Foundation; player.RealmStage = 1; }
        else if (player.Level >= 5 && player.Realm < RealmKind.QiRefining) { player.Realm = RealmKind.QiRefining; player.RealmStage = 1; }
    }

    async Task<Player> Load(Guid userId, CancellationToken ct) => await _db.Players.Include(p => p.Profession).Include(p => p.Items).ThenInclude(i => i.Definition).Include(p => p.Equipment).Include(p => p.Techniques).Include(p => p.ActiveTechnique).FirstOrDefaultAsync(p => p.UserId == userId, ct) ?? throw new AppException("no_player", "Chua co nhan vat.", 404);
    StatBreakdown Breakdown(Player player) => _stats.Calculate(player, player.Profession!, player.Items.Where(i => i.IsEquipped && i.Definition is not null).Select(i => (i.Definition!, i.Definition!.PreferredProfessionCode == player.Profession!.Code)).ToList(), player.ActiveTechnique);
    void Discover(Player player, int x, int y)
    {
        for (var dx = -2; dx <= 2; dx++) for (var dy = -2; dy <= 2; dy++)
        { var nx = x + dx; var ny = y + dy; if (nx < 0 || ny < 0 || nx >= MapWidth || ny >= MapHeight) continue; if (player.Discoveries.Any(d => d.X == nx && d.Y == ny)) continue; var disc = new PlayerDiscovery { Id = Guid.NewGuid(), PlayerId = player.Id, X = nx, Y = ny, DiscoveredAtUtc = DateTime.UtcNow }; player.Discoveries.Add(disc); _db.PlayerDiscoveries.Add(disc); }
    }
    async Task<WorldZone?> ZoneAt(int x, int y, CancellationToken ct) => await _db.WorldZones.FirstOrDefaultAsync(z => x >= z.MinX && x <= z.MaxX && y >= z.MinY && y <= z.MaxY, ct);
    async Task<List<VisibleEntityDto>> Visible(Player player, CancellationToken ct)
    {
        var r = Math.Max(3, Breakdown(player).DetectionRadius);
        var list = new List<VisibleEntityDto>();
        foreach (var m in await _db.MonsterSpawns.Include(s => s.Definition).Where(s => s.IsAlive).ToListAsync(ct))
            if (Math.Abs(m.X - player.MapX) <= r && Math.Abs(m.Y - player.MapY) <= r) list.Add(new VisibleEntityDto("monster", m.Id, m.Definition!.Name, m.X, m.Y, m.Definition.Kind.ToString()));
        foreach (var c in await _db.ChestSpawns.Include(s => s.Definition).Where(s => s.Status == ChestStatus.Spawned).ToListAsync(ct))
            if (Math.Abs(c.X - player.MapX) <= r && Math.Abs(c.Y - player.MapY) <= r) list.Add(new VisibleEntityDto("chest", c.Id, c.Definition!.Name, c.X, c.Y, c.Definition.Quality.ToString()));
        return list;
    }
    static CombatStateDto ToCombat(CombatSession s, Player p) => new(s.Id, s.MonsterName, s.MonsterHp, s.MonsterMaxHp, p.Hp, p.MaxHp, s.Status.ToString(), s.Actions.OrderBy(a => a.CreatedAtUtc).Select(a => a.Note).ToList());
    static InventoryItemDto ToItem(PlayerItem i, string prof) { var d = i.Definition!; return new InventoryItemDto(i.Id, d.Id, d.Code, d.Name, d.Icon, d.Type.ToString(), d.Quality.ToString(), i.Quantity, i.IsEquipped, i.IsLocked, d.Attack, d.Defense, d.Spirit, d.HealAmount, d.PreferredProfessionCode, d.ProfessionBonusPercent, d.PreferredProfessionCode == prof); }
    PlayerProfileDto ToProfile(Player p, StatBreakdown b) => new(p.Id, p.Name, p.Profession!.Code, p.Profession.Name, p.Level, p.Realm.ToString(), p.RealmStage, p.CultivationXp, _stats.XpRequired(p), p.Hp, b.MaxHp, p.Mp, b.MaxMp, b.Attack, b.Defense, b.Spirit, b.Agility, b.Fortune, p.Stability, p.SpiritStones, p.ProfessionPoints, p.ExtremePoints, p.RealmLocked, p.MapX, p.MapY, p.ActiveTechnique?.Code, p.PityScore);
}
