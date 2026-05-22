using StyleForge.Application.DTOs.Clients;

namespace StyleForge.Application.Interfaces;

public interface IClientService
{
    Task<List<ClientDto>> GetAll();
    Task<ClientDto> Create(CreateClientRequest request);
}