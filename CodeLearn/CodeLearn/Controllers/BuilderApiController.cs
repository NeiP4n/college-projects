using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using CodeLearn.Data;
using CodeLearn.Helpers;
using CodeLearn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeLearn.Controllers
{
    // ── DTOs ─────────────────────────────────────────────────────────────────────
    public record ModuleDto(
        int?    CourseId,
        string  Title,
        string? Description,
        bool?   IsLocked,
        bool?   IsChallenge,
        int?    Order,
        double? MapX,
        double? MapY,
        string? Color,
        int?    LabelFontSize);

    public record CourseDto(
        string Title,
        string? Slug,
        string? Description,
        string? Track,
        string? Tag,
        string? Color,
        string? State,
        string? Badge,
        string? Categories,
        int? EstimatedHours,
        int? Order,
        bool? IsPublished);

    public record MapPositionDto(double MapX, double MapY, string? Color);

    public record LessonDto(string Title, bool? IsLocked, int? Order);
    public record BlockDto(string Type, string Content, int? Order);
    public record ReorderItem(int Id, int Order);
    public record ReorderDto(string EntityType, List<ReorderItem> Items);

    public record SettingsUpdateDto(
        // AI
        string? AiProvider,
        string? AiApiKey,
        string? AiModel,
        int?    AiDailyMessageLimit,
        int?    AiMaxTokens,
        int?    AiHistoryMessages,
        // Platform
        string? PlatformSiteName,
        bool?   PlatformRegistrationOpen,
        bool?   PlatformAllowComments,
        bool?   PlatformAllowAiChat,
        int?    PlatformCommentMaxLength,
        int?    PlatformHintThreshold,
        int?    PlatformChatPollIntervalSec,
        int?    PlatformCommentsPerPage,
        bool?   PlatformProgressionLock,
        // Email
        string? EmailSmtpHost,
        int?    EmailSmtpPort,
        string? EmailUser,
        string? EmailAppPassword,
        string? EmailFrom);

    [ApiController]
    [LocalOnly]
    [Route("api/builder")]
    [IgnoreAntiforgeryToken]
    public class BuilderApiController : ControllerBase
    {
        private readonly AppDbContext              _db;
        private readonly IWebHostEnvironment       _env;
        private readonly UserManager<AppUser>      _users;

        public BuilderApiController(AppDbContext db, IWebHostEnvironment env, UserManager<AppUser> users)
        {
            _db    = db;
            _env   = env;
            _users = users;
        }

        private string SettingsPath =>
            Path.Combine(_env.ContentRootPath, "appsettings.json");

        // ── Courses ──────────────────────────────────────────────────────────────

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            await EnsureDefaultCourseAsync();

            return Ok(await _db.Courses
                .Include(c => c.Modules.OrderBy(m => m.Order))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.Order))
                .OrderBy(c => c.Order)
                .ToListAsync());
        }

        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseDto dto)
        {
            var maxOrder = await _db.Courses.MaxAsync(c => (int?)c.Order) ?? -1;
            var course = new Course
            {
                Title = dto.Title,
                Slug = await MakeUniqueSlugAsync(dto.Slug, dto.Title),
                Description = dto.Description ?? "",
                Track = dto.Track ?? "BACKEND",
                Tag = dto.Tag ?? "",
                Color = dto.Color ?? "#ffffff",
                State = dto.State ?? "available",
                Badge = dto.Badge ?? "START",
                Categories = dto.Categories ?? "backend",
                EstimatedHours = dto.EstimatedHours ?? 6,
                Order = maxOrder + 1,
                IsPublished = dto.IsPublished ?? true
            };
            _db.Courses.Add(course);
            await _db.SaveChangesAsync();
            return Ok(course);
        }

        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseDto dto)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Title = dto.Title;
            course.Slug = await MakeUniqueSlugAsync(dto.Slug, dto.Title, id);
            course.Description = dto.Description ?? "";
            course.Track = dto.Track ?? "BACKEND";
            course.Tag = dto.Tag ?? "";
            course.Color = dto.Color ?? "#ffffff";
            course.State = dto.State ?? "available";
            course.Badge = dto.Badge ?? "START";
            course.Categories = dto.Categories ?? "backend";
            course.EstimatedHours = dto.EstimatedHours ?? course.EstimatedHours;
            if (dto.Order.HasValue) course.Order = dto.Order.Value;
            course.IsPublished = dto.IsPublished ?? course.IsPublished;

            await _db.SaveChangesAsync();
            return Ok(course);
        }

        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Modules ──────────────────────────────────────────────────────────────

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules() =>
            Ok(await _db.Modules
                .Include(m => m.Lessons.OrderBy(l => l.Order))
                .OrderBy(m => m.Order)
                .ToListAsync());

        [HttpPost("modules")]
        public async Task<IActionResult> CreateModule([FromBody] ModuleDto dto)
        {
            var maxOrder = await _db.Modules.MaxAsync(m => (int?)m.Order) ?? -1;
            var module = new LearningModule
            {
                CourseId    = dto.CourseId,
                Title       = dto.Title,
                Description = dto.Description ?? "",
                IsLocked    = dto.IsLocked ?? true,
                IsChallenge = dto.IsChallenge ?? false,
                Order       = maxOrder + 1,
                MapX          = dto.MapX         ?? 50,
                MapY          = dto.MapY         ?? 50,
                Color         = dto.Color        ?? "#ffffff",
                LabelFontSize = dto.LabelFontSize ?? 9,
            };
            _db.Modules.Add(module);
            await _db.SaveChangesAsync();
            return Ok(module);
        }

        [HttpPut("modules/{id}")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] ModuleDto dto)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return NotFound();
            module.Title       = dto.Title;
            if (dto.CourseId.HasValue) module.CourseId = dto.CourseId.Value;
            module.Description  = dto.Description  ?? module.Description;
            module.IsLocked     = dto.IsLocked     ?? module.IsLocked;
            module.IsChallenge  = dto.IsChallenge  ?? module.IsChallenge;
            if (dto.Order.HasValue) module.Order = dto.Order.Value;
            if (dto.MapX.HasValue)          module.MapX          = dto.MapX.Value;
            if (dto.MapY.HasValue)          module.MapY          = dto.MapY.Value;
            if (dto.Color != null)          module.Color          = dto.Color;
            if (dto.LabelFontSize.HasValue) module.LabelFontSize  = dto.LabelFontSize.Value;
            await _db.SaveChangesAsync();
            return Ok(module);
        }

        // Отдельный endpoint для обновления только позиции/цвета на карте (drag-and-drop)
        [HttpPut("modules/{id}/map")]
        public async Task<IActionResult> UpdateModuleMap(int id, [FromBody] MapPositionDto dto)
        {
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return NotFound();
            module.MapX = dto.MapX;
            module.MapY = dto.MapY;
            if (dto.Color != null) module.Color = dto.Color;
            await _db.SaveChangesAsync();
            return Ok(new { module.Id, module.MapX, module.MapY, module.Color });
        }

        [HttpDelete("modules/{id}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            // Уроки и блоки остаются в БД (ModuleId станет NULL через SetNull)
            var module = await _db.Modules.FindAsync(id);
            if (module == null) return NotFound();
            _db.Modules.Remove(module);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Lessons ──────────────────────────────────────────────────────────────

        [HttpGet("modules/{moduleId}/lessons")]
        public async Task<IActionResult> GetLessons(int moduleId) =>
            Ok(await _db.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.Order)
                .ToListAsync());

        [HttpPost("modules/{moduleId}/lessons")]
        public async Task<IActionResult> CreateLesson(int moduleId, [FromBody] LessonDto dto)
        {
            var module = await _db.Modules.FindAsync(moduleId);
            if (module == null) return NotFound();

            var maxOrder = await _db.Lessons
                .Where(l => l.ModuleId == moduleId)
                .MaxAsync(l => (int?)l.Order) ?? -1;

            var lesson = new Lesson
            {
                ModuleId = moduleId,
                Title    = dto.Title,
                IsLocked = dto.IsLocked ?? true,
                Order    = maxOrder + 1
            };
            _db.Lessons.Add(lesson);
            await _db.SaveChangesAsync();
            return Ok(lesson);
        }

        [HttpPut("lessons/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonDto dto)
        {
            var lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            lesson.Title    = dto.Title;
            lesson.IsLocked = dto.IsLocked ?? lesson.IsLocked;
            if (dto.Order.HasValue) lesson.Order = dto.Order.Value;
            await _db.SaveChangesAsync();
            return Ok(lesson);
        }

        [HttpDelete("lessons/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            // ContentBlocks остаются в БД (LessonId станет NULL через SetNull)
            var lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            _db.Lessons.Remove(lesson);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── ContentBlocks ─────────────────────────────────────────────────────────

        [HttpGet("lessons/{lessonId}/blocks")]
        public async Task<IActionResult> GetBlocks(int lessonId) =>
            Ok(await _db.ContentBlocks
                .Where(b => b.LessonId == lessonId)
                .OrderBy(b => b.Order)
                .ToListAsync());

        [HttpPost("lessons/{lessonId}/blocks")]
        public async Task<IActionResult> CreateBlock(int lessonId, [FromBody] BlockDto dto)
        {
            var lesson = await _db.Lessons.FindAsync(lessonId);
            if (lesson == null) return NotFound();

            var maxOrder = await _db.ContentBlocks
                .Where(b => b.LessonId == lessonId)
                .MaxAsync(b => (int?)b.Order) ?? -1;

            var block = new ContentBlock
            {
                LessonId = lessonId,
                Type     = dto.Type,
                Content  = dto.Content,
                Order    = maxOrder + 1
            };
            _db.ContentBlocks.Add(block);
            await _db.SaveChangesAsync();
            return Ok(block);
        }

        [HttpPut("blocks/{id}")]
        public async Task<IActionResult> UpdateBlock(int id, [FromBody] BlockDto dto)
        {
            var block = await _db.ContentBlocks.FindAsync(id);
            if (block == null) return NotFound();
            block.Type    = dto.Type;
            block.Content = dto.Content;
            if (dto.Order.HasValue) block.Order = dto.Order.Value;
            await _db.SaveChangesAsync();
            return Ok(block);
        }

        [HttpDelete("blocks/{id}")]
        public async Task<IActionResult> DeleteBlock(int id)
        {
            var block = await _db.ContentBlocks.FindAsync(id);
            if (block == null) return NotFound();
            _db.ContentBlocks.Remove(block);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Reorder ───────────────────────────────────────────────────────────────

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderDto dto)
        {
            foreach (var item in dto.Items)
            {
                switch (dto.EntityType)
                {
                    case "module":
                        var mod = await _db.Modules.FindAsync(item.Id);
                        if (mod != null) mod.Order = item.Order;
                        break;
                    case "lesson":
                        var les = await _db.Lessons.FindAsync(item.Id);
                        if (les != null) les.Order = item.Order;
                        break;
                    case "block":
                        var blk = await _db.ContentBlocks.FindAsync(item.Id);
                        if (blk != null) blk.Order = item.Order;
                        break;
                }
            }
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ── Unlock All ──────────────────────────────────────────────────────────
        [HttpPost("unlock-all")]
        public async Task<IActionResult> UnlockAll()
        {
            await _db.Modules.ForEachAsync(m => m.IsLocked = false);
            await _db.Lessons.ForEachAsync(l => l.IsLocked = false);
            await _db.SaveChangesAsync();

            // Также отключаем блокировку по прохождению → реально открывается всё
            SetProgressionLock(false);

            return Ok(new { ok = true, message = "Все модули и уроки разблокированы (доступ открыт)" });
        }

        // ── Блокировать всё (включить прогрессию) ────────────────────────────────
        [HttpPost("lock-all")]
        public IActionResult LockAll()
        {
            SetProgressionLock(true);
            return Ok(new { ok = true, message = "Блокировка по прохождению включена" });
        }

        // Записывает Platform:ProgressionLock в appsettings.json
        private void SetProgressionLock(bool value)
        {
            try
            {
                var json = System.IO.File.ReadAllText(SettingsPath);
                var root = JsonNode.Parse(json)!.AsObject();
                if (root["Platform"] is not JsonObject plat) { plat = new JsonObject(); root["Platform"] = plat; }
                plat["ProgressionLock"] = value;
                var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                System.IO.File.WriteAllText(SettingsPath, root.ToJsonString(opts));
            }
            catch { /* настройка не критична для ответа */ }
        }

        // ── Пройти всё: отметить все уроки завершёнными для всех пользователей ────
        [HttpPost("complete-all")]
        public async Task<IActionResult> CompleteAll()
        {
            // Отмечаем прохождение каждого урока каждым пользователем (кого ещё нет)
            var added = await _db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""LessonCompletions"" (""UserId"", ""LessonId"", ""CompletedAt"")
                SELECT u.""Id"", l.""Id"", now()
                FROM ""Users"" u CROSS JOIN ""Lessons"" l
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""LessonCompletions"" c
                    WHERE c.""UserId"" = u.""Id"" AND c.""LessonId"" = l.""Id"");");

            // Прогресс по блокам — на максимум (для шкалы внутри урока)
            await _db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""LessonProgresses"" (""UserId"", ""LessonId"", ""CompletedBlockCount"", ""UpdatedAt"")
                SELECT u.""Id"", l.""Id"",
                       (SELECT count(*) FROM ""ContentBlocks"" b WHERE b.""LessonId"" = l.""Id""), now()
                FROM ""Users"" u CROSS JOIN ""Lessons"" l
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""LessonProgresses"" p
                    WHERE p.""UserId"" = u.""Id"" AND p.""LessonId"" = l.""Id"");");

            return Ok(new { ok = true, message = $"Отмечено пройдено: {added} прохождений добавлено" });
        }

        // ── Сброс прогресса всех пользователей (для теста блокировки) ─────────────
        [HttpPost("reset-progress")]
        public async Task<IActionResult> ResetProgress()
        {
            var completions = await _db.LessonCompletions.CountAsync();
            _db.LessonCompletions.RemoveRange(_db.LessonCompletions);
            _db.LessonProgresses.RemoveRange(_db.LessonProgresses);
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, message = $"Прогресс сброшен ({completions} прохождений удалено)" });
        }

        // ── Seed C# course ───────────────────────────────────────────────────────
        [HttpPost("seed-csharp")]
        public async Task<IActionResult> SeedCsharp()
        {
            await CSharpSeeder.SeedAsync(_db);
            return Ok(new { ok = true, message = "C# курс пересоздан" });
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  НАСТРОЙКИ ПРОЕКТА
        // ══════════════════════════════════════════════════════════════════════════

        // GET /api/builder/settings
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            try
            {
                var json = System.IO.File.ReadAllText(SettingsPath);
                var root = JsonNode.Parse(json)!.AsObject();

                return Ok(new
                {
                    // AI
                    aiProvider          = root["Ai"]?["Provider"]?.GetValue<string>() ?? "groq",
                    aiApiKey            = MaskKey(root["Ai"]?["ApiKey"]?.GetValue<string>() ?? ""),
                    aiApiKeySet         = IsRealKey(root["Ai"]?["ApiKey"]?.GetValue<string>()),
                    aiModel             = root["Ai"]?["Model"]?.GetValue<string>() ?? "",
                    aiDailyMessageLimit = root["Ai"]?["DailyMessageLimit"]?.GetValue<int>() ?? 15,
                    aiMaxTokens         = root["Ai"]?["MaxTokens"]?.GetValue<int>() ?? 512,
                    aiHistoryMessages   = root["Ai"]?["HistoryMessages"]?.GetValue<int>() ?? 6,
                    // Platform
                    platformSiteName          = root["Platform"]?["SiteName"]?.GetValue<string>() ?? "CodeLearn",
                    platformRegistrationOpen  = root["Platform"]?["RegistrationOpen"]?.GetValue<bool>() ?? true,
                    platformAllowComments     = root["Platform"]?["AllowComments"]?.GetValue<bool>() ?? true,
                    platformAllowAiChat       = root["Platform"]?["AllowAiChat"]?.GetValue<bool>() ?? true,
                    platformCommentMaxLength  = root["Platform"]?["CommentMaxLength"]?.GetValue<int>() ?? 1000,
                    platformHintThreshold     = root["Platform"]?["HintThreshold"]?.GetValue<int>() ?? 3,
                    platformChatPollInterval  = root["Platform"]?["ChatPollIntervalSec"]?.GetValue<int>() ?? 3,
                    platformCommentsPerPage   = root["Platform"]?["CommentsPerPage"]?.GetValue<int>() ?? 30,
                    platformProgressionLock   = root["Platform"]?["ProgressionLock"]?.GetValue<bool>() ?? true,
                    // Email
                    emailSmtpHost    = root["Email"]?["SmtpHost"]?.GetValue<string>() ?? "",
                    emailSmtpPort    = root["Email"]?["SmtpPort"]?.GetValue<int>() ?? 587,
                    emailUser        = root["Email"]?["User"]?.GetValue<string>() ?? "",
                    emailFrom        = root["Email"]?["From"]?.GetValue<string>() ?? "",
                    emailAppPassword = MaskKey(root["Email"]?["AppPassword"]?.GetValue<string>() ?? ""),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST /api/builder/settings
        [HttpPost("settings")]
        public async Task<IActionResult> SaveSettings([FromBody] SettingsUpdateDto dto)
        {
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(SettingsPath);
                var root = JsonNode.Parse(json)!.AsObject();

                // ── AI ────────────────────────────────────────────────────────────
                if (root["Ai"] is not JsonObject ai) { ai = new JsonObject(); root["Ai"] = ai; }

                if (dto.AiProvider != null) ai["Provider"] = dto.AiProvider;
                if (!string.IsNullOrWhiteSpace(dto.AiApiKey) && !dto.AiApiKey!.Contains('*'))
                    ai["ApiKey"] = dto.AiApiKey;
                if (dto.AiModel             != null) ai["Model"]             = dto.AiModel;
                if (dto.AiDailyMessageLimit.HasValue) ai["DailyMessageLimit"]= dto.AiDailyMessageLimit.Value;
                if (dto.AiMaxTokens.HasValue)         ai["MaxTokens"]        = dto.AiMaxTokens.Value;
                if (dto.AiHistoryMessages.HasValue)   ai["HistoryMessages"]  = dto.AiHistoryMessages.Value;

                // ── Platform ──────────────────────────────────────────────────────
                if (root["Platform"] is not JsonObject plat) { plat = new JsonObject(); root["Platform"] = plat; }

                if (dto.PlatformSiteName != null)             plat["SiteName"]          = dto.PlatformSiteName;
                if (dto.PlatformRegistrationOpen.HasValue)    plat["RegistrationOpen"]  = dto.PlatformRegistrationOpen.Value;
                if (dto.PlatformAllowComments.HasValue)       plat["AllowComments"]     = dto.PlatformAllowComments.Value;
                if (dto.PlatformAllowAiChat.HasValue)         plat["AllowAiChat"]       = dto.PlatformAllowAiChat.Value;
                if (dto.PlatformCommentMaxLength.HasValue)    plat["CommentMaxLength"]  = dto.PlatformCommentMaxLength.Value;
                if (dto.PlatformHintThreshold.HasValue)       plat["HintThreshold"]     = dto.PlatformHintThreshold.Value;
                if (dto.PlatformChatPollIntervalSec.HasValue) plat["ChatPollIntervalSec"] = dto.PlatformChatPollIntervalSec.Value;
                if (dto.PlatformCommentsPerPage.HasValue)     plat["CommentsPerPage"]   = dto.PlatformCommentsPerPage.Value;
                if (dto.PlatformProgressionLock.HasValue)     plat["ProgressionLock"]   = dto.PlatformProgressionLock.Value;

                // ── Email ─────────────────────────────────────────────────────────
                if (root["Email"] is not JsonObject email) { email = new JsonObject(); root["Email"] = email; }

                if (dto.EmailSmtpHost != null) email["SmtpHost"] = dto.EmailSmtpHost;
                if (dto.EmailSmtpPort.HasValue) email["SmtpPort"] = dto.EmailSmtpPort.Value;
                if (dto.EmailUser     != null) email["User"]     = dto.EmailUser;
                if (dto.EmailFrom     != null) email["From"]     = dto.EmailFrom;
                if (!string.IsNullOrWhiteSpace(dto.EmailAppPassword) && !dto.EmailAppPassword!.Contains('*'))
                    email["AppPassword"] = dto.EmailAppPassword;

                var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                await System.IO.File.WriteAllTextAsync(SettingsPath, root.ToJsonString(opts));
                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  СТАТИСТИКА
        // ══════════════════════════════════════════════════════════════════════════

        // GET /api/builder/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var today       = DateOnly.FromDateTime(DateTime.UtcNow);
            var totalUsers  = await _db.Users.CountAsync();
            var totalLessons = await _db.Lessons.CountAsync();
            var totalModules = await _db.Modules.CountAsync();
            var totalCompletions = await _db.LessonCompletions.CountAsync();
            var totalErrors = await _db.LessonErrors.SumAsync(e => (long?)e.ErrorCount) ?? 0;
            var aiToday     = await _db.AiUsages
                .Where(u => u.Date == today)
                .SumAsync(u => (long?)u.MessageCount) ?? 0;
            var aiTotal     = await _db.AiUsages.SumAsync(u => (long?)u.MessageCount) ?? 0;
            var totalComments = await _db.LessonComments.CountAsync();

            return Ok(new
            {
                totalUsers, totalLessons, totalModules,
                totalCompletions, totalErrors,
                aiToday, aiTotal, totalComments
            });
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  СИСТЕМНАЯ ИНФОРМАЦИЯ
        // ══════════════════════════════════════════════════════════════════════════

        private static readonly DateTime _startTime = DateTime.UtcNow;

        // GET /api/builder/sysinfo
        [HttpGet("sysinfo")]
        public IActionResult GetSysInfo()
        {
            var uptime = DateTime.UtcNow - _startTime;
            var proc   = Process.GetCurrentProcess();

            // Читаем последние строки лог-файла
            var logLines = new List<string>();
            try
            {
                var logDir = Path.Combine(_env.ContentRootPath, "logs");
                if (Directory.Exists(logDir))
                {
                    var logFile = Directory.GetFiles(logDir, "app-*.log")
                        .OrderByDescending(f => f)
                        .FirstOrDefault();
                    if (logFile != null)
                    {
                        // Читаем хвост файла без блокировки
                        using var fs   = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var sr   = new StreamReader(fs);
                        var allLines   = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        logLines       = allLines.TakeLast(20).ToList();
                    }
                }
            }
            catch { /* лог недоступен — не страшно */ }

            // Проверяем доступность БД
            string dbStatus;
            try
            {
                _db.Database.CanConnect();
                dbStatus = "OK";
            }
            catch (Exception ex)
            {
                dbStatus = "ERROR: " + ex.Message[..Math.Min(80, ex.Message.Length)];
            }

            return Ok(new
            {
                environment    = _env.EnvironmentName,
                startedAt      = _startTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
                uptimeSeconds  = (long)uptime.TotalSeconds,
                uptimeHuman    = $"{(int)uptime.TotalHours}ч {uptime.Minutes}м {uptime.Seconds}с",
                os             = RuntimeInformation.OSDescription,
                framework      = RuntimeInformation.FrameworkDescription,
                architecture   = RuntimeInformation.ProcessArchitecture.ToString(),
                machineName    = Environment.MachineName,
                processorCount = Environment.ProcessorCount,
                memoryMb       = proc.WorkingSet64 / 1024 / 1024,
                peakMemoryMb   = proc.PeakWorkingSet64 / 1024 / 1024,
                dbStatus,
                logLines,
            });
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  ПОЛЬЗОВАТЕЛИ
        // ══════════════════════════════════════════════════════════════════════════

        // GET /api/builder/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.NickName,
                    u.Email,
                    u.UserName,
                    u.AvatarUrl,
                    createdAt = u.CreatedAt,
                    hasGoogle = u.GoogleId != "",
                })
                .ToListAsync();

            // Считаем прогресс каждого пользователя
            var completions = await _db.LessonCompletions
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var errors = await _db.LessonErrors
                .GroupBy(e => e.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.ErrorCount) })
                .ToListAsync();

            var compMap  = completions.ToDictionary(x => x.UserId, x => x.Count);
            var errorMap = errors.ToDictionary(x => x.UserId, x => x.Total);

            var result = users.Select(u => new
            {
                u.Id, u.NickName, u.Email, u.hasGoogle,
                avatarUrl   = AvatarHelper.GetUrl(u.NickName, u.AvatarUrl),
                createdAt   = u.createdAt,
                completions = compMap.GetValueOrDefault(u.Id, 0),
                errors      = errorMap.GetValueOrDefault(u.Id, 0),
            });

            return Ok(result);
        }

        // DELETE /api/builder/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            await _users.DeleteAsync(user);
            return NoContent();
        }

        // ── Вспомогательные функции ───────────────────────────────────────────────
        private static string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key) || key == "YOUR_API_KEY_HERE") return key;
            if (key.Length <= 8) return new string('*', key.Length);
            return key[..4] + new string('*', key.Length - 8) + key[^4..];
        }

        private static bool IsRealKey(string? key) =>
            !string.IsNullOrEmpty(key) && key != "YOUR_API_KEY_HERE";

        private async Task EnsureDefaultCourseAsync()
        {
            if (await _db.Courses.AnyAsync()) return;

            var course = new Course
            {
                Title = "C# / .NET",
                Slug = "csharp",
                Description = "Основной курс CodeLearn",
                Track = "BACKEND",
                Tag = "основы - oop - asp.net",
                Color = "#8f7cff",
                State = "available",
                Badge = "START",
                Categories = "backend",
                EstimatedHours = 6,
                Order = 0,
                IsPublished = true
            };
            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            var modules = await _db.Modules.Where(m => m.CourseId == null).ToListAsync();
            foreach (var module in modules) module.CourseId = course.Id;
            await _db.SaveChangesAsync();
        }

        private async Task<string> MakeUniqueSlugAsync(string? slug, string title, int? currentId = null)
        {
            var baseSlug = Slugify(string.IsNullOrWhiteSpace(slug) ? title : slug);
            var candidate = baseSlug;
            var suffix = 2;

            while (await _db.Courses.AnyAsync(c => c.Slug == candidate && (!currentId.HasValue || c.Id != currentId.Value)))
            {
                candidate = $"{baseSlug}-{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string Slugify(string value)
        {
            var cleaned = new string(value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray());

            while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
            cleaned = cleaned.Trim('-');
            return string.IsNullOrWhiteSpace(cleaned) ? "course" : cleaned;
        }
    }
}
