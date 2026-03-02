using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Entities;

/// <summary>
/// Доменная сущность пользователя системы
/// Наследуется от IdentityUser для интеграции с ASP.NET Core Identity
/// </summary>
public class User : IdentityUser<Guid>
{
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
    /// Идентификатор Google аккаунта для OAuth аутентификации
    /// </summary>
    public string? GoogleId { get; set; }
    
    /// <summary>
    /// Дата и время создания учетной записи
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время последнего обновления учетной записи
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Флаг активности учетной записи
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Флаг мягкого удаления (soft delete)
    /// </summary>
    public bool Deleted { get; set; }
    
    /// <summary>
    /// Коллекция refresh токенов пользователя
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    /// <summary>
    /// Коллекция подписок пользователя
    /// </summary>
    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    
    /// <summary>
    /// Коллекция логов аутентификации пользователя
    /// </summary>
    public ICollection<AuthenticationLog> AuthenticationLogs { get; set; } = new List<AuthenticationLog>();
}
