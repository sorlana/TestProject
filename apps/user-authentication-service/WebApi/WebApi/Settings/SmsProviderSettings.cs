namespace WebApi.Settings;

/// <summary>
/// Настройки для интеграции с SMS провайдером
/// </summary>
public class SmsProviderSettings
{
    /// <summary>
    /// Название провайдера (Twilio, SMS.ru и т.д.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// API ключ для аутентификации
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL API провайдера
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
}
