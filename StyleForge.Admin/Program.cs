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
<html lang="es">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>StyleForge Admin</title>
<style>
  :root {
    --bg1: #eef2ff; --bg2: #f5f3ff;
    --card: #ffffff; --text: #1e1b4b; --muted: #6b7280;
    --border: #d1d5db; --primary: #4f46e5; --primary-hover: #4338ca;
    --error-bg: #fef2f2; --error-text: #b91c1c; --error-border: #fecaca;
  }
  @media (prefers-color-scheme: dark) {
    :root {
      --bg1: #0f172a; --bg2: #1e1b4b;
      --card: #1f2937; --text: #f3f4f6; --muted: #9ca3af;
      --border: #374151; --primary: #6366f1; --primary-hover: #818cf8;
      --error-bg: #450a0a; --error-text: #fca5a5; --error-border: #7f1d1d;
    }
  }
  * { box-sizing: border-box; }
  body {
    margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center;
    font-family: system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
    background: linear-gradient(135deg, var(--bg1), var(--bg2));
    color: var(--text); padding: 1rem;
  }
  .card {
    width: 100%; max-width: 380px; background: var(--card);
    border-radius: 16px; padding: 2rem;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.12);
  }
  .logo {
    width: 52px; height: 52px; border-radius: 14px; margin: 0 auto 1rem;
    background: linear-gradient(135deg, #6366f1, #8b5cf6);
    display: flex; align-items: center; justify-content: center;
    color: #fff; font-weight: 700; font-size: 1.4rem;
  }
  h1 { margin: 0; text-align: center; font-size: 1.4rem; }
  .sub { margin: 0.25rem 0 1.5rem; text-align: center; color: var(--muted); font-size: 0.9rem; }
  label { display: block; margin-bottom: 0.35rem; font-size: 0.85rem; font-weight: 600; }
  .field { margin-bottom: 1rem; position: relative; }
  input {
    width: 100%; padding: 0.7rem 0.85rem; font-size: 1rem;
    border: 1px solid var(--border); border-radius: 10px;
    background: transparent; color: var(--text);
  }
  input:focus { outline: 2px solid var(--primary); outline-offset: 1px; border-color: transparent; }
  .toggle {
    position: absolute; right: 0.6rem; top: 2.1rem; background: none; border: none;
    color: var(--muted); cursor: pointer; font-size: 0.8rem; padding: 0.25rem;
  }
  .error {
    background: var(--error-bg); color: var(--error-text);
    border: 1px solid var(--error-border); border-radius: 10px;
    padding: 0.65rem 0.85rem; margin-bottom: 1rem; font-size: 0.9rem;
  }
  button.submit {
    width: 100%; padding: 0.8rem; font-size: 1rem; font-weight: 600;
    border: none; border-radius: 10px; cursor: pointer;
    background: var(--primary); color: #fff; transition: background 0.15s;
  }
  button.submit:hover { background: var(--primary-hover); }
  button.submit:disabled { opacity: 0.7; cursor: wait; }
  .foot { margin-top: 1.25rem; text-align: center; color: var(--muted); font-size: 0.75rem; }
</style>
</head>
<body>
  <main class="card">
    <div class="logo">S</div>
    <h1>StyleForge Admin</h1>
    <p class="sub">Acceso restringido</p>

    {{(error == true ? "<div class='error' role='alert'>Usuario o contraseña incorrectos.</div>" : "")}}

    <form method="post" action="/login" id="loginForm">
      <div class="field">
        <label for="username">Usuario</label>
        <input id="username" name="username" autocomplete="username" required autofocus />
      </div>
      <div class="field">
        <label for="password">Contraseña</label>
        <input id="password" name="password" type="password" autocomplete="current-password" required />
        <button type="button" class="toggle" id="toggle">Mostrar</button>
      </div>
      <button type="submit" class="submit" id="submitBtn">Entrar</button>
    </form>

    <p class="foot">Panel interno · Gestión de licencias</p>
  </main>

  <script>
    const pwd = document.getElementById('password');
    const toggle = document.getElementById('toggle');
    toggle.addEventListener('click', () => {
      const show = pwd.type === 'password';
      pwd.type = show ? 'text' : 'password';
      toggle.textContent = show ? 'Ocultar' : 'Mostrar';
    });
    document.getElementById('loginForm').addEventListener('submit', () => {
      const btn = document.getElementById('submitBtn');
      btn.disabled = true;
      btn.textContent = 'Entrando...';
    });
  </script>
</body>
</html>
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