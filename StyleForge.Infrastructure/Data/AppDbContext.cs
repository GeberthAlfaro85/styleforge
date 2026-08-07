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
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();

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

        modelBuilder.Entity<Service>()
            .HasQueryFilter(x =>
                _currentUser.TenantId == null ||
                x.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(x =>
                _currentUser.TenantId == null ||
                x.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<BusinessHour>()
            .HasQueryFilter(x =>
                _currentUser.TenantId == null ||
                x.TenantId == _currentUser.TenantId);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<BusinessHour>()
            .HasIndex(h => new { h.TenantId, h.DayOfWeek })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Staff)
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}