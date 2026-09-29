using StyleForge.Application.Interfaces;

public class AdminCurrentUserService : ICurrentUserService
{
    public Guid? TenantId => null;
    public Guid? UserId => null;
    public string? Role => "SuperAdmin";
}