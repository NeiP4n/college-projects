using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeLearn.Data;
using CodeLearn.Models;

namespace CodeLearn.Controllers
{
    [Authorize]
    [Route("api/ai")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("ai")]
    public class AiTutorController : Controller
    {
        private readonly AppDbContext    _db;
        private readonly IConfiguration _cfg;
        private readonly IHttpClientFactory _http;

        // ── Провайдеры (OpenAI-совместимый формат) ───────────────────────────────
        private static readonly Dictionary<string, string> ProviderUrls
            = new(StringComparer.OrdinalIgnoreCase)
        {
            ["groq"]       = "https://api.groq.com/openai/v1/chat/completions",
            ["openrouter"] = "https://openrouter.ai/api/v1/chat/completions",
            ["gemini"]     = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
            ["gemini25"]   = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
            ["cerebras"]   = "https://api.cerebras.ai/v1/chat/completions",
            ["mistral"]    = "https://api.mistral.ai/v1/chat/completions",
            ["sambanova"]  = "https://api.sambanova.ai/v1/chat/completions",
            ["anthropic"]  = "__anthropic__",   // особый случай
        };

        private static readonly Dictionary<string, string> DefaultModels
            = new(StringComparer.OrdinalIgnoreCase)
        {
            // Groq: Llama 4 Scout — 500k TPD бесплатно (vs 100k у 70b), multimodal, 10M ctx
            ["groq"]       = "meta-llama/llama-4-scout-17b-16e-instruct",
            // OpenRouter: лучшие бесплатные модели
            ["openrouter"] = "meta-llama/llama-4-scout:free",
            // Gemini: лучший бесплатный лимит — 1M TPM, 1500 RPD
            ["gemini"]     = "gemini-2.0-flash",
            ["gemini25"]   = "gemini-2.5-flash",
            // Остальные
            ["cerebras"]   = "qwen-3-32b",
            ["mistral"]    = "mistral-medium-latest",
            ["sambanova"]  = "Meta-Llama-3.3-70B-Instruct",
            ["anthropic"]  = "claude-haiku-4-5",
        };

        public AiTutorController(AppDbContext db, IConfiguration cfg, IHttpClientFactory http)
        {
            _db   = db;
            _cfg  = cfg;
            _http = http;
        }

        public record AiRequest(int LessonId, string Message, List<AiHistoryMsg> History);
        public record AiHistoryMsg(string Role, string Content);

        // POST /api/ai/chat
        [HttpPost("chat"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> Chat([FromBody] AiRequest req)
        {
            // AI разрешён?
            if (!_cfg.GetValue<bool>("Platform:AllowAiChat", true))
                return StatusCode(503, new { error = "AI-наставник отключён администратором" });

            if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 2000)
                return BadRequest(new { error = "Сообщение пустое или слишком длинное" });

            var provider = (_cfg["Ai:Provider"] ?? "groq").ToLowerInvariant();
            var apiKey   = _cfg["Ai:ApiKey"] ?? "";
            var model    = _cfg["Ai:Model"]  ?? DefaultModels.GetValueOrDefault(provider, "llama-3.3-70b-versatile");
            var maxTokens = _cfg.GetValue<int>("Ai:MaxTokens", 512);

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
                return StatusCode(503, new { error = "AI недоступен: ключ не настроен" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var limit  = _cfg.GetValue<int>("Ai:DailyMessageLimit", 15);
            var today  = DateOnly.FromDateTime(DateTime.UtcNow);

            // Проверяем дневной лимит
            var usage = await _db.AiUsages
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Date == today);

            if (usage != null && usage.MessageCount >= limit)
                return Ok(new
                {
                    reply   = $"Дневной лимит исчерпан ({limit} сообщений). Возвращайся завтра!",
                    limited = true,
                    used    = usage.MessageCount,
                    limit
                });

            // Контекст урока
            var lesson = await _db.Lessons
                .Include(l => l.Module).ThenInclude(m => m!.Course)
                .Include(l => l.ContentBlocks.OrderBy(b => b.Order))
                .FirstOrDefaultAsync(l => l.Id == req.LessonId);

            var lessonTitle = lesson?.Title ?? "Неизвестный урок";
            var moduleTitle = lesson?.Module?.Title ?? "";
            var courseTitle = lesson?.Module?.Course?.Title ?? "C#";

            var theoryText = "";
            if (lesson != null)
            {
                var textBlocks = lesson.ContentBlocks
                    .Where(b => b.Type == "text")
                    .Take(3)
                    .Select(b =>
                    {
                        try {
                            var j = System.Text.Json.JsonDocument.Parse(b.Content).RootElement;
                            return j.TryGetProperty("markdown", out var m) ? m.GetString() ?? "" : "";
                        } catch { return ""; }
                    })
                    .Where(s => !string.IsNullOrEmpty(s));
                theoryText = string.Join("\n\n", textBlocks);
                if (theoryText.Length > 1500) theoryText = theoryText[..1500] + "...";
            }

            var systemPrompt = $"""
Ты — AI-наставник на образовательной платформе CodeLearn.
Курс: {courseTitle}
Модуль: {moduleTitle}
Урок: {lessonTitle}

{(string.IsNullOrEmpty(theoryText) ? "" : $"Теория урока:\n{theoryText}\n")}

ПРАВИЛА (строго соблюдай):
1. НЕ давай готовые решения задач — помогай студенту ДУМАТЬ самостоятельно.
2. Задавай наводящие вопросы, указывай на нужные концепции.
3. Если студент близко к ответу — подбодри и уточни деталь.
4. Отвечай кратко (2–5 предложений). Используй простой язык.
5. Если вопрос не по теме урока — мягко верни к теме.
6. Пиши на русском языке.
7. Можешь показывать небольшие примеры кода (НЕ решение задачи), только если это помогает понять концепцию.
""";

            var histCount = _cfg.GetValue<int>("Ai:HistoryMessages", 6);
            var history   = (req.History ?? new List<AiHistoryMsg>()).TakeLast(histCount).ToList();

            try
            {
                string reply;

                if (provider == "anthropic")
                    reply = await CallAnthropic(apiKey, model, maxTokens, systemPrompt, history, req.Message);
                else
                    reply = await CallOpenAiCompat(provider, apiKey, model, maxTokens, systemPrompt, history, req.Message);

                // Обновляем счётчик
                if (usage == null)
                    _db.AiUsages.Add(new UserAiUsage { UserId = userId, Date = today, MessageCount = 1 });
                else
                    usage.MessageCount++;
                await _db.SaveChangesAsync();

                var newCount = usage?.MessageCount ?? 1;
                return Ok(new { reply, limited = false, used = newCount, limit, remaining = limit - newCount });
            }
            catch (AiException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "Ошибка соединения с AI", detail = ex.Message });
            }
        }

