using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace CodeLearn.Services
{
    public class RegistrationFlowService : IRegistrationFlowService
    {
        private const string KeyGoogleId  = "PendingGoogleId";
        private const string KeyEmail     = "PendingEmail";
        private const string KeyAvatarUrl = "PendingAvatarUrl";
        private const string KeyRegEmail  = "PendingRegEmail";
        private const string KeyRegPwd    = "PendingRegPassword";
        private const string KeyCode      = "PendingRegCode";
        private const string KeyExpires   = "PendingRegCodeExpires";
        private const string KeyVerified  = "PendingRegVerified";

        private readonly IHttpContextAccessor _http;
        private readonly IDataProtector       _protector;
        private ISession Session => _http.HttpContext!.Session;

        public RegistrationFlowService(IHttpContextAccessor http, IDataProtectionProvider dp)
        {
            _http      = http;
            _protector = dp.CreateProtector("RegistrationFlow.Password");
        }

        public string GenerateCode() =>
            RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        public void StorePendingLocal(string email, string password, string code)
        {
            Session.SetString(KeyRegEmail, email);
            Session.SetString(KeyRegPwd,   _protector.Protect(password));
            Session.SetString(KeyCode,     code);
            Session.SetString(KeyExpires,  DateTime.UtcNow.AddMinutes(10).Ticks.ToString());
            Session.Remove(KeyVerified);
        }

        public void StorePendingGoogle(string googleId, string email, string avatarUrl)
        {
            Session.SetString(KeyGoogleId,  googleId);
            Session.SetString(KeyEmail,     email);
            Session.SetString(KeyAvatarUrl, avatarUrl);
        }

        public void MarkEmailVerified()
        {
            Session.SetString(KeyVerified, "1");
            Session.Remove(KeyCode);
            Session.Remove(KeyExpires);
        }

        public CodeValidationResult ValidateCode(string inputCode)
        {
            var code       = Session.GetString(KeyCode);
            var expiresRaw = Session.GetString(KeyExpires);

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(expiresRaw))
                return CodeValidationResult.Missing;

            if (!long.TryParse(expiresRaw, out var ticks))
                return CodeValidationResult.Missing;

            if (DateTime.UtcNow > new DateTime(ticks, DateTimeKind.Utc))
                return CodeValidationResult.Expired;

            return string.Equals(inputCode.Trim(), code, StringComparison.Ordinal)
                ? CodeValidationResult.Valid
                : CodeValidationResult.Invalid;
        }

        public string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 1) return email;
            var name   = email[..at];
            var domain = email[(at + 1)..];
            return name[0] + new string('*', Math.Min(3, name.Length - 1)) + "@" + domain;
        }

        public PendingLocalData? GetPendingLocal()
        {
            var email       = Session.GetString(KeyRegEmail);
            var protectedPwd = Session.GetString(KeyRegPwd);
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(protectedPwd))
                return null;
            try { return new PendingLocalData(email, _protector.Unprotect(protectedPwd)); }
            catch { return null; }
        }

        public PendingGoogleData? GetPendingGoogle()
        {
            var googleId  = Session.GetString(KeyGoogleId);
            var email     = Session.GetString(KeyEmail)     ?? "";
            var avatarUrl = Session.GetString(KeyAvatarUrl) ?? "";
            return string.IsNullOrEmpty(googleId)
                ? null
                : new PendingGoogleData(googleId, email, avatarUrl);
        }

        public bool IsEmailVerified() =>
            Session.GetString(KeyVerified) == "1";

        public void ClearPendingLocal()
        {
            Session.Remove(KeyRegEmail);
            Session.Remove(KeyRegPwd);
            Session.Remove(KeyVerified);
        }

        public void ClearPendingGoogle()
        {
            Session.Remove(KeyGoogleId);
            Session.Remove(KeyEmail);
            Session.Remove(KeyAvatarUrl);
        }
    }
}
