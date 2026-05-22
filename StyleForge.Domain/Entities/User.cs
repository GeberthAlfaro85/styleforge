namespace StyleForge.Domain.Entities;

/// <summary>
/// Usuario interno del salón. Puede ser Admin (dueño) o User (empleado).
/// Se autentica con POST /api/auth/login.
/// No confundir con Client, que es el cliente final del salón.
/// </summary>
public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Admin = dueño del salón. User = empleado.</summary>
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
