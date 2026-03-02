using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.PostgreSQL;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.PostgreSql;
using Piranha.Manager.Editor;
using Serilog;
using LandingCms.Data;

// Включаем устаревшее поведение временных меток для совместимости с Piranha CMS
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/landing-cms-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Добавление сервисов Piranha CMS
builder.Services.AddPiranha(options =>
{
    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseManager();
    
    // Настройка Entity Framework с PostgreSQL
    options.UseEF<PostgreSqlDb>(db =>
        db.UseNpgsql(builder.Configuration.GetConnectionString("PiranhaDb")));
    
    // Настройка Identity для Piranha Manager
    options.UseIdentity<IdentityPostgreSQLDb>(identity =>
        identity.UseNpgsql(builder.Configuration.GetConnectionString("PiranhaDb")));
});

// Добавление контроллеров и Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddPiranhaManagerOptions();

// Настройка Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PiranhaDb") ?? throw new InvalidOperationException("Connection string 'PiranhaDb' not found."),
        name: "postgresql",
        tags: new[] { "db", "ready" });

var app = builder.Build();

// Настройка middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Настройка Piranha CMS
app.UsePiranha(options =>
{
    // Инициализация Piranha
    App.Init(options.Api);
    
    // Регистрация моделей контента
    var contentTypeBuilder = new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly);
    contentTypeBuilder.Build();
    
    // Настройка маршрутов Piranha
    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
});

// Инициализация начальных данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    await SeedData.InitializeAdminAsync(services, logger);
    await SeedData.InitializeContentAsync(services, logger);
}

// Настройка маршрутов
app.MapRazorPages();

// Маршрут для покупки тарифа
app.MapControllerRoute(
    name: "tariff-purchase",
    pattern: "tariff/purchase/{tariffId}",
    defaults: new { controller = "Tariff", action = "Purchase" });

// Piranha CMS обрабатывает все остальные маршруты через свою систему
app.MapControllerRoute(
    name: "default",
    pattern: "{**slug}",
    defaults: new { controller = "Cms", action = "Page" });

// Health check endpoints
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health");

try
{
    Log.Information("Запуск Landing CMS");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с ошибкой");
}
finally
{
    Log.CloseAndFlush();
}
