using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private const long MaxLogoBytes = 3 * 1024 * 1024;

        private static readonly Dictionary<string, string> AllowedLogoTypes = new()
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
        };

        private readonly ITenantService _tenantService;
        private readonly ICurrentUserService _currentUser;
        private readonly IWebHostEnvironment _env;

        public TenantsController(ITenantService tenantService, ICurrentUserService currentUser, IWebHostEnvironment env)
        {
            _tenantService = tenantService;
            _currentUser = currentUser;
            _env = env;
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

        /// <summary>
        /// Sube (o reemplaza) el logo del salón. Solo el Admin dueño del tenant. Máx. 3 MB, JPG/PNG/WEBP.
        /// </summary>
        [HttpPost("{id}/logo")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(MaxLogoBytes)]
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile? file)
        {
            if (id != _currentUser.TenantId)
                return Forbid();

            if (file is null || file.Length == 0)
                return BadRequest(new { message = "Debes seleccionar una imagen." });

            if (file.Length > MaxLogoBytes)
                return BadRequest(new { message = "La imagen no puede pesar más de 3 MB." });

            if (!AllowedLogoTypes.TryGetValue(file.ContentType, out var ext))
                return BadRequest(new { message = "Formato no soportado. Usa JPG, PNG o WEBP." });

            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "logos");
            Directory.CreateDirectory(uploadsDir);

            DeleteExistingLogoFiles(uploadsDir, id);

            var fileName = $"{id}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/logos/{fileName}";
            var savedUrl = await _tenantService.UpdateLogoAsync(id, relativeUrl);

            return Ok(new { logoUrl = savedUrl });
        }

        /// <summary>Quita el logo actual del salón. Solo el Admin dueño del tenant.</summary>
        [HttpDelete("{id}/logo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLogo(Guid id)
        {
            if (id != _currentUser.TenantId)
                return Forbid();

            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "logos");
            DeleteExistingLogoFiles(uploadsDir, id);

            await _tenantService.UpdateLogoAsync(id, null);
            return NoContent();
        }

        private static void DeleteExistingLogoFiles(string uploadsDir, Guid tenantId)
        {
            if (!Directory.Exists(uploadsDir)) return;

            foreach (var existing in Directory.GetFiles(uploadsDir, $"{tenantId}.*"))
            {
                System.IO.File.Delete(existing);
            }
        }
    }
}
