using StyleForge.Application.DTOs.Tenants;

namespace StyleForge.Application.Interfaces
{
    public interface ITenantService
    {
        // En ITenantService
        Task<UpdateTenant> UpdateAsync(Guid tenantId, UpdateTenant dto);
        Task<UpdateTenant> GetByIdAsync(Guid tenantId);
        Task<PublicTenantDto> GetBySlugAsync(string slug);

        /// <summary>Guarda la ruta relativa del logo recién subido (o null para quitarlo) y la persiste.</summary>
        Task<string?> UpdateLogoAsync(Guid tenantId, string? logoUrl);

        /// <summary>Ruta relativa del logo actual del tenant, o null si no tiene. Usado para borrar el archivo anterior al reemplazarlo.</summary>
        Task<string?> GetLogoUrlAsync(Guid tenantId);
    }
}
