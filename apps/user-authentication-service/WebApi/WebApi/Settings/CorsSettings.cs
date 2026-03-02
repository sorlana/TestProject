namespace WebApi.Settings;

/// <summary>
/// Настройки CORS
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// Список доверенных доменов фронтенда
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Разрешенные HTTP методы
    /// </summary>
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE" };

    /// <summary>
    /// Разрешенные заголовки
    /// </summary>
    public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type" };

    /// <summary>
    /// Разрешить учетные данные (cookies, authorization headers)
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}
