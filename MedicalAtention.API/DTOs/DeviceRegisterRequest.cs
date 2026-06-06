using System.Text.Json.Serialization;

namespace MedicalAtention.API.DTOs;

public class DeviceRegisterRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }
}
