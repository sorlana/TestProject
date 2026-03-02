namespace Services.Implementations.Settings;

/// <summary>
/// Настройки JWT токенов
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Секретный ключ для подписи токенов
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Издатель токена (Issuer)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Аудитория токена (Audience)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Время жизни access токена в минутах (по умолчанию 15 минут)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    
    /// <summary>
    /// Время жизни refresh токена в днях (по умолчанию 7 дней)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Время жизни refresh токена в днях при выборе "Запомнить меня" (по умолчанию 30 дней)
    /// </summary>
    public int RefreshTokenExpirationDaysRememberMe { get; set; } = 30;
}
