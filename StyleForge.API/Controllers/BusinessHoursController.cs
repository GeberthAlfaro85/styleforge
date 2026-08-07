using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.BusinessHours;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Horario de atención del salón (días abiertos y horas de trabajo).
/// </summary>
[Authorize]
[ApiController]
[Route("api/business-hours")]
public class BusinessHoursController : ControllerBase
{
    private readonly IBusinessHourService _service;

    public BusinessHoursController(IBusinessHourService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista el horario de atención del salón. Accesible para cualquier usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    /// <summary>
    /// Reemplaza el horario de atención del salón. Solo Admin.
    /// </summary>
    /// <response code="200">Horario actualizado.</response>
    /// <response code="400">Días repetidos o rango de horas inválido.</response>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(UpdateBusinessHoursRequest request)
    {
        return Ok(await _service.Update(request));
    }
}
