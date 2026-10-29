// CodeLearn\Controllers\MapObjectsApiController.cs
// CRUD для произвольных визуальных объектов на карте курса.
// Доступен только локально (LocalOnly) — часть Builder-а.

using System.Text.RegularExpressions;
using CodeLearn.Data;
using CodeLearn.Helpers;
using CodeLearn.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeLearn.Controllers
{
    // ── DTO ───────────────────────────────────────────────────────────────────────
    public record MapObjectDto(
        int?    CourseId,
        string  Type,
        string  Src,
        string? Label,
        double? MapX,
        double? MapY,
        int?    Width,
        int?    Height,
        bool?   Invert,
        double? Rotation,
        int?    ZIndex);

    public record MapObjectPositionDto(double MapX, double MapY, double? Rotation);

    public record MapSettingsDto(int? CourseId, int TopExtPx);

    // ВАЖНО: контроллер НЕ помечен [LocalOnly] целиком — иначе чтение объектов карты
    // (GET) недоступно удалённым игрокам и PNG/объекты у них не отображаются.
    // Чтение (GET) — публичное; изменение (POST/PUT/DELETE/upload) — только локально.
    [ApiController]
    [Route("api/builder/map-objects")]
    [IgnoreAntiforgeryToken]
    public class MapObjectsApiController : ControllerBase
    {
        // Служебный объект, хранящий расширение карты вверх (px) — в поле Height.
        private const string ExtType = "map_ext";
        private readonly AppDbContext        _db;
        private readonly IWebHostEnvironment _env;

        public MapObjectsApiController(AppDbContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        // ── Санитизация SVG: вырезаем всё исполняемое ──────────────────────────────
        // Объект рендерится через innerHTML на публичной карте, поэтому SVG обязан
        // быть безопасным независимо от того, кто его создал.
        private static readonly Regex[] _svgDanger =
        {
            new(@"<\s*script\b[^>]*>[\s\S]*?<\s*/\s*script\s*>", RegexOptions.IgnoreCase),
            new(@"<\s*script\b[^>]*/?>",                          RegexOptions.IgnoreCase),
            new(@"<\s*foreignObject\b[\s\S]*?<\s*/\s*foreignObject\s*>", RegexOptions.IgnoreCase),
            new(@"<\s*(iframe|object|embed|link|meta|style)\b[\s\S]*?(>|$)", RegexOptions.IgnoreCase),
            new(@"\son\w+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)",     RegexOptions.IgnoreCase), // on*=...
            new(@"(href|xlink:href)\s*=\s*(""\s*javascript:[^""]*""|'\s*javascript:[^']*'|javascript:[^\s>]*)", RegexOptions.IgnoreCase),
        };

        private static string SanitizeSvg(string svg)
        {
            if (string.IsNullOrEmpty(svg)) return svg;
            foreach (var rx in _svgDanger)
                svg = rx.Replace(svg, "");
            return svg;
        }

        private static string SanitizeBySrcType(string type, string src) =>
            type == "svg_inline" ? SanitizeSvg(src) : src;

        // GET /api/builder/map-objects?courseId=1  — публично (видят все игроки)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? courseId)
        {
            var q = _db.MapObjects.Where(o => o.Type != ExtType);   // служебную запись не отдаём
            if (courseId.HasValue)
                q = q.Where(o => o.CourseId == courseId || o.CourseId == null);
            return Ok(await q.OrderBy(o => o.ZIndex).ThenBy(o => o.Id).ToListAsync());
        }

        // GET /api/builder/map-settings?courseId=1  — публично: расширение карты вверх
        [HttpGet("/api/builder/map-settings")]
        public async Task<IActionResult> GetSettings([FromQuery] int? courseId)
        {
            var ext = await _db.MapObjects
                .Where(o => o.Type == ExtType && (o.CourseId == courseId || o.CourseId == null))
                .OrderByDescending(o => o.CourseId)   // курс-специфичная запись приоритетнее глобальной
                .FirstOrDefaultAsync();
            return Ok(new { topExtPx = ext?.Height ?? 0 });
        }

        // PUT /api/builder/map-settings  — только локально (редактор)
        [HttpPut("/api/builder/map-settings")]
        [LocalOnly]
        public async Task<IActionResult> SaveSettings([FromBody] MapSettingsDto dto)
        {
            var px = Math.Max(0, Math.Min(4000, dto.TopExtPx));
            var ext = await _db.MapObjects
                .FirstOrDefaultAsync(o => o.Type == ExtType && o.CourseId == dto.CourseId);
            if (ext == null)
            {
                ext = new MapObject
                {
                    Type = ExtType, CourseId = dto.CourseId, Src = "", Label = "map-ext",
                    Width = 0, Height = px, MapX = 0, MapY = 0, ZIndex = -999
                };
                _db.MapObjects.Add(ext);
            }
            else ext.Height = px;
            await _db.SaveChangesAsync();
            return Ok(new { topExtPx = px });
        }

        // POST /api/builder/map-objects  — только локально
        [HttpPost]
        [LocalOnly]
        public async Task<IActionResult> Create([FromBody] MapObjectDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Src))
                return BadRequest(new { error = "Src обязателен" });

            var type = dto.Type ?? "svg_inline";
            var obj = new MapObject
            {
                CourseId = dto.CourseId,
                Type     = type,
                Src      = SanitizeBySrcType(type, dto.Src),
                Label    = dto.Label ?? "",
                MapX     = dto.MapX     ?? 50,
                MapY     = dto.MapY     ?? 50,
                Width    = dto.Width    ?? 48,
                Height   = dto.Height   ?? 0,
                Invert   = dto.Invert   ?? false,
                Rotation = dto.Rotation ?? 0,
                ZIndex   = dto.ZIndex   ?? 0,
            };
            _db.MapObjects.Add(obj);
            await _db.SaveChangesAsync();
            return Ok(obj);
        }

        // PUT /api/builder/map-objects/{id}
        [HttpPut("{id}")]
        [LocalOnly]
        public async Task<IActionResult> Update(int id, [FromBody] MapObjectDto dto)
        {
            var obj = await _db.MapObjects.FindAsync(id);
            if (obj == null) return NotFound();

            if (dto.CourseId.HasValue) obj.CourseId = dto.CourseId.Value;
            obj.Type     = dto.Type  ?? obj.Type;
            // Src обновляем только если реально передали непустую строку
            // (редактор шлёт src:"" когда меняет лишь позицию/размер — не затираем)
            if (!string.IsNullOrEmpty(dto.Src))
                obj.Src = SanitizeBySrcType(obj.Type, dto.Src);
            obj.Label    = dto.Label ?? obj.Label;
            if (dto.MapX.HasValue)     obj.MapX     = dto.MapX.Value;
            if (dto.MapY.HasValue)     obj.MapY     = dto.MapY.Value;
            if (dto.Width.HasValue)    obj.Width     = dto.Width.Value;
            if (dto.Height.HasValue)   obj.Height    = dto.Height.Value;
            if (dto.Invert.HasValue)   obj.Invert    = dto.Invert.Value;
            if (dto.Rotation.HasValue) obj.Rotation  = dto.Rotation.Value;
            if (dto.ZIndex.HasValue)   obj.ZIndex    = dto.ZIndex.Value;

            await _db.SaveChangesAsync();
            return Ok(obj);
        }

        // PUT /api/builder/map-objects/{id}/position  (drag-and-drop)
        [HttpPut("{id}/position")]
        [LocalOnly]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] MapObjectPositionDto dto)
        {
            var obj = await _db.MapObjects.FindAsync(id);
            if (obj == null) return NotFound();

            obj.MapX = dto.MapX;
            obj.MapY = dto.MapY;
            if (dto.Rotation.HasValue) obj.Rotation = dto.Rotation.Value;

            await _db.SaveChangesAsync();
            return Ok(new { obj.Id, obj.MapX, obj.MapY, obj.Rotation });
        }

        // DELETE /api/builder/map-objects/{id}
        [HttpDelete("{id}")]
        [LocalOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var obj = await _db.MapObjects.FindAsync(id);
            if (obj == null) return NotFound();
            _db.MapObjects.Remove(obj);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/builder/map-objects/upload-image
        // Загружает PNG/SVG файл и возвращает URL для использования как image_url
        [HttpPost("upload-image")]
        [LocalOnly]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Файл не передан" });

            if (file.Length > 4 * 1024 * 1024)
                return BadRequest(new { error = "Файл слишком большой (макс. 4МБ)" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new HashSet<string> { ".png", ".jpg", ".jpeg", ".webp", ".svg", ".gif" };
            if (!allowed.Contains(ext))
                return BadRequest(new { error = "Допустимые форматы: PNG, JPG, SVG, GIF, WEBP" });

            var dir = Path.Combine(_env.WebRootPath, "uploads", "map");
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path     = Path.Combine(dir, fileName);

            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);

            return Ok(new { url = $"/uploads/map/{fileName}", fileName });
        }
    }
}
