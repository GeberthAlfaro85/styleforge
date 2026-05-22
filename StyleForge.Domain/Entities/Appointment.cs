namespace StyleForge.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
