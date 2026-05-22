using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Clients;

namespace StyleForge.Application.Interfaces;

public interface IClientService
{
    Task<PagedResult<ClientDto>> GetAll(string? search, int page, int pageSize);
    Task<ClientDto> Create(CreateClientRequest request);
    Task<ClientDto> Update(Guid id, UpdateClientRequest request);
    Task Delete(Guid id);
}