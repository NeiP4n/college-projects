using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeLearn.Data;
using CodeLearn.Helpers;
using CodeLearn.Models;

namespace CodeLearn.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        private readonly AppDbContext   _db;
        private readonly IConfiguration _config;
        public LessonController(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        public async Task<IActionResult> Index(int id)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m!.Course)
                .Include(l => l.ContentBlocks.OrderBy(b => b.Order))
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Прогрессия: нельзя открыть заблокированный/недоступный урок прямым URL.
            if (lesson.IsLocked)
                return RedirectToAction("Index", "Main");

            var progressionLock = _config.GetValue("Platform:ProgressionLock", true);
            if (progressionLock)
            {
                // Урок доступен, если он первый в модуле или предыдущий пройден
                var siblings = await _db.Lessons
                    .Where(l => l.ModuleId == lesson.ModuleId)
                    .OrderBy(l => l.Order)
                    .Select(l => l.Id)
                    .ToListAsync();

                var completed = await _db.LessonCompletions
                    .Where(c => c.UserId == userId)
                    .Select(c => c.LessonId)
                    .ToHashSetAsync();

                var idx = siblings.IndexOf(lesson.Id);
                var accessible = idx == 0 || (idx > 0 && completed.Contains(siblings[idx - 1]));
                if (!accessible)
                    return RedirectToAction("Index", "Course", new { id = lesson.ModuleId });
            }
            ViewBag.IsCompleted = await _db.LessonCompletions
                .AnyAsync(c => c.UserId == userId && c.LessonId == id);
            ViewBag.CompletedBlockCount = await _db.LessonProgresses
                .Where(p => p.UserId == userId && p.LessonId == id)
                .Select(p => p.CompletedBlockCount)
                .FirstOrDefaultAsync();

            var me = await _db.Users.FindAsync(userId);
            ViewBag.MyAvatarUrl = AvatarHelper.GetUrl(me?.NickName, me?.AvatarUrl);

            // Язык курса как fallback для блоков без явного поля language
            var slug = lesson.Module?.Course?.Slug ?? "";
            ViewBag.DefaultLang = slug switch {
                var s when s.Contains("python")     => "python",
                var s when s.Contains("js")         => "javascript",
                var s when s.Contains("java")       => "java",
                _                                   => "csharp"
            };

            return View(lesson);
        }

        // Вызывается JS-ом при завершении урока
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lesson = await _db.Lessons
                .Include(l => l.ContentBlocks)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();

            var blockCount = lesson.ContentBlocks.Count;

            // ── Проверка прогресса: требуем прохождения всех блоков ──────────────
            if (blockCount > 0)
            {
                var progress = await _db.LessonProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);

                var completedBlocks = progress?.CompletedBlockCount ?? 0;
                var required        = blockCount; // все блоки

                if (completedBlocks < required)
                    return BadRequest(new { ok = false, error = "Урок ещё не завершён", completedBlocks, required });
            }

            if (!await _db.LessonCompletions.AnyAsync(c => c.UserId == userId && c.LessonId == id))
            {
                _db.LessonCompletions.Add(new UserLessonCompletion
                {
                    UserId      = userId,
                    LessonId    = id,
                    CompletedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            await SaveProgressInternal(userId, id, blockCount);

            return Ok(new { ok = true });
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Progress(int id, [FromBody] LessonProgressRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var blockCount = await _db.ContentBlocks.CountAsync(b => b.LessonId == id);
            if (blockCount == 0 && !await _db.Lessons.AnyAsync(l => l.Id == id)) return NotFound();

            var completedBlocks = Math.Clamp(request.CompletedBlocks, 0, blockCount);
            await SaveProgressInternal(userId, id, completedBlocks);

            return Ok(new { ok = true, completedBlocks });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var record = await _db.LessonCompletions
                .FirstOrDefaultAsync(c => c.UserId == userId && c.LessonId == id);
            if (record != null)
            {
                _db.LessonCompletions.Remove(record);
            }
            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);
            if (progress != null)
            {
                _db.LessonProgresses.Remove(progress);
            }
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }

        private async Task SaveProgressInternal(int userId, int lessonId, int completedBlocks)
        {
            var progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                _db.LessonProgresses.Add(new UserLessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    CompletedBlockCount = completedBlocks,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (completedBlocks >= progress.CompletedBlockCount)
            {
                progress.CompletedBlockCount = completedBlocks;
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public record LessonProgressRequest(int CompletedBlocks);

        // POST /Lesson/AddError?id={lessonId} — вызывается JS при неправильном ответе
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddError(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var record = await _db.LessonErrors
                .FirstOrDefaultAsync(e => e.UserId == userId && e.LessonId == id);

            if (record == null)
            {
                _db.LessonErrors.Add(new UserLessonError
                {
                    UserId     = userId,
                    LessonId   = id,
                    ErrorCount = 1
                });
            }
            else
            {
                record.ErrorCount++;
            }

            await _db.SaveChangesAsync();

            // Возвращаем количество ошибок в этом уроке и суммарно по всем урокам
            var lessonErrors = record?.ErrorCount ?? 1;
            var totalErrors  = await _db.LessonErrors
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.ErrorCount);

            return Ok(new { lessonErrors, totalErrors });
        }

        // GET /Lesson/GetErrors?id={lessonId} — сколько ошибок в текущем уроке
        [HttpGet]
        public async Task<IActionResult> GetErrors(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lessonErrors = await _db.LessonErrors
                .Where(e => e.UserId == userId && e.LessonId == id)
                .Select(e => e.ErrorCount)
                .FirstOrDefaultAsync();

            var totalErrors = await _db.LessonErrors
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.ErrorCount);

            return Ok(new { lessonErrors, totalErrors });
        }

        // ── Чат урока ─────────────────────────────────────────────────────────────

        // GET /Lesson/Comments?id={lessonId}&page={page}
        [HttpGet]
        public async Task<IActionResult> Comments(int id, int page = 1)
        {
            const int pageSize = 30;

            var total = await _db.LessonComments.CountAsync(c => c.LessonId == id);

            var userId2 = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rawComments = await _db.LessonComments
                .Where(c => c.LessonId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.User)
                .Select(c => new
                {
                    id        = c.Id,
                    text      = c.Text,
                    createdAt = c.CreatedAt,
                    nickName  = c.User.NickName,
                    avatarUrl = c.User.AvatarUrl,
                    isOwn     = c.UserId == userId2
                })
                .ToListAsync();

            // Применяем letter-avatar и разворачиваем в хронологический порядок
            rawComments.Reverse();
            var comments = rawComments.Select(c => new {
                c.id, c.text, c.createdAt, c.nickName,
                avatarUrl = AvatarHelper.GetUrl(c.nickName, c.avatarUrl),
                c.isOwn
            }).ToList();

            return Ok(new { comments, total, page, pageSize });
        }

        // POST /Lesson/PostComment?id={lessonId}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int id, [FromBody] PostCommentRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Text) || req.Text.Length > 1000)
                return BadRequest(new { error = "Сообщение пустое или слишком длинное" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = new LessonComment
            {
                LessonId  = id,
                UserId    = userId,
                Text      = req.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.LessonComments.Add(comment);
            await _db.SaveChangesAsync();

            // Возвращаем полный объект с данными пользователя
            var user = await _db.Users.FindAsync(userId);
            return Ok(new
            {
                id        = comment.Id,
                text      = comment.Text,
                createdAt = comment.CreatedAt,
                nickName  = user!.NickName,
                avatarUrl = AvatarHelper.GetUrl(user.NickName, user.AvatarUrl),
                isOwn     = true
            });
        }

        // GET /Lesson/NewComments?id={lessonId}&afterId={lastSeenId}
        // Возвращает только сообщения новее afterId — используется для polling
        [HttpGet]
        public async Task<IActionResult> NewComments(int id, int afterId = 0)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rawNew = await _db.LessonComments
                .Where(c => c.LessonId == id && c.Id > afterId)
                .OrderBy(c => c.Id)
                .Take(50)
                .Include(c => c.User)
                .Select(c => new
                {
                    id        = c.Id,
                    text      = c.Text,
                    createdAt = c.CreatedAt,
                    nickName  = c.User.NickName,
                    avatarUrl = c.User.AvatarUrl,
                    isOwn     = c.UserId == userId
                })
                .ToListAsync();

            var comments = rawNew.Select(c => new {
                c.id, c.text, c.createdAt, c.nickName,
                avatarUrl = AvatarHelper.GetUrl(c.nickName, c.avatarUrl),
                c.isOwn
            }).ToList();

            var total = await _db.LessonComments.CountAsync(c => c.LessonId == id);
            return Ok(new { comments, total });
        }

        // DELETE /Lesson/DeleteComment/{commentId}
        [HttpDelete("{commentId}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = await _db.LessonComments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId) return Forbid();
            _db.LessonComments.Remove(comment);
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }

        public record PostCommentRequest(string Text);

        // POST /Lesson/Check — серверная проверка ответа (без утечки ответа в HTML)
        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Check([FromBody] CheckRequest req)
        {
            var block = await _db.ContentBlocks
                .Include(b => b.Lesson)
                .FirstOrDefaultAsync(b => b.Id == req.BlockId);

            if (block == null) return NotFound();

            // ── IDOR: блок должен принадлежать незаблокированному уроку ─────────
            if (block.Lesson == null || block.Lesson.IsLocked)
                return Forbid();

            JsonElement content;
            try { content = JsonDocument.Parse(block.Content).RootElement; }
            catch { return BadRequest(new { correct = false }); }

            bool correct;
            string? explanation = null;

            switch (block.Type)
            {
                // ── Тесты с вариантами (индекс) ──────────────────────────────────
                case "quiz":
                case "knowledge_check":
                case "transfer_test":
                case "code_to_intent":
                case "compare_variants":
                {
                    int correctIdx = 0;
                    if      (content.TryGetProperty("correct",       out var c1)) correctIdx = c1.GetInt32();
                    else if (content.TryGetProperty("correctIntent", out var c2)) correctIdx = c2.GetInt32();
                    else if (content.TryGetProperty("correctIndex",  out var c3)) correctIdx = c3.GetInt32();
                    correct = int.TryParse(req.Answer, out var chosen) && chosen == correctIdx;
                    if (correct && content.TryGetProperty("explanation", out var expl))
                        explanation = expl.GetString();
                    break;
                }
                // ── Заполни пропуски ─────────────────────────────────────────────
                case "fill_blank":
                case "template_completion":
                {
                    var blanks = content.TryGetProperty("blanks", out var bl)
                        ? bl.EnumerateArray().Select(b => b.GetString() ?? "").ToList()
                        : new List<string>();
                    var given = req.Answer.Split('|');
                    correct = blanks.Count > 0 &&
                              given.Length == blanks.Count &&
                              given.Zip(blanks, (g, b) => NormalizeAnswer(g) == NormalizeAnswer(b)).All(ok => ok);
                    break;
                }
                // ── Порядок блоков ───────────────────────────────────────────────
                case "drag_order":
                {
                    var solution = content.TryGetProperty("solution", out var sol)
                        ? sol.EnumerateArray().Select(s => s.GetInt32()).ToList()
                        : new List<int>();
                    var userOrder = req.Answer.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out var n) ? n : -1)
                        .ToList();
                    correct = solution.Count > 0 && userOrder.SequenceEqual(solution);
                    break;
                }
                // ── Предсказание вывода ──────────────────────────────────────────
                // Пользователь ВРУЧНУЮ печатает, что выведет программа.
                // Здесь переносы строк и регистр ЗНАЧИМЫ (это и есть суть задания):
                // ответ в одну строку или в другом регистре — НЕ верный.
                case "prediction":
                {
                    var expected = GetExpected(content);
                    if (expected == null) { correct = req.Answer.Trim().Length > 0; break; }
                    correct = NormalizeOutput(req.Answer) == NormalizeOutput(expected);
                    break;
                }
                // ── Всё остальное: текстовый ответ ИЛИ вывод запущенного кода ─────
                // Клиент шлёт: для текстовых блоков — введённый текст,
                // для кодовых (data-run) — фактический вывод программы.
                default:
                {
                    var got = NormalizeAnswer(req.Answer);
                    if (got.Length == 0) { correct = false; break; }   // пустой ответ — не верно

                    var expected = GetExpected(content);
                    if (expected == null)
                    {
                        // Эталона в блоке нет (открытый вопрос) — принимаем непустой ответ
                        correct = true;
                        break;
                    }
                    var exp = NormalizeAnswer(expected);
                    correct = got == exp || (exp.Length >= 3 && got.Contains(exp));
                    break;
                }
            }

            return Ok(new { correct, explanation });
        }

        // Достаёт эталонный ответ из любого из известных полей блока
        private static string? GetExpected(JsonElement c)
        {
            foreach (var field in new[] { "answer", "expectedOutput", "expected", "edgeCaseInput" })
                if (c.TryGetProperty(field, out var v) &&
                    v.ValueKind == JsonValueKind.String)
                {
                    var s = v.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            return null;
        }

        private static string NormalizeAnswer(string s) =>
            (s ?? "").Trim().ToLowerInvariant()
                     .Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ")
                     .Replace("  ", " ");

        // Сравнение вывода программы: переносы строк и регистр ЗНАЧИМЫ,
        // но хвостовые пробелы в строках и пустые строки в начале/конце игнорируются
        // (чтобы лишний пробел или перевод строки в конце не считались ошибкой).
        private static string NormalizeOutput(string s)
        {
            s = (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = s.Split('\n').Select(l => l.TrimEnd()).ToList();
            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0) lines.RemoveAt(lines.Count - 1);
            while (lines.Count > 0 && lines[0].Length == 0) lines.RemoveAt(0);
            return string.Join("\n", lines);
        }

        public record CheckRequest(int BlockId, string Answer);
    }
}
