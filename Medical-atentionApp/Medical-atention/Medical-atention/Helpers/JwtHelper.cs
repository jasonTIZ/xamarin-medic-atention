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
                var exp = GetClaim(token, "exp");
                if (exp == null) return false;
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
                return expirationTime > DateTimeOffset.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public static string GetClaim(string token, string claimKey)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

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

                return claims.TryGetValue(claimKey, out var value) ? value.ToString() : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
