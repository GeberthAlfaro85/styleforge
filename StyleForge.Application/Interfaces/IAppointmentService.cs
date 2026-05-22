using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Appointments;

namespace StyleForge.Application.Interfaces;

/// <summary>
/// Gestión de citas del salón.
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Lista todas las citas del tenant ordenadas por fecha descendente.
    /// Para uso de Admin y empleados.
    /// </summary>
    Task<PagedResult<AppointmentDto>> GetAll(int page, int pageSize);

    /// <summary>
    /// Lista solo las citas del cliente autenticado.
    /// El clientId se extrae del claim NameIdentifier del JWT.
    /// </summary>
    Task<PagedResult<AppointmentDto>> GetMyAppointments(int page, int pageSize);

    /// <summary>
    /// Crea una cita. Si el rol del usuario autenticado es Client, el ClientId
    /// se toma automáticamente del token. Si es Admin o User, debe venir en el request.
    /// </summary>
    Task<AppointmentDto> Create(CreateAppointmentRequest request);

    /// <summary>
    /// Cambia el estado de una cita. Estados válidos: Pending, Confirmed, Cancelled, Completed.
    /// Solo Admin puede ejecutar esta operación.
    /// </summary>
    Task<AppointmentDto> UpdateStatus(Guid id, UpdateAppointmentStatusRequest request);
}
