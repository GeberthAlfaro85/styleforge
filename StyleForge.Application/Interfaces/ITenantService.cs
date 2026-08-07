using StyleForge.Application.DTOs.Tenants;

namespace StyleForge.Application.Interfaces
{
    public interface ITenantService
    {
        // En ITenantService
        Task<UpdateTenant> UpdateAsync(Guid tenantId, UpdateTenant dto);
        Task<UpdateTenant> GetByIdAsync(Guid tenantId);
        Task<PublicTenantDto> GetBySlugAsync(string slug);
    }
}
