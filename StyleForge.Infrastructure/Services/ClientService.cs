using Microsoft.EntityFrameworkCore;
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

    public async Task<List<ClientDto>> GetAll()
    {
        return await _context.Clients
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email
            })
            .ToListAsync();
    }

    public async Task<ClientDto> Create(CreateClientRequest request)
    {
        var client = new Client
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            TenantId = _current.TenantId!.Value
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
}