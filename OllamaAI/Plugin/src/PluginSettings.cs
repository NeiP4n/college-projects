using System;
using System.IO;
using Newtonsoft.Json;

namespace OllamaWordPlugin
{
    public class PluginSettings
    {
        public string OllamaUrl      { get; set; }
        public int    TimeoutSeconds { get; set; }
        public string SystemPrompt   { get; set; }
        public string LastModel      { get; set; }

        public PluginSettings()
        {
            OllamaUrl      = "http://localhost:11434";
            TimeoutSeconds = 120;
            SystemPrompt   = "You are a helpful assistant for Word documents. Reply in the user's language.";
            LastModel      = "";
        }

        private static string FilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OllamaWordPlugin",
                    "settings.json");
            }
        }

        public static PluginSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var text = File.ReadAllText(FilePath);
                    var s = JsonConvert.DeserializeObject<PluginSettings>(text);
                    if (s != null) return s;
                }
            }
            catch { }
            return new PluginSettings();
        }

        public static void Save(PluginSettings s)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(s, Formatting.Indented));
            }
            catch { }
        }

        public PluginSettings Clone()
        {
            return new PluginSettings
            {
                OllamaUrl      = OllamaUrl,
                TimeoutSeconds = TimeoutSeconds,
                SystemPrompt   = SystemPrompt,
                LastModel      = LastModel
            };
        }
    }
}
