namespace CodeLearn.Services
{
    public interface IAvatarService
    {
        Task<(bool ok, string? error, string? url)> SaveAsync(IFormFile file);
    }
}
