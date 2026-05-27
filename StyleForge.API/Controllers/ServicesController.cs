using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Services;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Catálogo de servicios del salón (cortes, tintes, tratamientos, etc.).
/// Cualquier usuario autenticado puede consultar. Solo Admin puede modificar.
/// </summary>
[Authorize]
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _service;

    public ServicesController(IServiceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos los servicios disponibles en el salón con paginación.
    /// Accesible para Admin, empleados y clientes autenticados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAll(page, pageSize));
    }

    /// <summary>
    /// Crea un nuevo servicio en el catálogo del salón. Solo Admin.
    /// </summary>
    /// <response code="200">Servicio creado.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateServiceRequest request)
    {
        return Ok(await _service.Create(request));
    }

    /// <summary>
    /// Actualiza un servicio existente. Solo Admin.
    /// </summary>
    /// <param name="id">ID del servicio.</param>
    /// <param name="request">Datos actualizados del servicio.</param>
    /// <response code="200">Servicio actualizado.</response>
    /// <response code="404">Servicio no encontrado.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, CreateServiceRequest request)
    {
        try
        {
            return Ok(await _service.Update(id, request));
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un servicio del catálogo. Solo Admin.
    /// </summary>
    /// <param name="id">ID del servicio.</param>
    /// <response code="204">Servicio eliminado.</response>
    /// <response code="404">Servicio no encontrado.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.Delete(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
