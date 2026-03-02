namespace Services.Contracts.DTOs;

/// <summary>
/// DTO для запроса входа в систему
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Флаг "Запомнить меня" для увеличения времени жизни refresh токена
    /// </summary>
    public bool RememberMe { get; set; }
}
