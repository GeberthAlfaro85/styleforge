using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Clients;

public class UpdateClientRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(7)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }
}
