using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using WebApi.Settings;
using WebApi.Middleware;

// Настройка Serilog из конфигурации
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск микросервиса аутентификации пользователей");

    var builder = WebApplication.CreateBuilder(args);

    // Настройка Serilog из appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId());

    // Настройка JWT Settings
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JwtSettings не настроены в конфигурации");

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    // Настройка CORS Settings
    var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>()
        ?? throw new InvalidOperationException("CorsSettings не настроены в конфигурации");

    builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("CorsSettings"));

    // Настройка Google OAuth Settings
    builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));

    // Настройка SMS Provider Settings
    builder.Services.Configure<SmsProviderSettings>(builder.Configuration.GetSection("SmsProvider"));

    // Настройка NopCommerce API Settings
    builder.Services.Configure<NopCommerceApiSettings>(builder.Configuration.GetSection("NopCommerceApi"));

    // Настройка Rate Limiting Settings
    builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection("RateLimiting"));

    // Регистрация AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Настройка CORS
const string corsPolicy = "DefaultCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .WithMethods(corsSettings.AllowedMethods)
              .WithHeaders(corsSettings.AllowedHeaders);

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Настройка Redis для distributed cache
var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string не настроена");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "UserAuthService:";
});

// Настройка Health Checks
var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection не настроена");

var rabbitmqConnection = builder.Configuration.GetConnectionString("RabbitMQ")
    ?? "amqp://guest:guest@localhost:5672";

builder.Services.AddHealthChecks()
    .AddNpgSql(
        postgresConnection,
        name: "postgresql",
        tags: new[] { "db", "ready" })
    .AddRedis(
        redisConnection,
        name: "redis",
        tags: new[] { "cache", "ready" })
    .AddRabbitMQ(
        rabbitConnectionString: rabbitmqConnection,
        name: "rabbitmq",
        tags: new[] { "messaging", "ready" });

// Настройка JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true; // В продакшене требуется HTTPS
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // Убираем дополнительное время на разницу часов
    };
});

builder.Services.AddAuthorization();

// Настройка HSTS для продакшена
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Настройка HTTPS редиректа
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});

// Регистрация контроллеров
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API микросервиса аутентификации пользователей",
        Version = "v1",
        Description = "REST API для регистрации, аутентификации и управления пользователями",
        Contact = new OpenApiContact
        {
            Name = "Команда разработки",
            Email = "support@example.com"
        }
    });

    // Настройка JWT авторизации в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {ваш токен}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Включаем XML комментарии если они есть
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Настройка Graceful Shutdown
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Настройка таймаута для graceful shutdown
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
});

// Настройка Host Shutdown Timeout
builder.Host.ConfigureHostOptions(options =>
{
    // Таймаут для завершения обработки текущих запросов при получении SIGTERM
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Регистрация обработчика graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Получен сигнал завершения работы (SIGTERM). Начинается graceful shutdown...");
    Log.Information("Прекращение приема новых запросов");
});

lifetime.ApplicationStopped.Register(() =>
{
    Log.Information("Все текущие запросы обработаны. Закрытие подключений к внешним сервисам");
    
    // Закрытие подключений будет выполнено автоматически через Dispose
    // для зарегистрированных сервисов (DbContext, Redis, RabbitMQ)
    
    Log.Information("Graceful shutdown завершен успешно");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API микросервиса аутентификации v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "API микросервиса аутентификации";
        options.DefaultModelsExpandDepth(-1); // Скрываем секцию Schemas по умолчанию
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DisplayRequestDuration();
    });
}
else
{
    // В продакшене используем HSTS
    app.UseHsts();
}

// Добавляем Exception Handling Middleware первым для перехвата всех исключений
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Добавляем Request Logging Middleware для обогащения логов
app.UseMiddleware<RequestLoggingMiddleware>();

// Редирект на HTTPS
app.UseHttpsRedirection();

// Добавляем CORS
app.UseCors(corsPolicy);

// Добавляем Rate Limiting Middleware перед аутентификацией
app.UseMiddleware<RateLimitingMiddleware>();

// Добавляем middleware аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Настройка Health Check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        });
        
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        });
        
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // Liveness probe не проверяет зависимости, только работоспособность процесса
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
    }
});

// Маппинг контроллеров
app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с критической ошибкой");
    throw;
}
finally
{
    Log.Information("Завершение работы микросервиса аутентификации");
    Log.CloseAndFlush();
}

