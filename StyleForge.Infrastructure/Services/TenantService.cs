// StyleForge.Infrastructure/Services/TenantService.cs
using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs.Tenants;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace StyleForge.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private static readonly Regex SlugFormat = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

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

            if (!string.IsNullOrWhiteSpace(dto.Slug) && dto.Slug != tenant.Slug)
            {
                if (!SlugFormat.IsMatch(dto.Slug))
                    throw new ArgumentException("El slug solo puede tener minúsculas, números y guiones (ej. \"salon-de-ana\").");

                var taken = await _context.Tenants.AnyAsync(t => t.Slug == dto.Slug && t.Id != tenantId);
                if (taken)
                    throw new ArgumentException("Ese slug ya está en uso por otro salón.");

                tenant.Slug = dto.Slug;
            }

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
                Slug = tenant.Slug ?? string.Empty,
                TypBusiness = tenant.TypBusiness,
                Address = tenant.Address,
                City = tenant.City,
                Phone = tenant.Phone,
                Description = tenant.Description,
                LogoUrl = tenant.LogoUrl
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
                Slug = tenant.Slug ?? string.Empty,
                TypBusiness = tenant.TypBusiness,
                Address = tenant.Address,
                City = tenant.City,
                Phone = tenant.Phone,
                Description = tenant.Description,
                LogoUrl = tenant.LogoUrl
            };
        }

        public async Task<PublicTenantDto> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug requerido.");

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);

            if (tenant is null)
                throw new KeyNotFoundException("Salón no encontrado.");

            return new PublicTenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug ?? string.Empty,
                TypBusiness = tenant.TypBusiness,
                Address = tenant.Address,
                City = tenant.City,
                Phone = tenant.Phone,
                Description = tenant.Description,
                LogoUrl = tenant.LogoUrl
            };
        }

        public async Task<string?> UpdateLogoAsync(Guid tenantId, string? logoUrl)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant is null)
                throw new KeyNotFoundException("Tenant no encontrado.");

            tenant.LogoUrl = logoUrl;
            await _context.SaveChangesAsync();

            return tenant.LogoUrl;
        }

        public async Task<string?> GetLogoUrlAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant is null)
                throw new KeyNotFoundException("Tenant no encontrado.");

            return tenant.LogoUrl;
        }
    }
}