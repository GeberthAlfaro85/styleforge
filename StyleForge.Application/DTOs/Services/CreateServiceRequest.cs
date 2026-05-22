using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Services;

public class CreateServiceRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, Range(1, 480)]
    public int DurationMinutes { get; set; }
}
