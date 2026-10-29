using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Npgsql;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using CodeLearn.Data;
using CodeLearn.Helpers;
using CodeLearn.Models;
using CodeLearn.Services;

// ── Serilog bootstrap (до создания builder) ──────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Устанавливаем AsyncLocalWriter один раз — перехватывает Console.Out/Error глобально,
// но направляет вывод только в async-контекст запустившего скрипт (остальные → реальный stdout).
Console.SetOut(new CodeLearn.Helpers.AsyncLocalWriter(Console.Out));
Console.SetError(new CodeLearn.Helpers.AsyncLocalWriter(Console.Error));

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

// ── Dev / Prod cookie policy ──────────────────────────────────────────────────
var isDev = builder.Environment.IsDevelopment();
var securePolicy = isDev
    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
    : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;

// ── HttpClient (для AI API) ───────────────────────────────────────────────────
builder.Services.AddHttpClient();

// ── MVC + Anti-Forgery ────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(opt =>
{
    opt.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(o =>
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddAntiforgery(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SecurePolicy = securePolicy;
    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation(cfg =>
    cfg.DisableDataAnnotationsValidation = true);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── PostgreSQL ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ── Session ───────────────────────────────────────────────────────────────────
builder.Services.AddSession(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.SecurePolicy = securePolicy;
    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(opt =>
{
    // Auth: Login, Register, ForgotPassword
    opt.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Компилятор кода — дорогой CPU, жёсткий лимит
    opt.AddFixedWindowLimiter("run", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 20;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // AI тьютор — дорогой по $, лимит по IP
    opt.AddFixedWindowLimiter("ai", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    opt.RejectionStatusCode = 429;
});

// ── ASP.NET Core Identity ─────────────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.SignIn.RequireConfirmedAccount = false;
    opt.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Кастомные claims (email, picture, nickname) в cookie
builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

// ── Сервисы приложения ────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();
builder.Services.AddScoped<IRegistrationFlowService, RegistrationFlowService>();

// Настройки Identity cookie
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.Cookie.Name = "CodeLearnAuth";
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SecurePolicy = securePolicy;
    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    opt.ExpireTimeSpan = TimeSpan.FromDays(7);
    opt.SlidingExpiration = true;

    // API-маршруты (/api/...) получают 401, а не redirect на страницу логина
    opt.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// ── Google OAuth (поверх Identity) ───────────────────────────────────────────
builder.Services.AddAuthentication()
    .AddGoogle(opt =>
    {
        opt.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        opt.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        opt.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
        opt.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        opt.CorrelationCookie.HttpOnly = true;
        opt.Events.OnCreatingTicket = ctx =>
        {
            var picture = ctx.User.GetProperty("picture").GetString();
            if (!string.IsNullOrEmpty(picture))
                ctx.Identity?.AddClaim(new System.Security.Claims.Claim("picture", picture));
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// ── Диагностика DB + автосидинг C# курса ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.GetDbConnection() is NpgsqlConnection conn)
        app.Logger.LogInformation("DB target: {Host}:{Port}/{Database}", conn.Host, conn.Port, conn.Database);

    await db.Database.MigrateAsync();
    await CSharpSeeder.SeedAsync(db);
    await CSharpSeeder.SeedExtraLessonsAsync(db);
    await CourseSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var fwdOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                     | ForwardedHeaders.XForwardedProto
                     | ForwardedHeaders.XForwardedHost
};
fwdOptions.KnownNetworks.Clear();
fwdOptions.KnownProxies.Clear();
app.UseForwardedHeaders(fwdOptions);

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// ── Заголовки безопасности ────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://www.youtube.com https://s.ytimg.com https://open.spotify.com https://w.soundcloud.com; " +
        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: https://lh3.googleusercontent.com https://i.ytimg.com https://i1.sndcdn.com https://i.scdn.co; " +
        "connect-src 'self' https://www.youtube.com https://api-widget.soundcloud.com https://api.soundcloud.com https://open.spotify.com; " +
        "frame-src https://www.youtube.com https://www.youtube-nocookie.com https://open.spotify.com https://w.soundcloud.com; " +
        "frame-ancestors 'none';";
    await next();
});

app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// ── Маршруты БЕЗ глобального rate limiting ────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();
// Rate limiting применяется точечно через [EnableRateLimiting("auth")]
// на конкретных actions в AccountController (Login, Register, ForgotPassword)

app.Run();