using Microsoft.EntityFrameworkCore;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;

namespace StyleForge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser
    ) : base(options)
    {
        _currentUser = currentUser;
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Client> Clients => Set<Client>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasQueryFilter(x =>
                _currentUser.TenantId == null ||
                x.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<Client>()
            .HasQueryFilter(x =>
                _currentUser.TenantId == null ||
                x.TenantId == _currentUser.TenantId);
    }
}