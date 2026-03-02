namespace WebApi.Settings;

/// <summary>
/// Настройки для интеграции с внешним nopCommerce API
/// </summary>
public class NopCommerceApiSettings
{
    /// <summary>
    /// Базовый URL внешнего nopCommerce API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API ключ для аутентификации запросов
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Таймаут HTTP запросов в секундах
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Количество повторных попыток при сбоях
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Задержка между повторными попытками в секундах
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}
