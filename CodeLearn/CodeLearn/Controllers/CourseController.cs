using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeLearn.Data;

namespace CodeLearn.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly AppDbContext   _db;
        private readonly IConfiguration _config;
        public CourseController(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        public async Task<IActionResult> Index(int id)
        {
            var module = await _db.Modules
                .Include(m => m.Lessons.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var completedIds = (await _db.LessonCompletions
                .Where(c => c.UserId == userId)
                .Select(c => c.LessonId)
                .ToListAsync())
                .ToHashSet();

            var progressionLock = _config.GetValue("Platform:ProgressionLock", true);

            // ── Гейт модуля: открыт только если предыдущие модули курса пройдены ──
            if (progressionLock && module.CourseId != null)
            {
                var courseModules = await _db.Modules
                    .Where(m => m.CourseId == module.CourseId)
                    .OrderBy(m => m.Order)
                    .Include(m => m.Lessons)
                    .ToListAsync();

                var prevDone = true;
                var unlocked = false;
                foreach (var m in courseModules)
                {
                    var open = prevDone;
                    if (m.Id == module.Id) { unlocked = open; break; }
                    var done = !m.Lessons.Any() ||
                               m.Lessons.All(l => completedIds.Contains(l.Id));
                    prevDone = done;
                }
                if (!unlocked) return RedirectToAction("Index", "Main");
            }

            // ── Доступность уроков: первый открыт, следующий — после прохождения ──
            var accessible = new HashSet<int>();
            if (!progressionLock)
            {
                accessible = module.Lessons.Select(l => l.Id).ToHashSet();
            }
            else
            {
                var prevDone = true;
                foreach (var l in module.Lessons)   // уже отсортированы по Order
                {
                    if (prevDone) accessible.Add(l.Id);
                    prevDone = completedIds.Contains(l.Id);
                }
            }

            ViewBag.CompletedLessonIds  = completedIds;
            ViewBag.AccessibleLessonIds = accessible;
            ViewBag.ProgressionLock     = progressionLock;
            return View(module);
        }
    }
}
