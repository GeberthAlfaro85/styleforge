using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Services;

namespace StyleForge.Application.Interfaces;

public interface IServiceService
{
    Task<PagedResult<ServiceDto>> GetAll(int page, int pageSize);
    Task<ServiceDto> Create(CreateServiceRequest request);
    Task<ServiceDto> Update(Guid id, CreateServiceRequest request);
    Task Delete(Guid id);
}
