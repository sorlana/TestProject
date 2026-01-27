// D:\SITES\My\TestProject\apps\test-service\WebApi\Program.cs
using System; // ДОБАВЬТЕ ЭТУ СТРОКУ
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем Swagger для тестирования
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Настройка пайплайна запросов
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

app.Run();