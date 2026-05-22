using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public Guid? ClientId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public string? Notes { get; set; }
}
