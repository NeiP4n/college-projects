using System;
using System.IO;
using System.Web.Script.Serialization;

namespace OllamaAI
{
    public class PluginSettings
    {
        public string OllamaUrl { get; set; }
        public int TimeoutSeconds { get; set; }
        public string SystemPrompt { get; set; }
        public string LastModel { get; set; }
        public string LastTemplateId { get; set; }
        public bool AutoRefreshEnabled { get; set; }
        public int AutoRefreshSeconds { get; set; }
        public bool AutoDiscoverLan { get; set; }
        public string LastDiscoveredUrl { get; set; }

        public PluginSettings()
        {
            OllamaUrl = "http://localhost:11434";
            TimeoutSeconds = 120;
            SystemPrompt = "Ты полезный помощник для документов Word. По умолчанию отвечай на русском языке, если пользователь явно не попросил другой язык.";
            LastModel = "";
            LastTemplateId = "";
            AutoRefreshEnabled = true;
            AutoRefreshSeconds = 15;
            AutoDiscoverLan = true;
            LastDiscoveredUrl = "";
        }

        private static string FilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OllamaAI.VSTO",
                    "settings.json");
            }
        }

        public static PluginSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new PluginSettings();
                }

                var text = File.ReadAllText(FilePath);
                var settings = new JavaScriptSerializer().Deserialize<PluginSettings>(text);
                return Normalize(settings ?? new PluginSettings());
            }
            catch
            {
                return new PluginSettings();
            }
        }

        public static void Save(PluginSettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var text = new JavaScriptSerializer().Serialize(settings);
                File.WriteAllText(FilePath, text);
            }
            catch
            {
            }
        }

        public PluginSettings Clone()
        {
            return new PluginSettings
            {
                OllamaUrl = OllamaUrl,
                TimeoutSeconds = TimeoutSeconds,
                SystemPrompt = SystemPrompt,
                LastModel = LastModel,
                LastTemplateId = LastTemplateId,
                AutoRefreshEnabled = AutoRefreshEnabled,
                AutoRefreshSeconds = AutoRefreshSeconds,
                AutoDiscoverLan = AutoDiscoverLan,
                LastDiscoveredUrl = LastDiscoveredUrl
            };
        }

        private static PluginSettings Normalize(PluginSettings s)
        {
            if (s == null)
            {
                return new PluginSettings();
            }

            if (string.IsNullOrWhiteSpace(s.OllamaUrl))
            {
                s.OllamaUrl = "http://localhost:11434";
            }

            if (s.TimeoutSeconds < 10)
            {
                s.TimeoutSeconds = 120;
            }

            if (string.IsNullOrWhiteSpace(s.SystemPrompt) ||
                string.Equals(
                    s.SystemPrompt.Trim(),
                    "You are a helpful assistant for Word documents. Reply in the user's language.",
                    StringComparison.Ordinal))
            {
                s.SystemPrompt = "Ты полезный помощник для документов Word. По умолчанию отвечай на русском языке, если пользователь явно не попросил другой язык.";
            }

            if (s.AutoRefreshSeconds < 5)
            {
                s.AutoRefreshSeconds = 15;
            }

            if (string.IsNullOrWhiteSpace(s.LastDiscoveredUrl))
            {
                s.LastDiscoveredUrl = "";
            }

            return s;
        }
    }
}
