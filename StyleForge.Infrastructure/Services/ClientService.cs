using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs;
using StyleForge.Application.DTOs.Clients;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _current;

    public ClientService(AppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<PagedResult<ClientDto>> GetAll(string? search, int page, int pageSize)
    {
        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.Name.ToLower().Contains(search.ToLower()) ||
                c.Phone.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email
            })
            .ToListAsync();

        return new PagedResult<ClientDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ClientDto> Create(CreateClientRequest request)
    {
        var client = new Client
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            TenantId = _current.TenantId!.Value,
            PasswordHash = request.Password != null
                ? BCrypt.Net.BCrypt.HashPassword(request.Password)
                : null
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Phone = client.Phone,
            Email = client.Email
        };
    }

    public async Task<ClientDto> Update(Guid id, UpdateClientRequest request)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            throw new Exception("Client not found");

        client.Name = request.Name;
        client.Phone = request.Phone;
        client.Email = request.Email;

        await _context.SaveChangesAsync();

        return new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Phone = client.Phone,
            Email = client.Email
        };
    }

    public async Task Delete(Guid id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            throw new Exception("Client not found");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }
}