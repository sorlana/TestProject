namespace Services.Contracts.DTOs;

/// <summary>
/// DTO для запроса регистрации нового пользователя
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email адрес
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя (необязательное поле)
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Фамилия (необязательное поле)
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Отчество (необязательное поле)
    /// </summary>
    public string? MiddleName { get; set; }
}
