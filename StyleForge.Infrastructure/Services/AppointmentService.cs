using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Appointments;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _current;

    public AppointmentService(AppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<PagedResult<AppointmentDto>> GetAll(int page, int pageSize)
    {
        var query = _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Staff);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => ToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AppointmentDto>> GetMyAppointments(int page, int pageSize)
    {
        var clientId = _current.UserId!.Value;

        var query = _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.ClientId == clientId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => ToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AppointmentDto> Create(CreateAppointmentRequest request)
    {
        var clientId = _current.Role == "Client"
            ? _current.UserId!.Value
            : request.ClientId ?? throw new Exception("ClientId is required");

        var appointment = new Appointment
        {
            ClientId = clientId,
            ServiceId = request.ServiceId,
            StaffId = request.StaffId,
            ScheduledAt = request.ScheduledAt.ToUniversalTime(),
            Notes = request.Notes,
            TenantId = _current.TenantId!.Value
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        await _context.Entry(appointment).Reference(a => a.Client).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Service).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Staff).LoadAsync();

        return ToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateStatus(Guid id, UpdateAppointmentStatusRequest request)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new Exception("Appointment not found");

        appointment.Status = request.Status;
        await _context.SaveChangesAsync();

        return ToDto(appointment);
    }

    private static AppointmentDto ToDto(Appointment a) => new()
    {
        Id = a.Id,
        ClientId = a.ClientId,
        ClientName = a.Client.Name,
        ServiceId = a.ServiceId,
        ServiceName = a.Service.Name,
        StaffId = a.StaffId,
        StaffName = a.Staff.Name,
        ScheduledAt = a.ScheduledAt,
        Notes = a.Notes,
        Status = a.Status
    };
}
