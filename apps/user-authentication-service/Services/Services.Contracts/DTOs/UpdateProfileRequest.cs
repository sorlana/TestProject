namespace Services.Contracts.DTOs;

/// <summary>
/// DTO для запроса обновления профиля пользователя
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Email адрес
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Отчество пользователя
    /// </summary>
    public string? MiddleName { get; set; }
}
