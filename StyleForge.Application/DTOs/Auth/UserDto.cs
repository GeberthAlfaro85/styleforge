using System;
using System.Collections.Generic;
using System.Text;

namespace StyleForge.Application.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public Guid TenantId { get; set; }
}