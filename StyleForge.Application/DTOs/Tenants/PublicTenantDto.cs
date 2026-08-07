namespace StyleForge.Application.DTOs.Tenants;

/// <summary>
/// Datos del salón visibles públicamente (ej. página pública por slug). No incluye Email ni datos de licencia.
/// </summary>
public class PublicTenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string TypBusiness { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
