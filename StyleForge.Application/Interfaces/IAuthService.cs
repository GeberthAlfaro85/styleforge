using StyleForge.Application.DTOs.Auth;

namespace StyleForge.Application.Interfaces;

/// <summary>
/// Maneja registro y autenticación de usuarios y clientes.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo salón: crea el Tenant y el primer usuario Admin.
    /// </summary>
    Task<AuthResponse> Register(RegisterRequest request);

    /// <summary>
    /// Autentica a un Admin o empleado (rol User). Busca por email sin filtro de tenant
    /// porque el JWT aún no existe en ese momento.
    /// </summary>
    Task<AuthResponse> Login(LoginRequest request);

    /// <summary>
    /// Autentica a un cliente del salón. Requiere que el cliente tenga contraseña asignada.
    /// Genera un token con rol Client y el TenantId del salón al que pertenece.
    /// </summary>
    Task<AuthResponse> LoginClient(LoginClientRequest request);
}
