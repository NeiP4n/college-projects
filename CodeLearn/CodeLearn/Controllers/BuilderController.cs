using CodeLearn.Data;
using CodeLearn.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeLearn.Controllers
{
    [LocalOnly]
    [Route("Builder")]
    public class BuilderController : Controller
    {
        private readonly AppDbContext _db;
        public BuilderController(AppDbContext db) => _db = db;

        [Route("")]
        [Route("Index")]
        public IActionResult Index() => View();

        [Route("MapEditor")]
        public async Task<IActionResult> MapEditor(int? courseId)
        {
            var course = courseId.HasValue
                ? await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId)
                : await _db.Courses.OrderBy(c => c.Order).FirstOrDefaultAsync();

            if (course == null) return RedirectToAction("Index");

            var modules = await _db.Modules
                .Where(m => m.CourseId == course.Id)
                .OrderBy(m => m.Order)
                .Include(m => m.Lessons)
                .ToListAsync();

            var objects = await _db.MapObjects
                .Where(o => (o.CourseId == course.Id || o.CourseId == null) && o.Type != "map_ext")
                .OrderBy(o => o.ZIndex).ThenBy(o => o.Id)
                .ToListAsync();

            ViewBag.Course  = course;
            ViewBag.Objects = objects;
            return View(modules);
        }
    }
}
