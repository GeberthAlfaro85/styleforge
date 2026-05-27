using Microsoft.Extensions.Configuration;
using Moq;
using StyleForge.Application.DTOs.Auth;
using StyleForge.Application.Interfaces;
using StyleForge.Infrastructure.Services;
using StyleForge.Tests.Helpers;

namespace StyleForge.Tests;

public class AuthServiceTests
{
    private AuthService BuildService()
    {
        var db = DbContextHelper.CreateInMemory();

        var config = new Mock<IConfiguration>();
        config.Setup(x => x["Jwt:Key"]).Returns("super-secret-key-para-tests-1234567890");
        config.Setup(x => x["Jwt:Issuer"]).Returns("StyleForge");
        config.Setup(x => x["Jwt:Audience"]).Returns("StyleForgeApp");
        config.Setup(x => x["Jwt:ExpireMinutes"]).Returns("60");

        var jwtService = new JwtService(config.Object);
        return new AuthService(db, jwtService);
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenDataIsValid()
    {
        var svc = BuildService();

        var result = await svc.Register(new RegisterRequest
        {
            Name = "Daniel",
            Email = "daniel@test.com",
            Password = "Password123",
            CompanyName = "Salón Test"
        });

        Assert.NotNull(result.Token);
        Assert.Equal("Admin", result.User.Role);
        Assert.Equal("daniel@test.com", result.User.Email);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var svc = BuildService();

        await svc.Register(new RegisterRequest
        {
            Name = "Daniel",
            Email = "daniel@test.com",
            Password = "Password123",
            CompanyName = "Salón Test"
        });

        var result = await svc.Login(new LoginRequest
        {
            Email = "daniel@test.com",
            Password = "Password123"
        });

        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsWrong()
    {
        var svc = BuildService();

        await svc.Register(new RegisterRequest
        {
            Name = "Daniel",
            Email = "daniel@test.com",
            Password = "Password123",
            CompanyName = "Salón Test"
        });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            svc.Login(new LoginRequest
            {
                Email = "daniel@test.com",
                Password = "WrongPassword"
            }));

        Assert.Contains("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenUserDoesNotExist()
    {
        var svc = BuildService();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            svc.Login(new LoginRequest
            {
                Email = "noexiste@test.com",
                Password = "cualquiera"
            }));

        Assert.Contains("Invalid credentials", ex.Message);
    }
}
