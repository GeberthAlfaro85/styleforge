using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Appointments;

namespace StyleForge.Application.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetAll(int page, int pageSize);
    Task<PagedResult<AppointmentDto>> GetMyAppointments(int page, int pageSize);
    Task<AppointmentDto> Create(CreateAppointmentRequest request);
    Task<AppointmentDto> UpdateStatus(Guid id, UpdateAppointmentStatusRequest request);
}
