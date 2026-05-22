namespace StyleForge.Domain.Entities;

/// <summary>
/// Cita agendada en el salón. Relaciona un cliente, un servicio y un empleado (Staff)
/// en una fecha y hora específica.
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>Cliente que recibe el servicio.</summary>
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    /// <summary>Servicio a realizar en la cita.</summary>
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    /// <summary>Empleado (User) que atenderá la cita.</summary>
    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;

    /// <summary>Fecha y hora de la cita en UTC.</summary>
    public DateTime ScheduledAt { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Estado de la cita. Valores válidos: Pending, Confirmed, Cancelled, Completed.
    /// Solo el Admin puede cambiar el estado.
    /// </summary>
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
