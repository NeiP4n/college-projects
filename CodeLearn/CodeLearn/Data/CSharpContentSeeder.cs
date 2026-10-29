using System.Text.Json;
using CodeLearn.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeLearn.Data
{
    public static class CSharpContentSeeder
    {
        public static async Task SeedContentAsync(AppDbContext db)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == "csharp");
            if (course == null) return;

            var lessons = await db.Lessons
                .Include(l => l.ContentBlocks)
                .Include(l => l.Module)
                .Where(l => l.Module != null && l.Module.CourseId == course.Id && !l.Module.IsChallenge)
                .ToListAsync();

            int added = 0;
            foreach (var lesson in lessons)
            {
                if (lesson.ContentBlocks.Any()) continue;
                var blocks = GetBlocks(lesson.Title);
                for (int i = 0; i < blocks.Count; i++)
                    db.ContentBlocks.Add(new ContentBlock
                    {
                        LessonId = lesson.Id,
                        Type     = blocks[i].t,
                        Content  = blocks[i].c,
                        Order    = i
                    });
                if (blocks.Count > 0) added++;
            }
            if (added > 0) await db.SaveChangesAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        static string J(string s) => JsonSerializer.Serialize(s);
        static string JArr(IEnumerable<string> a) => "[" + string.Join(",", a.Select(J)) + "]";

        static (string t, string c) Text(string md) =>
            ("text", $"{{\"markdown\":{J(md)}}}");

        static (string t, string c) Code(string title, string code, string lang = "csharp") =>
            ("code", $"{{\"language\":{J(lang)},\"title\":{J(title)},\"code\":{J(code)}}}");

        static (string t, string c) Quiz(string q, int correct, string expl, params string[] opts) =>
            ("quiz", $"{{\"question\":{J(q)},\"options\":{JArr(opts)},\"correct\":{correct},\"explanation\":{J(expl)}}}");

        static (string t, string c) Task_(string title, string desc, string starter, string expected) =>
            ("task", $"{{\"title\":{J(title)},\"description\":{J(desc)},\"starterCode\":{J(starter)},\"expectedOutput\":{J(expected)},\"language\":\"csharp\"}}");

        static (string t, string c) CQ(string q, string a) =>
            ("control_question", $"{{\"question\":{J(q)},\"answer\":{J(a)}}}");

        static (string t, string c) SA(string q, string a, string hint = "") =>
            ("short_answer", $"{{\"question\":{J(q)},\"answer\":{J(a)},\"hint\":{J(hint)}}}");

        // ── Router ───────────────────────────────────────────────────────────
        static List<(string t, string c)> GetBlocks(string title) => title switch
        {
            _ => new()
        };
    }
}
