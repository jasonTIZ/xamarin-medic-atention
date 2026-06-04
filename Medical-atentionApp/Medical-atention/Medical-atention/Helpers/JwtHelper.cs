using Newtonsoft.Json.Linq;
using System;

namespace Medical_atention.Helpers
{
    public static class JwtHelper
    {
        public static bool IsTokenValid(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                var payload = parts[1]
                    .Replace('-', '+')
                    .Replace('_', '/');

                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var claims = JObject.Parse(json);

                if (!claims.TryGetValue("exp", out var expToken)) return false;

                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expToken.Value<long>());
                return expirationTime > DateTimeOffset.UtcNow;
            }
            catch
            {
                return false;
            }
        }
    }
}
