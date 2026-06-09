using Medical_atention.Helpers;

namespace Medical_atention.Tests;

public class StringNormalizationHelperTests
{
    [Theory]
    [InlineData("Jose", "José")]
    [InlineData("Maria", "María")]
    [InlineData("Andres", "Andrés")]
    [InlineData("Oscar", "Óscar")]
    [InlineData("Angel", "Ángel")]
    [InlineData("JOSE", "josé")]
    public void ContainsNormalized_AccentInsensitive_Matches(string query, string storedName)
    {
        Assert.True(StringNormalizationHelper.ContainsNormalized(storedName, query));
    }

    [Fact]
    public void NormalizeForSearch_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, StringNormalizationHelper.NormalizeForSearch("   "));
    }

    [Fact]
    public void ContainsNormalized_NoMatch_ReturnsFalse()
    {
        Assert.False(StringNormalizationHelper.ContainsNormalized("Carlos Ramírez", "xyz"));
    }

    [Fact]
    public void ContainsNormalized_EmptyQuery_ReturnsTrue()
    {
        Assert.True(StringNormalizationHelper.ContainsNormalized("José Martínez", string.Empty));
    }
}
