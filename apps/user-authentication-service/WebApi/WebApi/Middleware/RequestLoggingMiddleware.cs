using Serilog.Context;

namespace WebApi.Middleware;

/// <summary>
/// Middleware для обогащения логов информацией о запросе
/// Добавляет IP адрес, User Agent и другую информацию в контекст логирования
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Получение IP адреса клиента
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        // Получение User Agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // Добавление информации в контекст логирования Serilog
        using (LogContext.PushProperty("IpAddress", ipAddress))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            // Логирование входящего запроса
            _logger.LogInformation("Входящий запрос: {Method} {Path} от {IpAddress}", 
                context.Request.Method, 
                context.Request.Path, 
                ipAddress);

            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                // Логирование завершенного запроса
                _logger.LogInformation("Запрос завершен: {Method} {Path} - Статус: {StatusCode} - Длительность: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                // Логирование ошибки при обработке запроса
                _logger.LogError(ex, "Ошибка при обработке запроса: {Method} {Path} - Длительность: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    duration);

                throw;
            }
        }
    }
}
