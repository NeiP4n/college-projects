using CodeLearn.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeLearn.Data
{
    public static class CourseSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // ── C# course ────────────────────────────────────────────────────────────
            var csharp = await db.Courses.FirstOrDefaultAsync(c => c.Slug == "csharp");
            if (csharp == null)
            {
                csharp = new Course
                {
                    Title          = "C# / .NET",
                    Slug           = "csharp",
                    Description    = "Основы – ООП – ASP.NET",
                    Track          = "BACKEND",
                    Tag            = "основы - oop - asp.net",
                    Color          = "#8f7cff",
                    State          = "available",
                    Badge          = "START",
                    Categories     = "backend",
                    EstimatedHours = 6,
                    Order          = 0,
                    IsPublished    = true
                };
                db.Courses.Add(csharp);
                await db.SaveChangesAsync();
            }

            // ── Assign C# modules (IDs: 24, 463–479) ─────────────────────────────────
            var csharpIds = new HashSet<int> { 24, 463, 464, 465, 466, 467, 468, 469, 470, 471, 472, 473, 474, 475, 476, 477, 478, 479 };
            var csharpModules = await db.Modules.Where(m => csharpIds.Contains(m.Id)).ToListAsync();
            foreach (var m in csharpModules)
                m.CourseId = csharp.Id;

            await db.SaveChangesAsync();
        }
    }
}
