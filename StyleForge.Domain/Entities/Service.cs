namespace StyleForge.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
