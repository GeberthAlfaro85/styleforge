using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Employees;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Gestión de empleados del salón. Solo accesible para Admin.
/// Los empleados se crean con rol User dentro del mismo tenant del Admin.
/// Para hacer login usan <c>POST /api/auth/login</c>.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos los empleados del salón con paginación. Incluye al Admin.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAll(page, pageSize));
    }

    /// <summary>
    /// Crea un nuevo empleado. Se le asigna rol <c>User</c> y queda vinculado al tenant del Admin.
    /// </summary>
    /// <response code="200">Empleado creado.</response>
    /// <response code="400">El email ya está en uso dentro del sistema.</response>
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequest request)
    {
        try
        {
            return Ok(await _service.Create(request));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza nombre y email de un empleado.
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <param name="request">Datos actualizados del empleado.</param>
    /// <response code="200">Empleado actualizado.</response>
    /// <response code="404">Empleado no encontrado en este tenant.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request)
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
    /// Elimina un empleado del salón.
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <response code="204">Empleado eliminado.</response>
    /// <response code="404">Empleado no encontrado.</response>
    [HttpDelete("{id}")]
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
