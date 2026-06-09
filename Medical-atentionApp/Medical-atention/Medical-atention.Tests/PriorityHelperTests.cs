using Medical_atention.Helpers;
using Medical_atention.Models;

namespace Medical_atention.Tests;

public class PriorityHelperTests
{
    [Theory]
    [InlineData(PriorityLevel.Urgent, "Urgente")]
    [InlineData(PriorityLevel.High, "Alta")]
    [InlineData(PriorityLevel.Medium, "Media")]
    [InlineData(PriorityLevel.Low, "Baja")]
    public void GetDisplayName_ReturnsSpanishLabel(PriorityLevel level, string expected)
    {
        Assert.Equal(expected, PriorityHelper.GetDisplayName(level));
    }

    [Fact]
    public void FormatTimeSinceLastConsultation_WhenNull_ReturnsNoPreviousConsultation()
    {
        var text = PriorityHelper.FormatTimeSinceLastConsultation(null);
        Assert.Equal("Sin consultas previas", text);
    }

    [Fact]
    public void FormatTimeSinceLastConsultation_WhenRecent_ReturnsMinutesLabel()
    {
        var recent = DateTime.UtcNow.AddMinutes(-10);
        var text = PriorityHelper.FormatTimeSinceLastConsultation(recent);
        Assert.StartsWith("Última consulta: hace", text);
        Assert.Contains("min", text);
    }

    [Fact]
    public void AllLevels_ContainsFourPriorityLevels()
    {
        Assert.Equal(4, PriorityHelper.AllLevels.Length);
        Assert.Contains(PriorityLevel.Urgent, PriorityHelper.AllLevels);
        Assert.Contains(PriorityLevel.Low, PriorityHelper.AllLevels);
    }
}
