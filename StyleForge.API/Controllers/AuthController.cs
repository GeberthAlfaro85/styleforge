using Microsoft.AspNetCore.Mvc;
using StyleForge.Application.DTOs.Auth;
using StyleForge.Application.Interfaces;

namespace StyleForge.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.Register(request);
        return Ok(result);
    }

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