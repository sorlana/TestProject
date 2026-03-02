namespace Infrastructure.ExternalServices.Configuration;

/// <summary>
/// Настройки SMS провайдера
/// </summary>
public class SmsSettings
{
    /// <summary>
    /// API ключ SMS провайдера
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// URL API SMS провайдера
    /// </summary>
    public string ApiUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя отправителя (если поддерживается провайдером)
    /// </summary>
    public string? SenderName { get; set; }
    
    /// <summary>
    /// Таймаут запросов в секундах
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
