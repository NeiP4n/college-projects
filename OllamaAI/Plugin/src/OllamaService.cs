using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OllamaWordPlugin
{
    public class OllamaModel
    {
        public string Name        { get; set; }
        public string DisplayName => Name != null ? Name.Split(':')[0] : "";
        public long   Size        { get; set; }
        public string SizeDisplay => Size > 0 ? string.Format("{0:F1} GB", Size / 1073741824.0) : "";
    }

    public class ChatMessage
    {
        [JsonProperty("role")]    public string Role    { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
    }

    public class OllamaService
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        private string _base;

        public OllamaService(string url = "http://localhost:11434")
        {
            _base = url.TrimEnd('/');
        }

        public void SetBaseUrl(string url) { _base = url.TrimEnd('/'); }

        public async Task<bool> IsRunningAsync()
        {
            try
            {
                var resp = await _http.GetAsync(_base + "/api/version");
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<OllamaModel>> GetModelsAsync()
        {
            var list = new List<OllamaModel>();
            try
            {
                var resp = await _http.GetAsync(_base + "/api/tags");
                var json = await resp.Content.ReadAsStringAsync();
                var arr = JObject.Parse(json)["models"] as JArray;
                if (arr != null)
                {
                    foreach (var m in arr)
                    {
                        list.Add(new OllamaModel
                        {
                            Name = m["name"] != null ? m["name"].ToString() : "",
                            Size = m["size"] != null ? m["size"].Value<long>() : 0
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        public async Task<string> GenerateAsync(string model, string prompt, string system = "")
        {
            var payload = new { model = model, prompt = prompt, system = system, stream = false };
            var body = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(_base + "/api/generate", body);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var val = JObject.Parse(json)["response"];
            return val != null ? val.ToString() : "";
        }

        public async Task<string> ChatAsync(string model, List<ChatMessage> messages)
        {
            var payload = new { model = model, messages = messages, stream = false };
            var body = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(_base + "/api/chat", body);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var msg = JObject.Parse(json)["message"];
            if (msg == null) return "";
            var content = msg["content"];
            return content != null ? content.ToString() : "";
        }
    }
}
