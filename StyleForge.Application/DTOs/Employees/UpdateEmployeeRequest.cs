using System.ComponentModel.DataAnnotations;

namespace StyleForge.Application.DTOs.Employees;

public class UpdateEmployeeRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
