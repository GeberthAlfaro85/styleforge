namespace StyleForge.Domain.Entities;

/// <summary>
/// Cliente final del salón. Es quien recibe los servicios y agenda citas.
/// Si tiene PasswordHash asignado, puede autenticarse con POST /api/auth/login-client
/// y ver sus propias citas desde la app.
/// </summary>
public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    /// <summary>
    /// Opcional. Si se asigna, el cliente puede hacer login.
    /// Se almacena hasheado con BCrypt.
    /// </summary>
    public string? PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
