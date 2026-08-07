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

    /// <summary>
    /// Identificador único para la URL pública del salón (ej. "salon-de-ana").
    /// Se genera automáticamente al registrarse; el Admin puede cambiarlo después.
    /// </summary>
    public string? Slug { get; set; }

    public string TypBusiness { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Email del salón (mismo del usuario Admin al registrarse).</summary>
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de expiración de la licencia. Null = sin expiración.
    /// Al registrarse se otorgan 30 días de prueba automáticamente.
    /// </summary>
    public DateTime? LicenseExpiresAt { get; set; }

    /// <summary>Indica si la licencia está activa (no ha expirado).</summary>
    public bool IsLicenseActive => LicenseExpiresAt == null || LicenseExpiresAt > DateTime.UtcNow;
}
