using Microsoft.EntityFrameworkCore;
using StyleForge.Infrastructure.Data;
using System.Security.Claims;

namespace StyleForge.API.Middleware;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // Solo verificar en requests autenticados
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdValue = context.User.FindFirst("tenantId")?.Value;

            if (Guid.TryParse(tenantIdValue, out var tenantId))
            {
                var tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);

                if (tenant != null && !tenant.IsLicenseActive)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        $"{{\"message\":\"Licencia expirada. Contacta al soporte para renovar tu licencia. Expiró el {tenant.LicenseExpiresAt:yyyy-MM-dd}.\"}}"
                    );
                    return;
                }
            }
        }

        await _next(context);
    }
}
