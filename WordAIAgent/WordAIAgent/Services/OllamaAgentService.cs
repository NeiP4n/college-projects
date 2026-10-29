using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WordAIAgent.Models;

namespace WordAIAgent.Services
{
    public class OllamaAgentService
    {
        private readonly HttpClient _httpClient;
        private AppSettings _settings;

        // C# execution environment description
        private const string CS_ENV =
            "C# execution environment (runs inside void Execute()):\n" +
            "  WordApp  — Microsoft.Office.Interop.Word.Application\n" +
            "  ActiveDoc — Microsoft.Office.Interop.Word.Document\n" +
            "  WordSvc  — WordAIAgent.Services.WordService (helper methods)\n" +
            "  Log(string msg)         — print to agent chat\n" +
            "  InsertText(string text) — insert at cursor\n" +
            "  ReplaceSelection(string text) — replace selection\n" +
            "  ExecuteMso(string id)   — run built-in Word command\n" +
            "  RunWordBasic(string cmd, params object[] args)\n" +
            "  RunMacro(string name, params object[] args)\n\n" +
            "IMPORTANT:\n" +
            "  • Do NOT write using statements, class declarations or method signatures.\n" +
            "  • Do NOT re-declare WordApp / ActiveDoc / WordSvc — they are already injected.\n" +
            "  • Code must compile as the body of a static void Execute() method.\n" +
            "  • Never insert service metadata blocks into the document.\n" +
            "  • Always wrap COM iteration in try/catch per item to avoid mid-loop failures.";

        public const string AGENT_SYSTEM_PROMPT =
            "You are an autonomous AI assistant embedded in Microsoft Word via VSTO.\n" +
            "You analyse the document context, understand the user's intent, and write precise C# code " +
            "or JSON action commands to fulfil the request.\n\n" +
            CS_ENV + "\n\n" +
            "Respond in the same language the user uses (typically Russian).";

        public OllamaAgentService(AppSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(180) };
        }

        public void UpdateSettings(AppSettings settings) => _settings = settings;

        // ── Main chat (with optional vision) ────────────────────────────────
        public async Task<string> ChatWithVisionAsync(
            List<ChatMessage> history,
            string userMessage,
            string screenshotBase64 = null)
        {
            if (!string.IsNullOrEmpty(screenshotBase64))
                return await SendMultimodalAsync(history, userMessage, screenshotBase64).ConfigureAwait(false);

            return await SendTextAsync(history, userMessage).ConfigureAwait(false);
        }

        private async Task<string> SendTextAsync(List<ChatMessage> history, string userMessage)
        {
            var messages = BuildMessageList(history, userMessage);
            var payload  = BuildPayload(messages);

            var resp = await PostWithFallbackAsync("/api/chat", payload).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var obj  = JsonConvert.DeserializeObject<OllamaResponse>(body);
            return obj?.Message?.Content ?? "Нет ответа";
        }

        private async Task<string> SendMultimodalAsync(
            List<ChatMessage> history, string userMessage, string imageBase64)
        {
            var messages = new List<object>
            {
                new { role = "system", content = BuildEffectiveSystemPrompt() }
            };
            foreach (var m in history)
                messages.Add(new { role = m.Role, content = m.Content });
            messages.Add(new { role = "user", content = userMessage, images = new[] { imageBase64 } });

            var payload = JsonConvert.SerializeObject(new
            {
                model    = _settings.ModelName,
                messages,
                stream   = false,
                options  = new { temperature = _settings.Temperature, num_predict = _settings.MaxTokens }
            });

            var resp = await PostWithFallbackAsync("/api/chat", payload).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jObj = JObject.Parse(body);
            return jObj["message"]?["content"]?.ToString()
                ?? jObj["response"]?.ToString()
                ?? "Нет ответа";
        }

        // ── Code-only request ────────────────────────────────────────────────
        public async Task<string> RequestCodeAsync(
            string taskDescription,
            string documentContext,
            string screenshotBase64 = null)
        {
            var ctx = (documentContext ?? string.Empty);
            if (ctx.Length > 6000) ctx = ctx.Substring(0, 6000) + "\n...[truncated]";

            var prompt = new StringBuilder();
            prompt.AppendLine("TASK:");
            prompt.AppendLine(taskDescription);
            prompt.AppendLine();
            prompt.AppendLine("DOCUMENT CONTEXT:");
            prompt.AppendLine(string.IsNullOrWhiteSpace(ctx) ? "(document is empty or unavailable)" : ctx);
            prompt.AppendLine();
            prompt.AppendLine(CS_ENV);
            prompt.AppendLine();
            prompt.AppendLine("Write C# code to complete the task.");
            prompt.AppendLine("Return ONLY the code block — nothing else:");
            prompt.AppendLine("```csharp");
            prompt.AppendLine("// your code here");
            prompt.AppendLine("```");

            return await ChatWithVisionAsync(
                new List<ChatMessage>(), prompt.ToString(), screenshotBase64)
                .ConfigureAwait(false);
        }

