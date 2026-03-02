namespace Domain.Entities.Entities;

/// <summary>
/// Доменная сущность токена обновления (refresh token)
/// Используется для обновления JWT токенов без повторной аутентификации
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Уникальный идентификатор токена
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя, которому принадлежит токен
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Значение токена (криптографически случайная строка)
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и время истечения срока действия токена
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Дата и время создания токена
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время отзыва токена
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Флаг отзыва токена
    /// </summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// IP адрес, с которого был отозван токен
    /// </summary>
    public string? RevokedByIp { get; set; }
    
    /// <summary>
    /// Токен, который заменил текущий (для цепочки токенов)
    /// </summary>
    public string? ReplacedByToken { get; set; }
    
    /// <summary>
    /// Навигационное свойство к пользователю
    /// </summary>
    public User User { get; set; } = null!;
}
