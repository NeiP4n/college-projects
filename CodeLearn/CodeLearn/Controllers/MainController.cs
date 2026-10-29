namespace CodeLearn.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private readonly AppDbContext           _db;
        private readonly UserManager<AppUser>   _userManager;
        private readonly IConfiguration         _config;

        public MainController(AppDbContext db, UserManager<AppUser> userManager, IConfiguration config)
        {
            _db          = db;
            _userManager = userManager;
            _config      = config;
        }

        public async Task<IActionResult> Index(string? course = null)
        {
            var selectedCourse = !string.IsNullOrWhiteSpace(course)
                ? course
                : HttpContext.Session.GetString("SelectedCourse");

            if (string.IsNullOrWhiteSpace(selectedCourse))
                selectedCourse = "csharp";

            HttpContext.Session.SetString("SelectedCourse", selectedCourse);

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var user   = await _db.Users
                .Include(u => u.LoginRecords)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var days = user.LoginRecords
                .Select(r => r.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            var today  = DateTime.UtcNow.Date;
            for (int i = 0; i < days.Count; i++)
            {
                if (days[i] == today.AddDays(-i)) streak++;
                else break;
            }

            var selectedCourseEntity = await _db.Courses
                .FirstOrDefaultAsync(c => c.Slug == selectedCourse);

            var modulesQuery = _db.Modules.AsQueryable();
            if (selectedCourseEntity != null)
                modulesQuery = modulesQuery.Where(m => m.CourseId == selectedCourseEntity.Id);

            var modules = await modulesQuery
                .Include(m => m.Lessons)
                .OrderBy(m => m.Order)
                .ToListAsync();

            // Какие модули полностью пройдены (все уроки завершены)?
            var allModuleLessonIds = modules
                .SelectMany(m => m.Lessons.Select(l => l.Id))
                .ToHashSet();

            var userCompletedLessonIds = await _db.LessonCompletions
                .Where(c => c.UserId == userId && allModuleLessonIds.Contains(c.LessonId))
                .Select(c => c.LessonId)
                .ToHashSetAsync();

            var completedModuleIds = modules
                .Where(m => m.Lessons.Any() && m.Lessons.All(l => userCompletedLessonIds.Contains(l.Id)))
                .Select(m => m.Id)
                .ToHashSet();

            ViewBag.CompletedModuleIds = completedModuleIds;

            // ── Разблокировка по прохождению ─────────────────────────────────────
            // ProgressionLock=true: открыт первый модуль; следующий — только когда
            // предыдущий полностью пройден. false → всё открыто (свободный доступ).
            var progressionLock = _config.GetValue("Platform:ProgressionLock", true);
            HashSet<int> unlockedModuleIds;
            if (!progressionLock)
            {
                unlockedModuleIds = modules.Select(m => m.Id).ToHashSet();
            }
            else
            {
                unlockedModuleIds = new HashSet<int>();
                var prevDone = true;                       // первый модуль всегда открыт
                foreach (var m in modules)                 // modules уже отсортированы по Order
                {
                    if (prevDone) unlockedModuleIds.Add(m.Id);
                    // пустой модуль не блокирует цепочку
                    prevDone = !m.Lessons.Any() || completedModuleIds.Contains(m.Id);
                }
            }
            ViewBag.UnlockedModuleIds = unlockedModuleIds;
            ViewBag.ProgressionLock   = progressionLock;

            // «Следующий» модуль для анимации тряски — первый открытый, но не пройденный
            ViewBag.NextModuleId = modules
                .Where(m => unlockedModuleIds.Contains(m.Id)
                            && !completedModuleIds.Contains(m.Id)
                            && m.Lessons.Any())
                .Select(m => (int?)m.Id)
                .FirstOrDefault() ?? 0;

            ViewBag.CourseId = selectedCourse;
            ViewBag.CourseDbId = selectedCourseEntity?.Id ?? 0;
            ViewBag.CourseTitle = selectedCourseEntity?.Title
                ?? (selectedCourse.Equals("csharp", StringComparison.OrdinalIgnoreCase)
                    ? "C# / .NET"
                    : selectedCourse.ToUpperInvariant());

            var totalErrors = await _db.LessonErrors
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.ErrorCount);

            return View(new MainViewModel
            {
                NickName      = user.NickName,
                AvatarUrl     = user.AvatarUrl,
                Streak        = streak,
                TotalDays     = days.Count,
                CurrentModule = user.CurrentModule,
                TotalErrors   = totalErrors,
                Modules       = modules
            });
        }
    }
}
