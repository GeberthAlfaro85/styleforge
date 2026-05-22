namespace StyleForge.Domain.Entities;

/// <summary>
/// Clase base para todas las entidades del sistema.
/// Provee Id único global y TenantId para aislamiento multi-tenant.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Identificador único de la entidad.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifica a qué salón pertenece este registro.
    /// Todos los queries aplican un filtro global por este campo.
    /// </summary>
    public Guid TenantId { get; set; }
}
