using System;
using System.Collections.Generic;
using System.Text;

namespace StyleForge.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public UserDto User { get; set; } = new();
}