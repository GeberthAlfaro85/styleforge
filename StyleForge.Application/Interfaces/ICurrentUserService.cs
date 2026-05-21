using System;
using System.Collections.Generic;
using System.Text;

namespace StyleForge.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    string? Role { get; }
}
