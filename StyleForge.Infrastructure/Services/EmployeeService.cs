using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Employees;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _current;

    public EmployeeService(AppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<PagedResult<EmployeeDto>> GetAll(int page, int pageSize)
    {
        var query = _context.Users.AsQueryable();

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new EmployeeDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeDto> Create(CreateEmployeeRequest request)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (exists)
            throw new Exception("Email already in use");

        var employee = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            TenantId = _current.TenantId!.Value
        };

        _context.Users.Add(employee);
        await _context.SaveChangesAsync();

        return ToDto(employee);
    }

    public async Task<EmployeeDto> Update(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (employee == null)
            throw new Exception("Employee not found");

        employee.Name = request.Name;
        employee.Email = request.Email;

        await _context.SaveChangesAsync();

        return ToDto(employee);
    }

    public async Task Delete(Guid id)
    {
        var employee = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (employee == null)
            throw new Exception("Employee not found");

        _context.Users.Remove(employee);
        await _context.SaveChangesAsync();
    }

    private static EmployeeDto ToDto(User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role
    };
}
