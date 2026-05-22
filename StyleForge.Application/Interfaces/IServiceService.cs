using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Services;

namespace StyleForge.Application.Interfaces;

/// <summary>
/// Gestión del catálogo de servicios del salón.
/// </summary>
public interface IServiceService
{
    /// <summary>Lista todos los servicios del tenant ordenados por nombre.</summary>
    Task<PagedResult<ServiceDto>> GetAll(int page, int pageSize);

    /// <summary>Crea un servicio en el catálogo del tenant actual.</summary>
    Task<ServiceDto> Create(CreateServiceRequest request);

    /// <summary>Actualiza nombre, precio y duración de un servicio.</summary>
    Task<ServiceDto> Update(Guid id, CreateServiceRequest request);

    /// <summary>Elimina un servicio. Lanza excepción si no existe.</summary>
    Task Delete(Guid id);
}
