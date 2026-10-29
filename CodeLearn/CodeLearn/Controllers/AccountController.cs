using CodeLearn.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CodeLearn.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService              _auth;
        private readonly IEmailService             _email;
        private readonly IAvatarService            _avatar;
        private readonly IRegistrationFlowService  _regFlow;
        private readonly IConfiguration            _cfg;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public AccountController(
            IAuthService             auth,
            IEmailService            email,
            IAvatarService           avatar,
            IRegistrationFlowService regFlow,
            IConfiguration           cfg,
            Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _auth    = auth;
            _email   = email;
            _avatar  = avatar;
            _regFlow = regFlow;
            _cfg     = cfg;
            _cache   = cache;
        }

        // Кулдаун отправки письма на один email (анти-спам в чужой ящик)
        private bool OnCooldown(string key, TimeSpan window)
        {
            if (_cache.TryGetValue(key, out _)) return true;
            _cache.Set(key, true, window);
            return false;
        }

        // ── Login ───────────────────────────────────────────────────────────

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");

            if (!ModelState.IsValid) return View(model);

            var (ok, lockedOut) = await _auth.LoginAsync(model.Email.Trim().ToLowerInvariant(), model.Password);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, lockedOut
                    ? "Аккаунт заблокирован на 10 минут из-за множества неверных попыток"
                    : "Неверный email или пароль");
                return View(model);
            }

            return RedirectToAction("Index", "Courses");
        }

        // ── Register ────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");
            if (!_cfg.GetValue<bool>("Platform:RegistrationOpen", true))
                return RedirectToAction("Login");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("email")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");
            if (!_cfg.GetValue<bool>("Platform:RegistrationOpen", true))
                return RedirectToAction("Login");

            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();

            if (await _auth.EmailExistsAsync(email))
            {
                ModelState.AddModelError(nameof(model.Email), "Этот email уже используется");
                return View(model);
            }

            // не шлём код на один email чаще раза в минуту
            if (OnCooldown("regcode:" + email, TimeSpan.FromMinutes(1)))
            {
                ModelState.AddModelError(string.Empty, "Код уже отправлен. Подожди минуту перед повтором.");
                return View(model);
            }

            var code = _regFlow.GenerateCode();
            if (!await _email.SendVerificationCodeAsync(email, code))
            {
                ModelState.AddModelError(string.Empty, "Не удалось отправить код. Проверьте почтовые настройки.");
                return View(model);
            }

            _regFlow.StorePendingLocal(email, model.Password, code);
            return RedirectToAction("VerifyEmail");
        }

        // ── Сброс пароля ────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("email")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            // не отправляем повторно на тот же email чаще раза в минуту
            var token = OnCooldown("pwreset:" + email, TimeSpan.FromMinutes(1))
                ? null
                : await _auth.GeneratePasswordResetTokenAsync(email);
            if (token != null)
            {
                var encoded = Microsoft.AspNetCore.WebUtilities.WebEncoders
                    .Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
                var link = Url.Action("ResetPassword", "Account",
                    new { email, token = encoded }, Request.Scheme)!;
                await _email.SendPasswordResetAsync(email, link);
            }

            // Не раскрываем, существует ли аккаунт
            ViewBag.Sent = true;
            return View(new ForgotPasswordViewModel());
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string rawToken;
            try
            {
                rawToken = System.Text.Encoding.UTF8.GetString(
                    Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ссылка повреждена. Запроси сброс заново.");
                return View(model);
            }

            var (ok, errors) = await _auth.ResetPasswordAsync(
                model.Email.Trim().ToLowerInvariant(), rawToken, model.Password);

            if (!ok)
            {
                foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
                return View(model);
            }

            TempData["ResetOk"] = "Пароль изменён. Войди с новым паролем.";
            return RedirectToAction("Login");
        }

        // ── VerifyEmail ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");

            var pending = _regFlow.GetPendingLocal();
            if (pending == null) return RedirectToAction("Register");

            ViewBag.Email = "Код отправлен на " + _regFlow.MaskEmail(pending.Email);
            return View(new VerifyEmailViewModel());
        }

        [HttpPost]
        public IActionResult VerifyEmail(VerifyEmailViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Courses");

            var pending = _regFlow.GetPendingLocal();
            if (pending == null) return RedirectToAction("Register");

            if (!ModelState.IsValid)
            {
                ViewBag.Email = "Код отправлен на " + _regFlow.MaskEmail(pending.Email);
                return View(model);
            }

            var validation = _regFlow.ValidateCode(model.Code ?? "");

            if (validation == CodeValidationResult.Missing)
                return RedirectToAction("Register");

            if (validation == CodeValidationResult.Expired)
            {
                ModelState.AddModelError(string.Empty, "Код просрочен. Запросите новый.");
                ViewBag.Email = "Код отправлен на " + _regFlow.MaskEmail(pending.Email);
                return View(model);
            }

            if (validation == CodeValidationResult.Invalid)
            {
                ModelState.AddModelError(string.Empty, "Неверный код");
                ViewBag.Email = "Код отправлен на " + _regFlow.MaskEmail(pending.Email);
                return View(model);
            }

            _regFlow.MarkEmailVerified();
            return RedirectToAction("SetNickname");
        }

        // ── Google OAuth ────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account")
            };
            props.Parameters["prompt"] = "select_account";
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var googleId  = result.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var email     = result.Principal!.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var avatarUrl = result.Principal!.FindFirst("picture")?.Value ?? "";

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var user = await _auth.FindByGoogleIdAsync(googleId);
            if (user != null)
            {
                await _auth.EnsureLoginRecordAsync(user);
                await _auth.SignInAsync(user);
                return RedirectToAction("Index", "Courses");
            }

            _regFlow.StorePendingGoogle(googleId, InputSanitizer.StripHtml(email), avatarUrl);
            return RedirectToAction("SetNickname");
        }

        // ── SetNickname ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult SetNickname()
        {
            var hasGoogle = _regFlow.GetPendingGoogle() != null;
            var hasLocal  = _regFlow.IsEmailVerified();
            if (!hasGoogle && !hasLocal) return RedirectToAction("Login");

            return View(new SetNicknameViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SetNickname(SetNicknameViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var cleanNick = InputSanitizer.SanitizeNickname(model.NickName);
            if (!InputSanitizer.IsValidNickname(cleanNick))
            {
                ModelState.AddModelError(nameof(model.NickName), "Недопустимые символы в никнейме");
                return View(model);
            }

            string? avatarUrl = null;
            if (model.Avatar is { Length: > 0 })
            {
                var (ok, error, url) = await _avatar.SaveAsync(model.Avatar);
                if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
                avatarUrl = url;
            }

            var google = _regFlow.GetPendingGoogle();
            if (google != null)
            {
                var (user, errors) = await _auth.CreateGoogleUserAsync(
                    google.GoogleId, google.Email, cleanNick, avatarUrl ?? google.AvatarUrl);

                if (user == null)
                {
                    foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
                    return View(model);
                }

                _regFlow.ClearPendingGoogle();
                await _auth.SignInAsync(user);
                return RedirectToAction("Index", "Courses");
            }

            var local = _regFlow.GetPendingLocal();
            if (local == null || !_regFlow.IsEmailVerified())
                return RedirectToAction("Register");

            var (localUser, localErrors) = await _auth.CreateLocalUserAsync(
                local.Email, local.Password, cleanNick, avatarUrl ?? "");

            if (localUser == null)
            {
                foreach (var e in localErrors) ModelState.AddModelError(string.Empty, e);
                return View(model);
            }

            _regFlow.ClearPendingLocal();
            await _auth.SignInAsync(localUser);
            return RedirectToAction("Index", "Courses");
        }

        // ── Logout ──────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _auth.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ── Letter Avatar (SVG) ─────────────────────────────────────────────
        // GET /Account/LetterAvatar?nick=Alex
        // Returns a pixel-style SVG avatar with the first letter of the nickname.
        // Color is deterministic based on the nickname (hash), so it's always the
        // same for a given user but looks random across users.
        [HttpGet, Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult LetterAvatar(string nick = "?")
        {
            if (string.IsNullOrWhiteSpace(nick)) nick = "?";
            var letter = char.ToUpperInvariant(nick[0]).ToString();

            // Palette of saturated colors that look good on a pixel bg
            var colors = new[] {
                "#cc3355", "#3366cc", "#22aa77", "#cc8800",
                "#8844cc", "#cc5522", "#1199bb", "#aa9900"
            };
            var hash  = nick.Aggregate(0, (acc, c) => acc * 31 + c) & 0x7FFFFFFF;
            var color = colors[hash % colors.Length];

            // Pixel border effect: 2-px dark strips on all 4 sides
            var svg = $"""
<svg xmlns="http://www.w3.org/2000/svg" width="34" height="34">
  <rect width="34" height="34" fill="{color}"/>
  <rect x="0" y="0" width="34" height="2" fill="rgba(0,0,0,0.4)"/>
  <rect x="0" y="32" width="34" height="2" fill="rgba(0,0,0,0.4)"/>
  <rect x="0" y="0" width="2" height="34" fill="rgba(0,0,0,0.4)"/>
  <rect x="32" y="0" width="2" height="34" fill="rgba(0,0,0,0.4)"/>
  <text x="17" y="23" text-anchor="middle"
        font-family="'Courier New',Courier,monospace"
        font-weight="900" font-size="18" fill="white"
        style="image-rendering:pixelated">{letter}</text>
</svg>
""";

            Response.Headers["Cache-Control"] = "public, max-age=86400";
            return Content(svg, "image/svg+xml");
        }
    }
}

