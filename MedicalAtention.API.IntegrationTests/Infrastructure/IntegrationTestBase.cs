using MedicalAtention.API.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MedicalAtention.API.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    private readonly CustomWebApplicationFactory _factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> LoginAsAdminAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@medic.com",
            password = "Admin123!"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var token = await LoginAsAdminAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
