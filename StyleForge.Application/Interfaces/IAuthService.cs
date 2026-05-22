using System;
using System.Collections.Generic;
using System.Text;
using StyleForge.Application.DTOs.Auth;

namespace StyleForge.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterRequest request);
    Task<AuthResponse> Login(LoginRequest request);
    Task<AuthResponse> LoginClient(LoginClientRequest request);
}