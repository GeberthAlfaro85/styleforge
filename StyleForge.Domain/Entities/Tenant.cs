namespace StyleForge.Domain.Entities;

/// <summary>
/// Representa un salón de belleza registrado en el sistema.
/// Se crea automáticamente al hacer POST /api/auth/register.
/// Cada Tenant tiene sus propios usuarios, clientes, servicios y citas aislados.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nombre comercial del salón.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Email del salón (mismo del usuario Admin al registrarse).</summary>
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
