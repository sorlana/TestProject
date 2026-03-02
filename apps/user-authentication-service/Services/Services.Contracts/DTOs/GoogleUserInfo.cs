namespace Services.Contracts.DTOs;

/// <summary>
/// DTO с информацией о пользователе из Google OAuth
/// </summary>
public class GoogleUserInfo
{
    /// <summary>
    /// Уникальный идентификатор Google пользователя
    /// </summary>
    public string GoogleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Email адрес
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Флаг подтверждения email в Google
    /// </summary>
    public bool EmailVerified { get; set; }
}
