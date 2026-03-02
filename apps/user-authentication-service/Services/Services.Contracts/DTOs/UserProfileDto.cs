namespace Services.Contracts.DTOs;

/// <summary>
/// DTO с полной информацией профиля пользователя
/// </summary>
public class UserProfileDto
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
    
    /// <summary>
    /// Дата создания учетной записи
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Флаг наличия связанного Google аккаунта
    /// </summary>
    public bool HasGoogleAccount { get; set; }
}
