using System;
using System.IO;

namespace OllamaAI
{
    internal static class AddinLog
    {
        private static readonly object Sync = new object();

        public static void WriteInfo(string context, string message)
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OllamaAI.VSTO");
                Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, "addin.log");
                var text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{context}] {message}\r\n";
                lock (Sync)
                {
                    File.AppendAllText(path, text);
                }
            }
            catch
            {
            }
        }

        public static void Write(string context, Exception ex)
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OllamaAI.VSTO");
                Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, "addin.log");
                var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{context}]\r\n{ex}\r\n";
                lock (Sync)
                {
                    File.AppendAllText(path, message);
                }
            }
            catch
            {
            }
        }
    }
}
