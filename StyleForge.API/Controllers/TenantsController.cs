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
        private readonly ICurrentUserService _currentUser;

        public TenantsController(ITenantService tenantService, ICurrentUserService currentUser)
        {
            _tenantService = tenantService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Actualiza los datos del salón del Admin autenticado. Solo puede editar su propio tenant.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenant dto)
        {
            if (id != _currentUser.TenantId)
                return Forbid();

            var result = await _tenantService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyTenant()
        {
            var tenantId = _currentUser.TenantId;
            if (tenantId == null) return Unauthorized();

            var result = await _tenantService.GetByIdAsync(tenantId.Value);
            return Ok(result);
        }

        /// <summary>
        /// Consulta los datos públicos de un salón por su slug (ej. para su página pública de reservas).
        /// No requiere autenticación. No incluye email ni datos de licencia.
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            return Ok(await _tenantService.GetBySlugAsync(slug));
        }
    }
}
