using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TuTien.Api;
using TuTien.Application.Abstractions;
using TuTien.Application.Common;
using TuTien.Application.Services;
using TuTien.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("Default") ?? "Data Source=tutien.db";
var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) opt.UseSqlServer(connection);
    else opt.UseSqlite(connection);
});
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GameplayService>();
builder.Services.AddScoped<AdminCatalogService>();
builder.Services.AddScoped<BalanceService>();
builder.Services.AddHostedService<WorldHeartbeatService>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-change-me-tutien-du-hanh-32ch!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "tutien";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true,
        ValidIssuer = issuer, ValidAudience = issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey.PadRight(32)))
    };
});
builder.Services.AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireRole("Admin")));
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddPolicy("web", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.EnsureAsync(db);
    await SeedRules.EnsureAsync(db);
}
app.UseExceptionHandler(err =>
{
    err.Run(async ctx =>
    {
        var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        ctx.Response.ContentType = "application/json";
        if (ex is AppException appEx)
        {
            ctx.Response.StatusCode = appEx.StatusCode;
            await ctx.Response.WriteAsJsonAsync(new ApiError(appEx.Code, appEx.Message));
        }
        else
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new ApiError("server_error", "Loi may chu."));
        }
    });
});
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
public partial class Program;
