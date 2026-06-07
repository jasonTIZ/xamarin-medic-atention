using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Medical_atention.Helpers
{
    public static class ApiExceptionHandler
    {
        public const string UnauthorizedMessage = "App_Unauthorized";

        /// <summary>
        /// Processes an HTTP response. Returns null on success or on 409 Conflict (caller handles it).
        /// Returns a user-facing error string for all other failure codes.
        /// Fires UnauthorizedMessage via MessagingCenter on 401.
        /// </summary>
        public static Task<string> ProcessResponseAsync(HttpResponseMessage response, string endpoint)
        {
            if (response.IsSuccessStatusCode)
                return Task.FromResult<string>(null);

            var statusCode = (int)response.StatusCode;
            string userMessage;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    userMessage = "Sesión expirada. Por favor, inicia sesión nuevamente.";
                    AppLogger.LogError(endpoint, "HTTP 401 — sesión expirada");
                    MessagingCenter.Send<object>(new object(), UnauthorizedMessage);
                    break;

                case HttpStatusCode.Forbidden:
                    userMessage = "No tienes permisos para esta acción.";
                    AppLogger.LogError(endpoint, "HTTP 403 — acceso denegado");
                    break;

                case HttpStatusCode.NotFound:
                    userMessage = "El recurso solicitado no existe.";
                    AppLogger.LogError(endpoint, "HTTP 404 — not found");
                    break;

                case HttpStatusCode.Conflict:
                    // Caller handles conflict (e.g. cédula duplicada) with its own message.
                    userMessage = null;
                    break;

                case HttpStatusCode.InternalServerError:
                    userMessage = "Error del servidor. Intenta más tarde.";
                    AppLogger.LogError(endpoint, "HTTP 500 — internal server error");
                    break;

                case HttpStatusCode.ServiceUnavailable:
                    userMessage = "Servicio no disponible. Intenta más tarde.";
                    AppLogger.LogError(endpoint, "HTTP 503 — service unavailable");
                    break;

                default:
                    userMessage = $"Error en la solicitud ({statusCode}).";
                    AppLogger.LogError(endpoint, $"HTTP {statusCode}");
                    break;
            }

            return Task.FromResult(userMessage);
        }

        /// <summary>
        /// Handles an exception from an HTTP call.
        /// Returns null for network/timeout errors (caller should fall back to local data).
        /// Returns a user-facing error string for unexpected exceptions.
        /// </summary>
        public static string HandleException(Exception ex, string endpoint)
        {
            bool isNetworkError =
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is OperationCanceledException;

            if (isNetworkError)
            {
                AppLogger.LogError(endpoint, $"Network — {ex.GetType().Name}: {ex.Message}");
                return null;
            }

            AppLogger.LogError(endpoint, $"Unexpected — {ex.GetType().Name}: {ex.Message}");
            return "Error inesperado. Intenta más tarde.";
        }
    }
}
