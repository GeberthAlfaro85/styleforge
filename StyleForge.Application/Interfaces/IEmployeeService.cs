using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Employees;

namespace StyleForge.Application.Interfaces;

/// <summary>
/// Operaciones CRUD sobre los empleados del salón (usuarios con rol User).
/// Incluye al Admin en el listado porque también puede atender citas.
/// </summary>
public interface IEmployeeService
{
    /// <summary>Lista todos los usuarios del tenant (Admin + empleados).</summary>
    Task<PagedResult<EmployeeDto>> GetAll(int page, int pageSize);

    /// <summary>
    /// Crea un empleado con rol User dentro del tenant del Admin autenticado.
    /// Lanza excepción si el email ya está en uso.
    /// </summary>
    Task<EmployeeDto> Create(CreateEmployeeRequest request);

    /// <summary>Actualiza nombre y email de un empleado.</summary>
    Task<EmployeeDto> Update(Guid id, UpdateEmployeeRequest request);

    /// <summary>Elimina un empleado. Lanza excepción si no existe en el tenant.</summary>
    Task Delete(Guid id);
}
