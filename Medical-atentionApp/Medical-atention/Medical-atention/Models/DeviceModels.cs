using System.Collections.Generic;
using Newtonsoft.Json;

namespace Medical_atention.Models
{
    // Cuerpo de POST /api/devices/register → { token, platform, user_id }
    public class DeviceRegisterRequest
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }

    // Mensaje push normalizado que se pasa a la capa de UI.
    public class PushMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
