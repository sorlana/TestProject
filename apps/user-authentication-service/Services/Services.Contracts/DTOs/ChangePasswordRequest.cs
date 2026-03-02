namespace Services.Contracts.DTOs;

/// <summary>
/// DTO для запроса смены пароля
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Текущий пароль
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Новый пароль
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
