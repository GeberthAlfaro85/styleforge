using Moq;
using StyleForge.Application.DTOs.Appointments;
using StyleForge.Application.Interfaces;
using StyleForge.Domain.Entities;
using StyleForge.Infrastructure.Services;
using StyleForge.Tests.Helpers;

namespace StyleForge.Tests;

public class AppointmentServiceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _staffId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();

    private (AppointmentService service, Mock<ICurrentUserService> currentUser) BuildService()
    {
        var db = DbContextHelper.CreateInMemory(_tenantId);

        // Servicio de 60 minutos
        db.Services.Add(new Service
        {
            Id = _serviceId,
            TenantId = _tenantId,
            Name = "Corte",
            Price = 100,
            DurationMinutes = 60
        });

        // Empleado
        db.Users.Add(new User
        {
            Id = _staffId,
            TenantId = _tenantId,
            Name = "Empleado Test",
            Email = "empleado@test.com",
            PasswordHash = "hash",
            Role = "User"
        });

        // Cliente
        db.Clients.Add(new Client
        {
            Id = _clientId,
            TenantId = _tenantId,
            Name = "Cliente Test"
        });

        db.SaveChanges();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.TenantId).Returns(_tenantId);
        currentUser.Setup(x => x.UserId).Returns(_clientId);
        currentUser.Setup(x => x.Role).Returns("Admin");

        var svc = new AppointmentService(db, currentUser.Object);
        return (svc, currentUser);
    }

    [Fact]
    public async Task Create_ShouldSucceed_WhenNoConflict()
    {
        var (svc, _) = BuildService();

        var request = new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var result = await svc.Create(request);

        Assert.NotNull(result);
        Assert.Equal(_staffId, result.StaffId);
    }

    [Fact]
    public async Task Create_ShouldThrow_WhenEmployeeHasConflict()
    {
        var (svc, _) = BuildService();
        var scheduledAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        // Primera cita a las 10:00 (dura 60 min → hasta las 11:00)
        await svc.Create(new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = scheduledAt
        });

        // Segunda cita a las 10:30 → conflicto
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.Create(new CreateAppointmentRequest
            {
                ClientId = _clientId,
                ServiceId = _serviceId,
                StaffId = _staffId,
                ScheduledAt = scheduledAt.AddMinutes(30)
            }));

        Assert.Contains("horario", ex.Message);
    }

    [Fact]
    public async Task Create_ShouldSucceed_WhenAppointmentIsAfterPrevious()
    {
        var (svc, _) = BuildService();
        var scheduledAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        // Primera cita a las 10:00 (dura 60 min → hasta las 11:00)
        await svc.Create(new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = scheduledAt
        });

        // Segunda cita a las 11:00 → sin conflicto
        var result = await svc.Create(new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = scheduledAt.AddMinutes(60)
        });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Create_ShouldSucceed_WhenCancelledAppointmentExistsAtSameTime()
    {
        var (svc, _) = BuildService();
        var scheduledAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        // Crear y luego cancelar la primera cita
        var first = await svc.Create(new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = scheduledAt
        });

        await svc.UpdateStatus(first.Id, new UpdateAppointmentStatusRequest { Status = "Cancelled" });

        // Misma hora → debe permitirse porque la anterior fue cancelada
        var result = await svc.Create(new CreateAppointmentRequest
        {
            ClientId = _clientId,
            ServiceId = _serviceId,
            StaffId = _staffId,
            ScheduledAt = scheduledAt
        });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldThrow_WhenAppointmentNotFound()
    {
        var (svc, _) = BuildService();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateStatus(Guid.NewGuid(), new UpdateAppointmentStatusRequest { Status = "Confirmed" }));

        Assert.Contains("not found", ex.Message);
    }
}
