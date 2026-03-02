using Domain.Entities.Enums;

namespace Domain.Entities.Entities;

/// <summary>
/// Доменная сущность лога аутентификации
/// Используется для аудита и отслеживания операций аутентификации
/// </summary>
public class AuthenticationLog
{
    /// <summary>
    /// Уникальный идентификатор записи лога
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя (может быть null для неудачных попыток входа)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Имя пользователя (для случаев, когда UserId недоступен)
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Тип события аутентификации
    /// </summary>
    public AuthenticationEventType EventType { get; set; }
    
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Причина неудачи (если операция не успешна)
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// IP адрес, с которого выполнена операция
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent браузера/клиента
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Временная метка события
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Уровень важности лога
    /// </summary>
    public LogLevel LogLevel { get; set; }
    
    /// <summary>
    /// Навигационное свойство к пользователю
    /// </summary>
    public User? User { get; set; }
}
