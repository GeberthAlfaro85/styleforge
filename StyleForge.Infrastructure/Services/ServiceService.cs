using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Services;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _current;

    public ServiceService(AppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<PagedResult<ServiceDto>> GetAll(int page, int pageSize)
    {
        var query = _context.Services.AsQueryable();

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            })
            .ToListAsync();

        return new PagedResult<ServiceDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceDto> Create(CreateServiceRequest request)
    {
        var service = new Service
        {
            Name = request.Name,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            TenantId = _current.TenantId!.Value
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes
        };
    }

    public async Task<ServiceDto> Update(Guid id, CreateServiceRequest request)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            throw new Exception("Service not found");

        service.Name = request.Name;
        service.Price = request.Price;
        service.DurationMinutes = request.DurationMinutes;

        await _context.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes
        };
    }

    public async Task Delete(Guid id)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            throw new Exception("Service not found");

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}
