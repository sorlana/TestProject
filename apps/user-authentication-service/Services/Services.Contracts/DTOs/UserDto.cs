namespace Services.Contracts.DTOs;

/// <summary>
/// DTO с базовой информацией о пользователе
/// </summary>
public class UserDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public Guid Id { get; set; }
    
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
    
    /// <summary>
    /// Флаг подтверждения email
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// Флаг подтверждения телефона
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }
}
