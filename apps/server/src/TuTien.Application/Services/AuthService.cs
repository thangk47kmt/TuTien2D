using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TuTien.Application.Abstractions;
using TuTien.Application.Common;
using TuTien.Application.Dtos;
using TuTien.Domain.Entities;

namespace TuTien.Application.Services;

public class AuthService
{
    private readonly IAppDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AuthService(IAppDbContext db, IConfiguration config) { _db = db; _config = config; }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || req.UserName.Length < 3)
            throw new AppException("invalid_username", "Ten tai khoan toi thieu 3 ky tu.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new AppException("invalid_password", "Mat khau toi thieu 6 ky tu.");
        if (await _db.Users.AnyAsync(u => u.UserName == req.UserName, ct))
            throw new AppException("username_taken", "Ten tai khoan da ton tai.");
        var user = new AppUser
        {
            Id = Guid.NewGuid(), UserName = req.UserName.Trim(),
            Email = string.IsNullOrWhiteSpace(req.Email) ? $"{req.UserName.Trim()}@tutien.local" : req.Email.Trim(),
            Role = "Player", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return await IssueAsync(user, ct);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == req.UserName, ct)
                   ?? throw new AppException("invalid_login", "Sai tai khoan hoac mat khau.", 401);
        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            throw new AppException("invalid_login", "Sai tai khoan hoac mat khau.", 401);
        return await IssueAsync(user, ct);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await _db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAtUtc == null, ct)
            ?? throw new AppException("invalid_refresh", "Refresh token khong hop le.", 401);
        if (stored.ExpiresAtUtc < DateTime.UtcNow) throw new AppException("expired_refresh", "Refresh token het han.", 401);
        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueAsync(stored.User!, ct);
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken, CancellationToken ct)
    {
        var tokens = _db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAtUtc == null);
        if (!string.IsNullOrEmpty(refreshToken)) { var hash = Hash(refreshToken); tokens = tokens.Where(t => t.TokenHash == hash); }
        await tokens.ForEachAsync(t => t.RevokedAtUtc = DateTime.UtcNow, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AppUser> GetUserAsync(Guid userId, CancellationToken ct)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct) ?? throw new AppException("not_found", "Khong tim thay tai khoan.", 404);

    private async Task<TokenResponse> IssueAsync(AppUser user, CancellationToken ct)
    {
        var key = _config["Jwt:Key"] ?? "dev-only-change-me-tutien-du-hanh-32ch!";
        var issuer = _config["Jwt:Issuer"] ?? "tutien";
        var minutes = int.TryParse(_config["Jwt:AccessMinutes"], out var m) ? m : 120;
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.PadRight(32))), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(minutes);
        var jwt = new JwtSecurityToken(issuer, issuer,
            [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, user.Role)],
            expires: expires, signingCredentials: creds);
        var access = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        _db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, TokenHash = Hash(refresh), ExpiresAtUtc = DateTime.UtcNow.AddDays(14), CreatedAtUtc = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
        return new TokenResponse(access, refresh, expires, user.Role);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
