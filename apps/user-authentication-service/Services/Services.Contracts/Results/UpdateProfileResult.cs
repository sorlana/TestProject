using Services.Contracts.DTOs;

namespace Services.Contracts.Results;

/// <summary>
/// Результат обновления профиля пользователя
/// </summary>
public class UpdateProfileResult
{
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Обновленный профиль пользователя
    /// </summary>
    public UserProfileDto? Profile { get; set; }
    
    /// <summary>
    /// Список ошибок
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
    
    /// <summary>
    /// Флаг необходимости подтверждения нового email
    /// </summary>
    public bool RequiresEmailVerification { get; set; }
    
    /// <summary>
    /// Флаг необходимости подтверждения нового телефона
    /// </summary>
    public bool RequiresPhoneVerification { get; set; }
}
