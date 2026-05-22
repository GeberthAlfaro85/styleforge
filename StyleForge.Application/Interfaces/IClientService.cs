using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Clients;

namespace StyleForge.Application.Interfaces;

/// <summary>
/// Operaciones CRUD sobre los clientes del salón.
/// El aislamiento por tenant lo aplica el global query filter de AppDbContext.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Lista clientes del tenant actual. Filtra por nombre o teléfono si se provee <paramref name="search"/>.
    /// </summary>
    Task<PagedResult<ClientDto>> GetAll(string? search, int page, int pageSize);

    /// <summary>Crea un cliente en el tenant del usuario autenticado.</summary>
    Task<ClientDto> Create(CreateClientRequest request);

    /// <summary>Actualiza nombre, teléfono y email de un cliente.</summary>
    Task<ClientDto> Update(Guid id, UpdateClientRequest request);

    /// <summary>Elimina un cliente. Lanza excepción si no existe en el tenant.</summary>
    Task Delete(Guid id);
}
