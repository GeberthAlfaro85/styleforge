using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Tenants;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Lista todos los empleados del salón con paginación. Incluye al Admin.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenant dto)
        {
            var result = await _tenantService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyTenant()
        {
            var tenantId = Guid.Parse(User.FindFirst("tenantId")!.Value); // el claim que uses en tu JWT
            var result = await _tenantService.GetByIdAsync(tenantId);
            return Ok(result);
        }
    }
}
