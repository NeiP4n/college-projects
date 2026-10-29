namespace CodeLearn.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser>       _userManager;
        private readonly SignInManager<AppUser>     _signInManager;
        private readonly AppDbContext               _db;

        public AuthService(
            UserManager<AppUser>   userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext           db)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _db            = db;
        }

        public async Task<(bool ok, bool lockedOut)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return (false, false);

            var result = await _signInManager.PasswordSignInAsync(user, password,
                isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
                await EnsureLoginRecordAsync(user);

            return (result.Succeeded, result.IsLockedOut);
        }

        public Task<bool> EmailExistsAsync(string email) =>
            _userManager.Users.AnyAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<(bool ok, IEnumerable<string> errors)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return (false, new[] { "Пользователь не найден" });
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public Task<AppUser?> FindByGoogleIdAsync(string googleId) =>
            _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

        public Task SignInAsync(AppUser user) =>
            _signInManager.SignInAsync(user, isPersistent: true);

        public Task SignOutAsync() =>
            _signInManager.SignOutAsync();

        public async Task EnsureLoginRecordAsync(AppUser user)
        {
            var today = DateTime.UtcNow.Date;
            if (!await _db.LoginRecords.AnyAsync(r => r.AppUserId == user.Id && r.Date == today))
            {
                _db.LoginRecords.Add(new LoginRecord { AppUserId = user.Id, Date = today });
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(AppUser? user, IEnumerable<string> errors)> CreateGoogleUserAsync(
            string googleId, string email, string nickName, string avatarUrl)
        {
            var user = new AppUser
            {
                GoogleId       = googleId,
                Email          = email,
                UserName       = email,
                NickName       = nickName,
                AvatarUrl      = avatarUrl,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return (null, result.Errors.Select(e => e.Description));

            _db.LoginRecords.Add(new LoginRecord { AppUser = user, Date = DateTime.UtcNow.Date });
            await _db.SaveChangesAsync();

            return (user, []);
        }

        public async Task<(AppUser? user, IEnumerable<string> errors)> CreateLocalUserAsync(
            string email, string password, string nickName, string avatarUrl)
        {
            var user = new AppUser
            {
                GoogleId       = $"local:{Guid.NewGuid():N}",
                Email          = email,
                UserName       = email,
                NickName       = nickName,
                AvatarUrl      = avatarUrl,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (null, result.Errors.Select(e => e.Description));

            _db.LoginRecords.Add(new LoginRecord { AppUser = user, Date = DateTime.UtcNow.Date });
            await _db.SaveChangesAsync();

            return (user, []);
        }
    }
}
