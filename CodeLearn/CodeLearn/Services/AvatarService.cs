namespace CodeLearn.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> _allowed = [".png", ".jpg", ".jpeg", ".webp"];
        private const long MaxBytes = 2 * 1024 * 1024;

        public AvatarService(IWebHostEnvironment env) => _env = env;

        public async Task<(bool ok, string? error, string? url)> SaveAsync(IFormFile file)
        {
            if (file.Length > MaxBytes)
                return (false, "Файл слишком большой (макс. 2МБ)", null);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowed.Contains(ext))
                return (false, "Недопустимый формат файла", null);

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return (false, "Это не изображение", null);

            var dir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path     = Path.Combine(dir, fileName);
            await using var stream = File.Create(path);
            await file.CopyToAsync(stream);

            return (true, null, $"/uploads/avatars/{fileName}");
        }
    }
}
