using Medical_atention.Helpers;
using Medical_atention.Models;
using Medical_atention.Models.Entities;

namespace Medical_atention.Tests;

public class PatientMapperTests
{
    [Fact]
    public void ToEntity_FromDomain_PreservesCoreFields()
    {
        var patient = new Patient
        {
            Id = 7,
            FirstName = "María",
            LastName = "González",
            DocumentNumber = "123456789",
            DateOfBirth = new DateTime(1990, 3, 15),
            Gender = "Femenino",
            Priority = (int)PriorityLevel.High
        };

        var entity = PatientMapper.ToEntity(patient);

        Assert.Equal(7, entity.Id);
        Assert.Equal("María", entity.FirstName);
        Assert.Equal("González", entity.LastName);
        Assert.Equal("123456789", entity.DocumentNumber);
        Assert.Equal((int)PriorityLevel.High, entity.Priority);
    }

    [Theory]
    [InlineData("urgent", PriorityLevel.Urgent)]
    [InlineData("high", PriorityLevel.High)]
    [InlineData("medium", PriorityLevel.Medium)]
    [InlineData("low", PriorityLevel.Low)]
    [InlineData(null, PriorityLevel.Medium)]
    public void ParsePriority_ReturnsExpectedLevel(string input, PriorityLevel expected)
    {
        var result = (PriorityLevel)PatientMapper.ParsePriority(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RoundTrip_EntityToDomainToEntity_KeepsIdentity()
    {
        var original = new PatientEntity
        {
            Id = 3,
            FirstName = "José",
            LastName = "Martínez",
            DocumentNumber = "789123456",
            DateOfBirth = new DateTime(1985, 8, 20),
            Gender = "Masculino",
            Priority = (int)PriorityLevel.Urgent
        };

        var domain = PatientMapper.ToDomain(original);
        var roundTrip = PatientMapper.ToEntity(domain);

        Assert.Equal(original.Id, roundTrip.Id);
        Assert.Equal(original.FirstName, roundTrip.FirstName);
        Assert.Equal(original.DocumentNumber, roundTrip.DocumentNumber);
        Assert.Equal(original.Priority, roundTrip.Priority);
    }
}
