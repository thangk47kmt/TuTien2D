using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuTien.Application.Dtos;
using TuTien.Application.Services;
namespace TuTien.Api.Controllers;
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;
    [HttpPost("register")] public Task<TokenResponse> Register(RegisterRequest req, CancellationToken ct) => _auth.RegisterAsync(req, ct);
    [HttpPost("login")] public Task<TokenResponse> Login(LoginRequest req, CancellationToken ct) => _auth.LoginAsync(req, ct);
    [HttpPost("refresh")] public Task<TokenResponse> Refresh(RefreshRequest req, CancellationToken ct) => _auth.RefreshAsync(req.RefreshToken, ct);
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest? req, CancellationToken ct)
    {
        await _auth.LogoutAsync(UserId(), req?.RefreshToken, ct);
        return NoContent();
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _auth.GetUserAsync(UserId(), ct);
        return Ok(new { user.Id, user.UserName, user.Email, user.Role });
    }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
