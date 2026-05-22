using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Employees;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAll(page, pageSize));
    }

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
