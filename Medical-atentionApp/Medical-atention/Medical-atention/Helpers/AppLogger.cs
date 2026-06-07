using System;
using System.IO;

namespace Medical_atention.Helpers
{
    public static class AppLogger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "medic_app_errors.log");

        private const int MaxLogSizeBytes = 512 * 1024;

        public static void LogError(string endpoint, string message)
        {
            try
            {
                TrimIfNeeded();
                Append("ERROR", endpoint, message);
            }
            catch { }
        }

        public static void LogInfo(string endpoint, string message)
        {
            try { Append("INFO ", endpoint, message); }
            catch { }
        }

        private static void Append(string level, string endpoint, string message)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level} | {endpoint} | {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, entry);
        }

        private static void TrimIfNeeded()
        {
            if (File.Exists(LogPath) && new FileInfo(LogPath).Length > MaxLogSizeBytes)
                File.Delete(LogPath);
        }
    }
}
