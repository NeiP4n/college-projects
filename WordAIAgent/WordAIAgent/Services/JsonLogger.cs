using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WordAIAgent.Services
{
    public static class JsonLogger
    {
        private static readonly object _lock = new object();
        private static string _logPath;

        public static void Init(string baseDir)
        {
            var dir = string.IsNullOrWhiteSpace(baseDir) ? AppDomain.CurrentDomain.BaseDirectory : baseDir;
            _logPath = Path.Combine(dir, "ai_agent.log.jsonl");
        }

        public static void Log(string eventType, object payload = null)
        {
            if (string.IsNullOrWhiteSpace(eventType)) return;
            if (string.IsNullOrWhiteSpace(_logPath)) Init(AppDomain.CurrentDomain.BaseDirectory);

            var record = new Dictionary<string, object>
            {
                ["ts"] = DateTime.UtcNow.ToString("o"),
                ["event"] = eventType
            };

            if (payload != null)
                record["data"] = payload;

            try
            {
                var line = JsonConvert.SerializeObject(record);
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }
}
