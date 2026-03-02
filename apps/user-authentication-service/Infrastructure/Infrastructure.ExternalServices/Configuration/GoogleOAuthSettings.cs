namespace Infrastructure.ExternalServices.Configuration;

/// <summary>
/// Настройки Google OAuth
/// </summary>
public class GoogleOAuthSettings
{
    /// <summary>
    /// Client ID приложения в Google Cloud Console
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Client Secret приложения в Google Cloud Console
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}
