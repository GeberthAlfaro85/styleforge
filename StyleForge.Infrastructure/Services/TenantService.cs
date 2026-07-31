// StyleForge.Infrastructure/Services/TenantService.cs
using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs.Tenants;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;

        public TenantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateTenant> UpdateAsync(Guid tenantId, UpdateTenant dto)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant is null)
                throw new KeyNotFoundException("Tenant no encontrado.");

            tenant.Name = dto.Name;
            tenant.TypBusiness = dto.TypBusiness;
            tenant.Address = dto.Address;
            tenant.City = dto.City;
            tenant.Phone = dto.Phone;
            tenant.Description = dto.Description;

            await _context.SaveChangesAsync();

            return new UpdateTenant
            {
                Name = tenant.Name,
                TypBusiness = tenant.TypBusiness,
                Address = tenant.Address,
                City = tenant.City,
                Phone = tenant.Phone,
                Description = tenant.Description
            };
        }

        public async Task<UpdateTenant> GetByIdAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant is null)
                throw new KeyNotFoundException("Tenant no encontrado.");

            return new UpdateTenant
            {
                Id = tenant.Id,
                Name = tenant.Name,
                TypBusiness = tenant.TypBusiness,
                Address = tenant.Address,
                City = tenant.City,
                Phone = tenant.Phone,
                Description = tenant.Description
            };
        }
    }
}