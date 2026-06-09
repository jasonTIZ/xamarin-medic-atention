using MedicalAtention.API.DTOs;
using MedicalAtention.API.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace MedicalAtention.API.IntegrationTests;

public class ConsultationsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var patients = await client.GetFromJsonAsync<List<PatientResponseDto>>("/api/patients");
        var patientId = patients!.First().Id;

        var request = new ConsultationRequestDto
        {
            PatientId = patientId,
            ConsultationDate = DateTime.UtcNow,
            Symptoms = "Fiebre y dolor de cabeza",
            Diagnosis = "Gripe",
            Treatment = "Paracetamol",
            Notes = "Reposo",
            Priority = "medium"
        };

        var response = await client.PostAsJsonAsync("/api/consultations", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var consultation = await response.Content.ReadFromJsonAsync<ConsultationResponseDto>();
        Assert.NotNull(consultation);
        Assert.Equal(patientId, consultation.PatientId);
        Assert.Equal("Gripe", consultation.Diagnosis);
    }

    [Fact]
    public async Task Create_WithInvalidPatient_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = new ConsultationRequestDto
        {
            PatientId = 999999,
            ConsultationDate = DateTime.UtcNow,
            Symptoms = "Dolor",
            Diagnosis = "Sin paciente",
            Priority = "low"
        };

        var response = await client.PostAsJsonAsync("/api/consultations", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutSymptoms_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var patients = await client.GetFromJsonAsync<List<PatientResponseDto>>("/api/patients");

        var request = new ConsultationRequestDto
        {
            PatientId = patients!.First().Id,
            ConsultationDate = DateTime.UtcNow,
            Symptoms = "",
            Diagnosis = "Diagnóstico",
            Priority = "high"
        };

        var response = await client.PostAsJsonAsync("/api/consultations", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidPriority_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var patients = await client.GetFromJsonAsync<List<PatientResponseDto>>("/api/patients");

        var request = new ConsultationRequestDto
        {
            PatientId = patients!.First().Id,
            ConsultationDate = DateTime.UtcNow,
            Symptoms = "Mareo",
            Diagnosis = "Hipotensión",
            Priority = "invalida"
        };

        var response = await client.PostAsJsonAsync("/api/consultations", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
