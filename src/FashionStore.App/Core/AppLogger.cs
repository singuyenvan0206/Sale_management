using System;
using System.IO;

namespace FashionStore.App.Core
{
    public static class AppLogger
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FashionStore",
            "logs.txt");

        static AppLogger()
        {
            try
            {
                var directory = Path.GetDirectoryName(LogFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch { }
        }

        public static void Log(Exception ex, string context = "")
        {
            try
            {
                var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {context}\n" +
                              $"Exception: {ex.GetType().Name}\n" +
                              $"Message: {ex.Message}\n" +
                              $"StackTrace: {ex.StackTrace}\n" +
                              $"--------------------------------------------------\n";
                
                File.AppendAllText(LogFilePath, message);
            }
            catch { }
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}\n" +
                               $"--------------------------------------------------\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch { }
        }
    }
}
