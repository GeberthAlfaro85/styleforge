using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StyleForge.Application.DTOs.Auth;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

/// <summary>
/// Autenticación de usuarios. No requiere token para ningún endpoint de este controller.
/// </summary>
[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Registra un nuevo salón. Crea automáticamente un Tenant y un usuario Admin.
    /// </summary>
    /// <response code="200">Retorna el token JWT y los datos del usuario creado.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.Register(request);
        return Ok(result);
    }

    /// <summary>
    /// Login para Admin y empleados (rol User). Retorna un token JWT.
    /// </summary>
    /// <response code="200">Token JWT válido.</response>
    /// <response code="401">Credenciales incorrectas.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            return Ok(await _auth.Login(request));
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login exclusivo para clientes del salón. Retorna un token JWT con rol Client.
    /// El cliente debe haber sido creado previamente por el Admin con contraseña.
    /// </summary>
    /// <response code="200">Token JWT con rol Client.</response>
    /// <response code="401">Credenciales incorrectas o cliente sin contraseña asignada.</response>
    [HttpPost("login-client")]
    public async Task<IActionResult> LoginClient(LoginClientRequest request)
    {
        try
        {
            return Ok(await _auth.LoginClient(request));
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
