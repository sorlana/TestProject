using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Infrastructure.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// ДОБАВЛЯЕМ: Конфигурация с учетом переменных окружения
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// ДОБАВЛЯЕМ: Получаем строку подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Формируем строку подключения из отдельных переменных окружения
    var dbHost = builder.Configuration["DB_HOST"] ?? "localhost";
    var dbPort = builder.Configuration["DB_PORT"] ?? "5432";
    var dbName = builder.Configuration["DB_NAME"] ?? "testdb";
    var dbUser = builder.Configuration["DB_USER"] ?? "test_user";
    var dbPassword = builder.Configuration["DB_PASSWORD"] ?? "test123";

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

    // Добавляем SSL только если он не отключен
    var sslMode = builder.Configuration["DB_SSL_MODE"];
    if (!string.IsNullOrEmpty(sslMode) && sslMode.ToLower() != "disable")
    {
        connectionString += $";SslMode={sslMode}";
    }
}

// ИСПРАВЛЕНО: Регистрация DatabaseContext
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: builder.Configuration.GetValue<int>("Database:RetryCount", 3),
            maxRetryDelay: TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Database:RetryDelaySeconds", 5)),
            errorCodesToAdd: null
        );
    });

    // Включаем подробное логирование в Development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Сохраняем ваши существующие сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// УЛУЧШЕННЫЙ КОД: Автоматическое применение миграций при запуске с обработкой ошибок
var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", true);
Console.WriteLine($"AutoMigrate setting: {autoMigrate}");

if (autoMigrate)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            
            Console.WriteLine("Starting database migration check...");
            
            // Проверяем, есть ли ожидающие миграции
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            var appliedMigrations = dbContext.Database.GetAppliedMigrations();
            
            Console.WriteLine($"Applied migrations: {appliedMigrations.Count()}");
            Console.WriteLine($"Pending migrations: {pendingMigrations.Count()}");
            
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"  - {migration}");
                }
                
                dbContext.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            else
            {
                Console.WriteLine("No pending migrations.");
            }
            
            // Проверяем подключение к БД
            try
            {
                if (dbContext.Database.CanConnect())
                {
                    Console.WriteLine("Database connection successful.");
                }
                else
                {
                    Console.WriteLine("WARNING: Cannot connect to database.");
                }
            }
            catch (Exception connectEx)
            {
                Console.WriteLine($"WARNING: Cannot connect to database: {connectEx.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR during migration process: {ex.Message}");
        Console.WriteLine($"Error type: {ex.GetType().FullName}");
        
        // В Development мы можем попробовать продолжить без миграций
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("Continuing in development mode despite migration error...");
        }
        else
        {
            // В Production выходим с ошибкой
            throw;
        }
    }
}
else
{
    Console.WriteLine("AutoMigrate is disabled. Skipping migrations.");
}

// Сохраняем вашу существующую конфигурацию пайплайна запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Маппинг контроллеров
app.MapControllers();

// Health endpoint для Docker
app.MapGet("/health", () => "Healthy");

// Простой тестовый endpoint
app.MapGet("/", () => "API is running!");
app.MapGet("/api/test", () => new { message = "Hello from API!", time = DateTime.UtcNow });

// Диагностические endpoints
app.MapGet("/diagnostics/migrations", async (DatabaseContext dbContext, IConfiguration config) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        
        return Results.Ok(new
        {
            DatabaseConnected = canConnect,
            AppliedMigrations = appliedMigrations.ToArray(),
            PendingMigrations = pendingMigrations.ToArray(),
            AppliedCount = appliedMigrations.Count(),
            PendingCount = pendingMigrations.Count(),
            AutoMigrateEnabled = config.GetValue<bool>("Database:AutoMigrate", true),
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            DatabaseConnected = false,
            Error = ex.Message,
            Timestamp = DateTime.UtcNow
        });
    }
});

app.MapGet("/diagnostics/config", (IConfiguration config) =>
{
    var configValues = new
    {
        DatabaseAutoMigrate = config["Database:AutoMigrate"],
        DatabaseRetryCount = config["Database:RetryCount"],
        DatabaseRetryDelaySeconds = config["Database:RetryDelaySeconds"],
        ConnectionString = connectionString,
        AspNetCoreEnvironment = config["ASPNETCORE_ENVIRONMENT"],
        DbHost = config["DB_HOST"],
        DbName = config["DB_NAME"],
        DbUser = config["DB_USER"],
        DbPort = config["DB_PORT"],
        DbSslMode = config["DB_SSL_MODE"],
        Timestamp = DateTime.UtcNow
    };
    
    return Results.Ok(configValues);
});

app.Run();