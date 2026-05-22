using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Auth;

public class LoginClientRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