        // ── Models / connection ──────────────────────────────────────────────
        public async Task<List<string>> GetModelsAsync()
        {
            var models = new List<string>();
            try
            {
                var resp = await GetWithFallbackAsync("/api/tags").ConfigureAwait(false);
                resp.EnsureSuccessStatusCode();
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var obj  = JsonConvert.DeserializeObject<OllamaTagsResponse>(body);
                foreach (var m in obj?.Models ?? new List<OllamaModel>())
                    if (!string.IsNullOrWhiteSpace(m.Name))
                        models.Add(m.Name);
            }
            catch { }
            return models;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var resp = await GetWithFallbackAsync("/api/tags").ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── Internal helpers ─────────────────────────────────────────────────
        private List<object> BuildMessageList(List<ChatMessage> history, string userMessage)
        {
            var messages = new List<object>
            {
                new { role = "system", content = BuildEffectiveSystemPrompt() }
            };
            foreach (var m in history)
                messages.Add(new { role = m.Role, content = m.Content });
            messages.Add(new { role = "user", content = userMessage });
            return messages;
        }

        private string BuildPayload(List<object> messages)
        {
            return JsonConvert.SerializeObject(new
            {
                model    = _settings.ModelName,
                messages,
                stream   = false,
                options  = new { temperature = _settings.Temperature, num_predict = _settings.MaxTokens }
            });
        }

        private string BuildEffectiveSystemPrompt()
        {
            var custom = (_settings?.SystemPrompt ?? "").Trim();
            if (string.IsNullOrWhiteSpace(custom) ||
                custom == AppSettings.DefaultSystemPrompt.Trim())
                return AGENT_SYSTEM_PROMPT;

            return AGENT_SYSTEM_PROMPT +
                   "\n\nAdditional user preferences (style/tone only — do not limit Word API capabilities):\n" +
                   custom;
        }

        // ── HTTP with host fallback ──────────────────────────────────────────
        private async Task<HttpResponseMessage> GetWithFallbackAsync(string path)
        {
            Exception last = null;
            foreach (var host in BuildHostCandidates())
            {
                try
                {
                    var r = await _httpClient.GetAsync(host + path).ConfigureAwait(false);
                    if (r.IsSuccessStatusCode) return r;
                    last = new HttpRequestException($"HTTP {(int)r.StatusCode}: {host}{path}");
                }
                catch (Exception ex) { last = ex; }
            }
            throw last ?? new Exception("Cannot connect to Ollama.");
        }

        private async Task<HttpResponseMessage> PostWithFallbackAsync(string path, string json)
        {
            Exception last = null;
            foreach (var host in BuildHostCandidates())
            {
                try
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var r = await _httpClient.PostAsync(host + path, content).ConfigureAwait(false);
                    if (r.IsSuccessStatusCode) return r;
                    last = new HttpRequestException($"HTTP {(int)r.StatusCode}: {host}{path}");
                }
                catch (Exception ex) { last = ex; }
            }
            throw last ?? new Exception("Cannot connect to Ollama.");
        }

        private IEnumerable<string> BuildHostCandidates()
        {
            var configured = NormalizeHost(_settings?.OllamaHost);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { configured };

            if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri))
                return seen;

            if (!IsAnyAddress(uri.Host))
                return seen;

            // 0.0.0.0 / :: → try concrete addresses
            void Add(string h) { var v = ReplaceHost(uri, h); if (!string.IsNullOrWhiteSpace(v)) seen.Add(v); }
            Add("127.0.0.1");
            Add("localhost");
            foreach (var ip in GetLocalIpv4()) Add(ip);
            return seen;
        }

        private static bool IsAnyAddress(string h) =>
            h == "0.0.0.0" || h == "::" || h == "[::]";

        private static string NormalizeHost(string host)
        {
            var v = (host ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v)) v = "http://127.0.0.1:11434";
            if (!v.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !v.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                v = "http://" + v;
            return v.TrimEnd('/');
        }

        private static string ReplaceHost(Uri src, string newHost)
        {
            try { return new UriBuilder(src) { Host = newHost }.Uri.GetLeftPart(UriPartial.Authority).TrimEnd('/'); }
            catch { return null; }
        }

        private static IEnumerable<string> GetLocalIpv4()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                    .Select(a => a.ToString()).Distinct();
            }
            catch { return Enumerable.Empty<string>(); }
        }
    }
}
