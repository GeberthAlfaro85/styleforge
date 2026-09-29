using Microsoft.EntityFrameworkCore;
using StyleForge.Domain.Entities;             // ajusta al namespace real de Tenant
using StyleForge.Infrastructure.Data;         // AppDbContext (según tu error)

public class LicenseAdminService(AppDbContext db, ILogger<LicenseAdminService> log)
{
    public Task<List<Tenant>> GetTenantsAsync() =>
        db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<DateTime> RenewAsync(Guid tenantId, int days, string admin)
    {
        if (days is < 1 or > 3650)
            throw new ArgumentOutOfRangeException(nameof(days));

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId)
            ?? throw new KeyNotFoundException("Tenant no encontrado.");

        var now = DateTime.UtcNow;
        var previous = tenant.LicenseExpiresAt;
        var baseDate = previous is { } exp && exp > now ? exp : now;
        tenant.LicenseExpiresAt = baseDate.AddDays(days);

        await db.SaveChangesAsync();

        log.LogWarning("License renewed by {Admin}: tenant {Tenant}, +{Days}d, {Old} -> {New}",
            admin, tenantId, days, previous, tenant.LicenseExpiresAt);

        return tenant.LicenseExpiresAt.Value;
    }
}