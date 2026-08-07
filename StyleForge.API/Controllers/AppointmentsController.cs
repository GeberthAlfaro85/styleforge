using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Appointments;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Gestión de citas del salón.
/// Admin y empleados ven todas las citas. Los clientes solo ven las suyas.
/// </summary>
[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas las citas del salón ordenadas por fecha descendente. Solo Admin y empleados.
    /// </summary>
    /// <param name="staffId">Filtra solo las citas asignadas a ese empleado.</param>
    [HttpGet]
    [Authorize(Roles = "Admin,User,Client")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? staffId = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return Ok(await _service.GetAll(page, pageSize, staffId));
    }

    /// <summary>
    /// Lista las citas del cliente autenticado. Solo rol Client.
    /// El filtro por cliente se aplica automáticamente desde el token JWT.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return Ok(await _service.GetMyAppointments(page, pageSize));
    }

    /// <summary>
    /// Crea una nueva cita. Accesible para Admin, empleados y clientes.
    /// Si quien crea es un Cliente, el <c>clientId</c> se toma automáticamente del token JWT y no es necesario enviarlo.
    /// Si quien crea es un Admin o empleado, el <c>clientId</c> es obligatorio.
    /// </summary>
    /// <response code="200">Cita creada con estado <c>Pending</c>.</response>
    /// <response code="400">Falta el clientId cuando lo crea un Admin o empleado.</response>
    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentRequest request)
    {
        return Ok(await _service.Create(request));
    }

    /// <summary>
    /// Actualiza el estado de una cita. Solo Admin.
    /// Estados válidos: <c>Pending</c>, <c>Confirmed</c>, <c>Cancelled</c>, <c>Completed</c>.
    /// </summary>
    /// <param name="id">ID de la cita.</param>
    /// <param name="request">Nuevo estado de la cita.</param>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="404">Cita no encontrada.</response>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateAppointmentStatusRequest request)
    {
        try
        {
            return Ok(await _service.UpdateStatus(id, request));
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
