namespace WebApi.Settings;

/// <summary>
/// Настройки для интеграции с Google OAuth 2.0
/// </summary>
public class GoogleOAuthSettings
{
    /// <summary>
    /// Идентификатор клиента Google OAuth
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ клиента Google OAuth
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}
