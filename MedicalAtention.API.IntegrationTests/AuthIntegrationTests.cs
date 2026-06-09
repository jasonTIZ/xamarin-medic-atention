using MedicalAtention.API.DTOs;
using MedicalAtention.API.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MedicalAtention.API.IntegrationTests;

public class AuthIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@medic.com",
            password = "Admin123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("Administrador", body.User.Name);
        Assert.Equal("Admin", body.User.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@medic.com",
            password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "noexiste@medic.com",
            password = "Admin123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
