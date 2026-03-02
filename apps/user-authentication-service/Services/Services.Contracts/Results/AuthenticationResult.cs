using Services.Contracts.DTOs;

namespace Services.Contracts.Results;

/// <summary>
/// Результат операции аутентификации
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Refresh токен для обновления
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Дата истечения токена доступа
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public UserDto? User { get; set; }
    
    /// <summary>
    /// Список ошибок
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
    
    /// <summary>
    /// Флаг необходимости подтверждения телефона
    /// </summary>
    public bool RequiresPhoneVerification { get; set; }
}
