using StyleForge.Application.DTOs.Clients;

namespace StyleForge.Application.Interfaces;

public interface IClientService
{
    Task<List<ClientDto>> GetAll(string? search);
    Task<ClientDto> Create(CreateClientRequest request);
    Task<ClientDto> Update(Guid id, UpdateClientRequest request);
    Task Delete(Guid id);
}