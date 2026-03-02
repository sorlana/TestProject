namespace Services.Contracts.DTOs;

/// <summary>
/// DTO для запроса аутентификации через Google OAuth
/// </summary>
public class GoogleAuthRequest
{
    /// <summary>
    /// Google ID токен, полученный от клиента
    /// </summary>
    public string IdToken { get; set; } = string.Empty;
}
