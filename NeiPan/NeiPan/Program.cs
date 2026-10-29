var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Убираем порты и даем Kestrel использовать стандартные порты (80 и 443)
builder.WebHost.UseUrls("http://neipan.duckdns.org", "https://neipan.duckdns.org");

var app = builder.Build();

// Убираем различия между окружениями для теста
app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
