using MedicalAtention.API.DTOs;
using MedicalAtention.API.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MedicalAtention.API.IntegrationTests;

public class PatientsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/patients");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithValidToken_ReturnsPatients()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/patients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patients = await response.Content.ReadFromJsonAsync<List<PatientResponseDto>>();
        Assert.NotNull(patients);
        Assert.NotEmpty(patients);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new PatientRequestDto
        {
            Name = "Pedro",
            LastName = "Sánchez",
            IdentificationNumber = "1098765432",
            DateOfBirth = new DateTime(1990, 5, 10),
            Gender = "Masculino"
        };

        var response = await client.PostAsJsonAsync("/api/patients", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var patient = await response.Content.ReadFromJsonAsync<PatientResponseDto>();
        Assert.NotNull(patient);
        Assert.Equal("Pedro", patient.Name);
        Assert.Equal("1098765432", patient.IdentificationNumber);
    }

    [Fact]
    public async Task Create_WithDuplicateIdentification_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        var cedula = Random.Shared.Next(100_000_000, 999_999_999).ToString();
        var request = new PatientRequestDto
        {
            Name = "Duplicado",
            LastName = "Test",
            IdentificationNumber = cedula,
            DateOfBirth = new DateTime(1988, 1, 1),
            Gender = "Masculino"
        };

        var first = await client.PostAsJsonAsync("/api/patients", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/patients", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidIdentification_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new PatientRequestDto
        {
            Name = "Ana",
            LastName = "Pérez",
            IdentificationNumber = "ABC",
            DateOfBirth = new DateTime(1992, 2, 2),
            Gender = "Femenino"
        };

        var response = await client.PostAsJsonAsync("/api/patients", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/patients/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByPrioritySort_ReturnsOrderedList()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/patients?sort=priority");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patients = await response.Content.ReadFromJsonAsync<List<PatientResponseDto>>();
        Assert.NotNull(patients);
        Assert.True(patients.Count >= 2);
    }
}
