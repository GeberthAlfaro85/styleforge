using System;
using System.Collections.Generic;
using System.Text;

namespace StyleForge.Application.DTOs.Auth;

public class RegisterRequest
{
    public string CompanyName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}