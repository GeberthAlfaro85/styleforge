namespace StyleForge.Application.DTOs.Clients;

public class CreateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}