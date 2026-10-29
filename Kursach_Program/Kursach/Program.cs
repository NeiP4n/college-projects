using Kursach.Components;
using Microsoft.EntityFrameworkCore;
using Kursach.Data;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext (по умолчанию Scoped)
builder.Services.AddDbContext<KursachContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KursachContext"))); // Используем строку подключения "KursachContext"

// Регистрация IDbContextFactory с жизненным циклом Scoped
builder.Services.AddDbContextFactory<KursachContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KursachContext")), ServiceLifetime.Scoped); // Здесь тоже заменили на "KursachContext"

// Добавление MudBlazor сервисов
builder.Services.AddMudServices();

// Добавление поддержки Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Добавление поддержки QuickGrid с Entity Framework
builder.Services.AddQuickGridEntityFrameworkAdapter();

// Настройка фильтра ошибок для разработки
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Настройка пайплайна
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
