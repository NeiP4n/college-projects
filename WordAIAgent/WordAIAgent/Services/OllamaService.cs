using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private AppSettings _settings;

        public OllamaService(AppSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };
        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> ChatAsync(List<ChatMessage> history, string userMessage)
        {
            var messages = new List<OllamaMessage>
            {
                new OllamaMessage { Role = "system", Content = _settings.SystemPrompt }
            };

            foreach (var msg in history)
                messages.Add(new OllamaMessage { Role = msg.Role, Content = msg.Content });

            messages.Add(new OllamaMessage { Role = "user", Content = userMessage });

            var request = new OllamaRequest
            {
                Model = _settings.ModelName,
                Messages = messages,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = _settings.Temperature,
                    NumPredict = _settings.MaxTokens
                }
            };

            var json = JsonConvert.SerializeObject(request);
            var response = await PostJsonWithFallbackAsync("/api/chat", json);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonConvert.DeserializeObject<OllamaResponse>(responseBody);
            if (!string.IsNullOrEmpty(ollamaResponse?.Error))
                throw new Exception($"Ollama error: {ollamaResponse.Error}");

            return ollamaResponse?.Message?.Content ?? "Нет ответа от модели";
        }

        public async Task<string> ChatStreamAsync(List<ChatMessage> history, string userMessage, Action<string> onChunk)
        {
            var messages = new List<OllamaMessage>
            {
                new OllamaMessage { Role = "system", Content = _settings.SystemPrompt }
            };

            foreach (var msg in history)
                messages.Add(new OllamaMessage { Role = msg.Role, Content = msg.Content });
            messages.Add(new OllamaMessage { Role = "user", Content = userMessage });

            var request = new OllamaRequest
            {
                Model = _settings.ModelName,
                Messages = messages,
                Stream = true,
                Options = new OllamaOptions
                {
                    Temperature = _settings.Temperature,
                    NumPredict = _settings.MaxTokens
                }
            };

            var payload = JsonConvert.SerializeObject(request);
            Exception lastError = null;
            var full = new StringBuilder();

            foreach (var host in BuildHostCandidates())
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Post, host + "/api/chat")
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };
                    using var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var obj = JObject.Parse(line);
                            var chunk = obj["message"]?["content"]?.ToString();
                            if (!string.IsNullOrEmpty(chunk))
                            {
                                full.Append(chunk);
                                onChunk?.Invoke(chunk);
                            }
                        }
                        catch
                        {
                        }
                    }

                    return full.ToString();
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            throw lastError ?? new Exception("Не удалось подключиться к Ollama.");
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var request = new
            {
                model = _settings.ModelName,
                prompt,
                stream = false,
                options = new { temperature = _settings.Temperature, num_predict = _settings.MaxTokens }
            };

            var json = JsonConvert.SerializeObject(request);
            var response = await PostJsonWithFallbackAsync("/api/generate", json);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);
            return result?.response ?? "Нет ответа";
        }

        public async Task<List<string>> GetModelsAsync()
        {
            try
            {
                var response = await GetWithFallbackAsync("/api/tags");
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var tagsResponse = JsonConvert.DeserializeObject<OllamaTagsResponse>(body);
                return (tagsResponse?.Models ?? new List<OllamaModel>())
                    .Select(m => m.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await GetWithFallbackAsync("/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<HttpResponseMessage> GetWithFallbackAsync(string path)
        {
            Exception lastError = null;
            foreach (var host in BuildHostCandidates())
            {
                try
                {
                    var response = await _httpClient.GetAsync(host + path);
                    if (response.IsSuccessStatusCode)
                        return response;

                    lastError = new HttpRequestException($"HTTP {(int)response.StatusCode}: {host}{path}");
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            throw lastError ?? new Exception("Не удалось подключиться к Ollama.");
        }

        private async Task<HttpResponseMessage> PostJsonWithFallbackAsync(string path, string json)
        {
            Exception lastError = null;
            foreach (var host in BuildHostCandidates())
            {
                try
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(host + path, content);
                    if (response.IsSuccessStatusCode)
                        return response;

                    lastError = new HttpRequestException($"HTTP {(int)response.StatusCode}: {host}{path}");
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            throw lastError ?? new Exception("Не удалось подключиться к Ollama.");
        }

        private IEnumerable<string> BuildHostCandidates()
        {
            var configured = NormalizeHost(_settings?.OllamaHost);
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddUnique(unique, configured);

            if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri))
                return unique;

            if (!IsAnyAddress(uri.Host))
                return unique;

            // 0.0.0.0 is a bind address. For client calls try concrete addresses.
            AddUnique(unique, ReplaceHost(uri, "127.0.0.1"));
            AddUnique(unique, ReplaceHost(uri, "localhost"));

            foreach (var ip in GetLocalIpv4Addresses())
                AddUnique(unique, ReplaceHost(uri, ip));

            return unique;
        }

        private static bool IsAnyAddress(string host)
        {
            return string.Equals(host, "0.0.0.0", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(host, "::", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(host, "[::]", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeHost(string host)
        {
            var value = (host ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                value = "http://127.0.0.1:11434";

            if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                value = "http://" + value;
            }

            return value.TrimEnd('/');
        }

        private static string ReplaceHost(Uri source, string host)
        {
            var builder = new UriBuilder(source)
            {
                Host = host
            };
            return builder.Uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        private static IEnumerable<string> GetLocalIpv4Addresses()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                    .Select(a => a.ToString())
                    .Distinct()
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static void AddUnique(ISet<string> set, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                set.Add(value);
        }
    }
}
