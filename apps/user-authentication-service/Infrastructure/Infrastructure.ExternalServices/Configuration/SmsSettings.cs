namespace Infrastructure.ExternalServices.Configuration;

/// <summary>
/// Настройки SMS провайдера
/// </summary>
public class SmsSettings
{
    /// <summary>
    /// Провайдер SMS (Twilio, Mock и т.д.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// API ключ SMS провайдера
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// URL API SMS провайдера
    /// </summary>
    public string ApiUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Номер телефона отправителя
    /// </summary>
    public string FromPhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Account SID (для Twilio)
    /// </summary>
    public string? AccountSid { get; set; }
    
    /// <summary>
    /// Таймаут запросов в секундах
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
