namespace CodeLearn.Services
{
    public interface IAuthService
    {
        Task<(bool ok, bool lockedOut)> LoginAsync(string email, string password);
        Task<bool>     EmailExistsAsync(string email);

        // Сброс пароля: токен (или null если юзера нет) и применение нового пароля
        Task<string?>  GeneratePasswordResetTokenAsync(string email);
        Task<(bool ok, IEnumerable<string> errors)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<AppUser?> FindByGoogleIdAsync(string googleId);
        Task           SignInAsync(AppUser user);
        Task           SignOutAsync();
        Task           EnsureLoginRecordAsync(AppUser user);

        Task<(AppUser? user, IEnumerable<string> errors)> CreateGoogleUserAsync(
            string googleId, string email, string nickName, string avatarUrl);

        Task<(AppUser? user, IEnumerable<string> errors)> CreateLocalUserAsync(
            string email, string password, string nickName, string avatarUrl);
    }
}
