using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WebApi.Middleware;

/// <summary>
/// Middleware для ограничения частоты неудачных попыток входа
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Настройки для блокировки по пользователю
    private const int MaxUserAttempts = 5;
    private const int UserLockoutMinutes = 15;
    private const int UserAttemptWindowMinutes = 15;

    // Настройки для блокировки по IP
    private const int MaxIpAttempts = 20;
    private const int IpLockoutMinutes = 60;
    private const int IpAttemptWindowMinutes = 60;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Проверяем только запросы на вход
        if (context.Request.Path.StartsWithSegments("/api/auth/login") && 
            context.Request.Method == HttpMethods.Post)
        {
            var ipAddress = GetClientIpAddress(context);

            // Проверяем блокировку по IP
            if (await IsIpBlockedAsync(ipAddress))
            {
                _logger.LogWarning("IP адрес {IpAddress} заблокирован из-за превышения лимита попыток входа", ipAddress);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Слишком много неудачных попыток входа с вашего IP адреса. Попробуйте позже."
                });
                return;
            }

            // Сохраняем оригинальный response body stream
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Проверяем результат запроса
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                // Увеличиваем счетчик неудачных попыток по IP
                await IncrementIpAttemptsAsync(ipAddress);

                // Пытаемся получить username из тела запроса для блокировки по пользователю
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                
                try
                {
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (!string.IsNullOrEmpty(loginRequest?.UserName))
                    {
                        await IncrementUserAttemptsAsync(loginRequest.UserName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке тела запроса для rate limiting");
                }
            }

            // Копируем response обратно
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Проверяет, заблокирован ли пользователь
    /// </summary>
    public async Task<bool> IsUserBlockedAsync(string userName)
    {
        var key = $"ratelimit:user:{userName}:blocked";
        var blocked = await _cache.GetStringAsync(key);
        return blocked != null;
    }

    /// <summary>
    /// Проверяет, заблокирован ли IP адрес
    /// </summary>
    private async Task<bool> IsIpBlockedAsync(string ipAddress)
    {
        var key = $"ratelimit:ip:{ipAddress}:blocked";
        var blocked = await _cache.GetStringAsync(key);
        return blocked != null;
    }

    /// <summary>
    /// Увеличивает счетчик неудачных попыток для пользователя
    /// </summary>
    private async Task IncrementUserAttemptsAsync(string userName)
    {
        var key = $"ratelimit:user:{userName}:attempts";
        var attemptsStr = await _cache.GetStringAsync(key);
        
        int attempts = string.IsNullOrEmpty(attemptsStr) ? 0 : int.Parse(attemptsStr);
        attempts++;

        if (attempts >= MaxUserAttempts)
        {
            // Блокируем пользователя
            var blockKey = $"ratelimit:user:{userName}:blocked";
            await _cache.SetStringAsync(blockKey, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(UserLockoutMinutes)
            });

            _logger.LogWarning("Пользователь {UserName} заблокирован на {Minutes} минут после {Attempts} неудачных попыток",
                userName, UserLockoutMinutes, attempts);

            // Сбрасываем счетчик
            await _cache.RemoveAsync(key);
        }
        else
        {
            // Сохраняем счетчик
            await _cache.SetStringAsync(key, attempts.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(UserAttemptWindowMinutes)
            });
        }
    }

    /// <summary>
    /// Увеличивает счетчик неудачных попыток для IP адреса
    /// </summary>
    private async Task IncrementIpAttemptsAsync(string ipAddress)
    {
        var key = $"ratelimit:ip:{ipAddress}:attempts";
        var attemptsStr = await _cache.GetStringAsync(key);
        
        int attempts = string.IsNullOrEmpty(attemptsStr) ? 0 : int.Parse(attemptsStr);
        attempts++;

        if (attempts >= MaxIpAttempts)
        {
            // Блокируем IP
            var blockKey = $"ratelimit:ip:{ipAddress}:blocked";
            await _cache.SetStringAsync(blockKey, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(IpLockoutMinutes)
            });

            _logger.LogWarning("IP адрес {IpAddress} заблокирован на {Minutes} минут после {Attempts} неудачных попыток",
                ipAddress, IpLockoutMinutes, attempts);

            // Сбрасываем счетчик
            await _cache.RemoveAsync(key);
        }
        else
        {
            // Сохраняем счетчик
            await _cache.SetStringAsync(key, attempts.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(IpAttemptWindowMinutes)
            });
        }
    }

    /// <summary>
    /// Получает IP адрес клиента с учетом прокси
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // Проверяем заголовки прокси
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// DTO для десериализации запроса входа
    /// </summary>
    private class LoginRequestDto
    {
        public string? UserName { get; set; }
    }
}
