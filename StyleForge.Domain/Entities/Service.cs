namespace StyleForge.Domain.Entities;

/// <summary>
/// Servicio ofrecido por el salón (corte, tinte, manicure, etc.).
/// Forma parte del catálogo del tenant y se referencia desde las citas.
/// </summary>
public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Precio del servicio en la moneda local.</summary>
    public decimal Price { get; set; }

    /// <summary>Duración estimada del servicio en minutos.</summary>
    public int DurationMinutes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
