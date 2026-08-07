using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs.BusinessHours;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services;

public class BusinessHourService : IBusinessHourService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _current;

    public BusinessHourService(AppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<List<BusinessHourDto>> GetAll()
    {
        return await _context.BusinessHours
            .OrderBy(h => h.DayOfWeek)
            .Select(h => ToDto(h))
            .ToListAsync();
    }

    public async Task<List<BusinessHourDto>> Update(UpdateBusinessHoursRequest request)
    {
        if (request.Days.Select(d => d.DayOfWeek).Distinct().Count() != request.Days.Count)
            throw new ArgumentException("No puede haber días repetidos en el horario.");

        foreach (var d in request.Days)
        {
            if (d.IsOpen && (d.OpenTime == null || d.CloseTime == null || d.CloseTime <= d.OpenTime))
                throw new ArgumentException($"Horario inválido para {d.DayOfWeek}.");
        }

        var tenantId = _current.TenantId!.Value;

        var existing = await _context.BusinessHours
            .Where(h => h.TenantId == tenantId)
            .ToListAsync();

        _context.BusinessHours.RemoveRange(existing);

        var updated = request.Days.Select(d => new BusinessHour
        {
            TenantId = tenantId,
            DayOfWeek = d.DayOfWeek,
            IsOpen = d.IsOpen,
            OpenTime = d.IsOpen ? d.OpenTime : null,
            CloseTime = d.IsOpen ? d.CloseTime : null
        }).ToList();

        _context.BusinessHours.AddRange(updated);
        await _context.SaveChangesAsync();

        return updated.OrderBy(h => h.DayOfWeek).Select(ToDto).ToList();
    }

    private static BusinessHourDto ToDto(BusinessHour h) => new()
    {
        DayOfWeek = h.DayOfWeek,
        IsOpen = h.IsOpen,
        OpenTime = h.OpenTime,
        CloseTime = h.CloseTime
    };
}
