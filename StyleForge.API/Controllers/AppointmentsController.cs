using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Appointments;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

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

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAll(page, pageSize));
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetMyAppointments(page, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentRequest request)
    {
        return Ok(await _service.Create(request));
    }

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
