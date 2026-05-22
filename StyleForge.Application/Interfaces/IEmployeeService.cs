using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Employees;

namespace StyleForge.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetAll(int page, int pageSize);
    Task<EmployeeDto> Create(CreateEmployeeRequest request);
    Task<EmployeeDto> Update(Guid id, UpdateEmployeeRequest request);
    Task Delete(Guid id);
}
