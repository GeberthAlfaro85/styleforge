using StyleForge.Application.DTOs.BusinessHours;

namespace StyleForge.Application.Interfaces;

public interface IBusinessHourService
{
    Task<List<BusinessHourDto>> GetAll();
    Task<List<BusinessHourDto>> Update(UpdateBusinessHoursRequest request);
}
