namespace Infrastructure.ExternalServices.Configuration;

/// <summary>
/// Настройки интеграции с внешним nopCommerce API
/// </summary>
public class NopCommerceSettings
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
    /// Таймаут запросов в секундах
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Количество попыток повтора при ошибках
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Время жизни кеша планов подписок в минутах
    /// </summary>
    public int CacheTtlMinutes { get; set; } = 60;
}
