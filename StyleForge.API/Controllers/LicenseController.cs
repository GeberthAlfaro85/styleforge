using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Data;

namespace StyleForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LicenseController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public LicenseController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Retorna el estado actual de la licencia del tenant.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLicenseStatus()
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId == null) return Unauthorized();

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return NotFound();

        return Ok(new
        {
            isActive = tenant.IsLicenseActive,
            expiresAt = tenant.LicenseExpiresAt,
            daysRemaining = tenant.LicenseExpiresAt.HasValue
                ? Math.Max(0, (int)(tenant.LicenseExpiresAt.Value - DateTime.UtcNow).TotalDays)
                : (int?)null,
            status = tenant.LicenseExpiresAt == null
                ? "Permanente"
                : tenant.IsLicenseActive
                    ? "Activa"
                    : "Expirada"
        });
    }
}
