using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace StyleForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LicenseController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;

    public LicenseController(AppDbContext db, ICurrentUserService currentUser, IConfiguration config)
    {
        _db = db;
        _currentUser = currentUser;
        _config = config;
    }

    /// <summary>Retorna el estado actual de la licencia del tenant autenticado.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLicenseStatus()
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId == null) return Unauthorized();

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return NotFound();

        return Ok(BuildLicenseResponse(tenant.IsLicenseActive, tenant.LicenseExpiresAt));
    }

    /// <summary>
    /// Renueva la licencia de un tenant por N días.
    /// Requiere el header X-Master-Key con la clave maestra configurada en el servidor.
    /// </summary>
    [HttpPost("renew")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RenewLicense([FromBody] RenewLicenseRequest request)
    {
        var masterKey = _config["License:MasterKey"];
        var providedKey = Request.Headers["X-Master-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(masterKey) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedKey),
                Encoding.UTF8.GetBytes(masterKey)))
            return Unauthorized(new { message = "Master key inválida." });

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId);
        if (tenant == null) return NotFound(new { message = "Tenant no encontrado." });

        // Si ya expiró, renovar desde hoy. Si aún está activa, extender desde la fecha actual de expiración.
        var baseDate = tenant.IsLicenseActive && tenant.LicenseExpiresAt.HasValue
            ? tenant.LicenseExpiresAt.Value
            : DateTime.UtcNow;

        tenant.LicenseExpiresAt = baseDate.AddDays(request.Days);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = $"Licencia renovada por {request.Days} días.",
            tenantId = tenant.Id,
            tenantName = tenant.Name,
            license = BuildLicenseResponse(tenant.IsLicenseActive, tenant.LicenseExpiresAt)
        });
    }

    private static object BuildLicenseResponse(bool isActive, DateTime? expiresAt) => new
    {
        isActive,
        expiresAt,
        daysRemaining = expiresAt.HasValue
            ? Math.Max(0, (int)(expiresAt.Value - DateTime.UtcNow).TotalDays)
            : (int?)null,
        status = expiresAt == null
            ? "Permanente"
            : isActive
                ? "Activa"
                : "Expirada"
    };
}

public record RenewLicenseRequest(Guid TenantId, int Days);
