using System.Security.Claims;
using CodeLearn.Data;
using CodeLearn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeLearn.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private const string SelectedCourseSessionKey = "SelectedCourse";
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public CoursesController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var courses = await _db.Courses
                .Include(c => c.Modules.OrderBy(m => m.Order))
                    .ThenInclude(m => m.Lessons)
                .OrderBy(c => c.Order)
                .ToListAsync();

            var lessonIds = courses
                .SelectMany(c => c.Modules)
                .SelectMany(m => m.Lessons)
                .Select(l => l.Id)
                .ToHashSet();

            var completedIds = (await _db.LessonCompletions
                .Where(c => c.UserId == userId && lessonIds.Contains(c.LessonId))
                .Select(c => c.LessonId)
                .ToListAsync())
                .ToHashSet();

            var model = new CourseSelectionViewModel
            {
                NickName = user.NickName,
                TotalModules = courses.Sum(c => c.Modules.Count),
                Courses = courses
                    .Where(c => c.IsPublished)
                    .Select(c => BuildCourseCard(c, completedIds))
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Select(string id)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Slug == id && c.State == "available");
            if (course == null)
                return RedirectToAction(nameof(Index));

            HttpContext.Session.SetString(SelectedCourseSessionKey, course.Slug);
            return RedirectToAction("Index", "Main", new { course = course.Slug });
        }

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

        private static CourseCardViewModel BuildCourseCard(Course course, HashSet<int> completedIds)
        {
            var lessonsCount = course.Modules.Sum(m => m.Lessons.Count);
            var done = course.Modules
                .SelectMany(m => m.Lessons)
                .Count(l => completedIds.Contains(l.Id));
            var progress = lessonsCount > 0 ? done * 100 / lessonsCount : 0;

            return new CourseCardViewModel
            {
                Id = course.Slug,
                Title = course.Title,
                Track = course.Track,
                Tag = course.Tag,
                Color = course.Color,
                State = course.State,
                Badge = course.State == "available" && progress > 0 ? "PLAY" : course.Badge,
                Categories = course.Categories,
                LessonsCount = lessonsCount,
                ModulesCount = course.Modules.Count,
                EstimatedHours = course.EstimatedHours,
                ProgressPercent = progress,
                Icon = course.Slug,
                Requirement = course.State switch
                {
                    "available" => progress > 0 ? "Продолжить" : "Начать",
                    "soon" => "Скоро",
                    _ => "Locked"
                }
            };
        }
    }
}
