namespace Domain.Entities.Entities;

/// <summary>
/// Доменная сущность кода подтверждения телефона
/// Используется для верификации номера телефона через SMS
/// </summary>
public class PhoneVerificationCode
{
    /// <summary>
    /// Уникальный идентификатор кода
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Номер телефона, для которого создан код
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Код подтверждения (6-значный числовой код)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и время истечения срока действия кода (10 минут)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Флаг использования кода
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// Дата и время создания кода
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата и время использования кода
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// Количество попыток проверки кода
    /// </summary>
    public int AttemptCount { get; set; }
}
