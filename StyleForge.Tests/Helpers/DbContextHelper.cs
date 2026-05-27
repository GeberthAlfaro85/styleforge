using Microsoft.EntityFrameworkCore;
using Moq;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Data;

namespace StyleForge.Tests.Helpers;

public static class DbContextHelper
{
    public static AppDbContext CreateInMemory(Guid? tenantId = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.TenantId).Returns(tenantId);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, currentUser.Object);
    }
}
