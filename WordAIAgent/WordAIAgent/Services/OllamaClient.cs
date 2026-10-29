using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class OllamaClient
    {
        private readonly HttpClient _httpClient;

        public OllamaClient()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        }

        public async Task<string> ChatAsync(
            string host,
            string model,
            IEnumerable<ChatMessage> messages,
            double temperature,
            int maxTokens)
        {
            var request = new OllamaRequest
            {
                Model = model,
                Messages = messages.Select(m => new OllamaMessage { Role = m.Role, Content = m.Content }).ToList(),
                Stream = false,
                Options = new OllamaOptions { Temperature = temperature, NumPredict = maxTokens }
            };

            var json = JsonConvert.SerializeObject(request);
            var response = await _httpClient.PostAsync(
                NormalizeHost(host) + "/api/chat",
                new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<OllamaResponse>(body);
            return obj?.Message?.Content ?? "No response";
        }

        public async Task<List<string>> GetModelsAsync(string host)
        {
            var response = await _httpClient.GetAsync(NormalizeHost(host) + "/api/tags");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<OllamaTagsResponse>(body);
            return (obj?.Models ?? new List<OllamaModel>())
                .Select(m => m.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList();
        }

        private static string NormalizeHost(string host)
        {
            var value = (host ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                value = "http://127.0.0.1:11434";

            if (value.StartsWith("http://0.0.0.0", StringComparison.OrdinalIgnoreCase))
                value = "http://127.0.0.1:11434";

            if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                value = "http://" + value;
            }

            return value.TrimEnd('/');
        }
    }
}
