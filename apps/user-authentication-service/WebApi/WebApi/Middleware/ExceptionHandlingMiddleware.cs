using System.Net;
using System.Text.Json;
using WebApi.Exceptions;

namespace WebApi.Middleware;

/// <summary>
/// Middleware для централизованной обработки исключений
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Ошибка валидации данных",
                Errors = validationEx.Errors
            },
            UnauthorizedException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Ошибка аутентификации. Проверьте учетные данные."
            },
            ForbiddenException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = "Доступ запрещен. У вас недостаточно прав для выполнения этой операции."
            },
            NotFoundException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = exception.Message
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже."
            }
        };

        context.Response.StatusCode = response.StatusCode;

        // Логируем детали ошибки
        LogException(exception, context, response.StatusCode);

        // В режиме разработки добавляем детали исключения
        if (_environment.IsDevelopment() && response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            response.Details = new ExceptionDetails
            {
                Type = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private void LogException(Exception exception, HttpContext context, int statusCode)
    {
        var logLevel = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, exception,
            "Обработано исключение {ExceptionType} для запроса {Method} {Path}. StatusCode: {StatusCode}",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path,
            statusCode);
    }

    /// <summary>
    /// Модель ответа об ошибке
    /// </summary>
    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Errors { get; set; }
        public ExceptionDetails? Details { get; set; }
    }

    /// <summary>
    /// Детали исключения (только для режима разработки)
    /// </summary>
    private class ExceptionDetails
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }
}
