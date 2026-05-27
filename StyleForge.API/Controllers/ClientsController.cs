using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Clients;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Gestión de clientes del salón. Los clientes pertenecen al tenant del Admin autenticado.
/// </summary>
[Authorize]
[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista los clientes del salón con paginación. Acepta búsqueda por nombre o teléfono.
    /// </summary>
    /// <param name="search">Texto a buscar en nombre o teléfono.</param>
    /// <param name="page">Número de página (inicia en 1).</param>
    /// <param name="pageSize">Cantidad de resultados por página. Default: 20.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAll(search, page, pageSize));
    }

    /// <summary>
    /// Crea un nuevo cliente en el salón. Solo Admin.
    /// Si se incluye <c>password</c>, el cliente podrá hacer login con <c>POST /api/auth/login-client</c>.
    /// </summary>
    /// <response code="200">Cliente creado correctamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateClientRequest request)
    {
        return Ok(await _service.Create(request));
    }

    /// <summary>
    /// Actualiza los datos de un cliente existente. Solo Admin.
    /// </summary>
    /// <param name="id">ID del cliente a actualizar.</param>
    /// <param name="request">Datos actualizados del cliente.</param>
    /// <response code="200">Cliente actualizado.</response>
    /// <response code="404">Cliente no encontrado en este tenant.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateClientRequest request)
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
    /// Elimina un cliente del salón. Solo Admin.
    /// </summary>
    /// <param name="id">ID del cliente a eliminar.</param>
    /// <response code="204">Cliente eliminado.</response>
    /// <response code="404">Cliente no encontrado.</response>
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
