using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StyleForge.Application.DTOs.Auth;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace StyleForge.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthService(AppDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var tenant = new Tenant
        {
            Name = request.CompanyName,
            Email = request.Email,
            Slug = await GenerateUniqueSlug(request.CompanyName),
            LicenseExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _context.Add(tenant);
        await _context.SaveChangesAsync();

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Admin",
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = _jwt.GenerateToken(user),
            User = Map(user)
        };
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            throw new Exception("Invalid credentials");

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!valid)
            throw new Exception("Invalid credentials");

        return new AuthResponse
        {
            Token = _jwt.GenerateToken(user),
            User = Map(user)
        };
    }

    public async Task<AuthResponse> LoginClient(LoginClientRequest request)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (client == null || client.PasswordHash == null)
            throw new Exception("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, client.PasswordHash))
            throw new Exception("Invalid credentials");

        return new AuthResponse
        {
            Token = _jwt.GenerateClientToken(client),
            User = new UserDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email ?? string.Empty,
                Role = "Client",
                TenantId = client.TenantId
            }
        };
    }

    private async Task<string> GenerateUniqueSlug(string companyName)
    {
        var baseSlug = Regex.Replace(companyName.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "salon";

        var slug = baseSlug;
        var counter = 2;
        while (await _context.Tenants.AnyAsync(t => t.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private UserDto Map(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            TenantId = user.TenantId
        };
    }
}