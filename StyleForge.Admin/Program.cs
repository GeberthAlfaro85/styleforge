using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using StyleForge.Admin.Components;
using StyleForge.Infrastructure.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using StyleForge.Application.Interfaces;

// Modo utilitario: dotnet run -- hash "MiPassword"
if (args.Length == 2 && args[0] == "hash")
{
    Console.WriteLine(AdminPassword.Hash(args[1]));
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<ICurrentUserService, AdminCurrentUserService>();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<LicenseAdminService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

// Render está detrás de un proxy: necesitamos la IP y el esquema reales
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/login", (bool? error) => Results.Content($$"""
<!DOCTYPE html>
<html lang="es"><head><meta charset="utf-8" /><title>Admin</title>
<meta name="viewport" content="width=device-width, initial-scale=1" /></head>
<body style="font-family:sans-serif;max-width:320px;margin:15vh auto">
  <h2>StyleForge Admin</h2>
  {{(error == true ? "<p style='color:#b00'>Credenciales inválidas.</p>" : "")}}
  <form method="post" action="/login">
    <p><input name="username" placeholder="Usuario" autocomplete="username" required style="width:100%" /></p>
    <p><input name="password" type="password" placeholder="Contraseña" autocomplete="current-password" required style="width:100%" /></p>
    <button type="submit">Entrar</button>
  </form>
</body></html>
""", "text/html")).AllowAnonymous();

app.MapPost("/login", async (HttpContext ctx, IConfiguration cfg) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var user = form["username"].ToString();
    var pass = form["password"].ToString();

    var expectedUser = cfg["Admin:Username"] ?? "";
    var hash = cfg["Admin:PasswordHash"] ?? "";

    // Se evalúan ambas siempre, para no revelar cuál falló por el tiempo de respuesta
    var userOk = expectedUser.Length > 0 && CryptographicOperations.FixedTimeEquals(
        SHA256.HashData(Encoding.UTF8.GetBytes(user)),
        SHA256.HashData(Encoding.UTF8.GetBytes(expectedUser)));
    var passOk = hash.Length > 0 && AdminPassword.Verify(pass, hash);

    if (!(userOk && passOk))
        return Results.Redirect("/login?error=true");

    var identity = new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.Name, expectedUser) },
        CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(new ClaimsPrincipal(identity));
    return Results.Redirect("/tenants");
}).AllowAnonymous().RequireRateLimiting("login");

app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync();
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();