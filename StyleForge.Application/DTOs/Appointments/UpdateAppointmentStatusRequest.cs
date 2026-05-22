using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Appointments;

public class UpdateAppointmentStatusRequest
{
    [Required]
    [RegularExpression("^(Pending|Confirmed|Cancelled|Completed)$",
        ErrorMessage = "Status must be: Pending, Confirmed, Cancelled or Completed")]
    public string Status { get; set; } = string.Empty;
}