        // GET /api/ai/status
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var limit  = _cfg.GetValue<int>("Ai:DailyMessageLimit", 15);
            var today  = DateOnly.FromDateTime(DateTime.UtcNow);

            var used = await _db.AiUsages
                .Where(u => u.UserId == userId && u.Date == today)
                .Select(u => u.MessageCount)
                .FirstOrDefaultAsync();

            return Ok(new { used, limit, remaining = limit - used });
        }

        // ── OpenAI-совместимый вызов (Groq / OpenRouter / Gemini / Cerebras / Mistral / SambaNova) ──
        private async Task<string> CallOpenAiCompat(
            string provider, string apiKey, string model, int maxTokens,
            string systemPrompt, List<AiHistoryMsg> history, string userMessage)
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };
            messages.AddRange(history.Select(h => new { role = h.Role, content = h.Content } as object));
            messages.Add(new { role = "user", content = userMessage });

            var body = new { model, max_tokens = maxTokens, messages };

            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

            if (provider == "openrouter")
            {
                client.DefaultRequestHeaders.Add("HTTP-Referer", "https://codelearn.app");
                client.DefaultRequestHeaders.Add("X-Title", "CodeLearn");
            }

            var url = ProviderUrls.GetValueOrDefault(provider, ProviderUrls["groq"]);
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);

            if (!resp.IsSuccessStatusCode)
            {
                if ((int)resp.StatusCode == 401)
                    throw new AiException("Ключ API недействителен. Обновите ключ: Builder → AI-наставник.");
                if ((int)resp.StatusCode == 429)
                    throw new AiException("Превышен лимит запросов к AI. Попробуй через минуту.");
                throw new AiException($"AI временно недоступен (ошибка {(int)resp.StatusCode})");
            }

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Не удалось получить ответ";
        }

        // ── Anthropic API (особый формат) ────────────────────────────────────────
        private async Task<string> CallAnthropic(
            string apiKey, string model, int maxTokens,
            string systemPrompt, List<AiHistoryMsg> history, string userMessage)
        {
            var messages = history
                .Select(h => new { role = h.Role, content = h.Content })
                .ToList<object>();
            messages.Add(new { role = "user", content = userMessage });

            var body = new { model, max_tokens = maxTokens, system = systemPrompt, messages };

            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

            if (!resp.IsSuccessStatusCode)
                throw new AiException($"AI временно недоступен ({(int)resp.StatusCode})");

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "Не удалось получить ответ";
        }

        private class AiException : Exception
        {
            public AiException(string msg) : base(msg) { }
        }
    }
}
